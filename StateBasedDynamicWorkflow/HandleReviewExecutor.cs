using System;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace StateBasedDynamicWorkflow;

public class HandleReviewExecutor() : ReflectingExecutor<HandleReviewExecutor>("HandleReviewExecutor"), IMessageHandler<ReviewResult>
{
    public async ValueTask HandleAsync(ReviewResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (message.Result == "Yes")
        {
            await context.YieldOutputAsync("Yes");
        }
        else
        {
            throw new InvalidOperationException("The draft content is not good, cannot publish.");
        }
    }
}

