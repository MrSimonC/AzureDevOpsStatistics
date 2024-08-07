using System.Text.Json.Serialization;

namespace AzureDevOpsStatistics.Models;

public class PullRequest
{
    [JsonPropertyName("repository")]
    public string Repository { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("daysActive")]
    public double DaysActive { get; set; }

    [JsonPropertyName("createdByName")]
    public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("affectedFilesCount")]
    public int AffectedFilesCount { get; set; }
}
