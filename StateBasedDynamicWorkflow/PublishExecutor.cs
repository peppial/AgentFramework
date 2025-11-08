using System;
using System.Threading.Tasks;
using Azure.AI.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace StateBasedDynamicWorkflow;

public class PublishExecutor : ReflectingExecutor<PublishExecutor>, IMessageHandler<ReviewResult>
{
    private readonly AIAgent _publishAgent;

    public PublishExecutor(AIAgent publishAgent) : base("PublishExecutor")
    {
        this._publishAgent = publishAgent;
    }

    public async ValueTask HandleAsync(ReviewResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("PublishExecutor .......loading");
        var response = await this._publishAgent.RunAsync(message.DraftContent);
        Console.WriteLine($"Response from PublishExecutor: {response.Text}");
        await context.YieldOutputAsync($"Publishing result: {response.Text}");
    }
}

