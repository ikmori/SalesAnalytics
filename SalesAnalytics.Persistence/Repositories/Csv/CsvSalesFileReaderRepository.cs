// En SalesAnalytics.Persistence/Repositories/Csv/
using CsvHelper;
using System.Globalization;
using SalesAnalytics.Domain.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

public class CsvSalesFileReaderRepository<TClass> : IFileReaderRepository<TClass> where TClass : class
{
    private readonly ILogger<CsvSalesFileReaderRepository<TClass>> _logger;

    public CsvSalesFileReaderRepository(ILogger<CsvSalesFileReaderRepository<TClass>> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<TClass>> ReadFileAsync(string filePath)
    {
        _logger.LogInformation("Iniciando lectura de archivo CSV: {FilePath}", filePath);
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("El archivo especificado no existe: {FilePath}", filePath);
                return Enumerable.Empty<TClass>();
            }

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<TClass>();
                await foreach (var record in csv.GetRecordsAsync<TClass>())
                {
                    records.Add(record);
                }
                _logger.LogInformation("Lectura de {Count} registros completada.", records.Count);
                return records;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al leer el archivo CSV: {FilePath}", filePath);
            return Enumerable.Empty<TClass>();
        }
    }
}