using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using APICotacoes.Models;

namespace APICotacoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CotacoesController : ControllerBase
    {
        private static readonly Contador _CONTADOR = new Contador();
        private readonly ILogger<CotacoesController> _logger;

        public CotacoesController(ILogger<CotacoesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public object Get()
        {
            return new
            {
                NumeroMensagensEnviadas = _CONTADOR.ValorAtual
            };
        }

        [HttpPost]
        public object Post(
            [FromServices] IConfiguration config,
            CotacaoMoeda cotacao)
        {
            var conteudoCotacao = JsonSerializer.Serialize(cotacao);
            _logger.LogInformation($"Dados: {conteudoCotacao}");

            var body = Encoding.UTF8.GetBytes(conteudoCotacao);

            string topic = config["AzureServiceBus:Topic"];
            var client = new TopicClient(
                config["AzureServiceBus:ConnectionString"], topic);
            client.SendAsync(new Message(body)).Wait();
            _logger.LogInformation(
                $"Azure Service Bus - Envio para o tópico {topic} concluído");

            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();
            }

            return new
            {
                Resultado = "Mensagem enviada com sucesso!"
            };
        }
    }
}