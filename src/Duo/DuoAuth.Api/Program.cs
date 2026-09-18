using DuoAuth.Api.Options;
using DuoAuth.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Add Services to the Container
// ==========================================

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddOptions<DuoOptions>()
    .Bind(builder.Configuration.GetSection("Duo"))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.ClientId) &&
        !string.IsNullOrWhiteSpace(options.ClientSecret) &&
        !string.IsNullOrWhiteSpace(options.ApiHost) &&
        Uri.TryCreate(options.CallbackUrl, UriKind.Absolute, out _),
        "Duo configuration is incomplete or invalid.")
    .ValidateOnStart();

builder.Services.AddSingleton<IDuoAuthService, DuoAuthService>();

// ==========================================
// 2. Configure the HTTP Request Pipeline
// ==========================================

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSession();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }