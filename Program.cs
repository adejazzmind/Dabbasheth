using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. DATABASE CONNECTION (from Render)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("❌ Connection string 'DefaultConnection' is missing in Render Environment Variables.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsql.CommandTimeout(60);
    });
});

// ================================================
// 2. SERVICES
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================================================
// 3. BUILD APP
var app = builder.Build();

// ================================================
// 4. MIGRATION + SEEDING
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        context.Database.Migrate();
        Console.WriteLine("✅ Neon Database Migration Successful.");

        // Seed Admin Users (only once)
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.AddRange(
                new User
                {
                    FullName = "Samson Mayowa Braimoh",
                    Email = "adejazzmind@gmail.com",
                    Password = "123",
                    PhoneNumber = "08000000000",
                    Role = "Admin"
                },
                new User
                {
                    FullName = "Tolulope Jumoke Samson",
                    Email = "tolubabe2k@gmail.com",
                    Password = "123",
                    PhoneNumber = "08000000000",
                    Role = "Admin"
                }
            );
            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ Admins seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ DATABASE ERROR: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("Inner: " + ex.InnerException.Message);
    }
}

// ================================================
// 5. MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();        // Must be before Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();