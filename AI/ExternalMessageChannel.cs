using Microsoft.SemanticKernel;

namespace MyProject.AI;

// this class is used by EmitExternalEvent 
// for details see https://learn.microsoft.com/en-us/semantic-kernel/frameworks/process/examples/example-human-in-loop
#pragma warning disable SKEXP0080 
public class ExternalKernelMessageChannel : IExternalKernelProcessMessageChannel
{
    public AgentResponse? Response { get; set; }

    public Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        Response = message.EventData?.ToObject() as AgentResponse;

        return Task.CompletedTask;
    }

    public ValueTask Initialize()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Uninitialize()
    {
        return ValueTask.CompletedTask;
    }
}

public class AgentResponse
{
    public required string AgentMessage { get; set; }

    public required string ChatHistory { get; set; }

    public object? Tag { get; set; }

}
#pragma warning restore SKEXP0080