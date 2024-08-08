using AzureDevOpsStatistics.Models;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsStatistics.Helpers;

public class BuildStatusHelper(VssConnection connection, string project, List<string> pipelineNames)
{
    private readonly BuildHttpClient _pipelineClient = connection.GetClient<BuildHttpClient>();
    private readonly string _project = project;
    private readonly List<string> _pipelineNames = pipelineNames;

    public async Task<PipelineBuildStatuses> GetBuildStatusListAsync()
    {
        var builds = await GetBuildStatusesAsync();
        var result = new PipelineBuildStatuses
        {
            Total = builds.Count,
            RefreshDateUtc = DateTime.UtcNow,
            PipelineBuildStatusList = builds
        };
        return result;
    }

    public async Task<List<PipelineBuildStatus>> GetBuildStatusesAsync()
    {
        var definitions = await _pipelineClient.GetDefinitionsAsync(_project);
        var pipelineIds = GetPipelineIdsByName(definitions, _pipelineNames);
        var buildStatusList = new List<PipelineBuildStatus>();

        foreach (int pipelineId in pipelineIds)
        {
            var pipelineDefinition = definitions.Find(x => x.Id == pipelineId);

            if (pipelineDefinition == null)
            {
                Console.WriteLine($"Pipeline {pipelineId} not found.");
                continue;
            }

            var builds = await _pipelineClient.GetBuildsAsync(_project, new List<int> { pipelineId }, top: 1);
            var latestBuild = builds.FirstOrDefault();

            var buildStatus = CreateBuildStatus(pipelineDefinition, latestBuild);
            buildStatusList.Add(buildStatus);
        }

        return buildStatusList;
    }

    private static List<int> GetPipelineIdsByName(IEnumerable<BuildDefinitionReference> definitions, List<string> pipelineNames)
    {
        var pipelineIds = new List<int>();

        foreach (var name in pipelineNames)
        {
            var definition = definitions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (definition != null)
            {
                pipelineIds.Add(definition.Id);
            }
            else
            {
                Console.WriteLine($"Pipeline '{name}' not found.");
            }
        }

        return pipelineIds;
    }

    private static PipelineBuildStatus CreateBuildStatus(BuildDefinitionReference pipelineDefinition, Build? latestBuild)
    {
        if (latestBuild != null)
        {
            return new PipelineBuildStatus
            {
                PipelineName = pipelineDefinition.Name,
                BuildId = latestBuild.Id,
                Status = latestBuild.Status.ToString() ?? string.Empty,
                Result = latestBuild.Result.ToString() ?? string.Empty,
                BuildStartDate = latestBuild.StartTime
            };
        }
        else
        {
            return new PipelineBuildStatus
            {
                PipelineName = $"No builds found for pipeline {pipelineDefinition.Id}."
            };
        }
    }
}
