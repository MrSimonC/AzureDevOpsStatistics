using System.Text.Json.Serialization;

namespace AzureDevOpsStatistics.Models;

public class GitRepo
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
