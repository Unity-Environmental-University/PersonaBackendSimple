using System.Text.Json.Serialization;

public class Persona
{
    [JsonPropertyName("defaultUser")]
    public string DefaultUser { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; set; }
}