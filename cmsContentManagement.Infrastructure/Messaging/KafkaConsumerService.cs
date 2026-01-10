using System.Text.Json;
using cmsContentManagement.Application.Common.Settings;
using cmsContentManagement.Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace cmsContentManagement.Infrastructure.Messaging;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IConsumer<Ignore, string> _consumer;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        IOptions<KafkaSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;

        _logger.LogInformation($"Kafka Config: BootstrapServers={_settings.BootstrapServers}, Topic={_settings.Topic}, GroupId={_settings.GroupId}");

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            // Add robustness
            SocketTimeoutMs = 60000,
            SessionTimeoutMs = 30000,
        };

        try 
        {
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to create Kafka consumer.");
             // Don't throw here to avoid crashing start. Will handle null consumer in ExecuteAsync.
             _consumer = null; 
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield to ensure we don't block the startup thread, as Consume is blocking.
        await Task.Yield();

        if (_consumer == null) 
        {
            _logger.LogError("Kafka consumer is not initialized. Stopping service.");
            return;
        }

        try
        {
            _consumer.Subscribe(_settings.Topic);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, $"Failed to subscribe to topic {_settings.Topic}");
             return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var message = consumeResult.Message.Value;
                
                _logger.LogInformation($"Received Kafka message: {message}");

                try 
                {
                    var fileEvent = JsonSerializer.Deserialize<FileUploadedEvent>(message);

                    if (fileEvent != null && fileEvent.EntryId != Guid.Empty && !string.IsNullOrEmpty(fileEvent.Url))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var contentService = scope.ServiceProvider.GetRequiredService<IContentManagmentService>();
                            // We need a method that doesn't require UserId, or we need to know the UserId
                            // Assuming for now a new method UpdateContentAssetUrl exists or we use the existing one if we had UserId
                            // Since we don't have UserId in the event (assumed), we'll add a system update method.
                            await contentService.UpdateContentAssetUrl(fileEvent.EntryId, fileEvent.Url);
                        }
                        
                        _consumer.Commit(consumeResult);
                    }
                    else 
                    {
                         _logger.LogWarning("Invalid message format");
                         // Commit anyway to avoid blocking? Or DLQ? For now commit.
                         _consumer.Commit(consumeResult);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing Kafka message");
                    _consumer.Commit(consumeResult); // Skip bad message
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
                // Wait a bit before retrying
                await Task.Delay(1000, stoppingToken);
            }
        }

        _consumer.Close();
    }
}
