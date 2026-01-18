using cmsContentManagement.Application.Common.Settings;
using cmsContentManagement.Application.Interfaces;
using cmsContentManagement.Middleware;
using cmsContentManagement.API.Middleware;
using cmsContentManagment.Infrastructure.Persistance;
using cmsContentManagment.Infrastructure.Repositories;
using cmsContentManagement.Infrastructure.Messaging;
using Microsoft.OpenApi.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CMS Content Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

string? redisOptionsConfiguration = builder.Configuration["Redis:Connection"];
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    redisOptions.Configuration = redisOptionsConfiguration;
});

builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ElasticSettings>>().Value;
    if (string.IsNullOrWhiteSpace(options.Url))
    {
        throw new InvalidOperationException("ElasticSettings:Url is not configured");
    }

    var clientSettings = new ElasticsearchClientSettings(new Uri(options.Url))
        .DefaultIndex(string.IsNullOrWhiteSpace(options.DefaultIndex) ? "content" : options.DefaultIndex);

    if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
    {
        clientSettings = clientSettings.Authentication(new BasicAuthentication(options.Username, options.Password));
    }

    return new ElasticsearchClient(clientSettings);
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<IContentManagmentService, ContentManagmentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowSpecificOrigins");
app.UseMiddleware<JwtValidationMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
