using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PromocaoService>();
builder.Services.AddScoped<ConfiguracaoService>();
builder.Services.AddScoped<DashboardService>();

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
