using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using WebApp.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
namespace WebApp.Services
{
    public class CSVService
    {
        private static string InferTypeByMajority(List<string> values)
        {
            int dateCount = 0, decimalCount = 0, total = values.Count;

            foreach (var val in values)
            {
                if (DateTime.TryParse(val, out _))
                    dateCount++;
                else if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    decimalCount++;
            }

            if (dateCount == total)
                return "DateTime";
            else if (decimalCount == total)
                return "Decimal";
            else
                return "String";
        }

        public static async Task<List<FileMapping>> getRecommendedFileMapping(IFormFile file, IConfiguration configuration, string delimiter = ",") 
        {

            var recommendations = new List<FileMapping>();

            string cleanedCsv;
            using (var tempReader = new StreamReader(file.OpenReadStream()))
            {
                cleanedCsv = tempReader.ReadToEnd().Replace("\"", ""); // Reemplaza comillas
            }

            //using var reader = new StreamReader(file.OpenReadStream());
            using var csvReader = new StringReader(cleanedCsv);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = delimiter,
                Quote = '"',
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            };

            using var csv = new CsvReader(csvReader, config);

            await csv.ReadAsync();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            var rows = new List<Dictionary<string, string>>();
            int maxLines = Int32.Parse(configuration["ChunkSize"]);

            while (await csv.ReadAsync() && rows.Count < maxLines)
            {
                var row = new Dictionary<string, string>();
                foreach (var header in headers)
                {
                    row[header] = csv.GetField(header)?.Trim();
                }
                rows.Add(row);
            }

            // Contadores para numeración
            int dateTimeCount = 0;
            int decimalCount = 0;
            int stringCount = 0;

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                var values = rows.Select(r => r[header]).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
                var type = InferTypeByMajority(values);

                string auxName = type switch
                {
                    "DateTime" => $"AuxDateTime{++dateTimeCount}",
                    "Decimal" => $"AuxDecimal{++decimalCount}",
                    _ => $"AuxString{++stringCount}"
                };

                recommendations.Add(new FileMapping
                {
                    CsvColumnName = header,
                    AuxColumnName = auxName,
                    IndexCsvColumn = i.ToString(),
                });
            }

            return recommendations;
        }

    }
}
