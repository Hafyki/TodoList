using Microsoft.EntityFrameworkCore;
using TodoList.Models.Entities;

namespace TodoList.Data
{
    
    public class ToDoListDbContext(DbContextOptions<ToDoListDbContext> options) : DbContext(options)
    {
        public DbSet<ToDo> Quests { get; set; }
        public DbSet<User> Users { get; set; }

        //Mockup de usuários
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, name = "João" },
                new User { Id = 2, name = "Gustavo" },
                new User { Id = 3, name = "Maria" },
                new User { Id = 4, name = "Igor" },
                new User { Id = 5, name = "Gabriel" },
                new User { Id = 6, name = "Mario" },
                new User { Id = 7, name = "Luiz" },
                new User { Id = 8, name = "Felipe" },
                new User { Id = 9, name = "Ygor" },
                new User { Id = 10, name = "Fayad" }
                );
        }
    }
       
}

  