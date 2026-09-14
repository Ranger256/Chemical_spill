using System;
using System.Collections.Generic;

namespace ChemicalSpill.Models
{
    /// <summary>
    /// Снимок состояния одной станции (блока мнемосхемы).
    /// Источник: «Функциональный интерфейс», п. 2.3 и 2.4.
    /// </summary>
    public class StageSnapshot
    {
        public StageId Id { get; set; }
        public StageState State { get; set; }

        /// <summary>Наименование ключевого параметра стадии, например «Доза».</summary>
        public string KeyCaption { get; set; }

        /// <summary>Значение ключевого параметра в реальном времени, например «20,06 мл (+0,30 %)».</summary>
        public string KeyValue { get; set; }

        /// <summary>Время выполнения текущей операции, с.</summary>
        public double ElapsedSeconds { get; set; }

        /// <summary>Заданный таймаут стадии, с.</summary>
        public double TimeoutSeconds { get; set; }

        /// <summary>Номер гнезда ротора, обрабатываемого станцией.</summary>
        public int NestNumber { get; set; }

        /// <summary>Счётчик обработанных флаконов по данной станции.</summary>
        public int ProcessedCount { get; set; }

        /// <summary>Счётчик отказов по данной станции.</summary>
        public int FaultCount { get; set; }

        /// <summary>Причина отказа, выводится текстом рядом с блоком.</summary>
        public string FaultReason { get; set; }
    }

    /// <summary>
    /// Значение параметра, выводимого оператору.
    /// Источник: «Перечень параметров, выводимых оператору».
    /// </summary>
    public class ParameterSnapshot
    {
        /// <summary>Неизменяемый ключ параметра, по которому его находит ядро.</summary>
        public string Key { get; set; }

        /// <summary>Группа из документа: «Атмосфера», «Ротор», «Дозирование» и т. д.</summary>
        public string Group { get; set; }

        public string Caption { get; set; }
        public string Unit { get; set; }

        /// <summary>Числовое значение, если параметр числовой.</summary>
        public double? Value { get; set; }

        /// <summary>Готовое к выводу представление значения (учитывает разрешение).</summary>
        public string ValueText { get; set; }

        /// <summary>Диапазон измерения или допустимых значений, например «0–1000».</summary>
        public string RangeText { get; set; }

        /// <summary>Текст действующей уставки или порога, например «порог 10 ppm».</summary>
        public string LimitText { get; set; }

        public ParameterKind Kind { get; set; }
        public ValueStatus Status { get; set; }

        /// <summary>Пояснение «что это и для чего» из документа, выводится подсказкой.</summary>
        public string Description { get; set; }

        public double? LowLimit { get; set; }
        public double? HighLimit { get; set; }
    }

    /// <summary>Разрешающее условие пуска. Источник: «Функциональный интерфейс», п. 4.3.</summary>
    public class StartCondition
    {
        public string Text { get; set; }
        public bool Fulfilled { get; set; }

        /// <summary>Фактическое значение или уточнение причины невыполнения.</summary>
        public string Detail { get; set; }
    }

    /// <summary>Действующее отклонение. Источник: «Перечень параметров», п. 7.</summary>
    public class AlarmRecord
    {
        public DateTime Time { get; set; }
        public AlarmSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }

        /// <summary>Узел-источник: станция, датчик, подсистема.</summary>
        public string Source { get; set; }

        public bool Acknowledged { get; set; }
    }

    /// <summary>Данные задания на партию. В рецепт не входят (п. 5.1).</summary>
    public class BatchInfo
    {
        public string Number { get; set; }
        public string Product { get; set; }
        public string OperatorName { get; set; }

        /// <summary>Задание на партию, шт.</summary>
        public int Target { get; set; }

        /// <summary>Счётчик годных флаконов.</summary>
        public int Good { get; set; }

        /// <summary>Счётчик брака по причинам.</summary>
        public Dictionary<RejectReason, int> Rejects { get; set; }

        public DateTime StartedAt { get; set; }
        public TimeSpan Elapsed { get; set; }

        /// <summary>Оценка времени до завершения, расчётная величина.</summary>
        public TimeSpan? Remaining { get; set; }

        /// <summary>Фактическая производительность, шт/мин.</summary>
        public double Throughput { get; set; }

        public BatchInfo()
        {
            Rejects = new Dictionary<RejectReason, int>();
        }

        /// <summary>Общее число забракованных флаконов.</summary>
        public int RejectTotal
        {
            get
            {
                int sum = 0;
                foreach (var pair in Rejects) sum += pair.Value;
                return sum;
            }
        }
    }

    /// <summary>Оснастка и текущее положение ротора. Источник: «Перечень параметров», п. 3.</summary>
    public class RotorInfo
    {
        public string RotorType { get; set; }
        public int NestCount { get; set; }
        public int CurrentPosition { get; set; }
        public double VialSizeMl { get; set; }

        /// <summary>Количество пустых флаконов во входном накопителе.</summary>
        public int InputMagazine { get; set; }

        /// <summary>Заполненность приёмника годной продукции.</summary>
        public int GoodBin { get; set; }

        /// <summary>Заполненность приёмника негодной продукции.</summary>
        public int RejectBin { get; set; }

        /// <summary>Остаток крышек в магазине укупорщика.</summary>
        public int CapMagazine { get; set; }
    }

    /// <summary>Строка консоли событий главного экрана.</summary>
    public class ConsoleRecord
    {
        public DateTime Time { get; set; }
        public ConsoleLineKind Kind { get; set; }
        public string Text { get; set; }
    }
}
