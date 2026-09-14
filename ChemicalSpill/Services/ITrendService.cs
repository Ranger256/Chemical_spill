using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Графики параметров во времени. Источник: «Перечень параметров», п. 2.1:
    /// по одному мгновенному числу нельзя отличить стабильную камеру от камеры,
    /// в которой значение медленно растёт.
    /// </summary>
    public interface ITrendService
    {
        /// <summary>Кривые указанных параметров за выбранное окно наблюдения.</summary>
        IReadOnlyList<TrendSeries> GetSeries(IEnumerable<string> parameterKeys, TrendWindow window);

        /// <summary>Выгрузка данных графиков в файл; входят в отчёт о партии.</summary>
        void Export(IEnumerable<string> parameterKeys, TrendWindow window, string path);

        /// <summary>Добавлены новые точки.</summary>
        event EventHandler SeriesUpdated;
    }
}
