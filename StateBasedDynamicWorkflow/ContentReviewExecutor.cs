using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace StateBasedDynamicWorkflow;

public class ContentReviewExecutor : ReflectingExecutor<ContentReviewExecutor>, IMessageHandler<ContentResult, ReviewResult>
{
    private readonly AIAgent _contentReviewerAgent;

    public ContentReviewExecutor(AIAgent contentReviewerAgent) : base("ContentReviewExecutor")
    {
        this._contentReviewerAgent = contentReviewerAgent;
    }

    public async ValueTask<ReviewResult> HandleAsync(ContentResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("ContentReviewExecutor .......loading");
        var response = await this._contentReviewerAgent.RunAsync(message.DraftContent);
        var reviewResult = JsonSerializer.Deserialize<ReviewResult>(response.Text);
        Console.WriteLine($"ContentReviewExecutor review result: {reviewResult?.Result}, reason: {reviewResult?.Reason}");

        return reviewResult ?? new ReviewResult();
    }
}

