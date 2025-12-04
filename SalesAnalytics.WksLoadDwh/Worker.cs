using SalesAnalytics.Application.Interfaces;

namespace SalesAnalytics.WksLoadDwh
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker iniciado a las: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var etlService = scope.ServiceProvider.GetRequiredService<ISalesDataHandlerService>();

                    try
                    {
                        _logger.LogInformation(">>> INICIANDO CICLO ETL <<<");

                        var result = await etlService.ProcessSalesDataAsync();

                        if (result.IsSuccess)
                            _logger.LogInformation("ETL Exitoso: {Msg}", result.Message);
                        else
                            _logger.LogError("ETL Falló: {Msg}", result.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error crítico en el Worker");
                    }
                }

                var horas = _configuration.GetValue<int>("EtlSettings:IntervaloHoras", 24);
                _logger.LogInformation("Esperando {h} horas...", horas);
                await Task.Delay(TimeSpan.FromHours(horas), stoppingToken);
            }
        }
    }
}