using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text;

namespace SiginalRChat.Hubs
{
    public interface IAgenteIAService
    {
        Task<RespostaModel> PerguntarAgenteIA(string pergunta);
    }
}
        