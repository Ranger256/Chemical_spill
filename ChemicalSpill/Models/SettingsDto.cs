using System.Collections.Generic;

namespace ChemicalSpill.Models
{
    /// <summary>
    /// Описание одной настройки цикла: диапазон, единица измерения, признак
    /// смены в работе и назначение. Источник: «Функциональный интерфейс», п. 3.2.
    /// </summary>
    public class SettingDefinition
    {
        public string Key { get; set; }

        /// <summary>Раздел таблицы настроек: «Общие параметры цикла», «Стадия 2. Наполнение» и т. д.</summary>
        public string Group { get; set; }

        public string Caption { get; set; }
        public string Unit { get; set; }

        public double? Minimum { get; set; }
        public double? Maximum { get; set; }

        /// <summary>Значение по умолчанию, показываемое оператору вместе с диапазоном.</summary>
        public double? DefaultValue { get; set; }

        /// <summary>Смена в работе: true — изменяемая на ходу, false — требует останова цикла.</summary>
        public bool ChangeableWhileRunning { get; set; }

        /// <summary>Назначение настройки из документа, выводится пояснением.</summary>
        public string Purpose { get; set; }

        /// <summary>Варианты для настроек с перечислением (режим работы, правило сортировки).</summary>
        public List<string> Options { get; set; }

        /// <summary>Текущее значение в виде текста; числовые значения разбирает контроллер.</summary>
        public string Value { get; set; }

        public SettingDefinition()
        {
            Options = new List<string>();
        }
    }

    /// <summary>
    /// Результат расчёта времени такта. Источник: «Функциональный интерфейс», п. 3.3.
    /// Для роторной схемы такт определяется самой длительной стадией плюс индексация.
    /// </summary>
    public class CycleTimeEstimate
    {
        /// <summary>Длительность каждой стадии, с.</summary>
        public Dictionary<StageId, double> StageDurations { get; set; }

        /// <summary>Время индексации ротора, с.</summary>
        public double IndexingSeconds { get; set; }

        /// <summary>Время такта установки, с.</summary>
        public double TactSeconds { get; set; }

        /// <summary>Ожидаемая производительность, шт/мин.</summary>
        public double Throughput { get; set; }

        /// <summary>Требование технического задания, шт/мин (не менее 6).</summary>
        public double RequiredThroughput { get; set; }

        /// <summary>Стадия, ограничивающая производительность.</summary>
        public StageId LimitingStage { get; set; }

        public CycleTimeEstimate()
        {
            StageDurations = new Dictionary<StageId, double>();
            RequiredThroughput = 6.0;
        }

        public bool MeetsRequirement
        {
            get { return Throughput >= RequiredThroughput; }
        }
    }

    /// <summary>Результат одной проверки при сохранении или загрузке рецепта (п. 5.5).</summary>
    public class CheckResult
    {
        public string Text { get; set; }
        public bool Passed { get; set; }

        /// <summary>Пояснение, что именно не сошлось.</summary>
        public string Detail { get; set; }

        public CheckResult()
        {
        }

        public CheckResult(string text, bool passed, string detail)
        {
            Text = text;
            Passed = passed;
            Detail = detail;
        }
    }
}
