using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>Демонстрационная реализация <see cref="ITrendService"/>: правдоподобные кривые.</summary>
    public class DemoTrendService : ITrendService
    {
        private readonly Random _random = new Random(4242);

        public event EventHandler SeriesUpdated;

        public IReadOnlyList<TrendSeries> GetSeries(IEnumerable<string> parameterKeys, TrendWindow window)
        {
            var minutes = window == TrendWindow.LastTenMinutes ? 10 : window == TrendWindow.LastHour ? 60 : 18;
            var result = new List<TrendSeries>();

            foreach (var key in parameterKeys)
            {
                result.Add(Build(key, minutes));
            }

            return result;
        }

        /// <summary>
        /// Выгрузка данных графиков в файл. Данные входят в отчёт о партии (п. 2.1).
        /// </summary>
        public void Export(IEnumerable<string> parameterKeys, TrendWindow window, string path)
        {
            var series = GetSeries(parameterKeys, window);
            var lines = new List<string>();

            // Заголовок: время и по столбцу на каждую кривую с единицей измерения
            var header = new List<string> { "Время" };
            foreach (var item in series)
            {
                header.Add(string.IsNullOrEmpty(item.Unit) ? item.Caption : item.Caption + ", " + item.Unit);
            }
            lines.Add(DemoExport.Row(header.ToArray()));

            int count = 0;
            foreach (var item in series)
            {
                if (item.Points.Count > count) count = item.Points.Count;
            }

            for (int i = 0; i < count; i++)
            {
                var row = new List<string>();
                row.Add(series.Count > 0 && i < series[0].Points.Count
                    ? series[0].Points[i].Time.ToString("dd.MM.yyyy HH:mm:ss")
                    : string.Empty);

                foreach (var item in series)
                {
                    row.Add(i < item.Points.Count ? item.Points[i].Value.ToString("0.###") : string.Empty);
                }

                lines.Add(DemoExport.Row(row.ToArray()));
            }

            // Аварийные уставки выводятся отдельной справкой под таблицей
            lines.Add(string.Empty);
            lines.Add(DemoExport.Row("Параметр", "Нижняя граница", "Верхняя граница"));
            foreach (var item in series)
            {
                lines.Add(DemoExport.Row(
                    item.Caption,
                    item.LowLimit.HasValue ? item.LowLimit.Value.ToString("0.###") : "—",
                    item.HighLimit.HasValue ? item.HighLimit.Value.ToString("0.###") : "—"));
            }

            DemoExport.Write(path, lines);
        }

        private TrendSeries Build(string key, int minutes)
        {
            var series = new TrendSeries { Key = key };
            double baseValue;
            double amplitude;
            double drift;

            switch (key)
            {
                case "o2":
                    series.Caption = "Содержание кислорода O₂";
                    series.Unit = "ppm";
                    series.HighLimit = 10;
                    series.ScaleMinimum = 0;
                    series.ScaleMaximum = 12;
                    baseValue = 3.4; amplitude = 0.35; drift = -0.4;
                    break;
                case "h2o":
                    series.Caption = "Содержание влаги H₂O";
                    series.Unit = "ppm";
                    series.HighLimit = 10;
                    series.ScaleMinimum = 0;
                    series.ScaleMaximum = 12;
                    baseValue = 2.4; amplitude = 0.28; drift = -0.5;
                    break;
                case "pressure":
                    series.Caption = "Избыточное давление в камере";
                    series.Unit = "мбар";
                    series.HighLimit = 15;
                    series.LowLimit = -5;
                    series.ScaleMinimum = -6;
                    series.ScaleMaximum = 16;
                    baseValue = 4.1; amplitude = 0.5; drift = 0.1;
                    break;
                case "productTemp":
                    series.Caption = "Температура продукта";
                    series.Unit = "°C";
                    series.ScaleMinimum = 18;
                    series.ScaleMaximum = 26;
                    baseValue = 23.4; amplitude = 0.12; drift = -1.5;
                    break;
                case "chamberTemp":
                    series.Caption = "Температура в камере";
                    series.Unit = "°C";
                    series.ScaleMinimum = 18;
                    series.ScaleMaximum = 26;
                    baseValue = 23.2; amplitude = 0.1; drift = 0.2;
                    break;
                case "vapour":
                    series.Caption = "Содержание паров растворителя";
                    series.Unit = "ppm";
                    series.HighLimit = 200;
                    series.ScaleMinimum = 0;
                    series.ScaleMaximum = 220;
                    baseValue = 44; amplitude = 6; drift = 18;
                    break;
                case "doseDeviation":
                    series.Caption = "Отклонение дозы";
                    series.Unit = "%";
                    series.HighLimit = 1;
                    series.LowLimit = -1;
                    series.ScaleMinimum = -1.4;
                    series.ScaleMaximum = 1.4;
                    baseValue = 0.2; amplitude = 0.35; drift = 0.1;
                    break;
                case "throughput":
                    series.Caption = "Фактическая производительность";
                    series.Unit = "шт/мин";
                    series.LowLimit = 6;
                    series.ScaleMinimum = 0;
                    series.ScaleMaximum = 10;
                    baseValue = 7.3; amplitude = 0.3; drift = -0.2;
                    break;
                default:
                    series.Caption = key;
                    series.Unit = string.Empty;
                    baseValue = 50; amplitude = 3; drift = 0;
                    break;
            }

            var now = DateTime.Now;
            const int count = 120;

            for (int i = 0; i < count; i++)
            {
                double progress = (double)i / (count - 1);
                double value = baseValue + drift * progress + (_random.NextDouble() - 0.5) * 2 * amplitude;
                var time = now.AddMinutes(-minutes * (1 - progress));
                series.Points.Add(new TrendPoint(time, value));
            }

            return series;
        }
    }
}
