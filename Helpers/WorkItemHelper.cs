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
        var parentWorkItemRelations = await _workItemClient.GetWorkItemAsync(parentId, expand: WorkItemExpand.Relations);
        var childIds = parentWorkItemRelations.Relations
            .Where(r => r.Rel == "System.LinkTypes.Hierarchy-Forward")
            .Select(r => int.Parse(r.Url.Split('/')[^1]))
            .ToArray();

        var childIdsFullDetails = await _workItemClient.GetWorkItemsAsync(childIds, expand: WorkItemExpand.All);

        // Collate the required information
        var workItemInformation = childIdsFullDetails
            .Where(wi => includeRemoved || IncludeRemovedStatus(wi))
            .Select(wi => new WorkItemInfo
            {
                Id = wi.Id ?? 0,
                Title = HtmlToPlainText(GetFieldValue(wi, "System.Title")),
                Description = HtmlToPlainText(GetDescription(wi)),
                AcceptanceCriteria = HtmlToPlainText(GetAcceptanceCriteria(wi))
            }).ToList();

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
        return FindHtmlTags().Replace(plainText, string.Empty);
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
}
