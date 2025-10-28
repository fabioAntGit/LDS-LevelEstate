using API.FavoritosNotificacoes.Notificacoes.DTOs;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.FavoritosNotificacoes.Notificacoes.WebSockets
{
    public class NotificationWebSocketHandler : INotificationHandler

    {
        private static readonly ConcurrentDictionary<int, WebSocket> connections = new();
        public async Task HandleConnectionAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var userIdString = context.Request.Query["userId"];
            if (!int.TryParse(userIdString, out int userId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            connections[userId] = socket;

            await ListenForCloseAsync(userId, socket);
        }
        private async Task ListenForCloseAsync(int userId, WebSocket socket)
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    connections.TryRemove(userId, out _);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }
        public async Task SendNotificationAsync(int userId, NotificationDTO dto)
        {
            if (connections.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
            {
                var json = JsonConvert.SerializeObject(dto);
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
