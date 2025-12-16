using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

app.MapControllers();

app.Run();
