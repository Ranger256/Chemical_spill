using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Журнал событий и отчёты о партиях.
    /// Источник: «Перечень параметров», п. 9; «Подключение к ПК», п. 6.3.
    /// </summary>
    public interface IJournalService
    {
        /// <summary>Выборка записей журнала по периоду, категории и строке поиска.</summary>
        IReadOnlyList<JournalRecord> Query(DateTime from, DateTime to, JournalCategory? category, string search);

        /// <summary>Формирование отчёта о партии.</summary>
        BatchReport BuildReport(string batchNumber);

        /// <summary>Перечень партий, доступных для отчёта.</summary>
        IReadOnlyList<string> GetBatchNumbers();

        /// <summary>Выгрузка выбранных записей журнала в файл.</summary>
        void Export(IEnumerable<JournalRecord> records, string path);

        /// <summary>Выгрузка отчёта о партии в файл.</summary>
        void ExportReport(BatchReport report, string path);

        /// <summary>
        /// Заполнение памяти журнала установки, %. При 80 % выводится предупреждение
        /// с указанием оставшегося запаса.
        /// </summary>
        int MemoryUsedPercent { get; }

        /// <summary>Добавлена новая запись.</summary>
        event EventHandler<JournalRecord> RecordAdded;
    }
}
