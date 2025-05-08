using Microsoft.EntityFrameworkCore;

namespace UserLogin.Model
{
    public class MyDBContext : DbContext
    {
        public MyDBContext( DbContextOptions<MyDBContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
