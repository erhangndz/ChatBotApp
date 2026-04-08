namespace ChatBotApp.Services
{
    public interface IGeminiService
    {

        Task<string> GetChatResponseAsync(string userMessage);


    }
}
