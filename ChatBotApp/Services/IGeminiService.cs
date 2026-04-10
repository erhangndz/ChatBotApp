using System.Runtime.CompilerServices;

namespace ChatBotApp.Services
{
    public interface IGeminiService
    {

        Task<string> GetChatResponseAsync(string userMessage);

        IAsyncEnumerable<string> GetChatResponseStreamAsync(string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken=default);


    }
}
