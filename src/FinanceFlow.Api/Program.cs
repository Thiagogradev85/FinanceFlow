using FinanceFlow.Api.Common;
using FinanceFlow.Api.Endpoints;
using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Accounts.Infrastructure;
using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Infrastructure;
using FinanceFlow.SharedKernel;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging estruturado ───────────────────────────────────────────────
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

// ── Connection string (mesmo banco, schemas separados por módulo) ──────
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=financeflow;Username=financeflow;Password=financeflow123";

// ── Módulos (cada um registra seu DbContext + repositórios + UnitOfWork) ──
builder.Services.AddAccountsModule(connectionString);
builder.Services.AddTransactionsModule(connectionString);

// ── CQRS (MediatR) + validação de entrada (FluentValidation) ───────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(AccountsApplicationAssemblyMarker).Assembly,
        typeof(TransactionsApplicationAssemblyMarker).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblies(
[
    typeof(AccountsApplicationAssemblyMarker).Assembly,
    typeof(TransactionsApplicationAssemblyMarker).Assembly
]);

// ── Infra transversal ──────────────────────────────────────────────────
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IEventBus, LoggingEventBus>(); // Fase 2: trocar por AddKafkaEventBus()

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Swagger ────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS (o front em dev usa proxy do Vite; CORS fica de rede de segurança) ──
const string DevCors = "dev-cors";
builder.Services.AddCors(options =>
    options.AddPolicy(DevCors, policy =>
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors(DevCors);

// Aplica migrations e semeia dados de demonstração no boot.
await app.UseDatabaseStartupAsync();

// ── Endpoints ──────────────────────────────────────────────────────────
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("Health");
app.MapAccountsEndpoints();
app.MapTransactionsEndpoints();
app.MapDashboardEndpoints();

app.Run();
