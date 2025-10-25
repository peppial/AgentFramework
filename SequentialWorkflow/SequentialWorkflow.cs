// C# Script file for .NET 10 - Furniture Quote Workflow
// Run with: dotnet script FurnitureQuoteWorkflow.csx

#:package  Microsoft.Extensions.AI@9.9.1
#:package System.ClientModel@1.6.1.0
#:package Azure.Identity@1.15.0
#:package System.Linq.Async@6.0.3
#:package OpenTelemetry.Api@1.12.0
#:package Microsoft.Agents.AI.Workflows@1.0.0-preview.251001.3
#:package Microsoft.Agents.AI.OpenAI@1.0.0-preview.251001.3
#:package DotNetEnv@3.1.1

using System;
using System.IO;
using System.ComponentModel;
using System.ClientModel;
using OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using DotNetEnv;

// Load environment variables from .env file
Env.Load(".env");

var github_endpoint = Environment.GetEnvironmentVariable("GITHUB_ENDPOINT")
    ?? throw new InvalidOperationException("GITHUB_ENDPOINT is not set.");
var github_model_id = "gpt-4o";
var github_token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("GITHUB_TOKEN is not set.");

// Path to the image file
var imgPath = "home.png";

// Configure OpenAI client for GitHub Models
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(github_endpoint)
};
var openAIClient = new OpenAIClient(new ApiKeyCredential(github_token), openAIOptions);

// Agent definitions
const string SalesAgentName = "Sales-Agent";
const string SalesAgentInstructions = "You are my furniture sales consultant, you can find different furniture elements from the pictures and give me a purchase suggestion";

const string PriceAgentName = "Price-Agent";
const string PriceAgentInstructions = @"You are a furniture pricing specialist and budget consultant. Your responsibilities include:
        1. Analyze furniture items and provide realistic price ranges based on quality, brand, and market standards
        2. Break down pricing by individual furniture pieces
        3. Provide budget-friendly alternatives and premium options
        4. Consider different price tiers (budget, mid-range, premium)
        5. Include estimated total costs for room setups
        6. Suggest where to find the best deals and shopping recommendations
        7. Factor in additional costs like delivery, assembly, and accessories
        8. Provide seasonal pricing insights and best times to buy
        Always format your response with clear price breakdowns and explanations for the pricing rationale.";

const string QuoteAgentName = "Quote-Agent";
const string QuoteAgentInstructions = @"You are a assistant that create a quote for furniture purchase.
        1. Create a well-structured quote document that includes:
        2. A title page with the document title, date, and client name
        3. An introduction summarizing the purpose of the document
        4. A summary section with total estimated costs and recommendations
        5. Use clear headings, bullet points, and tables for easy readability
        6. All quotes are presented in markdown form";

// Helper function to read image bytes
async Task<byte[]> OpenImageBytesAsync(string path)
{
    return await File.ReadAllBytesAsync(path);
}

Console.WriteLine("Loading image...");
var imageBytes = await OpenImageBytesAsync(imgPath);
Console.WriteLine($"Image loaded: {imageBytes.Length} bytes");

// Create AI agents
Console.WriteLine("\nCreating AI agents...");
AIAgent salesagent = openAIClient.GetChatClient(github_model_id).CreateAIAgent(
    name: SalesAgentName,
    instructions: SalesAgentInstructions);

AIAgent priceagent = openAIClient.GetChatClient(github_model_id).CreateAIAgent(
    name: PriceAgentName,
    instructions: PriceAgentInstructions);

AIAgent quoteagent = openAIClient.GetChatClient(github_model_id).CreateAIAgent(
    name: QuoteAgentName,
    instructions: QuoteAgentInstructions);

Console.WriteLine("Agents created successfully!");

// Build sequential workflow
Console.WriteLine("\nBuilding workflow pipeline:");
Console.WriteLine("  Sales Agent → Price Agent → Quote Agent");

// Linear flow: Sales Agent → Price Agent → Quote Agent
var workflow = new WorkflowBuilder(salesagent)
    .AddEdge(salesagent, priceagent)
    .AddEdge(priceagent, quoteagent)
    .Build();

Console.WriteLine("Workflow built successfully!");

// Create user message with image
ChatMessage userMessage = new ChatMessage(ChatRole.User, [
    new DataContent(imageBytes, "image/png"),
    new TextContent("Please find the relevant furniture according to the image and give the corresponding price for each piece of furniture. Finally output generates a quotation")
]);

Console.WriteLine("\n" + new string('=', 80));
Console.WriteLine("EXECUTING SEQUENTIAL WORKFLOW");
Console.WriteLine(new string('=', 80));

// Execute workflow
StreamingRun run = await InProcessExecution.StreamAsync(workflow, userMessage);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

string currentExecutorId = "";
string messageData = "";
int agentCounter = 0;

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentRunUpdateEvent executorComplete)
    {
        // Detect agent change
        if (currentExecutorId == "")
        {
            currentExecutorId = executorComplete.ExecutorId;
            agentCounter++;
            Console.WriteLine($"\n[Stage {agentCounter}: {executorComplete.ExecutorId}]");
            Console.WriteLine(new string('-', 80));
        }

        if (currentExecutorId == executorComplete.ExecutorId)
        {
            // Accumulate message from same agent
            string chunk = executorComplete.Data?.ToString() ?? "";
            messageData += chunk;
            Console.Write(chunk); // Stream output in real-time
        }
        else
        {
            // New agent started
            currentExecutorId = executorComplete.ExecutorId;
            agentCounter++;
            Console.WriteLine($"\n\n[Stage {agentCounter}: {executorComplete.ExecutorId}]");
            Console.WriteLine(new string('-', 80));
            messageData = executorComplete.Data?.ToString() ?? "";
            Console.Write(messageData);
        }
    }
}

Console.WriteLine("\n" + new string('=', 80));
Console.WriteLine("WORKFLOW COMPLETED SUCCESSFULLY");
Console.WriteLine(new string('=', 80));

// Optionally save the final quote to a file
Console.WriteLine("\nDo you want to save the quote to a file? (y/n)");
var saveResponse = Console.ReadLine()?.ToLower();

if (saveResponse == "y" || saveResponse == "yes")
{
    string outputPath = $"furniture_quote_{DateTime.Now:yyyyMMdd_HHmmss}.md";
    await File.WriteAllTextAsync(outputPath, messageData);
    Console.WriteLine($"Quote saved to: {outputPath}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
