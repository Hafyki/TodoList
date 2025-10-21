namespace TodoList.Models.Entities
{
    public class User
    {
        public Guid id { get; set; }
        public required string name { get; set; }
        public List<Task> tasks;
    }
}
