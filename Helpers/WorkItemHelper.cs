using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text.RegularExpressions;

namespace AzureDevOpsStatistics.Helpers;

public partial class WorkItemHelper(VssConnection connection)
{
    private readonly WorkItemTrackingHttpClient _workItemClient = connection.GetClient<WorkItemTrackingHttpClient>();

    public async Task<List<WorkItemInfo>> GetWorkItemsAsync(int parentId, bool includeRemoved = false)
    {
        var parentWorkItem = await _workItemClient.GetWorkItemAsync(parentId, expand: WorkItemExpand.All);
        var childIds = parentWorkItem.Relations
            .Where(r => r.Rel == "System.LinkTypes.Hierarchy-Forward")
            .Select(r => int.Parse(r.Url.Split('/')[^1]))
            .ToArray();

        var childIdsFullDetails = await _workItemClient.GetWorkItemsAsync(childIds, expand: WorkItemExpand.All);

        // Collate the required information
        var workItemInformation = new List<WorkItemInfo>
        {
            // Add the description value from parentWorkItem as the first entry
            new() {
                Id = parentWorkItem.Id ?? 0,
                Title = HtmlToPlainText(GetFieldValue(parentWorkItem, "System.Title")),
                Description = HtmlToPlainText(GetDescription(parentWorkItem)),
                AcceptanceCriteria = HtmlToPlainText(GetAcceptanceCriteria(parentWorkItem)),
                Discussion = await GetDiscussionAsync(parentWorkItem.Id ?? 0)
            }
        };

        foreach (var wi in childIdsFullDetails)
        {
            if (includeRemoved || IncludeRemovedStatus(wi))
            {
                workItemInformation.Add(new WorkItemInfo
                {
                    Id = wi.Id ?? 0,
                    Title = HtmlToPlainText(GetFieldValue(wi, "System.Title")),
                    Description = HtmlToPlainText(GetDescription(wi)),
                    AcceptanceCriteria = HtmlToPlainText(GetAcceptanceCriteria(wi)),
                    Discussion = await GetDiscussionAsync(wi.Id ?? 0)
                });
            }
        }

        return workItemInformation;
    }

    private static string GetDescription(WorkItem wi)
    {
        return !string.IsNullOrEmpty(GetFieldValue(wi, "System.Description"))
            ? GetFieldValue(wi, "System.Description")
            : GetFieldValue(wi, "Microsoft.VSTS.TCM.ReproSteps");
    }

    private static string GetAcceptanceCriteria(WorkItem wi)
    {
        return !string.IsNullOrEmpty(GetFieldValue(wi, "Microsoft.VSTS.Common.AcceptanceCriteria"))
            ? GetFieldValue(wi, "Microsoft.VSTS.Common.AcceptanceCriteria")
            : GetFieldValue(wi, "Microsoft.VSTS.TCM.SystemInfo");
    }

    private static bool IncludeRemovedStatus(WorkItem workItem)
    {
        return GetFieldValue(workItem, "System.State") != "Removed";
    }

    private static string GetFieldValue(WorkItem workItem, string fieldName)
    {
        return workItem.Fields.TryGetValue(fieldName, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
    }

    private static string HtmlToPlainText(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        // Decode HTML entities and remove HTML tags
        var plainText = System.Net.WebUtility.HtmlDecode(html);
        plainText = plainText.Replace("<br>", "\n");
        return FindHtmlTags().Replace(plainText, string.Empty);
    }

    private async Task<string> GetDiscussionAsync(int workItemId)
    {
        var comments = await _workItemClient.GetCommentsAsync(workItemId);
        return string.Join("\n", comments.Comments.Select(c => HtmlToPlainText(c.Text)));
    }

    [GeneratedRegex("<.*?>")]
    private static partial Regex FindHtmlTags();
}

public class WorkItemInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcceptanceCriteria { get; set; } = string.Empty;
    public string Discussion { get; set; } = string.Empty;
}
