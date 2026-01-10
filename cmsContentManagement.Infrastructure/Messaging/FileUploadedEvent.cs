using System.Text.Json.Serialization;

namespace cmsContentManagement.Infrastructure.Messaging;

public class FileUploadedEvent
{
    [JsonPropertyName("entryId")]
    public Guid EntryId { get; set; }

    [JsonPropertyName("assetId")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
