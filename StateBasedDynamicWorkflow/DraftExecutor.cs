using System;
using System.Threading.Tasks;
using Azure.AI.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace StateBasedDynamicWorkflow;

public class DraftExecutor : ReflectingExecutor<DraftExecutor>, IMessageHandler<ChatMessage, ContentResult>
{
    private readonly AIAgent _evangelistAgent;

    public DraftExecutor(AIAgent evangelistAgent) : base("DraftExecutor")
    {
        this._evangelistAgent = evangelistAgent;
    }

    public async ValueTask<ContentResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"DraftExecutor .......loading \n{message.Text}");

        var response = await this._evangelistAgent.RunAsync(message);

        Console.WriteLine($"DraftExecutor response: {response.Text}");

        ContentResult contentResult = new ContentResult { DraftContent = Convert.ToString(response) ?? string.Empty };

        Console.WriteLine($"DraftExecutor generated content: {contentResult.DraftContent}");

        return contentResult;
    }
}

