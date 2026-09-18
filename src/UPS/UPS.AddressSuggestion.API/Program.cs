using FluentValidation;
using UPS.AddressSuggestion.API.Middleware;
using UPS.AddressSuggestion.Application.Behaviors;
using UPS.AddressSuggestion.Application.Handlers;
using UPS.AddressSuggestion.Application.Validators;
using UPS.AddressSuggestion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SuggestAddressCommandHandler).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<SuggestAddressCommandValidator>();
builder.Services.AddUpsAddressSuggestion(builder.Configuration);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
