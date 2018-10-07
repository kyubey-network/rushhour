using Microsoft.EntityFrameworkCore;

namespace Andoromeda.RushHour.Models
{
    public class RhContext : DbContext
    {
        public RhContext(DbContextOptions opt) : base(opt)
        { }

        public DbSet<User> Users { get; set; }

        public DbSet<Binding> Bindings { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(e =>
            {
                e.HasIndex(x => x.Plan);
                e.HasIndex(x => x.IsEmailValidated);
                e.HasIndex(x => x.Email);
            });

            builder.Entity<Binding>(e =>
            {
                e.HasKey(x => new { x.UserId, x.Account });
            });

            builder.Entity<Alert>(e =>
            {
                e.HasIndex(x => x.Type);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.TransactionId);
                e.HasIndex(x => x.CreatedTime);
                e.HasIndex(x => x.DeliveredTime);
            });
        }
    }
}
