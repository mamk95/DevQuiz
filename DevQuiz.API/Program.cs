using System.Text;
using DevQuiz.API.Data;
using DevQuiz.API.Hubs;
using DevQuiz.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IQuizHubService, QuizHubService>();

// JWT Authentication - validate required configuration
var jwtSecret = builder.Configuration["JwtSecret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException(
        "JwtSecret is not configured. Please set 'JwtSecret' in appsettings.json or environment variables.\n" +
        "For development: Add to appsettings.Development.json\n" +
        "For production: Set via environment variable or Azure App Service configuration");
}

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSecret must be at least 32 characters long for security. Current length: " + jwtSecret.Length);
}

var adminPassword = builder.Configuration["AdminPassword"];
if (string.IsNullOrWhiteSpace(adminPassword))
{
    throw new InvalidOperationException(
        "AdminPassword is not configured. Please set 'AdminPassword' in appsettings.json or environment variables.\n" +
        "For development: Add to appsettings.Development.json\n" +
        "For production: Set via environment variable or Azure App Service configuration");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:3000", "http://localhost:8080"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    await db.Database.MigrateAsync();
    await QuizDataSeeder.SeedQuestionsAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Don't cache index.html to ensure users get latest cache-busted assets
        if (ctx.File.Name == "index.html")
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
        else
        {
            // Cache other static assets for 1 year (they have hash in filename)
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.MapControllers();
app.MapHub<QuizHub>("/quizhub");
app.MapFallbackToFile("index.html");

app.Run();
