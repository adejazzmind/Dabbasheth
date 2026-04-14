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

        // ============================================================
        // 🚀 DATABASE TABLES (Schema Definition)
        // These define the tables that will be created in your DB
        // ============================================================

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<TransactionRecord> Transactions { get; set; }
        public DbSet<ThriftPlan> ThriftPlans { get; set; }

        // ✅ FIXED: Support for Ajo/Esusu Group Cycles
        public DbSet<ThriftGroup> ThriftGroups { get; set; }
    }
}