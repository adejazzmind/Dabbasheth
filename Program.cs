using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from Render environment variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing from Render Environment Variables.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsql.CommandTimeout(60);
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Auto-migrate + seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        context.Database.Migrate();
        Console.WriteLine("✅ Migration successful.");

        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.AddRange(
                new User { FullName = "Samson Mayowa Braimoh", Email = "adejazzmind@gmail.com", Password = "123", PhoneNumber = "08000000000", Role = "Admin" },
                new User { FullName = "Tolulope Jumoke Samson", Email = "tolubabe2k@gmail.com", Password = "123", PhoneNumber = "08000000000", Role = "Admin" }
            );
            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ Database connected and seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Database Error: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("Inner: " + ex.InnerException.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();