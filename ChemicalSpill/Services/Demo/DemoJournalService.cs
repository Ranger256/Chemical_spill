using System;
using System.Collections.Generic;
using System.Text;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>Демонстрационная реализация <see cref="IJournalService"/>.</summary>
    public class DemoJournalService : IJournalService
    {
        private readonly List<JournalRecord> _records = new List<JournalRecord>();
        private readonly IMachineService _machine;

        public DemoJournalService(IMachineService machine)
        {
            _machine = machine;

            var t = DateTime.Now.AddMinutes(-20);

            Add(t, JournalCategory.Link, "Соединение с ПК установлено", null, "WS-TECH-04 (192.168.10.25)", MachineState.Stopped);
            Add(t.AddMinutes(1), JournalCategory.Recipe, "Загружен рецепт «Ацетонитрил 20 мл»", null, "версия 3, к/сумма 8F3A-21C4", MachineState.Stopped);
            Add(t.AddMinutes(1.5), JournalCategory.OperatorAction, "Введено задание на партию", "—", "П-2026-0914-03, 500 шт", MachineState.Stopped);
            Add(t.AddMinutes(2), JournalCategory.SetpointChange, "Точка перехода фаз", "85 % дозы", "88 % дозы", MachineState.Stopped);
            Add(t.AddMinutes(2.4), JournalCategory.SetpointChange, "Время стабилизации весов", "1,0 с", "1,2 с", MachineState.Stopped);
            Add(t.AddMinutes(3), JournalCategory.OperatorAction, "Нажата кнопка «Пуск»", null, "режим: Автомат", MachineState.Ready);
            Add(t.AddMinutes(3.1), JournalCategory.Parameter, "Содержание O₂", "4,1 ppm", "3,2 ppm", MachineState.Running);
            Add(t.AddMinutes(9), JournalCategory.Alarm, "W-108 Отклонение дозы приблизилось к границе допуска", null, "0,78 %", MachineState.Running);
            Add(t.AddMinutes(10), JournalCategory.Parameter, "Флакон 64 отбракован", null, "отклонение дозы +1,12 %", MachineState.Running);
            Add(t.AddMinutes(12), JournalCategory.Parameter, "Флакон 81 отбракован", null, "момент затяжки не достигнут за 3 попытки", MachineState.Running);
            Add(t.AddMinutes(16), JournalCategory.Alarm, "W-214 Остаток крышек в магазине ниже 500 шт", null, "412 шт", MachineState.Running);
            Add(t.AddMinutes(18), JournalCategory.SetpointChange, "Скорость налива, основная фаза", "15,0 мл/с", "14,0 мл/с", MachineState.Running);
        }

        public int MemoryUsedPercent { get { return 42; } }

        public event EventHandler<JournalRecord> RecordAdded;

        public IReadOnlyList<JournalRecord> Query(DateTime from, DateTime to, JournalCategory? category, string search)
        {
            var result = new List<JournalRecord>();
            foreach (var record in _records)
            {
                if (record.Time < from || record.Time > to) continue;
                if (category.HasValue && record.Category != category.Value) continue;

                if (!string.IsNullOrEmpty(search))
                {
                    var haystack = (record.Event ?? string.Empty) + " " + (record.NewValue ?? string.Empty) +
                                   " " + (record.OldValue ?? string.Empty);
                    if (haystack.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) < 0) continue;
                }

                result.Add(record);
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Отчёт о партии. Состав определён п. 9 перечня параметров: идентификация
        /// партии и продукта, оснастка, уставки, статистика по флаконам, сводка по
        /// атмосфере и перечень аварий.
        /// </summary>
        public BatchReport BuildReport(string batchNumber)
        {
            var report = new BatchReport
            {
                Batch = _machine.Batch,
                Recipe = _machine.LoadedRecipe,
                Rotor = _machine.Rotor
            };

            foreach (var parameter in _machine.GetParameters("Атмосфера рабочей камеры"))
            {
                report.AtmosphereSummary.Add(parameter);
            }

            foreach (var alarm in _machine.GetActiveAlarms())
            {
                report.Alarms.Add(alarm);
            }

            return report;
        }

        public IReadOnlyList<string> GetBatchNumbers()
        {
            return new List<string> { "П-2026-0914-03", "П-2026-0914-02", "П-2026-0913-07", "П-2026-0913-06" };
        }

        public void Export(IEnumerable<JournalRecord> records, string path)
        {
            var lines = new List<string>
            {
                DemoExport.Row("Время", "Категория", "Событие", "Прежнее значение",
                    "Новое значение", "Оператор", "Состояние установки", "Партия")
            };

            foreach (var record in records)
            {
                lines.Add(DemoExport.Row(
                    record.Time.ToString("dd.MM.yyyy HH:mm:ss"),
                    DisplayNames.Of(record.Category),
                    record.Event,
                    record.OldValue,
                    record.NewValue,
                    record.OperatorName,
                    DisplayNames.Of(record.MachineState),
                    record.BatchNumber));
            }

            DemoExport.Write(path, lines);
        }

        public void ExportReport(BatchReport report, string path)
        {
            var text = new StringBuilder();

            text.AppendLine("ОТЧЁТ О ПАРТИИ");
            text.AppendLine("Сформирован: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            text.AppendLine();

            text.AppendLine("1. Идентификация партии и продукта");
            if (report.Batch != null)
            {
                text.AppendLine("   Номер партии:        " + report.Batch.Number);
                text.AppendLine("   Продукт:             " + report.Batch.Product);
                text.AppendLine("   Оператор:            " + report.Batch.OperatorName);
                text.AppendLine("   Начало:              " + report.Batch.StartedAt.ToString("dd.MM.yyyy HH:mm:ss"));
                text.AppendLine("   Наработка:           " + report.Batch.Elapsed.ToString(@"hh\:mm\:ss"));
            }
            text.AppendLine();

            text.AppendLine("2. Оснастка и типоразмер тары");
            if (report.Rotor != null)
            {
                text.AppendLine("   Тип ротора:          " + report.Rotor.RotorType);
                text.AppendLine("   Число гнёзд:         " + report.Rotor.NestCount);
                text.AppendLine("   Типоразмер флакона:  " + report.Rotor.VialSizeMl.ToString("0") + " мл");
            }
            text.AppendLine();

            text.AppendLine("3. Рецепт");
            if (report.Recipe != null)
            {
                text.AppendLine("   Наименование:        " + report.Recipe.Name);
                text.AppendLine("   Версия:              " + report.Recipe.Version);
                text.AppendLine("   Состояние:           " + DisplayNames.Of(report.Recipe.State));
                text.AppendLine("   Контрольная сумма:   " + report.Recipe.Checksum);
                text.AppendLine("   Доза:                " + report.Recipe.DoseMl.ToString("0.00") + " мл");
            }
            text.AppendLine();

            text.AppendLine("4. Статистика по флаконам");
            if (report.Batch != null)
            {
                text.AppendLine("   Задание на партию:   " + report.Batch.Target + " шт");
                text.AppendLine("   Годных:              " + report.Batch.Good + " шт");
                text.AppendLine("   Забраковано:         " + report.Batch.RejectTotal + " шт");

                foreach (var pair in report.Batch.Rejects)
                {
                    text.AppendLine("      " + DisplayNames.Of(pair.Key).PadRight(20) + pair.Value + " шт");
                }

                text.AppendLine("   Производительность:  " + report.Batch.Throughput.ToString("0.0") + " шт/мин");
            }
            text.AppendLine();

            text.AppendLine("5. Сводка по параметрам атмосферы");
            foreach (var parameter in report.AtmosphereSummary)
            {
                var unit = string.IsNullOrEmpty(parameter.Unit) ? string.Empty : " " + parameter.Unit;
                text.AppendLine("   " + parameter.Caption + ": " + parameter.ValueText + unit +
                                (string.IsNullOrEmpty(parameter.LimitText) ? string.Empty : " (" + parameter.LimitText + ")"));
            }
            text.AppendLine();

            text.AppendLine("6. Перечень возникших аварий и предупреждений");
            if (report.Alarms.Count == 0)
            {
                text.AppendLine("   Отклонений не зарегистрировано.");
            }
            else
            {
                foreach (var alarm in report.Alarms)
                {
                    text.AppendLine("   " + alarm.Time.ToString("dd.MM.yyyy HH:mm:ss") + "  " +
                                    DisplayNames.Of(alarm.Severity) + " " + alarm.Code + ": " + alarm.Text +
                                    " (" + alarm.Source + ")");
                }
            }

            DemoExport.WriteText(path, text.ToString());
        }

        private void Add(DateTime time, JournalCategory category, string name, string oldValue, string newValue, MachineState state)
        {
            var record = new JournalRecord
            {
                Time = time,
                Category = category,
                Event = name,
                OldValue = oldValue,
                NewValue = newValue,
                OperatorName = "Иванов И. И.",
                MachineState = state,
                BatchNumber = "П-2026-0914-03"
            };

            _records.Add(record);
            var handler = RecordAdded;
            if (handler != null) handler(this, record);
        }
    }
}
