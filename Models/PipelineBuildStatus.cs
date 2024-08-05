using System.Text.Json.Serialization;

namespace AzureDevOpsMonitoring.Models;

public class PipelineBuildStatus
{
    [JsonPropertyName("pipelineName")]
    public string PipelineName { get; set; } = string.Empty;

    [JsonPropertyName("buildId")]
    public int BuildId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;
}