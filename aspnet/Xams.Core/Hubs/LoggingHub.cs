using Microsoft.AspNetCore.SignalR;
using Xams.Core.Attributes;
using Xams.Core.Contexts;
using Xams.Core.Dtos;
using Xams.Core.Interfaces;
using Xams.Core.Utils;

namespace Xams.Core.Hubs;

[ServiceHub("Jobs Logging")]
public class LoggingHub : IServiceHub
{
    private static readonly string GroupName = "Xams_Jobs_Logging";
    public Task<Response<object?>> OnConnected(HubContext context)
    {
        context.Groups.AddToGroupAsync(context.SignalRContext.ConnectionId, GroupName);
        return Task.FromResult(ServiceResult.Success());
    }

    public Task<Response<object?>> OnDisconnected(HubContext context)
    {
        context.Groups.RemoveFromGroupAsync(context.SignalRContext.ConnectionId, GroupName);
        return Task.FromResult(ServiceResult.Success());
    }

    public Task<Response<object?>> OnReceive(HubContext context)
    {
        // Return error, clients should not be sending messages to this hub
        return Task.FromResult(ServiceResult.Error("Clients should not send messages to the Logging hub."));
    }

    public Task<Response<object?>> Send(HubSendContext context)
    {
        context.Clients.Group(GroupName).SendAsync("Log", context.Message);
        return Task.FromResult(ServiceResult.Success());
    }
}