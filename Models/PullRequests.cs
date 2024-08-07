using System.Text.Json.Serialization;

namespace AzureDevOpsStatistics.Models;

public class PullRequests
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("pullRequestList")]
    public List<PullRequest> PullRequestList { get; set; } = [];
}
