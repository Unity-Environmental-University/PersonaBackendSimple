using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;

namespace PersonaBackendSimple.Services
{
    public class OpenAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiApiKey;

        private readonly JsonSerializerOptions _jsonOptions;

        public OpenAIService(IHttpClientFactory httpClientFactory, string openAiApiKey)
        {
            _httpClientFactory = httpClientFactory;
            _openAiApiKey = openAiApiKey;

            // Initialize JSON serializer options with inline custom naming policy
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new JsonNamingPolicyLower(),
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IntentPersonaResponse> DetermineIntentAndPersona(
            List<ChatMessage> history, string message, List<string> personaNames)
        {
            var client = _httpClientFactory.CreateClient();
            string intent = await DetermineIntent(client, history, message);
            string persona = null;

            if (intent == "switch users")
            {
                persona = await DeterminePersona(client, history, message, personaNames);
            }

            if (!intent.Equals("converse", StringComparison.OrdinalIgnoreCase) && (persona == null || !personaNames.Contains(persona)))
            {
                return new IntentPersonaResponse { Intent = intent, Persona = null };
            }

            return new IntentPersonaResponse { Intent = intent, Persona = persona };
        }

        private async Task<string> DetermineIntent(HttpClient client, List<ChatMessage> history, string message)
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = $"Determine the intent of the following message: \"{message}\". Possible intents: [converse, switch users]. Respond with only the intent.", Name = "determineIntent" }
            };

            var response = await SendRequestToOpenAI(client, messages, 24);
            var intent = response.Choices[0]?.Message?.Content?.Trim().ToLower();
            if (string.IsNullOrEmpty(intent))
            {
                throw new Exception("The intent was not determined.");
            }

            return intent;
        }


        private async Task<string> DeterminePersona(HttpClient client, List<ChatMessage> history, string message, List<string> personaNames)
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = $"Determine the persona of the following message: {message}. Possible persona names: [{string.Join(", ", personaNames)}]. Respond with only the persona.", Name = "determinePersona" }
            };

            var response = await SendRequestToOpenAI(client, messages, 24);
            var persona = response.Choices[0]?.Message?.Content?.Trim().ToLower();
            return persona ?? "";
        }

        public async Task<(string responseMessage, List<ChatMessage> updatedHistory)> Chat(HttpClient client, List<ChatMessage> history, string message, Persona persona, string baseInstructions)
        {
            // Filter out system messages from the history
            var filteredHistory = history.Where(a => a.Role != "system").ToList();

            // Add new messages including persona instructions, base instructions, and user message
            var messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = $"You are {persona.Name}: {persona.Role}, follow the persona's role and instructions. {persona.Instructions}", Name = "roleInstructions" },
                    new ChatMessage { Role = "system", Content = $"Follow the base conversation guidelines. {baseInstructions}", Name = "BaseInstructions" },
                    new ChatMessage { Role = "user", Content = message, Name = "userMessage" }
                };

            // Combine filtered history with new messages
            filteredHistory.AddRange(messages);

            // Send request to OpenAI
            var response = await SendRequestToOpenAI(client, filteredHistory, 1000);

            var choice = response.Choices[0]?.Message?.Content?.Trim() ?? "ERROR";

            filteredHistory.Add(new ChatMessage { Content = choice, Role = "assistant" });
            // Add the assistant's response to history (you might want to store this elsewhere)

            return (choice, filteredHistory);
        }


        private async Task<OpenAIResponse> SendRequestToOpenAI(HttpClient client, List<ChatMessage> messages, int maxTokens)
        {
            var lcMessages = messages.Select((a) => new { role = a.Role, content = a.Content, name = a.Name });
            var requestContent = new { model = "gpt-4o-mini", messages = lcMessages, max_tokens = maxTokens, temperature = 0.5 };
            var requestBody = JsonSerializer.Serialize(requestContent, _jsonOptions); // Use custom JSON options

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {_openAiApiKey}");
            var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OpenAIResponse>(responseContent, _jsonOptions);
        }

        private class JsonNamingPolicyLower : JsonNamingPolicy
        {
            public override string ConvertName(string name) => name.ToLower();
        }
    }

}