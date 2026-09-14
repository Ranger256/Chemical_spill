using System;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Видеонаблюдение рабочей камеры. Источник: «Перечень параметров», раздел 8.
    /// Кадр передаётся в виде сжатого изображения, чтобы контракт не зависел от WPF.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>Идёт ли трансляция.</summary>
        bool IsStreaming { get; }

        /// <summary>Включено ли внутреннее освещение рабочей камеры.</summary>
        bool IsLightOn { get; }

        /// <summary>Ведётся ли запись (решается при уточнении требований).</summary>
        bool IsRecording { get; }

        /// <summary>Частота кадров.</summary>
        int Fps { get; }

        /// <summary>Задержка трансляции, мс.</summary>
        int LatencyMs { get; }

        /// <summary>Текущий кадр в формате JPEG; null, если сигнала нет.</summary>
        byte[] GetCurrentFrame();

        /// <summary>Управление освещением рабочей камеры из интерфейса.</summary>
        void SetLight(bool on);

        /// <summary>Сохранение снимка в файл.</summary>
        void TakeSnapshot(string path);

        void StartStream();
        void StopStream();

        /// <summary>Получен новый кадр.</summary>
        event EventHandler FrameReady;
    }
}
