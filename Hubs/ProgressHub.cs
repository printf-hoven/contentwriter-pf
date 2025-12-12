using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace MyProject.Hubs;

public class ProgressHub: Hub
{
    public const string USER = "user";

    // save the connection ID and user id for later use
    // SignalR provides a built-in "Group" feature for storage
    public override async Task OnConnectedAsync()
    {
        string userId = USER;

        // authentication was not added to keep the tutorial simple
        // if (Context.User?.Identity?.Name is string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }
}

