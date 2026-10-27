using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Middlewares;
using projeto_estoque_api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Respostas de erro de validação no mesmo formato dos erros de negócio:
// { "mensagem": "..." } — o cliente web exibe esse texto diretamente.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensagens = context.ModelState
            .SelectMany(estado => estado.Value?.Errors ?? new())
            .Select(erro => erro.ErrorMessage)
            .Where(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            .Distinct()
            .ToList();

        return new BadRequestObjectResult(new
        {
            mensagem = mensagens.Count > 0 ? string.Join(" ", mensagens) : "Dados inválidos."
        });
    };
});

var conexao = builder.Configuration.GetConnectionString("EstoqueDb");

if (string.IsNullOrWhiteSpace(conexao))
{
    throw new InvalidOperationException(
        "A connection string 'EstoqueDb' não foi configurada. " +
        "Defina com 'dotnet user-secrets set \"ConnectionStrings:EstoqueDb\" \"<connection string>\"' " +
        "ou pela variável de ambiente ConnectionStrings__EstoqueDb.");
}

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlServer(conexao));

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "A chave JWT 'Jwt:Key' não foi configurada (mínimo de 32 bytes). " +
        "Defina com 'dotnet user-secrets set \"Jwt:Key\" \"<chave>\"' " +
        "ou pela variável de ambiente Jwt__Key.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "projeto_estoque_api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "projeto_estoque_mobile",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PromocaoService>();
builder.Services.AddScoped<ConfiguracaoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddDefaultPolicy(politica =>
    politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ErroMiddleware>();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", async (EstoqueDbContext db) =>
{
    var conectado = await db.Database.CanConnectAsync();

    return Results.Ok(new
    {
        status = conectado ? "ok" : "sem-conexao",
        banco = conectado,
        hora = DateTime.Now
    });
});

app.Run();
