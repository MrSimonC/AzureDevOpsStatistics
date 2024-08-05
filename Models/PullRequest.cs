using System.Text.Json.Serialization;

namespace AzureDevOpsMonitoring.Models;

public class PullRequest
{
    [JsonPropertyName("repository")]
    public string Repository { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("daysActive")]
    public double DaysActive { get; set; }
}
