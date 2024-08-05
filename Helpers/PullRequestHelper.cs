using AzureDevOpsMonitoring.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsMonitoring.Helpers;

public class PullRequestHelper(VssConnection connection, string project, List<string> repoList)
{
    private readonly GitHttpClient _gitClient = connection.GetClient<GitHttpClient>();
    private readonly string _project = project;
    private readonly List<string> _repoList = repoList;

    public async Task<PullRequests> GetOpenPullRequests()
    {
        var repoIdAndName = await GetRepoIdsAsync(_repoList);
        var pullRequests = await GetOpenPullRequestsCountAsync(repoIdAndName);
        var result = new PullRequests
        {
            Total = pullRequests.Count,
            PullRequestList = pullRequests
        };
        return result;
    }

    private async Task<List<PullRequest>> GetOpenPullRequestsCountAsync(Dictionary<Guid, string> repoIdAndName)
    {
        var pullRequestList = new List<PullRequest>();

        foreach (var entry in repoIdAndName)
        {
            var pullRequests = await _gitClient.GetPullRequestsAsync(entry.Key, new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active
            });

            foreach (var pullRequest in pullRequests)
            {
                var daysActive = (DateTime.Now - pullRequest.CreationDate).TotalDays;
                var pullRequestJson = new PullRequest
                {
                    Repository = entry.Value,
                    Title = pullRequest.Title,
                    DaysActive = daysActive
                };
                pullRequestList.Add(pullRequestJson);
            }
        }

        return pullRequestList;
    }

    private async Task<Dictionary<Guid, string>> GetRepoIdsAsync(List<string> repoList)
    {
        var repoIds = new Dictionary<Guid, string>();

        foreach (string repoName in repoList)
        {
            var repo = await _gitClient.GetRepositoryAsync(_project, repoName);
            if (repo != null)
            {
                repoIds.Add(repo.Id, repo.Name);
            }
            else
            {
                Console.WriteLine($"Repository {repoName} not found.");
            }
        }

        return repoIds;
    }
}
