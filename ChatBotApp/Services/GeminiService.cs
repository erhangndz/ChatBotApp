
using ChatBotApp.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ChatBotApp.Services
{
    public class GeminiService : IGeminiService
    {

        private readonly HttpClient _client;
        private readonly GeminiOptions _geminiOptions;
        

        public GeminiService(HttpClient client, IOptions<GeminiOptions> geminiOptions)
        {
            _client = client;
            _geminiOptions = geminiOptions.Value;
        }

        public async Task<string> GetChatResponseAsync(string userMessage)
        {
           
            var apiKey = _geminiOptions.ApiKey;
            var apiUrl = _geminiOptions.ApiUrl;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text= "Sen bir hastane randevu sistemi için hazırlanmış bir chat botsun ve sana söylenen hastalıkla ilgili hangi poliklinikten randevu alınması gerektiğini söyleyeceksin:  " +userMessage
                            }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(apiUrl+apiKey, jsonContent);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(responseString);

            var reply = jsonDocument.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

            return reply ?? "Bir Hata Oluştu, Cevap Alınamadı";


        }
    }
}
