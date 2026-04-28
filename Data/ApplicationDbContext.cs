using Microsoft.EntityFrameworkCore;
using Dabbasheth.Models;

namespace Dabbasheth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ThriftPlan> ThriftPlans { get; set; }
        public DbSet<ThriftGroup> ThriftGroups { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entity.SetTableName(tableName.ToLower());

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(property.Name.ToLower());

                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName()?.ToLower());

                foreach (var fk in entity.GetForeignKeys())
                    fk.SetConstraintName(fk.GetConstraintName()?.ToLower());

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
            }

            modelBuilder.Entity<ThriftPlan>()
                .HasOne(p => p.ThriftGroup)
                .WithMany(g => g.MemberPlans)
                .HasForeignKey(p => p.ThriftGroupId)
                .IsRequired(false);
        }
    }
}