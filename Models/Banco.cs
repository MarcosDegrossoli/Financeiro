using System.ComponentModel.DataAnnotations;

public class Banco
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; }

    public ICollection<Conta> Contas { get; set; }
}