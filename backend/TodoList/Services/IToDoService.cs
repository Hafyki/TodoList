using TodoList.DTO;
using TodoList.Models.Entities;

namespace TodoList.Services
{
    public interface IToDoService
    {
        public  Task<object> GetAll(int pageNumber, int pageSize, string? sortBy, bool isDescending, string title);
        public Task<ToDo?> GetById(int id);
        public Task<ToDo?> ChangeCompletedStatus(int id);
        public Task<ToDo> Add(ToDoDto newToDoDto);
    }
}
