using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Assets._Scripts.Helpers
{
    internal static class CsvSerializer
    {
        private static CsvConfiguration _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            NewLine = Environment.NewLine,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            TrimOptions = TrimOptions.Trim
        };

        public static T[] Deserialize<T>(string csvFile)
        {
            string normalizedCsv = csvFile;

#if PLATFORM_MOBILE && !UNITY_EDITOR
            normalizedCsv = csvFile.Replace("\r\n", "\n").Replace("\r", "\n");
#endif
            using var stringReader = new StringReader(normalizedCsv);
            using var csvReader = new CsvReader(stringReader, _config);
            //convert null values to empty string to match legacy system
            csvReader.Context.TypeConverterOptionsCache.AddOptions<string>(new TypeConverterOptions {NullValues = { string.Empty }});
            var records = csvReader.GetRecords<T>();
            return records.ToArray();
        }
    }
}
