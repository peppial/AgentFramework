// based on https://github.com/microsoft/Agent-Framework-Samples/blob/main/07.Workflow/code_samples/dotNET/04.dotnet-agent-framework-workflow-aifoundry-condition.ipynb
// and https://learn.microsoft.com/en-us/agent-framework/tutorials/workflows/workflow-with-branching-logic?pivots=programming-language-csharp

using System;
using System.ComponentModel;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Azure.AI.Agents.Persistent;
using DotNetEnv;
using StateBasedDynamicWorkflow;

// Load environment variables from .env file
Env.Load(".env");

var azure_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var azure_foundry_model_id = "gpt-4o-mini";

var bing_conn_id = Environment.GetEnvironmentVariable("BING_CONNECTION_ID");

const string EvangelistInstructions = @"
You are a technology evangelist create a first draft for a technical tutorials.
1. Each knowledge point in the outline must include a link. Follow the link to access the content related to the knowledge point in the outline. Expand on that content.
2. Each knowledge point must be explained in detail.
3. Rewrite the content according to the entry requirements, including the title, outline, and corresponding content. It is not necessary to follow the outline in full order.
4. The content must be more than 200 words.
4. Output draft as Markdown format. set 'draft_content' to the draft content.
5. return result as JSON with fields 'draft_content' (string).";

const string ContentReviewerInstructions = @"
You are a content reviewer and need to check whether the tutorial's draft content meets the following requirements:

1. The draft content less than 200 words, set 'review_result' to 'No' and 'reason' to 'Content is too short'. If the draft content is more than 200 words, set 'review_result' to 'Yes' and 'reason' to 'The content is good'.
2. set 'draft_content' to the original draft content.
3. return result as JSON with fields 'review_result' ('Yes' or  'No' ) and 'reason' (string) and 'draft_content' (string).";

const string PublisherInstructions = @"
You are the content publisher ,run code to save the tutorial's draft content as a Markdown file. Saved file's name is marked with current date and time, such as yearmonthdayhourminsec. Note that if it is 1-9, you need to add 0, such as  20240101123045.md.
";

string OUTLINE_Content = @"
# Introduce AI Agent


## What's AI Agent

https://github.com/microsoft/ai-agents-for-beginners/tree/main/01-intro-to-ai-agents


***Note*** Don's create any sample code


## Introduce Azure AI Foundry Agent Service

https://learn.microsoft.com/en-us/azure/ai-foundry/agents/overview


***Note*** Don's create any sample code


## Microsoft Agent Framework

https://github.com/microsoft/agent-framework/tree/main/docs/docs-templates


***Note*** Don's create any sample code
";

// Uncomment to use Bing Grounding Tool
//var bingGroundingConfig = new BingGroundingSearchConfiguration(bing_conn_id);

// BingGroundingToolDefinition bingGroundingTool = new(
//    new BingGroundingSearchToolParameters(
//        [bingGroundingConfig]
//    )
//);

PersistentAgentsClient persistentAgentsClient = new PersistentAgentsClient(azure_foundry_endpoint, new AzureCliCredential());

// Create the three specialized agents
var evangelistMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: azure_foundry_model_id,
    name: "Evangelist",
    instructions: EvangelistInstructions
//tools: [bingGroundingTool]
);

var contentReviewerMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
     model: azure_foundry_model_id,
     name: "ContentReviewer",
     instructions: ContentReviewerInstructions
);

var publisherMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: azure_foundry_model_id,
    name: "Publisher",
    instructions: PublisherInstructions,
    tools: [new CodeInterpreterToolDefinition()]
);

string evangelist_agentId = evangelistMetadata.Value.Id;
string contentReviewer_agentId = contentReviewerMetadata.Value.Id;
string publisher_agentId = publisherMetadata.Value.Id;

ChatClientAgentOptions EvangelistAgentOptions = new(instructions: EvangelistInstructions)
{
    ChatOptions = new()
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ContentResult)), "ContentResult", "Content Result with DraftContent"),
    }
};

ChatClientAgentOptions ReviewAgentOptions = new(instructions: ContentReviewerInstructions)
{
    ChatOptions = new()
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ReviewResult)), "ReviewResult", "Review Result From DraftContent")
    },

};
ChatClientAgentOptions PublisherAgentOptions = new(instructions: PublisherInstructions)
{
};


AIAgent evangelistagent = await persistentAgentsClient.GetAIAgentAsync(evangelist_agentId, new ChatOptions
{
    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ContentResult)), "ContentResult", "Content Result with DraftContent"),
});

AIAgent contentRevieweragent = await persistentAgentsClient.GetAIAgentAsync(contentReviewer_agentId, new ChatOptions
{
    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(ReviewResult)), "ReviewResult", "Review Result From DraftContent")
});

AIAgent publisheragent = await persistentAgentsClient.GetAIAgentAsync(publisher_agentId);

var draftExecutor = new DraftExecutor(evangelistagent);
var contentReviewerExecutor = new ContentReviewExecutor(contentRevieweragent);
var publishExecutor = new PublishExecutor(publisheragent);
var sendReviewerExecutor = new SendReviewExecutor();

var reviewExecutor = new HandleReviewExecutor();

Func<object?, bool> GetCondition(string expectedResult) =>
        reviewResult => reviewResult is ReviewResult review && review.Result == expectedResult;

var workflow = new WorkflowBuilder(draftExecutor)
            .AddEdge(draftExecutor, contentReviewerExecutor)
            .AddEdge(contentReviewerExecutor, publishExecutor, condition: GetCondition(expectedResult: "Yes"))
            .AddEdge(contentReviewerExecutor, sendReviewerExecutor, condition: GetCondition(expectedResult: "No"))
            .Build();


string prompt = @"You need to write a  draft based on the following outline and the content provided in the link corresponding to the outline.
After draft create , the reviewer check it , if it meets the requirements, it will be submitted to the publisher and save it as a Markdown file,
otherwise need to rewrite draft until it meets the requirements.
The provided outline content and related links is as follows:" + OUTLINE_Content;

Console.WriteLine(prompt);

var chat = new ChatMessage(ChatRole.User, prompt);

StreamingRun run = await InProcessExecution.StreamAsync(workflow, chat);

await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
string id = "";
string messageData = "";
await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentRunUpdateEvent executorComplete)
    {
        if (id == "")
        {
            id = executorComplete.ExecutorId;
        }
        if (id == executorComplete.ExecutorId)
        {
            messageData += executorComplete.Data.ToString();
        }
        else
        {
            id = executorComplete.ExecutorId;
        }
    }
}

