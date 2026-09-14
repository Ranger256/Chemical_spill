using System;
using System.Collections.Generic;

namespace ChemicalSpill.Models
{
    /// <summary>
    /// Запись журнала. Источник: «Перечень параметров», п. 9 и «Функциональный интерфейс», п. 3.1.
    /// Изменения уставок записываются с прежним и новым значением, временем и именем оператора.
    /// </summary>
    public class JournalRecord
    {
        public DateTime Time { get; set; }
        public JournalCategory Category { get; set; }

        /// <summary>Наименование события или параметра.</summary>
        public string Event { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string OperatorName { get; set; }

        /// <summary>Состояние установки на момент записи.</summary>
        public MachineState MachineState { get; set; }

        /// <summary>Номер партии, к которой относится запись.</summary>
        public string BatchNumber { get; set; }
    }

    /// <summary>Точка графика: значение с меткой времени.</summary>
    public struct TrendPoint
    {
        public DateTime Time;
        public double Value;

        public TrendPoint(DateTime time, double value)
        {
            Time = time;
            Value = value;
        }
    }

    /// <summary>
    /// Кривая графика с линиями аварийных уставок.
    /// Источник: «Перечень параметров», п. 2.1.
    /// </summary>
    public class TrendSeries
    {
        /// <summary>Ключ параметра, совпадает с ParameterSnapshot.Key.</summary>
        public string Key { get; set; }

        public string Caption { get; set; }
        public string Unit { get; set; }

        public List<TrendPoint> Points { get; set; }

        /// <summary>Верхняя аварийная уставка, отображается линией на графике.</summary>
        public double? HighLimit { get; set; }

        /// <summary>Нижняя аварийная уставка, отображается линией на графике.</summary>
        public double? LowLimit { get; set; }

        /// <summary>Границы шкалы; если не заданы, шкала подбирается по данным.</summary>
        public double? ScaleMinimum { get; set; }
        public double? ScaleMaximum { get; set; }

        public TrendSeries()
        {
            Points = new List<TrendPoint>();
        }
    }

    /// <summary>
    /// Состав отчёта о партии. Источник: «Перечень параметров», п. 9.
    /// </summary>
    public class BatchReport
    {
        public BatchInfo Batch { get; set; }
        public RecipeSummary Recipe { get; set; }
        public RotorInfo Rotor { get; set; }

        /// <summary>Сводка по параметрам атмосферы за время партии.</summary>
        public List<ParameterSnapshot> AtmosphereSummary { get; set; }

        /// <summary>Перечень возникших аварий.</summary>
        public List<AlarmRecord> Alarms { get; set; }

        public BatchReport()
        {
            AtmosphereSummary = new List<ParameterSnapshot>();
            Alarms = new List<AlarmRecord>();
        }
    }
}
