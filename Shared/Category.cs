using System.Text.Json.Serialization;

namespace Assignment3.Shared
{
    public class Category
    {
        [JsonPropertyName("cid")]
        public int Id { get; set; }

        [JsonPropertyName("id")]
        public int IdAlias { set => Id = value; }

        public string Name { get; set; } = string.Empty;

        public Category() { }
    }
}
