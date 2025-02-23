using Health_Heroes.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Program.cs
builder.Services.AddHttpContextAccessor();

// Register the HealthHeroDbContext with a connection string

// Use SQLite for local development/testing, otherwise use SQL Server
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<HealthHeroDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}
else
{
    builder.Services.AddDbContext<HealthHeroDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // User settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Lock for 15 minutes
    options.Lockout.MaxFailedAccessAttempts = 3; // Lockout after 3 failed attempts
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddMemoryCache();

// Configure cookie policy options
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict; // Ensures cookies are sent only on same-site requests
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Token in headers for AJAX requests
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromSeconds(120); // Set cookie expiration to 10 seconds
        options.SlidingExpiration = false; // Disable sliding expiration
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                // For AJAX or API requests, send 401 status code
                context.Response.StatusCode = 401;
            }
            else
            {
                // Redirect to login for normal requests
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
    });

// Add session services with configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(120);  // Set session timeout duration
    options.Cookie.HttpOnly = true;                  // Ensure the cookie is accessible only by the server
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Mark the session cookie as essential
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware before authentication and authorization
app.UseSession();



// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Only redirect to login if accessing restricted pages (e.g., anything under /Home or other protected areas)
    var protectedPaths = new[] { "/Home", "/Dashboard", "/Profile" }; // Add your protected paths here

    if (!context.User.Identity.IsAuthenticated &&
        protectedPaths.Any(p => context.Request.Path.StartsWithSegments(p)) &&
        context.Request.Path != "/Account/Login" &&
        context.Request.Path != "/Account/SignUp" &&
        !context.Request.Path.StartsWithSegments("/static"))
    {
        // Redirect to login page only if accessing protected resources
        context.Response.Redirect("/Account/Login?message=Session%20expired");
    }
    else
    {
        await next();
    }
});


app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
