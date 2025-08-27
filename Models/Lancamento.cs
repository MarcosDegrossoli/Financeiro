using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financeiro.Models
{
    [Index(nameof(IdUnicoTransacao), IsUnique = true)]
    public class Lancamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TipoTransacao { get; set; }

        public DateTime DataPostagem { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Valor { get; set; }

        [Required]
        [MaxLength(255)]
        public string IdUnicoTransacao { get; set; }

        [MaxLength(500)]
        public string Memo { get; set; }

        public int ContaId { get; set; }
        
        public Conta Conta { get; set; }
    }
}
