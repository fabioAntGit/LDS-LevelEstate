using API.ChatVisitasFeedback.Chat.DTOs;
using API.ChatVisitasFeedback.Chat.Exceptions;
using API.ChatVisitasFeedback.Chat.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace API.ChatVisitasFeedback.Chat.WebSockets
{
    public class ChatWebSocketHandler
    {
        private readonly IMessageService messageService;
        private readonly IChatService chatService;

        private static readonly ConcurrentDictionary<int, List<WebSocket>> connections = new();

        public ChatWebSocketHandler(IMessageService messageService, IChatService chatService)
        {
            this.messageService = messageService;
            this.chatService=chatService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            var chatIdString = context.Request.Query["chatId"];
            if (!int.TryParse(chatIdString, out int chatId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            // AUTH NOT IMPLEMENTED YET
            var userIdString = context.Request.Query["userId"];
            if (!int.TryParse(userIdString, out int userId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            try
            {
                var chat = chatService.Get(chatId, userId);
            }
            catch (UnauthorizedChatAccessException)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            catch (ChatNotFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }


            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            if (!connections.ContainsKey(chatId))
            {
                connections[chatId] = new List<WebSocket>();
            }

            connections[chatId].Add(webSocket);

            await ReceiveMessagesAsync(chatId, userId, webSocket);
        }

        private async Task ReceiveMessagesAsync(int chatId, int userId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    connections[chatId].Remove(webSocket);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    continue;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message == null || message.Trim() == "")
                {
                    continue;
                }

                var messageJson = JsonConvert.DeserializeObject<SendMessageDTO>(message);

                if (messageJson == null)
                {
                    continue;
                }

                var messageObject = messageService.Send(chatId, userId, messageJson.Content);

                if (messageObject != null)
                {
                    await BroadcastMessageAsync(chatId, userId, messageObject);
                }
            }
        }

        private async Task BroadcastMessageAsync(int chatId, int senderId, MessageDTO message)
        {
            if (!connections.TryGetValue(chatId, out var sockets))
            {
                return;
            }

            var aliveSockets = new List<WebSocket>();

            foreach (var socket in sockets)
            {
                if (socket.State != WebSocketState.Open)
                {
                    continue;
                }

                aliveSockets.Add(socket);


                var messageString = JsonConvert.SerializeObject(message);
                await socket.SendAsync(Encoding.UTF8.GetBytes(messageString), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            connections[chatId] = aliveSockets;
        }
    }
}
