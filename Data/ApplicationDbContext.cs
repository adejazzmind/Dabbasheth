using Microsoft.EntityFrameworkCore;
using Dabbasheth.Models;

namespace Dabbasheth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        // ✅ CHANGED: Match the class name 'Transaction' exactly
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<ThriftPlan> ThriftPlans { get; set; }
        public DbSet<ThriftGroup> ThriftGroups { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // This block handles your Neon PostgreSQL lowercase requirements
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(tableName.ToLower());

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }
        }
    }
}