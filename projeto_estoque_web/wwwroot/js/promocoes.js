function catalogoProdutos() {

    const elemento = document.getElementById("catalogoProdutos");

    if (!elemento)
        return [];

    try {
        return JSON.parse(elemento.textContent) || [];
    } catch (erro) {
        return [];
    }

}


function descontoAtual() {

    const input = document.getElementById("descontoInput");

    if (!input)
        return 0;

    const valor = parseFloat(String(input.value).replace(",", "."));

    return isNaN(valor) ? 0 : valor;

}


function precoPromocional(precoTabela, desconto) {

    const resultado = precoTabela * (1 - desconto / 100);

    return Math.round((resultado + Number.EPSILON) * 100) / 100;

}


function formatarMoeda(valor) {

    return valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

}


function recalcularPrecos() {

    const desconto = descontoAtual();

    document.querySelectorAll("#itensContainer .item-row").forEach(linha => {

        const precoTabela = parseFloat(linha.dataset.preco);

        if (isNaN(precoTabela))
            return;

        const alvo = linha.querySelector("[data-preco-promo]");

        if (alvo)
            alvo.textContent = formatarMoeda(precoPromocional(precoTabela, desconto));

    });

}


function renumerarItens() {

    const container = document.getElementById("itensContainer");

    if (!container)
        return;

    const linhas = container.querySelectorAll(".item-row");

    linhas.forEach((linha, indice) => {

        const input = linha.querySelector('input[type="hidden"]');

        if (input)
            input.name = "Itens[" + indice + "].ProdutoId";

    });

    const vazio = document.getElementById("itensVazio");

    if (vazio)
        vazio.classList.toggle("hidden", linhas.length > 0);

}


function produtosSelecionados() {

    return Array.from(
        document.querySelectorAll("#itensContainer .item-row")
    ).map(linha => parseInt(linha.dataset.produtoId, 10));

}


function removerProduto(botao) {

    const linha = botao.closest(".item-row");

    if (linha)
        linha.remove();

    renumerarItens();
    filtrarCatalogo();

}


function adicionarProduto(produto) {

    const template = document.getElementById("itemTemplate");
    const container = document.getElementById("itensContainer");

    if (!template || !container)
        return;

    const clone = template.content.cloneNode(true);
    const linha = clone.querySelector(".item-row");

    linha.dataset.produtoId = produto.id;
    linha.dataset.preco = produto.preco;

    linha.querySelector('input[type="hidden"]').value = produto.id;
    linha.querySelector(".item-nome").textContent = produto.nome;
    linha.querySelector(".preco-tabela").textContent = formatarMoeda(produto.preco);

    container.appendChild(clone);

    renumerarItens();
    recalcularPrecos();
    filtrarCatalogo();

}


function filtrarCatalogo() {

    const busca = document.getElementById("buscaProduto");
    const lista = document.getElementById("listaProdutos");
    const vazio = document.getElementById("pickerVazio");

    if (!busca || !lista)
        return;

    const termo = busca.value.trim().toLowerCase();
    const selecionados = produtosSelecionados();

    lista.innerHTML = "";

    const encontrados = catalogoProdutos().filter(produto => {

        if (selecionados.includes(produto.id))
            return false;

        if (!termo)
            return true;

        return produto.nome.toLowerCase().includes(termo) ||
               produto.categoria.toLowerCase().includes(termo) ||
               String(produto.id) === termo;

    });

    encontrados.forEach(produto => {

        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = "picker-item";

        if (produto.bloqueado) {
            botao.classList.add("bloqueado");
            botao.disabled = true;
        } else {
            botao.addEventListener("click", () => adicionarProduto(produto));
        }

        const nome = document.createElement("span");
        nome.className = "picker-item-nome";
        nome.textContent = produto.nome;

        const categoria = document.createElement("span");
        categoria.className = "picker-item-categoria";
        categoria.textContent = "#" + produto.id + " · " + produto.categoria;

        const preco = document.createElement("span");
        preco.className = "picker-item-preco";
        preco.textContent = formatarMoeda(produto.preco);

        botao.appendChild(nome);
        botao.appendChild(categoria);
        botao.appendChild(preco);

        if (produto.bloqueado) {

            const aviso = document.createElement("span");
            aviso.className = "picker-item-bloqueio";
            aviso.textContent = "Em “" + (produto.bloqueioMotivo || "outra promoção") + "”";
            botao.appendChild(aviso);

        }

        lista.appendChild(botao);

    });

    if (vazio)
        vazio.classList.toggle("hidden", encontrados.length > 0);

}


document.addEventListener("DOMContentLoaded", function () {

    const busca = document.getElementById("buscaProduto");
    const desconto = document.getElementById("descontoInput");

    if (busca)
        busca.addEventListener("input", filtrarCatalogo);

    if (desconto) {
        desconto.addEventListener("input", recalcularPrecos);
        desconto.addEventListener("blur", recalcularPrecos);
    }

    renumerarItens();
    recalcularPrecos();
    filtrarCatalogo();

});