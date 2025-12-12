using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MyProject.Hubs;

namespace MyProject.AI.Processes.Steps;


#pragma warning disable SKEXP0080 
public class CreaterStep(IHubContext<ProgressHub> _hub) : KernelProcessStep<CreaterState>
{
    readonly string _prompt = @"You are an experienced writer specializing in 
clarity of expression with ten years of expertise. 
The text given to you has been written by a novice writer who has an average command over English
and expressive writing. 

The writer might have given you a collection of ideas, not in specific order.

Rewrite the provided text to ensure it is clear, concise, and enjoyable for the reader. Use language that
effectively conveys the author's intended message, while improving readability and flow without 
altering the original meaning. 

Since you have a vast knowledge, you can add additional explanations, examples, 
similies, idioms to make the write-up better.

Produce a single revised version that aligns with any specific instructions provided.

Format the text as html tags section h2 p ol li. Give a suitable heading in h1 tag.
";

    internal CreaterState? _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<CreaterState> state)
    {
        _state = state.State;

        return ValueTask.CompletedTask;
    }


    [KernelFunction(nameof(StartAsync))]
    public async Task StartAsync(Kernel _kernel, KernelProcessStepContext context, string text)
    {
        await _hub.Clients.Group(ProgressHub.USER).SendAsync("OnStatusUpdate", "starting...");

        _state!.Conversation.Add(new ChatMessageContent() { Role = AuthorRole.User, Content = $"The text is {text}" });

        ChatHistory chatHistory = new();

        chatHistory.AddSystemMessage(_prompt);

        chatHistory.AddRange(_state.Conversation);

        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();

        ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory);

        _state.Conversation.Add(new ChatMessageContent { Role = response.Role, Content = response.Content });

        await context.EmitEventAsync(new() { Id = ProcessEvents.DoneCreate, Data = response.Content });
    }

    [KernelFunction(nameof(RecheckAsync))]
    public async Task RecheckAsync(Kernel _kernel, KernelProcessStepContext context, string recheck)
    {
        await _hub.Clients.Group(ProgressHub.USER).SendAsync("OnStatusUpdate", "rewriting...");

        _state!.Conversation.Add(new ChatMessageContent() { Role = AuthorRole.User, Content = recheck });

        ChatHistory chatHistory = new();

        chatHistory.AddRange(_state.Conversation);

        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();

        ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory);

        _state.Conversation.Add(new ChatMessageContent { Role = response.Role, Content = response.Content });

        await context.EmitEventAsync(new() { Id = ProcessEvents.DoneCreate, Data = response.Content });
    }
}
#pragma warning restore SKEXP0080 

public class CreaterState
{
    internal List<ChatMessageContent> Conversation { get; set; } = [];
}
