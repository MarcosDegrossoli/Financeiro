using Financeiro.Data;
using Financeiro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Financeiro.Controllers
{
    public class LancamentosController : Controller
    {
        private readonly OfxDbContext _context;

        public LancamentosController(OfxDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Listar(DateTime? dataInicial, DateTime? dataFinal)
        {
            var query = _context.Lancamentos
                .Include(l => l.Conta)
                    .ThenInclude(c => c.Banco)
                .AsQueryable();

            if (dataInicial.HasValue)
            {
                query = query.Where(l => l.DataPostagem >= dataInicial.Value.ToUniversalTime());
                ViewBag.DataInicial = dataInicial.Value.ToString("yyyy-MM-dd");
            }

            if (dataFinal.HasValue)
            {
                query = query.Where(l => l.DataPostagem <= dataFinal.Value.ToUniversalTime().AddDays(1).AddSeconds(-1));
                ViewBag.DataFinal = dataFinal.Value.ToString("yyyy-MM-dd");
            }

            var lancamentos = await query
                .OrderByDescending(l => l.DataPostagem)
                .ToListAsync();

            return View(lancamentos);
        }
    }
}