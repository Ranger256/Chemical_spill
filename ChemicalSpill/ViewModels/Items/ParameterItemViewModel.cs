using ChemicalSpill.Models;

namespace ChemicalSpill.ViewModels.Items
{
    /// <summary>
    /// Один параметр, выводимый оператору: значение, единица измерения,
    /// диапазон, тип (И / У / А / Р) и пояснение назначения.
    /// </summary>
    public class ParameterItemViewModel : ViewModelBase
    {
        private ParameterSnapshot _snapshot;

        public ParameterItemViewModel(ParameterSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public string Key { get { return _snapshot.Key; } }
        public string Group { get { return _snapshot.Group; } }
        public string Caption { get { return _snapshot.Caption; } }
        public string Unit { get { return _snapshot.Unit; } }
        public string ValueText { get { return _snapshot.ValueText; } }
        public string RangeText { get { return _snapshot.RangeText; } }
        public string LimitText { get { return _snapshot.LimitText; } }
        public bool HasLimit { get { return !string.IsNullOrEmpty(_snapshot.LimitText); } }
        public bool HasUnit { get { return !string.IsNullOrEmpty(_snapshot.Unit); } }

        public ParameterKind Kind { get { return _snapshot.Kind; } }
        public string KindText { get { return DisplayNames.Of(_snapshot.Kind); } }
        public string KindDescription { get { return DisplayNames.DescriptionOf(_snapshot.Kind); } }

        public ValueStatus Status { get { return _snapshot.Status; } }
        public string Description { get { return _snapshot.Description; } }

        /// <summary>Подсказка: назначение параметра и тип величины.</summary>
        public string ToolTipText
        {
            get { return KindDescription + "\n\n" + _snapshot.Description; }
        }

        public void Update(ParameterSnapshot snapshot)
        {
            _snapshot = snapshot;
            OnPropertyChanged("ValueText");
            OnPropertyChanged("LimitText");
            OnPropertyChanged("HasLimit");
            OnPropertyChanged("Status");
        }
    }
}
