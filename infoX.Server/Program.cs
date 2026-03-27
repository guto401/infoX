using infoX.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE SERVIÇOS (Tudo que usa 'builder.Services') ---

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Pega a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configura o Entity Framework (DEVE VIR ANTES DO BUILD)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 2. CONSTRUÇÃO DO APP ---

var app = builder.Build();

// --- 3. MIDDLEWARES (Tudo que usa 'app.') ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "infoX API"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();