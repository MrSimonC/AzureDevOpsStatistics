using AzureDevOpsStatistics.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsStatistics.Helpers;

public class PullRequestHelper(VssConnection connection, string project, List<string> repoList)
{
    private readonly GitHttpClient _gitClient = connection.GetClient<GitHttpClient>();
    private readonly string _project = project;
    private readonly List<string> _repoList = repoList;

    public async Task<PullRequests> GetOpenPullRequests()
    {
        var gitRepos = await GetRepoIdsAsync(_repoList);
        var pullRequests = await GetOpenPullRequestsCountAsync(gitRepos);
        var result = new PullRequests
        {
            Total = pullRequests.Count,
            RefreshDateUtc = DateTime.UtcNow,
            PullRequestList = pullRequests
        };
        return result;
    }

    private async Task<List<PullRequest>> GetOpenPullRequestsCountAsync(List<GitRepo> gitRepos)
    {
        var pullRequestList = new List<PullRequest>();
        var tasks = new List<Task>();

        foreach (var gitRepo in gitRepos)
        {
            tasks.Add(Task.Run(async () =>
            {
                var pullRequests = await _gitClient.GetPullRequestsAsync(gitRepo.Id, new GitPullRequestSearchCriteria
                {
                    Status = PullRequestStatus.Active
                });

                foreach (var pullRequest in pullRequests)
                {
                    // Get the number of affected files
                    var iterations = await _gitClient.GetPullRequestIterationsAsync(gitRepo.Id, pullRequest.PullRequestId);

                    // Aggregate changes from all iterations
                    var affectedFiles = new HashSet<string>();
                    foreach (var iteration in iterations)
                    {
                        var changes = await _gitClient.GetPullRequestIterationChangesAsync(_project, gitRepo.Id, pullRequest.PullRequestId, iteration.Id ?? 0);
                        foreach (var change in changes.ChangeEntries)
                        {
                            affectedFiles.Add(change.Item.Path);
                        }
                    }

                    // get linked work items (as the GetPullRequestAsync() returns null for WorkItems)
                    var workItems = await _gitClient.GetPullRequestWorkItemRefsAsync(gitRepo.Id, pullRequest.PullRequestId);
                    var workItemIds = new List<int>();
                    foreach (var workItemIdUnparsed in workItems.Select(workItem => workItem.Id))
                    {
                        if (int.TryParse(workItemIdUnparsed, out int workItemId))
                        {
                            workItemIds.Add(workItemId);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse work item ID: {workItemIdUnparsed}");
                        }
                    }

                    var pullRequestJson = new PullRequest
                    {
                        Id = pullRequest.PullRequestId,
                        Repository = gitRepo.Name,
                        Title = pullRequest.Title,
                        CreatedDate = pullRequest.CreationDate,
                        CreatedByName = pullRequest.CreatedBy.DisplayName,
                        AffectedFilesCount = affectedFiles.Count,
                        LinkedWorkItemIds = workItemIds
                    };
                    pullRequestList.Add(pullRequestJson);
                }
            }));
        }

        await Task.WhenAll(tasks);

        return pullRequestList;
    }

    private async Task<List<GitRepo>> GetRepoIdsAsync(List<string> repoList)
    {
        var repoIds = new List<GitRepo>();
        var tasks = new List<Task>();

        foreach (string repoName in repoList)
        {
            tasks.Add(Task.Run(async () =>
            {
                var repo = await _gitClient.GetRepositoryAsync(_project, repoName);
                if (repo != null)
                {
                    repoIds.Add(new GitRepo
                    {
                        Id = repo.Id,
                        Name = repo.Name
                    });
                }
                else
                {
                    Console.WriteLine($"Repository {repoName} not found.");
                }
            }));
        }

        await Task.WhenAll(tasks);

        return repoIds;
    }
}
