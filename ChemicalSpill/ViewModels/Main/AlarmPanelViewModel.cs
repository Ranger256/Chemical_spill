using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Аварии и предупреждения, а также блокирующая панель аварии.
    /// При аварии управление блокируется: оператор подтверждает устранение
    /// причины и квитирует аварию, после чего экран разблокируется.
    /// Квитирование не выполняется автоматически (п. 4.1).
    /// </summary>
    public class AlarmPanelViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;
        private bool _causeEliminated;

        public AlarmPanelViewModel(IMachineService machine)
        {
            _machine = machine;
            Items = new ObservableCollection<AlarmItemViewModel>();
            AcknowledgeCommand = new RelayCommand(Acknowledge, CanAcknowledge);
            Refresh();
        }

        public ObservableCollection<AlarmItemViewModel> Items { get; private set; }

        public ICommand AcknowledgeCommand { get; private set; }

        /// <summary>Экран заблокирован: установка в состоянии «Авария».</summary>
        public bool IsLocked { get { return _machine.State == MachineState.Alarm; } }

        /// <summary>
        /// Оператор подтвердил, что причина устранена. Без подтверждения
        /// квитирование недоступно.
        /// </summary>
        public bool CauseEliminated
        {
            get { return _causeEliminated; }
            set
            {
                if (SetProperty(ref _causeEliminated, value)) RelayCommand.RaiseCanExecuteChanged();
            }
        }

        public int AlarmCount
        {
            get
            {
                int count = 0;
                foreach (var item in Items)
                {
                    if (item.IsAlarm && !item.Acknowledged) count++;
                }
                return count;
            }
        }

        public int WarningCount
        {
            get
            {
                int count = 0;
                foreach (var item in Items)
                {
                    if (!item.IsAlarm && !item.Acknowledged) count++;
                }
                return count;
            }
        }

        public bool HasItems { get { return Items.Count > 0; } }

        public string CountersText
        {
            get { return "аварий " + AlarmCount + " · предупреждений " + WarningCount; }
        }

        /// <summary>Текст действующей аварии для блокирующей панели.</summary>
        public string BlockingAlarmText
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.IsAlarm && !item.Acknowledged) return item.Code + " · " + item.Text;
                }
                return "Цикл остановлен по аварии.";
            }
        }

        public string BlockingAlarmSource
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.IsAlarm && !item.Acknowledged) return item.Source + ", " + item.TimeText;
                }
                return string.Empty;
            }
        }

        public void Refresh()
        {
            Items.Clear();
            foreach (var record in _machine.GetActiveAlarms())
            {
                Items.Add(new AlarmItemViewModel(record));
            }

            OnPropertyChanged("IsLocked");
            OnPropertyChanged("AlarmCount");
            OnPropertyChanged("WarningCount");
            OnPropertyChanged("HasItems");
            OnPropertyChanged("CountersText");
            OnPropertyChanged("BlockingAlarmText");
            OnPropertyChanged("BlockingAlarmSource");
        }

        private bool CanAcknowledge()
        {
            return IsLocked && CauseEliminated;
        }

        private void Acknowledge()
        {
            _machine.ResetAlarm(_machine.Batch.OperatorName);
            CauseEliminated = false;
            Refresh();
        }
    }
}
