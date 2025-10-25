using Microsoft.EntityFrameworkCore;
using TodoList.Models.Entities;

namespace TodoList.Data
{
    
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<User> Users { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.ToDos)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(e => e.Id);
        }

        internal async Task FindAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
       
}

  