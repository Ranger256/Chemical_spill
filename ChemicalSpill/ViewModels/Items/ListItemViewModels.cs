using ChemicalSpill.Models;

namespace ChemicalSpill.ViewModels.Items
{
    /// <summary>Строка перечня активных аварий и предупреждений.</summary>
    public class AlarmItemViewModel : ViewModelBase
    {
        private readonly AlarmRecord _record;

        public AlarmItemViewModel(AlarmRecord record)
        {
            _record = record;
        }

        public string TimeText { get { return _record.Time.ToString("HH:mm:ss"); } }
        public AlarmSeverity Severity { get { return _record.Severity; } }
        public string SeverityText { get { return DisplayNames.Of(_record.Severity); } }
        public string Code { get { return _record.Code; } }
        public string Text { get { return _record.Text; } }
        public string Source { get { return _record.Source; } }
        public bool Acknowledged { get { return _record.Acknowledged; } }
        public bool IsAlarm { get { return _record.Severity == AlarmSeverity.Alarm; } }
    }

    /// <summary>Строка консоли событий.</summary>
    public class ConsoleItemViewModel : ViewModelBase
    {
        private readonly ConsoleRecord _record;

        public ConsoleItemViewModel(ConsoleRecord record)
        {
            _record = record;
        }

        public string TimeText { get { return _record.Time.ToString("HH:mm:ss"); } }
        public ConsoleLineKind Kind { get { return _record.Kind; } }
        public string Text { get { return _record.Text; } }
    }

    /// <summary>Строка перечня разрешающих условий пуска.</summary>
    public class StartConditionViewModel : ViewModelBase
    {
        private readonly StartCondition _condition;

        public StartConditionViewModel(StartCondition condition)
        {
            _condition = condition;
        }

        public string Text { get { return _condition.Text; } }
        public bool Fulfilled { get { return _condition.Fulfilled; } }
        public string Detail { get { return _condition.Detail; } }
        public bool HasDetail { get { return !string.IsNullOrEmpty(_condition.Detail); } }
        public string Glyph { get { return _condition.Fulfilled ? "✓" : "✗"; } }
    }

    /// <summary>Строка перечня проверок рецепта при сохранении и загрузке.</summary>
    public class CheckResultViewModel : ViewModelBase
    {
        private readonly CheckResult _result;

        public CheckResultViewModel(CheckResult result)
        {
            _result = result;
        }

        public string Text { get { return _result.Text; } }
        public bool Passed { get { return _result.Passed; } }
        public string Detail { get { return _result.Detail; } }
        public bool HasDetail { get { return !string.IsNullOrEmpty(_result.Detail); } }
        public string Glyph { get { return _result.Passed ? "✓" : "✗"; } }
    }
}
