using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MultiAgent.AgentAsTool;
using OpenAI;

const string chatDeploymentName = "gpt-4o-mini";

AzureOpenAIClient client = new(new Uri("https://your-resource-name.openai.azure.com/"), new DefaultAzureCredential());


AIAgent stringAgent = client
    .GetChatClient(chatDeploymentName)
    .CreateAIAgent(
        name: "StringAgent",
        instructions: "You are a string manipulator and text analyzer",
        tools:
        [
            AIFunctionFactory.Create(StringTools.CountWords),
            AIFunctionFactory.Create(StringTools.TitleCase),
            AIFunctionFactory.Create(StringTools.RemoveSpaces),
            AIFunctionFactory.Create(StringTools.CountCharacters),
            AIFunctionFactory.Create(StringTools.CountVowels),
            AIFunctionFactory.Create(StringTools.ExtractNumbers),

        ])
    .AsBuilder()
    .Build();

AIAgent textAnalyzerAgent = client
    .GetChatClient(chatDeploymentName)
    .CreateAIAgent(
        name: "TextAnalyzerAgent",
        instructions: "You are a text analyzer",
        tools:
        [
            AIFunctionFactory.Create(TextAnalyzerTools.GetTextSummary),
            AIFunctionFactory.Create(TextAnalyzerTools.GetReadabilityScore)
        ])
    .AsBuilder()
    .Build();


AIAgent delegationAgent = client
    .GetChatClient(chatDeploymentName)
    .CreateAIAgent(
        name: "DelegateAgent",
        instructions: "You are a Delegator of String and Text Analyzer Tasks. Never does such work yourself. Delegate to appropriate agents.",
        tools:
        [
            stringAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "StringAgentAsTool"
            }),
            textAnalyzerAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "TextAnalyzerAgentAsTool"
            })
        ]
    )
    .AsBuilder()
    .Build();

AgentRunResponse responseFromDelegate = await delegationAgent.RunAsync("Analyze the text 'The quick brown fox jumps over the lazy dog.' and tell me the count of words and the readability score");
Console.WriteLine(responseFromDelegate);


