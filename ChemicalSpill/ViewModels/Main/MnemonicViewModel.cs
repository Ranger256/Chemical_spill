using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Мнемосхема технологического цикла (раздел 2 функционального интерфейса):
    /// четыре стадии основного цикла и вспомогательные блоки, две формы представления.
    /// </summary>
    public class MnemonicViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;
        private MnemonicMode _mode = MnemonicMode.ByStation;

        public MnemonicViewModel(IMachineService machine)
        {
            _machine = machine;

            Stages = new ObservableCollection<StageViewModel>();
            Auxiliary = new ObservableCollection<StageViewModel>();
            Nests = new ObservableCollection<int>();

            for (int i = 1; i <= _machine.Rotor.NestCount; i++) Nests.Add(i);

            SetByStationCommand = new RelayCommand(delegate { Mode = MnemonicMode.ByStation; });
            SetByVialCommand = new RelayCommand(delegate { Mode = MnemonicMode.ByVial; });

            Rebuild();
        }

        /// <summary>Стадии основного цикла: подача, наполнение, укупорка, выдача.</summary>
        public ObservableCollection<StageViewModel> Stages { get; private set; }

        /// <summary>Вспомогательные блоки: входной шлюз, индексация ротора, выходной шлюз.</summary>
        public ObservableCollection<StageViewModel> Auxiliary { get; private set; }

        /// <summary>Номера гнёзд для выбора прослеживаемого флакона.</summary>
        public ObservableCollection<int> Nests { get; private set; }

        public ICommand SetByStationCommand { get; private set; }
        public ICommand SetByVialCommand { get; private set; }

        /// <summary>
        /// Форма представления. По станциям — режим по умолчанию, отвечает на вопрос
        /// «что сейчас делает установка». По флакону — применяется при разборе брака.
        /// </summary>
        public MnemonicMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;
                _mode = value;
                OnPropertyChanged();
                OnPropertyChanged("IsByStation");
                OnPropertyChanged("IsByVial");
                OnPropertyChanged("ModeHint");
                Rebuild();
            }
        }

        public bool IsByStation { get { return _mode == MnemonicMode.ByStation; } }
        public bool IsByVial { get { return _mode == MnemonicMode.ByVial; } }

        public string ModeHint
        {
            get
            {
                return _mode == MnemonicMode.ByStation
                    ? "Станции работают одновременно: зелёными могут быть несколько блоков."
                    : "Прослеживается путь одного флакона: пройденные стадии отмечены как завершённые.";
            }
        }

        public int SelectedNest
        {
            get { return _machine.SelectedVialNest; }
            set
            {
                if (_machine.SelectedVialNest == value) return;
                _machine.SelectVial(value);
                OnPropertyChanged();
                Rebuild();
            }
        }

        public string RotorPositionText
        {
            get { return "Позиция ротора: " + _machine.Rotor.CurrentPosition + " из " + _machine.Rotor.NestCount; }
        }

        /// <summary>Перечитывает состояния блоков; вызывается по событию Updated сервиса.</summary>
        public void Refresh()
        {
            var snapshots = _machine.GetStages(_mode);

            if (Stages.Count == 0 || Auxiliary.Count == 0)
            {
                Rebuild();
                return;
            }

            foreach (var snapshot in snapshots)
            {
                var target = IsAuxiliary(snapshot.Id) ? Find(Auxiliary, snapshot.Id) : Find(Stages, snapshot.Id);
                if (target != null) target.Update(snapshot);
            }

            OnPropertyChanged("RotorPositionText");
        }

        private void Rebuild()
        {
            Stages.Clear();
            Auxiliary.Clear();

            foreach (var snapshot in _machine.GetStages(_mode))
            {
                if (IsAuxiliary(snapshot.Id))
                {
                    Auxiliary.Add(new StageViewModel(snapshot, true));
                }
                else
                {
                    Stages.Add(new StageViewModel(snapshot, false));
                }
            }

            OnPropertyChanged("RotorPositionText");
        }

        private static bool IsAuxiliary(StageId id)
        {
            return id == StageId.InputGate || id == StageId.RotorIndex || id == StageId.OutputGate;
        }

        private static StageViewModel Find(ObservableCollection<StageViewModel> items, StageId id)
        {
            foreach (var item in items)
            {
                if (item.Id == id) return item;
            }
            return null;
        }
    }
}
