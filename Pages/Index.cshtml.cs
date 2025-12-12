// NuGet\Install-Package Microsoft.SemanticKernel.Connectors.Google -Version 1.67.1-alpha
// NuGet\Install-Package Microsoft.SemanticKernel.Process.Core -Version 1.67.1-alpha
// NuGet\Install-Package Microsoft.SemanticKernel.Process.LocalRuntime -Version 1.67.1-alpha
// dotnet add package Microsoft.SemanticKernel.Connectors.SqliteVec --prerelease

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.SemanticKernel;
using MyProject.AI;
using MyProject.AI.Processes;
using MyProject.Filters;
using System.Text.Json.Nodes;

namespace MyProject.Pages;

#pragma warning disable SKEXP0080 

[GenerateAntiforgeryTokenCookieAttribute()]
public class IndexModel(Kernel _kernel, KernelProcess _kp) : PageModel
{
    // called by Ajax request coming from the page
    public async Task<IActionResult> OnPostAsync([FromBody] JsonNode data, CancellationToken cancellationToken)
    {
        string? chatText = data["text"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(chatText)) throw new Exception("chat message?");

        ExternalKernelMessageChannel myExternalMessageChannel = new();

        // send to process
        await _kp.StartAsync(_kernel, new KernelProcessEvent { Id = ProcessEvents.ProcessStarted, Data = chatText }, myExternalMessageChannel);
        

        return new JsonResult(new
        {
            text = myExternalMessageChannel.Response?.AgentMessage,
            chats = myExternalMessageChannel.Response?.ChatHistory
        });

    }

}
#pragma warning restore SKEXP0080