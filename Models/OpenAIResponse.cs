using System.Text.Json.Serialization;

public class OpenAIResponse

{

    [JsonPropertyName("choices")]

    public List<Choice> Choices { get; set; } = new();



    public class Choice

    {

        [JsonPropertyName("message")]

        public Message Message { get; set; } = new();

    }



    public class Message

    {

        [JsonPropertyName("content")]

        public string Content { get; set; } = string.Empty;

    }

}