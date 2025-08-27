using Financeiro.Models;
using System.ComponentModel.DataAnnotations;

public class Conta
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NumeroConta { get; set; }

    [Required]
    [MaxLength(50)]
    public string TipoConta { get; set; }

    [Required]
    [MaxLength(10)]
    public string Moeda { get; set; }

    public int BancoId { get; set; }
    
    public Banco Banco { get; set; }

    public ICollection<Lancamento> Lancamentos { get; set; }
}