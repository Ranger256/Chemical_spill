using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Запись файлов выгрузки. Формат CSV с разделителем «точка с запятой» и
    /// кодировкой UTF-8 с меткой порядка байтов: в таком виде файл открывается
    /// в Excel с русской локалью без дополнительных настроек.
    /// </summary>
    internal static class DemoExport
    {
        public const char Separator = ';';

        private static readonly Encoding FileEncoding = new UTF8Encoding(true);

        /// <summary>Экранирование значения для CSV.</summary>
        public static string Field(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            bool needsQuotes = value.IndexOf(Separator) >= 0
                               || value.IndexOf('"') >= 0
                               || value.IndexOf('\n') >= 0
                               || value.IndexOf('\r') >= 0;

            if (!needsQuotes) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Сборка строки CSV из значений.</summary>
        public static string Row(params string[] values)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) builder.Append(Separator);
                builder.Append(Field(values[i]));
            }
            return builder.ToString();
        }

        public static void Write(string path, IEnumerable<string> lines)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            File.WriteAllLines(path, lines, FileEncoding);
        }

        public static void WriteText(string path, string text)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            File.WriteAllText(path, text, FileEncoding);
        }

        public static IList<string> ReadLines(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8);
        }
    }
}
