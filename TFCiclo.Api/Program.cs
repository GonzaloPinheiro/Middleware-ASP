using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using TFCiclo.Connector;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Services;
using TimerModule;

var builder = WebApplication.CreateBuilder(args);


// --------------------------- Configuración básica de los servicios--------------------------- //
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JWT
string jwtSecret = builder.Configuration["JwtSecret"];
var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "tfciclo",
        ValidAudience = "tfciclo",
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero // opcional: elimina margen de expiración
    };
});

builder.Services.AddAuthorization();
//JWT

// --------------------------- Rate Limiting --------------------------- //

//Política basada en IP(strict-high)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("strict-auth", context =>
    {
        string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: clientIp,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,                     // máximo 5 requests
                TokensPerPeriod = 5,                // se repone 5 tokens por periodo
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,                     // rechaza inmediatamente
                AutoReplenishment = true
            });
    });



    options.AddPolicy("jwt-user", context =>
    {
        // Intenta varios tipos comunes (orden pensado para lo más habitual)
        string? userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value   //mapeo por defecto
                        ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value //"sub"
                        ?? context.User.FindFirst("sub")?.Value                       //literal "sub" si no se mapeó
                        ?? context.User.FindFirst(ClaimTypes.Name)?.Value;           //fallback a name

        if (!string.IsNullOrWhiteSpace(userId))
        {
            string partitionKey = $"user:{userId}";
             //Console.WriteLine($"[RateLimit] Using USER partition: {partitionKey}");
            return RateLimitPartition.GetTokenBucketLimiter(
                partitionKey,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 100,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        }

        // Fallback a IP
        string ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string ipKey = $"ip:{ip}";
         //Console.WriteLine($"[RateLimit] Using IP partition: {ipKey}");
        return RateLimitPartition.GetTokenBucketLimiter(
            ipKey,
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                TokensPerPeriod = 100,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });



    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", token);
    };
});



// --------------------------- Connection string --------------------------- //
string connectionString = builder.Configuration.GetConnectionString("DefaultEncrypted")!;

// --------------------------- Logging y BackgroundService --------------------------- //
// Registrar el logger con DI y pasar la cadena de conexión
builder.Services.AddSingleton<LogEntryRepository>(provider => new LogEntryRepository(connectionString));

// Registrar la implementación concreta
builder.Services.AddSingleton<LogBackgroundService>();

// Exponerla como ILogQueue
builder.Services.AddSingleton<ILogQueue>(sp =>
    sp.GetRequiredService<LogBackgroundService>());

// Arrancarla como HostedService
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<LogBackgroundService>());


// Registrar Logger singleton (depende de LogEntryRepository + ILogQueue)
builder.Services.AddSingleton<Logger>(sp =>
{
    LogEntryRepository repo = sp.GetRequiredService<LogEntryRepository>();
    ILogQueue logQueue = sp.GetRequiredService<ILogQueue>();
    return new Logger(repo, logQueue);
});


// --------------------------- Repositorios de aplicación --------------------------- //
//Registrar el repositorio con DI y pasar la cadena de conexión
builder.Services.AddScoped<WeatherRepository>(provider =>
{
    Logger logger = provider.GetRequiredService<Logger>();
    return new WeatherRepository(connectionString, logger);
});

//Registrar el repositorio con DI y pasar la cadena de conexión
builder.Services.AddScoped<UserRepository>(provider =>
{
    Logger logger = provider.GetRequiredService<Logger>();
    return new UserRepository(connectionString, logger);
});

//Registrar el repositorio con DI y pasar la cadena de conexión
builder.Services.AddScoped<RefreshTokenRepository>(provider =>
{
    Logger logger = provider.GetRequiredService<Logger>();
    return new RefreshTokenRepository(connectionString, logger);
});

// --------------------------- TimerConnector y TimedHostedService --------------------------- //
// TimerConnector recibe Logger y weather repo por DI
builder.Services.AddTransient<TimerConnector>(sp =>
{
    Logger logger = sp.GetRequiredService<Logger>();
    WeatherRepository weatherRepo = sp.GetRequiredService<WeatherRepository>();
    return new TimerConnector(logger, weatherRepo);
});

//Agregar el servicio del temporizador que usa TimmerConnector
builder.Services.AddHostedService<TimedHostedService>();


// --------------------------- Build y pipeline --------------------------- //
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else//Fuerza a los navegadores a usar una conexión segura https(solo se aplica fuera de development)
{
    app.UseHsts();
}

//Redirige cualquier petición http a https
app.UseHttpsRedirection();

//Autorización y mapeo de servicios
app.UseAuthentication(); //jwt--
app.UseAuthorization();//jwt--

//Activa el rate limiting
app.UseRateLimiter();

app.MapControllers();

app.Run();