using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Настройки цикла и расчёт времени такта.
    /// Источник: «Функциональный интерфейс», раздел 3.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>Полный перечень настроек с диапазонами и признаком смены в работе.</summary>
        IReadOnlyList<SettingDefinition> GetSettings();

        /// <summary>
        /// Попытка применить значение. Ввод вне диапазона отклоняется с пояснением
        /// причины; настройка, требующая останова, отклоняется при работающем цикле.
        /// </summary>
        CheckResult TryApply(string key, string value, string operatorName);

        /// <summary>
        /// Пересчёт длительности стадий, времени такта и ожидаемой производительности
        /// по текущим значениям настроек (п. 3.3).
        /// </summary>
        CycleTimeEstimate Estimate(IEnumerable<SettingDefinition> settings);

        /// <summary>Значения настроек изменились (в том числе при загрузке рецепта).</summary>
        event EventHandler SettingsChanged;
    }
}
