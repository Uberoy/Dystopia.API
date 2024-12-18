using Dystopia.API.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Dystopia.API.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5951");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqService>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services
    .AddCors(options =>
        options.AddPolicy(
            "Support",
            policy =>
                policy
                    .WithOrigins("https://localhost:7151", "https://localhost:7173", "https://localhost:7777")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
        )
    );


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Support");

app.MapPost("/tickets", async (Ticket ticket, RabbitMqService rabbitMqService) =>
{
    try
    {
        var jsonTicket = JsonSerializer.Serialize(ticket);
        rabbitMqService.PublishMessage(jsonTicket);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.StatusCode(500);
    }
});

app.Run();