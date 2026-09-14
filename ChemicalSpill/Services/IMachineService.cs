using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Доступ к состоянию установки и органам управления пуском и остановом.
    /// Это основная точка подключения ядра: представление ничего не знает о том,
    /// как данные получены — по сети от установки или из имитатора.
    /// </summary>
    public interface IMachineService
    {
        /// <summary>Обобщённое состояние установки.</summary>
        MachineState State { get; }

        /// <summary>Режим работы: автомат, шаг, наладка.</summary>
        OperatingMode Mode { get; }

        /// <summary>Текущий шаг цикла, например «Дозирование».</summary>
        string CurrentStepName { get; }

        /// <summary>Данные задания на партию.</summary>
        BatchInfo Batch { get; }

        /// <summary>Установленная оснастка и положение ротора.</summary>
        RotorInfo Rotor { get; }

        /// <summary>Загруженный рецепт; отображается на главном экране постоянно (п. 4.4).</summary>
        RecipeSummary LoadedRecipe { get; }

        /// <summary>Потребляемая мощность, Вт.</summary>
        double PowerWatts { get; }

        /// <summary>
        /// Состояния блоков мнемосхемы. В виде по станциям одновременно активными
        /// могут быть несколько блоков, в виде по флакону — один (п. 2.2).
        /// </summary>
        IReadOnlyList<StageSnapshot> GetStages(MnemonicMode mode);

        /// <summary>Все параметры, выводимые оператору; при необходимости фильтруются по группе.</summary>
        IReadOnlyList<ParameterSnapshot> GetParameters(string group);

        /// <summary>Разрешающие условия пуска с признаком выполнения (п. 4.3).</summary>
        IReadOnlyList<StartCondition> GetStartConditions();

        /// <summary>Действующие аварии и предупреждения.</summary>
        IReadOnlyList<AlarmRecord> GetActiveAlarms();

        /// <summary>История строк консоли событий.</summary>
        IReadOnlyList<ConsoleRecord> GetConsoleHistory();

        /// <summary>Номер гнезда, выбранного для прослеживания в виде по флакону.</summary>
        int SelectedVialNest { get; }

        bool CanStart { get; }
        bool CanStop { get; }
        bool CanPause { get; }
        bool CanStep { get; }
        bool CanResetAlarm { get; }

        /// <summary>Пуск цикла в выбранном режиме.</summary>
        void Start();

        /// <summary>Штатный останов: операции над флаконами в роторе завершаются, атмосфера сохраняется.</summary>
        void Stop();

        /// <summary>Приостановка цикла в текущем положении с сохранением состояния.</summary>
        void Pause();

        /// <summary>Выполнение одной стадии цикла в режиме «Шаг».</summary>
        void Step();

        /// <summary>
        /// Квитирование аварии и перевод установки в состояние «Стоп».
        /// Выполняется только после подтверждения оператором, что причина устранена.
        /// </summary>
        void ResetAlarm(string operatorName);

        void SetMode(OperatingMode mode);

        /// <summary>Выбор флакона (гнезда) для прослеживания в виде по флакону.</summary>
        void SelectVial(int nestNumber);

        /// <summary>Данные обновились — представлению следует перечитать снимки.</summary>
        event EventHandler Updated;

        /// <summary>Возникло новое отклонение.</summary>
        event EventHandler<AlarmRecord> AlarmRaised;

        /// <summary>Добавлена строка в консоль событий.</summary>
        event EventHandler<ConsoleRecord> ConsoleLineAdded;
    }
}
