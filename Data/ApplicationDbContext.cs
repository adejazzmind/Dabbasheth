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

        // These define the tables that will be created in Neon
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<TransactionRecord> Transactions { get; set; }
        public DbSet<ThriftPlan> ThriftPlans { get; set; }
    }
}