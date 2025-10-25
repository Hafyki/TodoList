using TodoList.Models.Entities;

namespace TodoList.Services
{
    public interface IUserService
    {
        public Task<User?> Add(int id);
    }
}