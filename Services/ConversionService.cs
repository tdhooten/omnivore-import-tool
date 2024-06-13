using CsvHelper;
using OmnivoreImportTool.Models;
using System.Globalization;

namespace OmnivoreImportTool.Services;

public class ConversionService
{
    public Stream ConvertFile(Stream stream, string inputFormat)
    {
        IEnumerable<OmnivoreModel> outputRecords = Enumerable.Empty<OmnivoreModel>();

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        IEnumerable<RaindropModel>? records = csvReader.GetRecords<RaindropModel>();

        outputRecords = inputFormat switch
        {
            "Raindrop" => ConvertRaindrop(records),
            _ => outputRecords
        };

        using var outputStream = new MemoryStream();
        using var writer = new StreamWriter(outputStream);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(outputRecords);
        csvWriter.Flush();

        return new MemoryStream(outputStream.ToArray());
    }

    private static IEnumerable<OmnivoreModel> ConvertRaindrop(IEnumerable<RaindropModel> records)
    {
        foreach (RaindropModel record in records)
        {
            var output = new OmnivoreModel
            {
                Url = record.Url
            };

            var createdUtc = new DateTimeOffset(DateTime.Parse(record.Created).ToUniversalTime());
            output.SavedAt = createdUtc.ToUnixTimeMilliseconds().ToString();

            if (!String.IsNullOrEmpty(record.Tags)) output.Labels = "[" + record.Tags + "]";

            if (record.Folder.Contains("archive", StringComparison.InvariantCultureIgnoreCase))
                output.State = "ARCHIVED";

            yield return output;
        }
    }
}
