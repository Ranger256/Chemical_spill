namespace ChemicalSpill.Models
{
    /// <summary>
    /// Русские наименования значений перечислений для вывода оператору.
    /// Собраны в одном месте, чтобы формулировки на всех экранах совпадали.
    /// </summary>
    public static class DisplayNames
    {
        public static string Of(MachineState value)
        {
            switch (value)
            {
                case MachineState.Stopped: return "Стоп";
                case MachineState.Purging: return "Продувка";
                case MachineState.Ready: return "Готова";
                case MachineState.Running: return "Работа";
                case MachineState.Paused: return "Пауза";
                case MachineState.Alarm: return "Авария";
                default: return "—";
            }
        }

        public static string Of(OperatingMode value)
        {
            switch (value)
            {
                case OperatingMode.Auto: return "Автомат";
                case OperatingMode.Step: return "Шаг";
                case OperatingMode.Setup: return "Наладка";
                default: return "—";
            }
        }

        public static string Of(StageState value)
        {
            switch (value)
            {
                case StageState.Running: return "выполняется";
                case StageState.Completed: return "завершена";
                case StageState.Waiting: return "ожидание";
                case StageState.Inactive: return "неактивна";
                case StageState.Fault: return "отказ";
                default: return "—";
            }
        }

        /// <summary>
        /// Графический знак состояния. Требование п. 2.3: цвет не должен быть
        /// единственным носителем информации.
        /// </summary>
        public static string GlyphOf(StageState value)
        {
            switch (value)
            {
                case StageState.Running: return "\u25B6";
                case StageState.Completed: return "\u2713";
                case StageState.Waiting: return "\u23F1";
                case StageState.Inactive: return "\u2013";
                case StageState.Fault: return "\u26A0";
                default: return "?";
            }
        }

        public static string Of(StageId value)
        {
            switch (value)
            {
                case StageId.Feed: return "1. Подача флакона";
                case StageId.Fill: return "2. Наполнение";
                case StageId.Cap: return "3. Укупорка";
                case StageId.Discharge: return "4. Выдача флакона";
                case StageId.InputGate: return "Входной шлюз";
                case StageId.RotorIndex: return "Индексация ротора";
                case StageId.OutputGate: return "Выходной шлюз";
                default: return "—";
            }
        }

        public static string Of(ParameterKind value)
        {
            switch (value)
            {
                case ParameterKind.Indication: return "И";
                case ParameterKind.Setpoint: return "У";
                case ParameterKind.AlarmLimit: return "А";
                case ParameterKind.Calculated: return "Р";
                default: return "—";
            }
        }

        public static string DescriptionOf(ParameterKind value)
        {
            switch (value)
            {
                case ParameterKind.Indication: return "И — индикация, оператор изменить не может";
                case ParameterKind.Setpoint: return "У — уставка, задаётся оператором";
                case ParameterKind.AlarmLimit: return "А — аварийная уставка";
                case ParameterKind.Calculated: return "Р — расчётная величина";
                default: return string.Empty;
            }
        }

        public static string Of(RecipeState value)
        {
            switch (value)
            {
                case RecipeState.Draft: return "Черновик";
                case RecipeState.Verified: return "Проверен";
                case RecipeState.Approved: return "Утверждён";
                case RecipeState.Archived: return "Архивный";
                default: return "—";
            }
        }

        public static string Of(PcLinkState value)
        {
            switch (value)
            {
                case PcLinkState.None: return "Нет соединения";
                case PcLinkState.AwaitingConfirmation: return "Ожидание подтверждения на панели";
                case PcLinkState.Established: return "Соединение установлено";
                default: return "—";
            }
        }

        public static string Of(DuplexMode value)
        {
            switch (value)
            {
                case DuplexMode.Full: return "полный дуплекс";
                case DuplexMode.Half: return "полудуплекс";
                default: return "неизвестно";
            }
        }

        public static string Of(GateState value)
        {
            switch (value)
            {
                case GateState.Open: return "открыт";
                case GateState.Closed: return "закрыт";
                case GateState.Purging: return "продувка";
                default: return "—";
            }
        }

        public static string Of(AlarmSeverity value)
        {
            return value == AlarmSeverity.Alarm ? "Авария" : "Предупреждение";
        }

        public static string Of(RejectReason value)
        {
            switch (value)
            {
                case RejectReason.NotFilled: return "Не заполнен";
                case RejectReason.DoseDeviation: return "Отклонение дозы";
                case RejectReason.CapFailure: return "Ошибка укупорки";
                case RejectReason.MechanicalFault: return "Механическая ошибка";
                default: return "—";
            }
        }

        public static string Of(JournalCategory value)
        {
            switch (value)
            {
                case JournalCategory.Parameter: return "Параметры";
                case JournalCategory.SetpointChange: return "Изменение уставок";
                case JournalCategory.OperatorAction: return "Действия оператора";
                case JournalCategory.Alarm: return "Аварии";
                case JournalCategory.Recipe: return "Рецепты";
                case JournalCategory.Link: return "Связь с ПК";
                default: return "—";
            }
        }

        public static string Of(TrendWindow value)
        {
            switch (value)
            {
                case TrendWindow.LastTenMinutes: return "10 минут";
                case TrendWindow.LastHour: return "1 час";
                case TrendWindow.CurrentBatch: return "Текущая партия";
                default: return "—";
            }
        }

        public static string Of(WorkingGas value)
        {
            return value == WorkingGas.Argon ? "Аргон" : "Азот";
        }

        public static string Of(AddressMode value)
        {
            return value == AddressMode.Dhcp ? "DHCP" : "Вручную";
        }
    }
}
