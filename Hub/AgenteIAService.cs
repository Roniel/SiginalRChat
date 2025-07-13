using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text;

namespace SiginalRChat.Hubs
{
    public class AgenteIAService : IAgenteIAService
    {
        private readonly HttpClient _httpClient;

        public AgenteIAService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<RespostaModel> PerguntarAgenteIA(string pergunta)
        {
            var content = MontarEstruturaContent(pergunta);
            var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar o serviço de IA: {response.ReasonPhrase}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var resposta = JsonSerializer.Deserialize<RespostaModel>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }); 
            return resposta;
        }

        private StringContent MontarEstruturaContent(string pergunta)
        {
            var body = new
            {
                model = "deepseek-r1:7b",
                prompt = pergunta,
                stream = false
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return content;
        }
    }
}