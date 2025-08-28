using Financeiro.Data;
using Financeiro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OFXSharp;
using System.Text;

namespace Financeiro.Controllers
{
    public class ImportacaoController : Controller
    {
        private readonly OfxDbContext _context;

        public ImportacaoController(OfxDbContext context)
        {
            _context = context;
        }

        // Action para exibir o formulário de upload
        public IActionResult Index()
        {
            return View();
        }

        // Action para processar o arquivo enviado
        [HttpPost]
        public async Task<IActionResult> Importar(IFormFile arquivo)
        {
            // Verifique se o arquivo foi enviado
            if (arquivo == null || arquivo.Length == 0)
            {
                ViewBag.Mensagem = "Por favor, selecione um arquivo para importar.";
                return View("Index");
            }

            try
            {
                var parser = new OFXDocumentParser();

                var conteudoOfx = string.Empty;
                using (var reader = new StreamReader(arquivo.OpenReadStream(), Encoding.UTF8))
                {
                    conteudoOfx = await reader.ReadToEndAsync();
                }

                var ofx = parser.Import(conteudoOfx);

                var extrato = ofx.Transactions;

                if (extrato == null || extrato.Count == 0)
                {
                    ViewBag.Mensagem = "O arquivo não contém transações válidas.";
                    return View("Index");
                }

                Banco? banco = await ObterBanco(ofx);

                Conta? conta = await ObterConta(ofx, banco);

                List<Lancamento> novosLancamentos = await ObterNovosLançamentos(extrato, conta);

                var mensagem = "Nenhum novo lançamento foi importado";

                if (novosLancamentos.Count() > 0) mensagem = $"Arquivo importado com sucesso! {novosLancamentos.Count} lançamentos importados!";

                ViewBag.Mensagem = mensagem;
                return View("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Exibir(IFormFile arquivo)
        {
            var lancamentos = await _context.Lancamentos.Select(l => new
            {
                l.Id,
                l.TipoTransacao,
                l.DataPostagem,
                l.Valor,
                l.IdUnicoTransacao,
                l.Memo,
                l.ContaId
            }).ToListAsync();

            return Json(lancamentos);
        }

        public async Task<IActionResult> Listar()
        {
            var lancamentos = await _context.Lancamentos
                .Include(l => l.Conta)
                    .ThenInclude(c => c.Banco)
                .OrderByDescending(l => l.DataPostagem)
                .ToListAsync();

            return View(lancamentos);
        }

        private async Task<List<Lancamento>> ObterNovosLançamentos(List<Transaction> extrato, Conta? conta)
        {
            var novosLancamentos = new List<Lancamento>();

            var idsDoOfx = extrato.Select(t => t.TransactionID).ToList();

            // 5. Consulte o banco de dados uma única vez para encontrar os IDs que já existem
            var idsExistentes = await _context.Lancamentos
                .Where(l => idsDoOfx.Contains(l.IdUnicoTransacao))
                .Select(l => l.IdUnicoTransacao)
                .ToListAsync();

            var novosIds = idsDoOfx.Except(idsExistentes).ToList();

            if (novosIds.Count != 0)
            {
                foreach (var item in extrato)
                {
                    if (novosIds.Contains(item.TransactionID))
                    {
                        var lancamento = new Lancamento
                        {
                            TipoTransacao = item.TransType.ToString(),
                            DataPostagem = item.Date.ToUniversalTime(),
                            Valor = item.Amount,
                            IdUnicoTransacao = item.TransactionID,
                            Memo = item.Memo,
                            ContaId = conta.Id
                        };

                        novosLancamentos.Add(lancamento);
                    }
                }
            }

            if (novosLancamentos.Count > 0)
            {
                _context.Lancamentos.AddRange(novosLancamentos);
                await _context.SaveChangesAsync();
            }

            return novosLancamentos;
        }

        private async Task<Conta?> ObterConta(OFXDocument ofx, Banco? banco)
        {
            var conta = await _context.Contas
                                .Include(c => c.Banco)
                                .FirstOrDefaultAsync(c => c.NumeroConta == ofx.Account.AccountID && c.BancoId == banco.Id);

            if (conta == null)
            {
                conta = new Conta
                {
                    NumeroConta = ofx.Account.AccountID,
                    TipoConta = ofx.Account.AccountType.ToString(),
                    Moeda = ofx.Currency,
                    BancoId = banco.Id
                };
                _context.Contas.Add(conta);
                await _context.SaveChangesAsync();
            }

            return conta;
        }

        private async Task<Banco?> ObterBanco(OFXDocument ofx)
        {
            var banco = await _context.Bancos.FirstOrDefaultAsync(b => b.Codigo == ofx.Account.BankID);

            if (banco == null)
            {
                banco = new Banco
                {
                    Codigo = ofx.Account.BankID,
                    Nome = "Nome do Banco Desconhecido"
                };
                _context.Bancos.Add(banco);
                await _context.SaveChangesAsync();
            }

            return banco;
        }
    }
}