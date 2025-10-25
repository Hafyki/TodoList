using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoList.Data;
using TodoList.Models.Entities;

namespace TodoList.Services
{
    public class UserService : IUserService
    {
        ApplicationDbContext _dbContext;
        public UserService(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task <User?> Add(int id)
        {
            var newUser = new User
            {
                Id = id
            };
            _dbContext.Users.AddRange(newUser);
            await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users ON");
            await _dbContext.SaveChangesAsync();
            await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users OFF");
            
            return newUser;
        }
        
    }
}
