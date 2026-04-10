
using ChatBotApp.Options;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
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

            var response = await _client.PostAsync(apiUrl + apiKey, jsonContent);

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

        public async IAsyncEnumerable<string> GetChatResponseStreamAsync(string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var apiKey = _geminiOptions.ApiKey;
            var apiUrl = _geminiOptions.StreamApiUrl;

            var requestUrl = apiUrl + apiKey;

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
                                text= userMessage
                            }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = jsonContent };

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }


                if (line.StartsWith("data: "))
                {
                    var jsonData = line.Substring("data: ".Length);

                    string? textChunk = null;
                    bool isValid = false;

                    try
                    {
                        var jsonDocument = JsonDocument.Parse(jsonData);

                        textChunk = jsonDocument.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                        isValid = !string.IsNullOrEmpty(textChunk);
                    }

                    catch (Exception)
                    {
                        isValid = false;
                    }

                    if (isValid && textChunk != null)
                    {
                        yield return textChunk;
                    }
                }
            }


        }


    }
}

