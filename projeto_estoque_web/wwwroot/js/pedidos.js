function renumerarItens() {

    const container = document.getElementById("itensContainer");

    if (!container)
        return;

    const linhas = container.querySelectorAll(".item-row");

    linhas.forEach((linha, indice) => {

        const select = linha.querySelector(".item-produto");
        const input = linha.querySelector(".item-qtd-input");

        if (select)
            select.name = "Itens[" + indice + "].ProdutoId";

        if (input)
            input.name = "Itens[" + indice + "].Quantidade";

    });

    const vazio = document.getElementById("itensVazio");

    if (vazio)
        vazio.classList.toggle("hidden", linhas.length > 0);

}


function adicionarItem() {

    const template = document.getElementById("itemTemplate");
    const container = document.getElementById("itensContainer");

    if (!template || !container)
        return;

    container.appendChild(template.content.cloneNode(true));
    renumerarItens();

}


function removerItem(botao) {

    const linha = botao.closest(".item-row");

    if (linha)
        linha.remove();

    renumerarItens();

}


document.addEventListener("DOMContentLoaded", function () {

    renumerarItens();

    document.addEventListener("click", function (evento) {

        const alvo = evento.target instanceof Element
            ? evento.target.closest(".btn-produtos")
            : null;

        if (!alvo)
            return;

        const idDetalhe = alvo.getAttribute("data-target");
        const detalhe = idDetalhe ? document.getElementById(idDetalhe) : null;

        if (!detalhe)
            return;

        const aberto = alvo.getAttribute("aria-expanded") === "true";

        alvo.setAttribute("aria-expanded", String(!aberto));
        alvo.classList.toggle("aberto", !aberto);
        detalhe.hidden = aberto;

    });

});