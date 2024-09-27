
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
        private static List<Persona> allPersonae = new();
        private static string instructions = string.Empty;

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
                var client = _httpClientFactory.CreateClient();
                var session = HttpContext.Session;
                var personae = session.Get<List<Persona>>("personae") ?? new List<Persona>();
                var currentPersona = session.Get<Persona>("currentPersona") ?? null;
                var conversationHistory = session.Get<List<ChatMessage>>("conversationHistory") ?? new List<ChatMessage>();
                if (allPersonae.Count == 0)
                {
                    await LoadPersonae();
                }

                if (!personae.Any())
                {
                    personae = Shuffle(allPersonae);
                    currentPersona ??= personae[0];
                    session.Set("personae", personae);
                }

                var personaNames = personae.Select(p => p.Name.ToLower()).ToList();
                var intentAndPersona = await _openAiService.DetermineIntentAndPersona(
                    conversationHistory, chatRequest.Text, personaNames);

                if(intentAndPersona.Intent == "switch users")
                {
                    var switchPersona = personae.Find((a) => a.Name.Equals(intentAndPersona.Persona, StringComparison.InvariantCultureIgnoreCase));
                    if (switchPersona != null && currentPersona != switchPersona)
                    {
                        conversationHistory.Add(new ChatMessage {
                            Role = "system",
                            Content = $"The persona just switched from {currentPersona?.Name}:{currentPersona?.Role} to {switchPersona.Name}:{switchPersona.Role}"
                        });
                        currentPersona = switchPersona;
                        session.Set("currentPersona", currentPersona);

                    }
                }
                    

                //private async Task<string> Respond(HttpClient client, List<ChatMessage> history, string message, string personaName)
                var response = await _openAiService.Chat(client, conversationHistory, chatRequest.Text, currentPersona ?? personae[0], instructions);
                session.Set("conversationHistory", response.updatedHistory);

                return Ok(new { Reply = response.responseMessage, Persona = currentPersona, AvailablePersonae = personae });
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

            instructions = Environment.GetEnvironmentVariable("Base Instructions");

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

