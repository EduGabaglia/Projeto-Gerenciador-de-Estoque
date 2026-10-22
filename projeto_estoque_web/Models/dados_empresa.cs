using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class DadosEmpresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da empresa.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe um CNPJ válido.")]
        [Display(Name = "CNPJ")]
        [RegularExpression(@"^(\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}|\d{14})$", ErrorMessage = "Informe um CNPJ válido, ex.: 00.000.000/0000-00.")]
        public string? Cnpj { get; set; }

        [Required(ErrorMessage = "Informe o CEP.")]
        [Display(Name = "CEP")]
        [RegularExpression(@"^(\d{5}-\d{3}|\d{8})$", ErrorMessage = "Informe um CEP válido, ex.: 00000-000.")]
        public string? Cep { get; set; }

        [Required(ErrorMessage = "Informe o endereço.")]
        [Display(Name = "Endereço")]
        [StringLength(150, ErrorMessage = "O endereço deve ter no máximo 150 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a cidade.")]
        [Display(Name = "Cidade")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a UF.")]
        [Display(Name = "UF")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 letras.")]
        public string? Uf { get; set; }

        [Required(ErrorMessage = "Informe o telefone.")]
        [RegularExpression(@"^(\(\d{2}\)\s?\d{4,5}-\d{4}|\d{10,11})$", ErrorMessage = "Informe um telefone válido, ex.: (11) 99999-9999.")]
        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail da empresa.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
        public string Email { get; set; } = string.Empty;
    }
}
