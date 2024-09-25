
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PersonaBackendSimple.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using PersonaBackendSimple.Models;

namespace PersonaBackendSimple.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly OpenAIService _openAiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private static List<ChatMessage> conversationHistory = new();
        private static List<Persona> allPersonae = new();
        private static Persona? currentPersona = null;

        public ChatController(OpenAIService openAiService, IHttpClientFactory httpClientFactory)
        {
            _openAiService = openAiService;
            _httpClientFactory = httpClientFactory; 
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {
            try
            {
                var session = HttpContext.Session;
                var personae = session.Get<List<Persona>>("personae") ?? new List<Persona>();

                if (allPersonae.Count == 0)
                {
                    await LoadPersonae();
                }

                if (!personae.Any())
                {
                    personae = Shuffle(allPersonae);
                    session.Set("personae", personae);
                }

                var personaNames = personae.Select(p => p.Name.ToLower()).ToList();
                var intentAndPersona = await _openAiService.DetermineIntentAndPersona(
                    conversationHistory, chatRequest.Text, personaNames);

                // Handle intent and persona logic
                // Code omitted for brevity

                return Ok(new { Reply = "Message processed", Persona = currentPersona, AvailablePersonae = personae });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        private async Task LoadPersonae()
        {
            var googleSheetsApiKey = Environment.GetEnvironmentVariable("GOOGLE_SHEETS_API_KEY");
            var spreadSheetId = Environment.GetEnvironmentVariable("PERSONA_SHEET_ID");

            if (string.IsNullOrEmpty(googleSheetsApiKey) || string.IsNullOrEmpty(spreadSheetId))
            {
                throw new InvalidOperationException("Google Sheets API key or Spreadsheet ID not set.");
            }

            var client = _httpClientFactory.CreateClient();
            var sheetRange = "Sheet1!A2:D"; // Adjust the range as needed

            var requestUri = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadSheetId}/values/{sheetRange}?key={googleSheetsApiKey}";

            try
            {
                var response = await client.GetStringAsync(requestUri);
                var sheetData = JsonSerializer.Deserialize<GooglePersonaResponse>(response);

                if (sheetData?.Values != null)
                {
                    allPersonae = sheetData.Values.Select(row => new Persona
                    {
                        DefaultUser = row.ElementAtOrDefault(0) ?? string.Empty,
                        Name = row.ElementAtOrDefault(1) ?? string.Empty,
                        Role = row.ElementAtOrDefault(2) ?? string.Empty,
                        Instructions = row.ElementAtOrDefault(3) ?? string.Empty,
                    }).ToList();
                }
                else
                {
                    throw new Exception("No data found in the specified range.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching personae from Google Sheets: {ex.Message}", ex);
            }
        }

        private List<T> Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            return list.OrderBy(_ => rng.Next()).ToList();
        }
    }

    public class ChatRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}

