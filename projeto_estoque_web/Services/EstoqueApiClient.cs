using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace projeto_estoque_web.Services
{
    /// <summary>
    /// Cliente HTTP tipado do sistema web para a API REST. Centraliza as rotas,
    /// a paginação e a tradução dos erros de negócio devolvidos pela API.
    /// </summary>
    public class EstoqueApiClient
    {
        private const int TamanhoMaximoPagina = 100;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;

        public EstoqueApiClient(HttpClient http) => _http = http;

        // ========================= PRODUTOS =========================

        public Task<ProdutosPaginaApiDto> ListarProdutosAsync(
            string? busca, int pagina, int itensPorPagina, int? categoriaId = null)
        {
            var url = ComQuery(
                "api/produtos",
                ("busca", busca),
                ("categoriaId", categoriaId?.ToString()),
                ("pagina", pagina.ToString()),
                ("itensPorPagina", itensPorPagina.ToString()));

            return ObterAsync<ProdutosPaginaApiDto>(url);
        }

        public Task<ProdutoApiDto> ObterProdutoAsync(int id)
            => ObterAsync<ProdutoApiDto>($"api/produtos/{id}");

        public Task<ProdutoApiDto> CriarProdutoAsync(ProdutoSalvarApiDto dados)
            => EnviarAsync<ProdutoApiDto>(HttpMethod.Post, "api/produtos", dados);

        public Task<ProdutoApiDto> AtualizarProdutoAsync(int id, ProdutoSalvarApiDto dados)
            => EnviarAsync<ProdutoApiDto>(HttpMethod.Put, $"api/produtos/{id}", dados);

        public Task ExcluirProdutoAsync(int id)
            => ExcluirAsync($"api/produtos/{id}");

        /// <summary>Todos os produtos ativos (percorre as páginas da API).</summary>
        public async Task<List<ProdutoApiDto>> TodosProdutosAsync()
        {
            var produtos = new List<ProdutoApiDto>();
            var pagina = 1;

            while (true)
            {
                var lote = await ListarProdutosAsync(null, pagina, TamanhoMaximoPagina);
                produtos.AddRange(lote.Itens);

                if (pagina >= lote.TotalPaginas)
                    break;

                pagina++;
            }

            return produtos;
        }

        // ========================= CATEGORIAS =========================

        public Task<CategoriasPaginaApiDto> ListarCategoriasAsync(
            string? busca, int pagina, int itensPorPagina)
        {
            var url = ComQuery(
                "api/categorias",
                ("busca", busca),
                ("pagina", pagina.ToString()),
                ("itensPorPagina", itensPorPagina.ToString()));

            return ObterAsync<CategoriasPaginaApiDto>(url);
        }

        public Task<List<OpcaoApiDto>> ListarOpcoesCategoriaAsync()
            => ObterAsync<List<OpcaoApiDto>>("api/categorias/opcoes");

        public Task<CategoriaApiDto> ObterCategoriaAsync(int id)
            => ObterAsync<CategoriaApiDto>($"api/categorias/{id}");

        public Task<CategoriaApiDto> CriarCategoriaAsync(CategoriaSalvarApiDto dados)
            => EnviarAsync<CategoriaApiDto>(HttpMethod.Post, "api/categorias", dados);

        public Task<CategoriaApiDto> AtualizarCategoriaAsync(int id, CategoriaSalvarApiDto dados)
            => EnviarAsync<CategoriaApiDto>(HttpMethod.Put, $"api/categorias/{id}", dados);

        public Task ExcluirCategoriaAsync(int id)
            => ExcluirAsync($"api/categorias/{id}");

        // ========================= ESTOQUE =========================

        public Task<EstoquePaginaApiDto> ListarEstoqueAsync(
            string? busca, int pagina, int itensPorPagina)
        {
            var url = ComQuery(
                "api/estoque",
                ("busca", busca),
                ("pagina", pagina.ToString()),
                ("itensPorPagina", itensPorPagina.ToString()));

            return ObterAsync<EstoquePaginaApiDto>(url);
        }

        public Task<MovimentoResultadoApiDto> MovimentarEstoqueAsync(
            int produtoId, string tipo, int quantidade)
        {
            var dados = new MovimentoSalvarApiDto
            {
                ProdutoId = produtoId,
                Tipo = tipo,
                Quantidade = quantidade
            };

            return EnviarAsync<MovimentoResultadoApiDto>(HttpMethod.Post, "api/estoque/movimentos", dados);
        }

        // ========================= PEDIDOS =========================

        public Task<PedidosPaginaApiDto> ListarPedidosAsync(
            string? busca, int pagina, int itensPorPagina)
        {
            var url = ComQuery(
                "api/pedidos",
                ("busca", busca),
                ("pagina", pagina.ToString()),
                ("itensPorPagina", itensPorPagina.ToString()));

            return ObterAsync<PedidosPaginaApiDto>(url);
        }

        public Task<PedidoApiDto> ObterPedidoAsync(int id)
            => ObterAsync<PedidoApiDto>($"api/pedidos/{id}");

        public Task<PedidoApiDto> CriarPedidoAsync(PedidoSalvarApiDto dados)
            => EnviarAsync<PedidoApiDto>(HttpMethod.Post, "api/pedidos", dados);

        public Task<PedidoApiDto> AtualizarPedidoAsync(int id, PedidoSalvarApiDto dados)
            => EnviarAsync<PedidoApiDto>(HttpMethod.Put, $"api/pedidos/{id}", dados);

        public async Task<PedidoApiDto> AlterarStatusPedidoAsync(int id, string status)
        {
            using var requisicao = new HttpRequestMessage(HttpMethod.Patch, $"api/pedidos/{id}/status")
            {
                Content = JsonContent.Create(new { status })
            };

            using var resposta = await _http.SendAsync(requisicao);

            return await LerAsync<PedidoApiDto>(resposta);
        }

        public Task ExcluirPedidoAsync(int id)
            => ExcluirAsync($"api/pedidos/{id}");

        // ========================= PROMOÇÕES =========================

        public Task<PromocoesPaginaApiDto> ListarPromocoesAsync(
            string? busca, int pagina, int itensPorPagina)
        {
            var url = ComQuery(
                "api/promocoes",
                ("busca", busca),
                ("pagina", pagina.ToString()),
                ("itensPorPagina", itensPorPagina.ToString()));

            return ObterAsync<PromocoesPaginaApiDto>(url);
        }

        /// <summary>Todas as promoções (percorre as páginas da API).</summary>
        public async Task<List<PromocaoApiDto>> TodasPromocoesAsync()
        {
            var promocoes = new List<PromocaoApiDto>();
            var pagina = 1;

            while (true)
            {
                var lote = await ListarPromocoesAsync(null, pagina, TamanhoMaximoPagina);
                promocoes.AddRange(lote.Itens);

                if (pagina >= lote.TotalPaginas)
                    break;

                pagina++;
            }

            return promocoes;
        }

        public Task<PromocaoApiDto> ObterPromocaoAsync(int id)
            => ObterAsync<PromocaoApiDto>($"api/promocoes/{id}");

        public Task<PromocaoApiDto> CriarPromocaoAsync(PromocaoSalvarApiDto dados)
            => EnviarAsync<PromocaoApiDto>(HttpMethod.Post, "api/promocoes", dados);

        public Task<PromocaoApiDto> AtualizarPromocaoAsync(int id, PromocaoSalvarApiDto dados)
            => EnviarAsync<PromocaoApiDto>(HttpMethod.Put, $"api/promocoes/{id}", dados);

        public async Task<PromocaoApiDto> AlternarStatusPromocaoAsync(int id)
        {
            using var requisicao = new HttpRequestMessage(HttpMethod.Patch, $"api/promocoes/{id}/status");
            using var resposta = await _http.SendAsync(requisicao);

            return await LerAsync<PromocaoApiDto>(resposta);
        }

        public Task ExcluirPromocaoAsync(int id)
            => ExcluirAsync($"api/promocoes/{id}");

        // ========================= CONFIGURAÇÕES =========================

        public Task<EmpresaApiDto> ObterEmpresaAsync()
            => ObterAsync<EmpresaApiDto>("api/configuracoes/empresa");

        public Task<EmpresaApiDto> SalvarEmpresaAsync(EmpresaSalvarApiDto dados)
            => EnviarAsync<EmpresaApiDto>(HttpMethod.Put, "api/configuracoes/empresa", dados);

        // ========================= DASHBOARD / APOIO =========================

        public Task<DashboardApiDto> ObterDashboardAsync()
            => ObterAsync<DashboardApiDto>("api/dashboard");

        public Task<List<OpcaoApiDto>> ListarLocalizacoesAsync()
            => ObterAsync<List<OpcaoApiDto>>("api/localizacoes");

        // ========================= INFRAESTRUTURA =========================

        private async Task<T> ObterAsync<T>(string url)
        {
            using var resposta = await _http.GetAsync(url);

            return await LerAsync<T>(resposta);
        }

        private async Task<T> EnviarAsync<T>(HttpMethod metodo, string url, object corpo)
        {
            using var requisicao = new HttpRequestMessage(metodo, url)
            {
                Content = JsonContent.Create(corpo)
            };

            using var resposta = await _http.SendAsync(requisicao);

            return await LerAsync<T>(resposta);
        }

        private async Task ExcluirAsync(string url)
        {
            using var resposta = await _http.DeleteAsync(url);

            if (!resposta.IsSuccessStatusCode)
            {
                var conteudo = await resposta.Content.ReadAsStringAsync();

                throw CriarExcecao(resposta.StatusCode, conteudo);
            }
        }

        private static async Task<T> LerAsync<T>(HttpResponseMessage resposta)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();

            if (!resposta.IsSuccessStatusCode)
                throw CriarExcecao(resposta.StatusCode, conteudo);

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new ApiException((int)resposta.StatusCode, "A API retornou uma resposta vazia.");

            try
            {
                return JsonSerializer.Deserialize<T>(conteudo, JsonOptions)
                    ?? throw new ApiException((int)resposta.StatusCode, "A API retornou uma resposta inválida.");
            }
            catch (JsonException)
            {
                throw new ApiException((int)resposta.StatusCode, "A API retornou uma resposta inválida.");
            }
        }

        private static ApiException CriarExcecao(HttpStatusCode statusCode, string conteudo)
        {
            var mensagem = LerMensagem(conteudo)
                ?? $"Não foi possível concluir a operação (HTTP {(int)statusCode}).";

            return new ApiException((int)statusCode, mensagem);
        }

        private static string? LerMensagem(string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
                return null;

            try
            {
                using var documento = JsonDocument.Parse(conteudo);

                if (documento.RootElement.ValueKind == JsonValueKind.Object &&
                    documento.RootElement.TryGetProperty("mensagem", out var elemento) &&
                    elemento.ValueKind == JsonValueKind.String)
                {
                    return elemento.GetString();
                }
            }
            catch (JsonException)
            {
                // Corpo não JSON: usa a mensagem padrão.
            }

            return null;
        }

        private static string ComQuery(string rota, params (string Nome, string? Valor)[] parametros)
        {
            var usados = parametros
                .Where(p => !string.IsNullOrWhiteSpace(p.Valor))
                .Select(p => $"{Uri.EscapeDataString(p.Nome)}={Uri.EscapeDataString(p.Valor!)}")
                .ToList();

            return usados.Count == 0 ? rota : $"{rota}?{string.Join("&", usados)}";
        }
    }
}
