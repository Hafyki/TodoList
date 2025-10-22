namespace TodoList.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string name { get; set; }
        public List<Task>? tasks;
    }
}
