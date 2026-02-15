using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Middleware;

// Load environment variables from .env file (development convenience)
// Look for .env in repository root (parent directory of src/)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else
{
    // Fallback to current directory
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure JSON serialization with snake_case
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure PostgreSQL Database
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DB_CONNECTION_STRING environment variable is not set.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
