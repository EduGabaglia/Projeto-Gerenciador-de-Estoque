(function () {
  "use strict";

  function soDigitos(v) {
    return (v || "").replace(/\D/g, "");
  }

  function formatCnpj(d) {
    d = d.slice(0, 14);
    if (d.length > 12)
      return d.replace(
        /^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/,
        "$1.$2.$3/$4-$5",
      );
    if (d.length > 8)
      return d.replace(/^(\d{2})(\d{3})(\d{3})(\d{1,4})$/, "$1.$2.$3/$4");
    if (d.length > 5) return d.replace(/^(\d{2})(\d{3})(\d{1,3})$/, "$1.$2.$3");
    if (d.length > 2) return d.replace(/^(\d{2})(\d{1,3})$/, "$1.$2");
    return d;
  }

  function formatCep(d) {
    d = d.slice(0, 8);
    if (d.length > 5) return d.replace(/^(\d{5})(\d{1,3})$/, "$1-$2");
    return d;
  }

  function formatTel(d) {
    d = d.slice(0, 11);
    if (d.length > 6)
      return d.replace(/^(\d{2})(\d{4,5})(\d{4})$/, "($1) $2-$3");
    if (d.length > 2) return d.replace(/^(\d{2})(\d{1,5})$/, "($1) $2");
    if (d.length > 0) return "(" + d;
    return "";
  }

  function bind(sel, fn) {
    var el = document.querySelector(sel);
    if (!el) return;
    function upd() {
      var d = soDigitos(el.value) || "";
      var f = fn(d) || "";
      if (el.value !== f) el.value = f;
    }
    el.addEventListener("input", upd);
    upd();
  }

  bind("#Dados_Cnpj", formatCnpj);
  bind("#Dados_Cep", formatCep);
  bind("#Dados_Telefone", formatTel);
})();
