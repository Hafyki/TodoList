using System.Text.Json.Serialization;
using TodoList.Models.Entities;

namespace TodoList.DTO
{
    public class ToDoDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("completed")]
        public bool IsCompleted { get; set; } = false;
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

    }
}
