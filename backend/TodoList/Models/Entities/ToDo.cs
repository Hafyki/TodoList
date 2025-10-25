using System.Text.Json.Serialization;

namespace TodoList.Models.Entities
{
    public class ToDo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("completed")]
        public bool IsCompleted {  get; set; } = false;
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
