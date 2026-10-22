using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class PedidoService
{
    private readonly EstoqueDbContext _db;

    public PedidoService(EstoqueDbContext db) => _db = db;

    public async Task<PedidosPaginaDto> ListarAsync(string? busca, string? status, int pagina, int itensPorPagina)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

        var consulta = _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Include(p => p.PedidoItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            var numerico = int.TryParse(termo, out var codigo);

            consulta = consulta.Where(p =>
                p.Usuario.Nome.ToLower().Contains(termo) ||
                (numerico && p.Id == codigo));
        }

        var statusFiltro = StatusPedido.Converter(status);

        if (statusFiltro is not null)
            consulta = consulta.Where(p => p.Status == statusFiltro);

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderByDescending(p => p.Data)
            .ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var resumo = new PedidosResumoDto(
            await _db.Pedidos.CountAsync(),
            await _db.Pedidos.CountAsync(p => p.Status == "PENDENTE"),
            await _db.Pedidos.CountAsync(p => p.Status == "ENTREGUE"),
            await _db.Pedidos
                .Where(p => p.Status != "CANCELADO")
                .SumAsync(p => (decimal?)p.ValorTotal) ?? 0m);

        return new PedidosPaginaDto(
            itens.Select(p => ParaDto(p)).ToList(),
            total,
            pagina,
            totalPaginas,
            itensPorPagina,
            resumo);
    }

    public async Task<PedidoDto> ObterAsync(int id)
    {
        var pedido = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Include(p => p.PedidoItems)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Pedido não encontrado.");

        return ParaDto(pedido);
    }

    public async Task<PedidoDto> CriarAsync(PedidoSalvarDto dados)
    {
        var itens = NormalizarItens(dados.Itens);
        var status = StatusPedido.Normalizar(dados.Status);
        var data = (dados.Data ?? DateTime.Today).Date;

        var usuario = await BuscarOuCriarUsuarioAsync(dados.Cliente);
        var produtos = await CarregarProdutosAsync(itens);

        if (status != "CANCELADO")
            ValidarEstoque(itens, produtos, null);

        await using var transacao = await _db.Database.BeginTransactionAsync();

        var pedido = new Pedido
        {
            UsuarioId = usuario.Id,
            Data = data,
            Status = status,
            Observacao = NormalizarObservacao(dados.Observacao)
        };

        foreach (var item in itens)
        {
            var produto = produtos[item.ProdutoId];
            pedido.PedidoItems.Add(new PedidoItem
            {
                ProdutoId = produto.Id,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        pedido.ValorTotal = pedido.PedidoItems.Sum(i => i.Quantidade * i.PrecoUnitario);

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        if (status != "CANCELADO")
            ReservarEstoque(pedido.Id, itens, produtos);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        pedido.Usuario = usuario;

        return ParaDto(pedido, produtos.ToDictionary(p => p.Key, p => p.Value.Nome));
    }

    public async Task<PedidoDto> AtualizarAsync(int id, PedidoSalvarDto dados)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.PedidoItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Pedido não encontrado.");

        var itens = NormalizarItens(dados.Itens);
        var status = StatusPedido.Normalizar(dados.Status);
        var data = (dados.Data ?? pedido.Data).Date;

        var usuario = await BuscarOuCriarUsuarioAsync(dados.Cliente);
        var produtos = await CarregarProdutosAsync(itens);

        // O saldo reservado pelo pedido anterior é considerado disponível (RN-32).
        if (status != "CANCELADO")
            ValidarEstoque(itens, produtos, pedido);

        await using var transacao = await _db.Database.BeginTransactionAsync();

        if (pedido.Status != "CANCELADO")
            await DevolverEstoqueAsync(pedido, $"Estorno da edição do pedido #{pedido.Id}");

        var itensAnteriores = pedido.PedidoItems.ToList();
        _db.PedidoItems.RemoveRange(itensAnteriores);
        await _db.SaveChangesAsync(); // libera o índice único (PedidoId, ProdutoId) antes de reinserir

        pedido.UsuarioId = usuario.Id;
        pedido.Data = data;
        pedido.Status = status;
        pedido.Observacao = NormalizarObservacao(dados.Observacao);
        pedido.PedidoItems = itens.Select(item => new PedidoItem
        {
            PedidoId = pedido.Id,
            ProdutoId = item.ProdutoId,
            Quantidade = item.Quantidade,
            PrecoUnitario = produtos[item.ProdutoId].Preco
        }).ToList();

        pedido.ValorTotal = pedido.PedidoItems.Sum(i => i.Quantidade * i.PrecoUnitario);

        if (status != "CANCELADO")
            ReservarEstoque(pedido.Id, itens, produtos);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        pedido.Usuario = usuario;

        return ParaDto(pedido, produtos.ToDictionary(p => p.Key, p => p.Value.Nome));
    }

    public async Task<PedidoDto> AtualizarStatusAsync(int id, string? statusValor)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.PedidoItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Pedido não encontrado.");

        var novoStatus = StatusPedido.Converter(statusValor)
            ?? throw new ErroDeNegocioException("Status inválido.");

        // Mesmo status é operação silenciosa (RN-36).
        if (novoStatus == pedido.Status)
            return ParaDto(pedido);

        await using var transacao = await _db.Database.BeginTransactionAsync();

        if (novoStatus == "CANCELADO")
        {
            // Cancelar devolve as quantidades ao estoque (RN-33).
            await DevolverEstoqueAsync(pedido, $"Cancelamento do pedido #{pedido.Id}");
        }
        else if (pedido.Status == "CANCELADO")
        {
            // Reativar exige validar tudo antes; qualquer falha aborta a operação (RN-34).
            await ReservarNovamenteAsync(pedido);
        }

        pedido.Status = novoStatus;

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return ParaDto(pedido);
    }

    public async Task ExcluirAsync(int id)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.PedidoItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Pedido não encontrado.");

        await using var transacao = await _db.Database.BeginTransactionAsync();

        // Excluir pedido não cancelado devolve o estoque (RN-35).
        if (pedido.Status != "CANCELADO")
            await DevolverEstoqueAsync(pedido, $"Exclusão do pedido #{pedido.Id}");

        var entrega = await _db.PedidoEntregas.FirstOrDefaultAsync(e => e.PedidoId == id);

        if (entrega is not null)
            _db.PedidoEntregas.Remove(entrega);

        _db.PedidoItems.RemoveRange(pedido.PedidoItems);
        _db.Pedidos.Remove(pedido);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();
    }

    private static List<ItemPedidoSalvarDto> NormalizarItens(List<ItemPedidoSalvarDto>? itens)
    {
        var normalizados = (itens ?? new List<ItemPedidoSalvarDto>())
            .Where(i => i.ProdutoId > 0 && i.Quantidade > 0)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new ItemPedidoSalvarDto(g.Key, g.Sum(i => i.Quantidade)))
            .ToList();

        if (normalizados.Count == 0)
            throw new ErroDeNegocioException("Adicione pelo menos um item ao pedido.");

        return normalizados;
    }

    private async Task<Dictionary<int, Produto>> CarregarProdutosAsync(IEnumerable<ItemPedidoSalvarDto> itens)
    {
        var ids = itens.Select(i => i.ProdutoId).Distinct().ToList();

        var produtos = await _db.Produtos.Where(p => ids.Contains(p.Id)).ToListAsync();

        foreach (var id in ids)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == id);

            if (produto is null || !produto.Ativo)
                throw new ErroDeNegocioException("Um dos produtos selecionados não existe mais no cadastro.");
        }

        return produtos.ToDictionary(p => p.Id);
    }

    private static void ValidarEstoque(
        List<ItemPedidoSalvarDto> itens,
        Dictionary<int, Produto> produtos,
        Pedido? pedidoExistente)
    {
        foreach (var item in itens)
        {
            var produto = produtos[item.ProdutoId];

            var reservadoPeloPedido = pedidoExistente is { Status: not "CANCELADO" }
                ? pedidoExistente.PedidoItems
                    .Where(i => i.ProdutoId == item.ProdutoId)
                    .Sum(i => i.Quantidade)
                : 0;

            var disponivel = produto.Quantidade + reservadoPeloPedido;

            if (item.Quantidade > disponivel)
            {
                throw new ErroDeNegocioException(
                    $"Estoque insuficiente para \"{produto.Nome}\" " +
                    $"(disponível: {disponivel}, solicitado: {item.Quantidade}).");
            }
        }
    }

    private void ReservarEstoque(int pedidoId, IEnumerable<ItemPedidoSalvarDto> itens, Dictionary<int, Produto> produtos)
    {
        foreach (var item in itens)
        {
            var produto = produtos[item.ProdutoId];
            produto.Quantidade -= item.Quantidade;

            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = "SAIDA",
                Quantidade = item.Quantidade,
                Data = DateTime.Now,
                Observacao = $"Baixa pelo pedido #{pedidoId}"
            });
        }
    }

    private async Task DevolverEstoqueAsync(Pedido pedido, string observacao)
    {
        foreach (var item in pedido.PedidoItems)
        {
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId);

            if (produto is null)
                continue; // produto apagado do banco: nada a devolver

            produto.Quantidade += item.Quantidade;

            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = "ENTRADA",
                Quantidade = item.Quantidade,
                Data = DateTime.Now,
                Observacao = observacao
            });
        }
    }

    private async Task ReservarNovamenteAsync(Pedido pedido)
    {
        var produtos = new Dictionary<int, Produto>();

        foreach (var item in pedido.PedidoItems)
        {
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId)
                ?? throw new ErroDeNegocioException(
                    "Um dos produtos do pedido não existe mais no cadastro. Status não alterado.");

            produtos[produto.Id] = produto;
        }

        // Valida todos antes de reservar qualquer um.
        foreach (var item in pedido.PedidoItems)
        {
            var produto = produtos[item.ProdutoId];

            if (item.Quantidade > produto.Quantidade)
            {
                throw new ErroDeNegocioException(
                    $"Estoque insuficiente para \"{produto.Nome}\" " +
                    $"(disponível: {produto.Quantidade}, solicitado: {item.Quantidade}). Status não alterado.");
            }
        }

        foreach (var item in pedido.PedidoItems)
        {
            var produto = produtos[item.ProdutoId];
            produto.Quantidade -= item.Quantidade;

            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = "SAIDA",
                Quantidade = item.Quantidade,
                Data = DateTime.Now,
                Observacao = $"Reativação do pedido #{pedido.Id}"
            });
        }
    }

    /// <summary>
    /// O banco exige um UsuarioId em Pedido. Como o sistema ainda não tem
    /// cadastro de usuários, o cliente digitado é buscado pelo nome; se não
    /// existir, é criado um usuário mínimo sem login (SenhaHash = SEM_LOGIN).
    /// </summary>
    private async Task<Usuario> BuscarOuCriarUsuarioAsync(string? cliente)
    {
        var nome = (cliente ?? string.Empty).Trim();

        if (nome.Length == 0)
            throw new ErroDeNegocioException("Informe o nome do cliente.");

        if (nome.Length > 120)
            throw new ErroDeNegocioException("O nome do cliente deve ter no máximo 120 caracteres.");

        var existente = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());

        if (existente is not null)
            return existente;

        var slug = GerarSlug(nome);
        var email = $"{slug}@clientes.estoque";
        var sufixo = 1;

        while (await _db.Usuarios.AnyAsync(u => u.Email == email))
            email = $"{slug}{sufixo++}@clientes.estoque";

        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            SenhaHash = "SEM_LOGIN",
            Ativo = true,
            DataCadastro = DateTime.Now
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return usuario;
    }

    private static string GerarSlug(string nome)
    {
        var normalizado = nome.Normalize(NormalizationForm.FormD);
        var construtor = new StringBuilder();

        foreach (var caractere in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(caractere))
                construtor.Append(char.ToLowerInvariant(caractere));
            else if (construtor.Length > 0 && construtor[^1] != '.')
                construtor.Append('.');
        }

        var slug = construtor.ToString().Trim('.');

        if (slug.Length == 0)
            slug = "cliente";

        return slug.Length > 100 ? slug[..100] : slug;
    }

    private static string? NormalizarObservacao(string? valor)
    {
        var observacao = (valor ?? string.Empty).Trim();

        if (observacao.Length == 0)
            return null;

        return observacao.Length > 500 ? observacao[..500] : observacao;
    }

    private static PedidoDto ParaDto(Pedido pedido, IReadOnlyDictionary<int, string>? nomesProdutos = null)
    {
        var itens = pedido.PedidoItems
            .OrderBy(i => i.Id)
            .Select(i => new PedidoItemDto(
                i.ProdutoId,
                nomesProdutos is not null && nomesProdutos.TryGetValue(i.ProdutoId, out var nome)
                    ? nome
                    : i.Produto?.Nome ?? $"Produto #{i.ProdutoId}",
                i.Quantidade,
                i.PrecoUnitario,
                i.Quantidade * i.PrecoUnitario))
            .ToList();

        return new PedidoDto(
            pedido.Id,
            pedido.UsuarioId,
            pedido.Usuario?.Nome ?? string.Empty,
            pedido.Data,
            pedido.Status,
            StatusPedido.Rotulo(pedido.Status),
            StatusPedido.Classe(pedido.Status),
            itens.Count,
            pedido.ValorTotal,
            pedido.Observacao,
            itens);
    }
}
