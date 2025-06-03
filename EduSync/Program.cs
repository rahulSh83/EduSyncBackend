using EduSync.Data;
using EduSyncAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                            "http://localhost:3000",
                            "https://gray-moss-0fe93b800.6.azurestaticapps.net"
            )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EventHubService>();

var config = builder.Configuration;

var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: 'DefaultConnection' is NULL or empty!");
    throw new Exception("Azure failed to load 'DefaultConnection'. Check App Service > Configuration > Connection Strings.");
}
else
{
    Console.WriteLine("Connection string loaded.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred.");
        throw; // rethrow so standard error handler still triggers
    }
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
