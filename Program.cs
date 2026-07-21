using Microsoft.EntityFrameworkCore;
using SignupApp.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var connectionString = GetDatabaseConnectionString(builder.Configuration);

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine(
        "WARNING: Database connection string is not set. " +
        "Link Render PostgreSQL or set ConnectionStrings__DefaultConnection / DATABASE_URL.");
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Database migration completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
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
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string? GetDatabaseConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
        connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (string.IsNullOrWhiteSpace(connectionString))
        return null;

    if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        connectionString = "postgresql://" + connectionString["postgres://".Length..];

    return connectionString;
}
