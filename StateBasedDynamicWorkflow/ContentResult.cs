using System.Text.Json.Serialization;

namespace StateBasedDynamicWorkflow;

public class ContentResult
{
    [JsonPropertyName("draft_content")]
    public string DraftContent { get; set; } = string.Empty;
}

