namespace ChemicalSpill.Models
{
    /// <summary>
    /// Обобщённое состояние установки.
    /// Источник: «Перечень параметров, выводимых оператору», п. 7.
    /// </summary>
    public enum MachineState
    {
        /// <summary>Стоп — цикл не выполняется.</summary>
        Stopped,
        /// <summary>Продувка камеры или шлюза.</summary>
        Purging,
        /// <summary>Готова — все разрешающие условия выполнены.</summary>
        Ready,
        /// <summary>Работа — цикл выполняется.</summary>
        Running,
        /// <summary>Пауза — цикл приостановлен с сохранением состояния.</summary>
        Paused,
        /// <summary>Авария — требуется вмешательство оператора.</summary>
        Alarm
    }

    /// <summary>Режим работы. Источник: «Функциональный интерфейс», п. 3.2.</summary>
    public enum OperatingMode
    {
        /// <summary>Автомат — непрерывное выполнение цикла.</summary>
        Auto,
        /// <summary>Шаг — одна стадия за одно нажатие.</summary>
        Step,
        /// <summary>Наладка — ручное управление механизмами.</summary>
        Setup
    }

    /// <summary>Пять состояний стадии. Источник: «Функциональный интерфейс», п. 2.3.</summary>
    public enum StageState
    {
        /// <summary>Выполняется — зелёная заливка.</summary>
        Running,
        /// <summary>Завершена — белая заливка, зелёный контур, отметка.</summary>
        Completed,
        /// <summary>Ожидание — жёлтая заливка.</summary>
        Waiting,
        /// <summary>Неактивна — серая заливка.</summary>
        Inactive,
        /// <summary>Отказ — красная заливка.</summary>
        Fault
    }

    /// <summary>Идентификатор стадии основного цикла или вспомогательного блока.</summary>
    public enum StageId
    {
        /// <summary>Стадия 1. Подача флакона.</summary>
        Feed,
        /// <summary>Стадия 2. Наполнение.</summary>
        Fill,
        /// <summary>Стадия 3. Укупорка.</summary>
        Cap,
        /// <summary>Стадия 4. Выдача флакона.</summary>
        Discharge,
        /// <summary>Вспомогательный блок: входной шлюз.</summary>
        InputGate,
        /// <summary>Вспомогательный блок: индексация ротора.</summary>
        RotorIndex,
        /// <summary>Вспомогательный блок: выходной шлюз.</summary>
        OutputGate
    }

    /// <summary>Форма представления мнемосхемы. Источник: «Функциональный интерфейс», п. 2.2.</summary>
    public enum MnemonicMode
    {
        /// <summary>Вид по станциям (основной): несколько станций работают параллельно.</summary>
        ByStation,
        /// <summary>Вид по флакону (вспомогательный): путь одного выбранного флакона.</summary>
        ByVial
    }

    /// <summary>Тип величины. Источник: «Перечень параметров», п. 1.</summary>
    public enum ParameterKind
    {
        /// <summary>И — индикация.</summary>
        Indication,
        /// <summary>У — уставка.</summary>
        Setpoint,
        /// <summary>А — аварийная уставка.</summary>
        AlarmLimit,
        /// <summary>Р — расчётная величина.</summary>
        Calculated
    }

    /// <summary>Оценка текущего значения относительно заданных границ.</summary>
    public enum ValueStatus { Normal, Warning, Alarm, Unknown }

    /// <summary>Уровень отклонения: предупреждение требует внимания, авария останавливает цикл.</summary>
    public enum AlarmSeverity { Warning, Alarm }

    /// <summary>Состояние рецепта. Источник: «Функциональный интерфейс», п. 5.4.</summary>
    public enum RecipeState { Draft, Verified, Approved, Archived }

    /// <summary>Стадия процедуры подключения ПК. Источник: «Подключение к ПК», п. 3.</summary>
    public enum PcLinkState
    {
        /// <summary>Нет соединения.</summary>
        None,
        /// <summary>Ключ 1 принят, ожидается ввод Ключа 2 на панели оператора.</summary>
        AwaitingConfirmation,
        /// <summary>Соединение установлено.</summary>
        Established
    }

    /// <summary>Способ получения сетевого адреса.</summary>
    public enum AddressMode { Dhcp, Manual }

    /// <summary>Режим передачи канала Ethernet.</summary>
    public enum DuplexMode { Unknown, Half, Full }

    /// <summary>Состояние шлюза. Источник: «Перечень параметров», п. 6.</summary>
    public enum GateState { Open, Closed, Purging }

    /// <summary>Тип строки консоли.</summary>
    public enum ConsoleLineKind { Info, Command, Warning, Error }

    /// <summary>Окно наблюдения графиков. Источник: «Перечень параметров», п. 2.1.</summary>
    public enum TrendWindow { LastTenMinutes, LastHour, CurrentBatch }

    /// <summary>Причина отбраковки флакона. Источник: «Перечень параметров», п. 3.</summary>
    public enum RejectReason { NotFilled, DoseDeviation, CapFailure, MechanicalFault }

    /// <summary>Категория записи журнала.</summary>
    public enum JournalCategory { Parameter, SetpointChange, OperatorAction, Alarm, Recipe, Link }

    /// <summary>Рабочий газ камеры.</summary>
    public enum WorkingGas { Nitrogen, Argon }
}
