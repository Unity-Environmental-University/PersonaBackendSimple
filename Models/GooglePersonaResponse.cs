using System.Text.Json.Serialization;

namespace PersonaBackendSimple.Models
{
    public class GooglePersonaResponse
    {
        [JsonPropertyName("values")]
        public List<string[]> Values { get; set; } 
    }


}


