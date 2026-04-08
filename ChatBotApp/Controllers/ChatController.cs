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
    }
}

