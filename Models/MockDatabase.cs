using System;
using System.Collections.Generic;
using System.Linq;

namespace Dabbasheth.Models
{
    public static class MockDatabase
    {
        // --- 1. DATA STORAGE LISTS ---
        public static List<User> Users = new List<User>();
        public static List<Wallet> Wallets = new List<Wallet>();
        public static List<TransactionRecord> Transactions = new List<TransactionRecord>();
        public static List<ThriftPlan> ThriftPlans = new List<ThriftPlan>();

        // --- 2. STATIC CONSTRUCTOR (The IES Life Hub Identity) ---
        static MockDatabase()
        {
            // --- THE LEADERSHIP (IES Life Hub Admins) ---
            // MD: Samson Mayowa Braimoh
            Users.Add(new User
            {
                Id = 1,
                FullName = "Samson Mayowa Braimoh",
                Email = "adejazzmind@gmail.com",
                Password = "123",
                Role = "Admin"
            });

            // CEO: Tolulope Jumoke Samson
            Users.Add(new User
            {
                Id = 2,
                FullName = "Tolulope Jumoke Samson",
                Email = "tolubabe2k@gmail.com",
                Password = "123",
                Role = "Admin"
            });

            // --- CUSTOMERS ---
            Users.Add(new User
            {
                Id = 3,
                FullName = "Eniola Apelogun",
                Email = "test@me.com",
                Password = "123",
                Role = "Customer"
            });

            // --- FINANCIAL INFRASTRUCTURE (IES Life Hub Wallets) ---
            Wallets.Add(new Wallet { Id = 1, UserEmail = "adejazzmind@gmail.com", Balance = 2500000.00m, Currency = "NGN" });
            Wallets.Add(new Wallet { Id = 2, UserEmail = "tolubabe2k@gmail.com", Balance = 2500000.00m, Currency = "NGN" });
            Wallets.Add(new Wallet { Id = 3, UserEmail = "test@me.com", Balance = 15000.00m, Currency = "NGN" });

            // --- WELCOME TRANSACTIONS ---
            Transactions.Add(new TransactionRecord
            {
                UserEmail = "adejazzmind@gmail.com",
                Reference = "IES-MD-INIT",
                Amount = 2500000.00m,
                Description = "MD Operational Capital",
                Date = DateTime.Now,
                Status = "Approved"
            });

            Transactions.Add(new TransactionRecord
            {
                UserEmail = "tolubabe2k@gmail.com",
                Reference = "IES-CEO-INIT",
                Amount = 2500000.00m,
                Description = "CEO Operational Capital",
                Date = DateTime.Now,
                Status = "Approved"
            });
        }
    }
}