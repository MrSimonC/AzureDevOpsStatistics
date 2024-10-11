using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsStatistics.Helpers;

public class WorkItemHelper(VssConnection connection)
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
                Title = GetFieldValue(wi, "System.Title"),
                Description = GetFieldValue(wi, "System.Description"),
                AcceptanceCriteria = GetFieldValue(wi, "Microsoft.VSTS.Common.AcceptanceCriteria")
            }).ToList();

        return workItemInformation;
    }

    private static bool IncludeRemovedStatus(WorkItem workItem)
    {
        var state = GetFieldValue(workItem, "System.State");
        return state != "Removed";
    }

    private static string GetFieldValue(WorkItem workItem, string fieldName)
    {
        if (workItem.Fields.TryGetValue(fieldName, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}

public class WorkItemInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AcceptanceCriteria { get; set; } = string.Empty;
}
