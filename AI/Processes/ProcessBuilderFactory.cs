using Microsoft.SemanticKernel;
using MyProject.AI.Processes.Steps;

namespace MyProject.AI.Processes;

#pragma warning disable SKEXP0080 

// for details see https://learn.microsoft.com/en-us/semantic-kernel/frameworks/process/examples/example-first-process
public static class ProcessBuilderFactory
{
    internal static KernelProcess CreateFirstProcess()
    {
        // Create a procBuilder that will interact with the chat completion service
        ProcessBuilder procBuilder = new(nameof(procBuilder));

        var createrStep = procBuilder.AddStepFromType<CreaterStep>();

        var proofReaderStep = procBuilder.AddStepFromType<ProofReader>();

        var proxyStep = procBuilder.AddProxyStep(id: "proxyStep", [ProcessEvents.ProcessTopics.Completed]);

        procBuilder
          .OnInputEvent(ProcessEvents.ProcessStarted)
          .SendEventTo(new ProcessFunctionTargetBuilder(createrStep, functionName: nameof(CreaterStep.StartAsync)));

        createrStep
          .OnEvent(ProcessEvents.DoneCreate)
          .SendEventTo(new ProcessFunctionTargetBuilder(proofReaderStep, parameterName: "text"));

        proofReaderStep
          .OnEvent(ProcessEvents.RecheckRequired)
          .SendEventTo(new ProcessFunctionTargetBuilder(createrStep, functionName: nameof(CreaterStep.RecheckAsync), parameterName: "recheck"));

        proofReaderStep
            .OnEvent(ProcessEvents.EndFormat)
            .EmitExternalEvent(proxyStep, ProcessEvents.ProcessTopics.Completed);

        return procBuilder.Build();
    }
}
#pragma warning restore SKEXP0080