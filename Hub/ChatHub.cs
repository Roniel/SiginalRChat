using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SiginalRChat.Hubs
{
    [Authorize] 
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, List<string>> _userConnections = new();
        protected readonly IAgenteIAService _agenteIAService;
        // Enviar para todos

        public ChatHub(IAgenteIAService agenteIAService)
        {
            _agenteIAService = agenteIAService;
        }
        public async Task SendMessageToAll(string message)
        {
            var user = Context.User?.Identity?.Name ?? "Anônimo";

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // Enviar para um grupo
        public async Task SendMessageToGroup(string groupName,  string message)
        {
             var user = Context.User?.Identity?.Name ?? "Anônimo";

            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", user, message);
        }

        // Enviar para um usuário específico
        public async Task SendMessageToUser(string toUsername, string message)
        {
            var sender = Context.User?.Identity?.Name ?? "Anônimo";

            if (_userConnections.TryGetValue(toUsername, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", sender, message);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Sistema", $"Usuário '{toUsername}' não está online.");
            }
        }
 
        public async Task sendAgentIa(string pergunta)
        {
            var sender = Context.User?.Identity?.Name ?? "Anônimo";
            var resposta = await _agenteIAService.PerguntarAgenteIA(pergunta);
            Console.WriteLine($"Resposta: {resposta.response}");
            await Clients.Caller.SendAsync("ReceiveMessage", sender, resposta.response);
        }
        // Adicionar conexão a um grupo
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Remover conexão de um grupo
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity.Name ?? Context.ConnectionId;

            _userConnections.AddOrUpdate(
                userName,
                new List<string>
                {
                    Context.ConnectionId
                }, (key, list) =>
                {
                    list.Add(Context.ConnectionId);
                    return list;
                }
            );

            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UpdateUserList", _userConnections.Keys.ToList());
        }
        public override async  Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name ?? Context.ConnectionId;

            if (_userConnections.TryGetValue(userName, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                    _userConnections.TryRemove(userName, out _);
            }

            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("UpdateUserList", _userConnections.Keys.ToList());
        }

        // Envia lista de usuários conectados para o chamador
        public Task<List<string>> GetConnectedUsers()
        {
            var users = _userConnections.Keys.ToList();
            return Task.FromResult(users);
        }
    }
}