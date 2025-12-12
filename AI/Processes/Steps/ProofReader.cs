using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using MyProject.Hubs;
using System.ComponentModel;
using System.Text.Json;

namespace MyProject.AI.Processes.Steps;

#pragma warning disable SKEXP0080 
public class ProofReader(IHubContext<ProgressHub> _hub) : KernelProcessStep<CreaterState>
{
    readonly string _prompt = @"
Your job is to proofread the write-up provided to you. The audience is Indian. Write in Indian English.
You must do the following things:
1. Determine if the text passes the following criteria:
    a. text must use active voice.
    b. text should be free of spelling or grammar mistakes.
    c. text should be free of any offensive or inappropriate language.
    d. text should follow all the rules of effective writing.
    e. text should be divided into short paragraphs wherever possible.
    f. sentences of a paragraph can be arranged from general to specific, or vice versa as per need.
    
2. If the documentation does not pass 1, you must write detailed feedback of the changes 
    that are needed to improve the documentation. 
";

    internal CreaterState? _state;

    int iterations = 0;

    public override ValueTask ActivateAsync(KernelProcessStepState<CreaterState> state)
    {
        _state = state.State;

        iterations = 0;

        return ValueTask.CompletedTask;
    }

    [KernelFunction(nameof(StartAsync))]
    public async Task StartAsync(Kernel _kernel, KernelProcessStepContext context, string text)
    {
        _state!.Conversation.Add(new ChatMessageContent() { Role = AuthorRole.User, Content = text });

        ChatHistory chatHistory = new();

        chatHistory.AddSystemMessage(_prompt);

        chatHistory.AddRange(_state.Conversation);

        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();

        GeminiPromptExecutionSettings geminiPromptExecutionSettings = new() { ResponseSchema = typeof(EvaluationResponse), ResponseMimeType = "application/json" };

        EvaluationResponse? formattedResponse;

        ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, executionSettings: geminiPromptExecutionSettings);

        formattedResponse = JsonSerializer.Deserialize<EvaluationResponse>(response.Content ?? "{}");

        _state.Conversation.Add(new ChatMessageContent { Role = response.Role, Content = response.Content });

        if ((true == formattedResponse?.MeetsExpectations) || (iterations++ > 3))
        {
            await _hub.Clients.Group(ProgressHub.USER).SendAsync("OnStatusUpdate", "");

            await context.EmitEventAsync(new()
            {
                Id = ProcessEvents.EndFormat,
                Data = new AgentResponse()
                {
                    AgentMessage = text,
                    ChatHistory = "[]",
                    Tag = null

                }
            });
        }
        else
        {
            await context.EmitEventAsync(new()
            {
                Id = ProcessEvents.RecheckRequired,
                Data = $"{formattedResponse?.Explanation}. Additional Suggestions: {string.Join(' ', formattedResponse!.Suggestions.ToArray())}"
            });
        }
    }


}
#pragma warning restore SKEXP0080 

class EvaluationResponse
{
    [Description("Specifies if the proposed draft meets the expected standards for publishing.")]
    public bool MeetsExpectations { get; set; }

    [Description("An explanation of why the draft does or does not meet expectations.")]
    public string Explanation { get; set; } = "";

    [Description("A list of suggestions, may be empty if there no suggestions for improvement.")]
    public List<string> Suggestions { get; set; } = [];
}
