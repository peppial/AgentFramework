using System.Text.Json.Serialization;

namespace StateBasedDynamicWorkflow;

public class ReviewResult
{
    [JsonPropertyName("review_result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("draft_content")]
    public string DraftContent { get; set; } = string.Empty;
}

