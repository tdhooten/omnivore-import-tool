using CsvHelper;
using OmnivoreImportTool.Models;
using System.Globalization;

namespace OmnivoreImportTool.Services;

public class ConversionService
{
    public Stream ConvertRaindrop(Stream stream)
    {
        List<OmnivoreModel> outputRecords = new();

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csvReader.GetRecords<RaindropModel>();

        foreach (var record in records)
        {
            var output = new OmnivoreModel()
            {
                url = record.url,
            };

            var createdUtc = new DateTimeOffset(DateTime.Parse(record.created).ToUniversalTime());
            output.saved_at = createdUtc.ToUnixTimeMilliseconds().ToString();

            if (!String.IsNullOrEmpty(record.tags)) output.labels = "[" + record.tags + "]";

            if (record.folder.Contains("archive", StringComparison.InvariantCultureIgnoreCase)) output.state = "ARCHIVED";

            outputRecords.Add(output);
        }

        using var outputStream = new MemoryStream();
        using var writer = new StreamWriter(outputStream);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(outputRecords);
        csvWriter.Flush();

        return new MemoryStream(outputStream.ToArray());
    }
}
