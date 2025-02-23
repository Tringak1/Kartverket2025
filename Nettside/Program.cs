using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Nettside.Data;
using Nettside.Models;
using Nettside.Repositiories;
using Nettside.Services;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets only in Development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure database connection with secrets
var connectionString = builder.Configuration.GetConnectionString("MariaDbConnection")
    .Replace("{DB_USER}", builder.Configuration["DB_USER"])
    .Replace("{DB_PASSWORD}", builder.Configuration["DB_PASSWORD"]);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 5, 9)))
);

// Register repository-classes for dependency injection (DI)
builder.Services.AddScoped<IAreaChangeRepository, AreaChangeRepository>();

// Register the EmailSender service
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Configure identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(25);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure authentication and authorization
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login/";
    options.AccessDeniedPath = "/Account/AccessDenied/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});

// Configure EmailSettings from secrets
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = builder.Configuration["SMTP_SERVER"];
    options.Port = int.Parse(builder.Configuration["SMTP_PORT"] ?? "587");
    options.Username = builder.Configuration["SMTP_USERNAME"];
    options.Password = builder.Configuration["SMTP_PASSWORD"];
    options.FromEmail = builder.Configuration["SMTP_FROM_EMAIL"];
});

var app = builder.Build();

// Run migrations and add seed-data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "En feil oppstod under migrasjon eller seeding");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
