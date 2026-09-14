using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>Демонстрационная реализация <see cref="IJournalService"/>.</summary>
    public class DemoJournalService : IJournalService
    {
        private readonly List<JournalRecord> _records = new List<JournalRecord>();

        public DemoJournalService()
        {
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

        public BatchReport BuildReport(string batchNumber)
        {
            return new BatchReport();
        }

        public IReadOnlyList<string> GetBatchNumbers()
        {
            return new List<string> { "П-2026-0914-03", "П-2026-0914-02", "П-2026-0913-07", "П-2026-0913-06" };
        }

        public void Export(IEnumerable<JournalRecord> records, string path)
        {
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
