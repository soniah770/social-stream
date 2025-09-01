//Manages SignalR client connections
//Provides methods for clients to Join & leave the "PostStream" group
//Handle connection/disconnection events
using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Hubs
{
    public class PostHub : Hub     // Inherits from Hub base class

    {
        private readonly ILogger<PostHub> _logger;

        public PostHub(ILogger<PostHub> logger)  // SignalR hub for real-time post streaming

        {
            _logger = logger;  //Enables logging of hub activities
        }
// When a client wants to receive real-time posts
    // Allow clients to join post stream group

        public async Task JoinPostStream()
        {     // Add this  client connection to "PostStream" group
              
            await Groups.AddToGroupAsync(Context.ConnectionId, "PostStream");
                // Log client joining

            _logger.LogDebug("Client {ConnectionId} joined post stream", Context.ConnectionId); //Unique connection identifier
        }

        public async Task LeavePostStream()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "PostStream");
            _logger.LogDebug("Client {ConnectionId} left post stream", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug("Client {ConnectionId} disconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}