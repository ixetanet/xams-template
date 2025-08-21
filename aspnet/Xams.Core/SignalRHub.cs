using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xams.Core.Contexts;
using Xams.Core.Interfaces;

namespace Xams.Core;

public static class SignalRConfiguration
{
    public static Func<HttpContext, Task<Guid>>? GetUserId { get; set; }
}

public class SignalRHub : Hub
{
    public static readonly ConcurrentDictionary<Guid, UserConnection> UserConnections = new();
    private ILogger<SignalRHub> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    public SignalRHub(ILogger<SignalRHub> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }
    public async Task<object?> OnReceive(string hubName, string message)
    {
        var userId = await GetCurrentUserId();
        var permissions = await PermissionCache.GetUserPermissions(userId, [$"HUB_{hubName}"]);

        if (permissions.Length == 0)
        {
            Logger.LogInformation($"User {userId} has no permissions for hub {hubName}");
            return null;
        }

        try
        {
            var hub = GetHub(hubName);
            using var scope = ServiceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            var response = await dataService.ExecuteTransaction(userId, async (pipelineContext) =>
            {
                var response = await hub.OnReceive(new HubContext(pipelineContext, message, this));
                return response;
            });
            
        
            if (!response.Succeeded)
            {
                Logger.LogError($"User {userId} failed to receive message {message}, {response.LogMessage}");
            }

            return response.Data;
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error processing message {message} for user {userId} on hub {hubName}");
        }

        return null;

    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        var userId = await GetCurrentUserId();
        if (!UserConnections.TryAdd(userId, new UserConnection { NumberOfConnections = 1 }))
        {
            UserConnections[userId].NumberOfConnections++;
        }
        
        UserConnections[userId].ConnectionIds.Add(Context.ConnectionId);
        UserConnections[userId].ConnectionContexts.Add(Context);
        UserConnections[userId].ConnectionHubs[Context.ConnectionId] = [];

        var permissions = await PermissionCache.GetUserPermissions(userId);

        foreach (var permission in permissions)
        {
            if (!permission.StartsWith("HUB_"))
            {
                continue;
            }
            
            try
            {
                var hubName = permission.Substring(4, permission.Length - 4);
                UserConnections[userId].ConnectionHubs[Context.ConnectionId].Add(hubName);
                var hub = GetHub(hubName);
                using var scope = ServiceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
                var response = await dataService.ExecuteTransaction(userId, async (pipelineContext) =>
                {
                    var response = await hub.OnConnected(new HubContext(pipelineContext, "", this));
                    return response;
                });
                
                if (!response.Succeeded)
                {
                    Logger.LogError($"User {userId} failed to connect to hub {permission}, {response.LogMessage}");
                    continue;
                }
                
                Logger.LogInformation($"User {userId} connected to hub {permission}");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error processing user {userId} connected to hub {permission}");
            }
            
        }
        Logger.LogInformation($"User {userId} connected with connection ID {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        var userId = await GetCurrentUserId();
        var userHubs = new List<string>();
        if (UserConnections.TryGetValue(userId, out var userConnection))
        {
            userHubs = userConnection.ConnectionHubs[Context.ConnectionId].ToList();
            userConnection.ConnectionIds.Remove(Context.ConnectionId);
            userConnection.ConnectionContexts.RemoveAll(ctx => ctx.ConnectionId == Context.ConnectionId);
            userConnection.ConnectionHubs.Remove(Context.ConnectionId);
            if (userConnection.NumberOfConnections <= 1)
            {
                UserConnections.TryRemove(userId, out _);
            }
            else
            {
                UserConnections[userId].NumberOfConnections--;
            }
        }
        
        foreach (var hubName in userHubs)
        {
            try
            {
                var hub = GetHub(hubName);
                using var scope = ServiceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
                var response = await dataService.ExecuteTransaction(userId, async (pipelineContext) =>
                {
                    var response = await hub.OnDisconnected(new HubContext(pipelineContext, "", this));
                    return response;
                });
                
                if (!response.Succeeded)
                {
                    Logger.LogError($"User {userId} failed to disconnect from hub {hubName}, {response.LogMessage}");
                    continue;
                }

                Logger.LogInformation($"User {userId} disconnected from hub {hubName}");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error processing user {userId} disconnected from hub {hubName}");
            }
        }
        Logger.LogInformation($"User {userId} disconnected with connection ID {Context.ConnectionId}");
    }
    

    private async Task<Guid> GetCurrentUserId()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext not available");
        }

        // Use the configured GetUserId function if available, otherwise fall back to default
        if (SignalRConfiguration.GetUserId != null)
        {
            return await SignalRConfiguration.GetUserId(httpContext);
        }

        // Fallback to default implementation (header-based)
        return await GetUserIdFromQueryString(httpContext);
    }

    private static Task<Guid> GetUserIdFromQueryString(HttpContext httpContext)
    {
        if (httpContext.Request.Query.ContainsKey("userid"))
        {
            string userId = httpContext.Request.Query["userid"].ToString();
            if (Guid.TryParse(userId, out Guid guid))
            {
                return Task.FromResult(guid);
            }
            throw new Exception("UserId in header is not a Guid");
        }
        
        throw new Exception("UserId not found in request headers");
    }
    
    private IServiceHub GetHub(string hubName)
    {
        if (!Cache.Instance.ServiceHubs.TryGetValue(hubName, out var hubMetadata))
        {
            throw new Exception($"Service hub {hubName} not found");
        }

        var hub = Activator.CreateInstance(hubMetadata.Type);
        if (hub == null)
        {
            throw new Exception($"Failed to instantiate service hub {hubName}");
        }

        return (IServiceHub)hub;
    }

    public class UserConnection
    {
        public int NumberOfConnections { get; set; }
        // public HashSet<string> Hubs { get; set; } = new();
        public HashSet<string> ConnectionIds { get; set; } = new();
        public Dictionary<string, List<string>> ConnectionHubs { get; set; } = new();
        public List<HubCallerContext> ConnectionContexts { get; set; } = new();
    }

    public static Task ForceDisconnectUser(Guid userId, string reason)
    {
        if (UserConnections.TryGetValue(userId, out var userConnection))
        {
            var contexts = userConnection.ConnectionContexts.ToList();
            foreach (var context in contexts)
            {
                try
                {
                    // Send a notification before disconnecting (more graceful)
                    // Note: We can't use Clients here since this is a static method
                    // So we'll still use Abort() but the client now has better reconnection logic
                    context.Abort();
                }
                catch (Exception ex)
                {
                    // Logger.LogError($"Failed to abort connection {context.ConnectionId}: {ex.Message}");
                }
            }
        }

        return Task.CompletedTask;
    }
}