using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Data;
using RoomBooking.Api.Middleware;
using RoomBooking.Api.Services;
using RoomBooking.Api.Services.Interfaces;
using Scalar.AspNetCore;

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

// Register services
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Configure OpenAPI with metadata
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Room Booking API",
            Version = "v1",
            Description = "API for managing room bookings, buildings, and rooms"
        };
        return Task.CompletedTask;
    });

    // Apply snake_case naming to OpenAPI schema
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (schema.Properties != null)
        {
            var properties = schema.Properties.ToList();
            schema.Properties.Clear();
            
            foreach (var property in properties)
            {
                var snakeCaseName = ToSnakeCase(property.Key);
                schema.Properties[snakeCaseName] = property.Value;
            }
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable OpenAPI and Scalar UI only in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1.json");
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();

// Helper function to convert PascalCase to snake_case
static string ToSnakeCase(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    
    var result = new System.Text.StringBuilder();
    result.Append(char.ToLowerInvariant(input[0]));
    
    for (int i = 1; i < input.Length; i++)
    {
        if (char.IsUpper(input[i]))
        {
            result.Append('_');
            result.Append(char.ToLowerInvariant(input[i]));
        }
        else
        {
            result.Append(input[i]);
        }
    }
    
    return result.ToString();
}
