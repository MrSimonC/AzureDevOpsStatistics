using System.Text.Json.Serialization;

namespace AzureDevOpsStatistics.Models;

public class PipelineBuildStatuses
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("refreshDateUtc")]
    public DateTime RefreshDateUtc { get; set; }

    [JsonPropertyName("pipelineBuildStatusList")]
    public List<PipelineBuildStatus> PipelineBuildStatusList { get; set; } = [];
}