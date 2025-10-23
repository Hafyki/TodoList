namespace TodoList.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public ICollection<ToDo>? ToDos;
    }
}
