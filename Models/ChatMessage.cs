using System.Text.Json.Serialization;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class IntentPersonaResponse
{
    [JsonPropertyName("intent")]
    public string Intent { get; set; } = string.Empty;
    [JsonPropertyName("persona")]
    public string? Persona { get; set; }
}