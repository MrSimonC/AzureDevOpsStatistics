# AzureDevOpsStatistics

AzureDevOpsStatistics is a .NET project designed to monitor and report on various statistics from Azure DevOps, such as build statuses and open pull requests.

## Introduction

This project provides a way to fetch and display statistics from Azure DevOps, including the latest build statuses and the number of open pull requests for specified repositories.

## Features

- Fetch the latest build statuses for specified pipelines.
- Retrieve the number of open pull requests for specified repositories.
- Display the results in JSON format.

## Example Usage


# AzureDevOpsStatistics

AzureDevOpsStatistics is a .NET project designed to monitor and report on various statistics from Azure DevOps, such as build statuses and open pull requests.

## Introduction

This project provides a way to fetch and display statistics from Azure DevOps, including the latest build statuses and the number of open pull requests for specified repositories.

## Features

- Fetch the latest build statuses for specified pipelines.
- Retrieve the number of open pull requests for specified repositories.
- Display the results in JSON format.

## Example Usage

To use AzureDevOpsStatistics, follow these steps:

Set the `organization`, `project`, `repoList`, `pipelineNames`, and `personalAccessToken` variables according to your Azure DevOps configuration.

Use the example code below to get started:

```csharp
string organization = "your-organization";
string project = "YourProject";
var repoList = new List<string> { "Repo1", "Repo2", "Repo3" };
var pipelineNames = new List<string> { "Pipeline1", "Pipeline2" };
string personalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEV_OPS_PAT_TOKEN") ?? throw new InvalidOperationException("Environment variable 'AZURE_DEV_OPS_PAT_TOKEN' not found.");
using VssConnection? connection = new(new Uri($"https://dev.azure.com/{organization}"), new VssBasicCredential(string.Empty, personalAccessToken));

var buildStatusHelper = new BuildStatusHelper(connection, project, pipelineNames);
var latestBuildStatusJson = await buildStatusHelper.GetLatestBuildStatus();
Console.WriteLine(JsonSerializer.Serialize(latestBuildStatusJson));

var pullRequestHelper = new PullRequestHelper(connection, project, repoList);
var prs = await pullRequestHelper.GetOpenPullRequests();
Console.WriteLine(JsonSerializer.Serialize(prs));
```

Explanation of the code:

1. Create a new instance of `VssConnection` using the `organization` and `personalAccessToken` variables.
3. Create an instance of `BuildStatusHelper` and pass the `connection`, `project`, and `pipelineNames` variables.
4. Call the `GetLatestBuildStatus` method to fetch the latest build statuses and store the result in the `latestBuildStatusJson` variable.
5. Display the `latestBuildStatusJson` in JSON format using `Console.WriteLine` and `JsonSerializer.Serialize`.
6. Create an instance of `PullRequestHelper` and pass the `connection`, `project`, and `repoList` variables.
7. Call the `GetOpenPullRequests` method to retrieve the number of open pull requests and store the result in the `prs` variable.
8. Display the `prs` in JSON format using `Console.WriteLine` and `JsonSerializer.Serialize`.

Example output:

```json
[
    {
        "pipelineName": "Project E2E Testing Daily Run",
        "buildId": 101234,
        "status": "Completed",
        "result": "Succeeded"
    },
    {
        "pipelineName": "Main Integration Test",
        "buildId": 101235,
        "status": "Completed",
        "result": "Succeeded"
    }
]
```

and

```json
{
    "total": 2,
    "pullRequestList": [
        {
            "repository": "MyProject",
            "title": "Add HTTPS support to the Web API",
            "daysActive": 3.5318986591216
        },
        {
            "repository": "MyProject",
            "title": "Update main.yml for Azure Pipelines",
            "daysActive": 2.9816853024375
        }
    ]
}
```
