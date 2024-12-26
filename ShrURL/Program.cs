using Microsoft.EntityFrameworkCore;
using ShrURL.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                       builder.Configuration.GetConnectionString("ShrURL") ??
                       throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(dbConnectionString));

// Si bien implementa la interfaz IDistributedCache, no es una caché distribuida real
// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-9.0#distributed-memory-cache
//builder.Services.AddDistributedMemoryCache(options => {});

// Redis cache
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    options.InstanceName = "ShrURLLocalInstance";
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try {
        //context.Database.EnsureCreated();
        context.Database.Migrate();

    } catch (Exception ex) {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Short}/{id?}");

app.MapControllerRoute(
    name: "root",
    pattern: "{id?}",
    defaults: new { controller = "Home", action = "Index" }
);

app.Run();
