namespace TodoList.Models.Entities
{
    public class ToDo
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool IsCompleted {  get; set; } = false;
    }
}
