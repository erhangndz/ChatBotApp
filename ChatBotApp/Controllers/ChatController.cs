using ChatBotApp.Models;
using ChatBotApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatBotApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IGeminiService _geminiService;

        public ChatController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, message = "Mesaj boş olamaz." });
            }

            try
            {
                // Gemini servisine mesajı gönderip yanıtı alıyoruz
                string aiResponse = await _geminiService.GetChatResponseAsync(request.Message);

                // Başarılı olursa JSON olarak geri dönüyoruz
                return Json(new { success = true, reply = aiResponse });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task StreamMessage([FromBody] ChatRequest request)
        {
            try
            {
                Response.ContentType = "text/plain; charset=utf-8";

                // Eğer mesaj boş gelirse uyar
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    await Response.WriteAsync("Hata: Mesaj boş olamaz.");
                    return;
                }

                // Akışı başlat
                await foreach (var chunk in _geminiService.GetChatResponseStreamAsync(request.Message))
                {
                    await Response.WriteAsync(chunk);
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                // KOD BURADA PATLIYORSA HATANIN NE OLDUĞUNU EKRANA YAZDIRACAK
                var errorMessage = $"[SUNUCU HATASI]: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Detay: {ex.InnerException.Message}";
                }

                await Response.WriteAsync(errorMessage);
                await Response.Body.FlushAsync();
            }
        }
    }
}

