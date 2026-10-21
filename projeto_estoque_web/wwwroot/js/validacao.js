(function () {

    if (!window.jQuery || !$.validator)
        return;

    function valorNormalizado(value) {

        if (typeof value !== "string")
            return value;

        return value.replace(",", ".");
    }

    $.validator.methods.number = function (value, element) {

        value = valorNormalizado(value);

        return this.optional(element) ||
            !isNaN(parseFloat(value)) && isFinite(value);
    };

    $.validator.methods.range = function (value, element, param) {

        value = parseFloat(valorNormalizado(value));

        return this.optional(element) ||
            (param[0] === null || value >= parseFloat(param[0])) &&
            (param[1] === null || value <= parseFloat(param[1]));
    };

    $.extend($.validator.messages, {
        required: "Este campo é obrigatório.",
        remote: "Por favor, corrija este campo.",
        email: "Informe um endereço de e-mail válido.",
        url: "Informe uma URL válida.",
        date: "Informe uma data válida.",
        dateISO: "Informe uma data válida (ISO).",
        number: "Informe um número válido.",
        digits: "Informe apenas dígitos.",
        creditcard: "Informe um número de cartão de crédito válido.",
        equalTo: "Informe o mesmo valor novamente.",
        maxlength: $.validator.format("Por favor, não digite mais do que {0} caracteres."),
        minlength: $.validator.format("Por favor, digite ao menos {0} caracteres."),
        rangelength: $.validator.format("Por favor, digite entre {0} e {1} caracteres."),
        range: $.validator.format("Por favor, digite um valor entre {0} e {1}."),
        max: $.validator.format("Por favor, digite um valor menor ou igual a {0}."),
        min: $.validator.format("Por favor, digite um valor maior ou igual a {0}."),
        step: $.validator.format("Por favor, digite um valor com passo {0}."),
        valid: "Informe um valor válido."
    });

})();