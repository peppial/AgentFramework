using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace StateBasedDynamicWorkflow;

public class SendReviewExecutor : ReflectingExecutor<SendReviewExecutor>, IMessageHandler<ReviewResult>
{
    public SendReviewExecutor() : base("SendReviewExecutor")
    {
    }

    public async ValueTask HandleAsync(ReviewResult message, IWorkflowContext context, CancellationToken cancellationToken = default) =>
        await context.YieldOutputAsync($"Draft content sent: {message.Result}");
}

