using FedEx.Shipment.Application;
using FedEx.Shipment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Add Services to the Container
// ==========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture Dependency Injection Extensions
builder.Services.AddShipmentApplication();
builder.Services.AddShipmentInfrastructure(builder.Configuration);

// ==========================================
// 2. Configure the HTTP Request Pipeline
// ==========================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();