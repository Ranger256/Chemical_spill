using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Органы управления пуском и остановом (раздел 4 функционального интерфейса).
    /// Кнопки постоянно находятся на главном экране и не скрываются в меню.
    /// </summary>
    public class ControlPanelViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;
        private readonly IDialogService _dialogs;

        private MachineState _lastState = (MachineState)(-1);
        private int _lastBlockingCount = -1;

        public ControlPanelViewModel(IMachineService machine, IDialogService dialogs)
        {
            _machine = machine;
            _dialogs = dialogs;

            Modes = new ObservableCollection<OperatingMode>
            {
                OperatingMode.Auto,
                OperatingMode.Step,
                OperatingMode.Setup
            };

            StartConditions = new ObservableCollection<StartConditionViewModel>();

            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand = new RelayCommand(Stop, CanStop);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StepCommand = new RelayCommand(Step, CanStep);
            ResetAlarmCommand = new RelayCommand(ResetAlarm, CanResetAlarm);
            SelectRecipeCommand = new RelayCommand(SelectRecipe, CanSelectRecipe);

            Refresh();
        }

        public ObservableCollection<OperatingMode> Modes { get; private set; }

        /// <summary>Разрешающие условия пуска: оператор не должен искать причину сам (п. 4.3).</summary>
        public ObservableCollection<StartConditionViewModel> StartConditions { get; private set; }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StepCommand { get; private set; }
        public ICommand ResetAlarmCommand { get; private set; }
        public ICommand SelectRecipeCommand { get; private set; }

        public MachineState State { get { return _machine.State; } }
        public string StateText { get { return DisplayNames.Of(_machine.State); } }
        public string CurrentStepText { get { return _machine.CurrentStepName; } }

        public OperatingMode SelectedMode
        {
            get { return _machine.Mode; }
            set
            {
                if (_machine.Mode == value) return;
                _machine.SetMode(value);
                OnPropertyChanged();
            }
        }

        /// <summary>Режим работы меняется только при остановленном цикле.</summary>
        public bool CanChangeMode
        {
            get { return _machine.State == MachineState.Stopped || _machine.State == MachineState.Ready; }
        }

        // --- Сводка рецепта, выводимая перед пуском (п. 4.4) -------------------

        public string RecipeName
        {
            get { return _machine.LoadedRecipe == null ? "не загружен" : _machine.LoadedRecipe.Name; }
        }

        public string RecipeVersionText
        {
            get
            {
                var recipe = _machine.LoadedRecipe;
                if (recipe == null) return "—";
                return "версия " + recipe.Version + " · " + DisplayNames.Of(recipe.State) + " · к/сумма " + recipe.Checksum;
            }
        }

        public string ProductText { get { return _machine.Batch.Product; } }

        public string VialSizeText { get { return _machine.Rotor.VialSizeMl.ToString("0") + " мл"; } }

        public string DoseText
        {
            get { return _machine.LoadedRecipe == null ? "—" : _machine.LoadedRecipe.DoseMl.ToString("0.00") + " мл"; }
        }

        public string BatchTargetText
        {
            get { return _machine.Batch.Good + " из " + _machine.Batch.Target + " шт"; }
        }

        public string BatchNumberText { get { return _machine.Batch.Number; } }

        public string OperatorText { get { return _machine.Batch.OperatorName; } }

        /// <summary>Число невыполненных разрешающих условий.</summary>
        public int BlockingCount
        {
            get
            {
                int count = 0;
                foreach (var condition in StartConditions)
                {
                    if (!condition.Fulfilled) count++;
                }
                return count;
            }
        }

        public bool HasBlockingConditions { get { return BlockingCount > 0; } }

        public string BlockingText
        {
            get
            {
                return BlockingCount == 0
                    ? "Все разрешающие условия выполнены"
                    : "Пуск заблокирован: не выполнено условий — " + BlockingCount;
            }
        }

        public void Refresh()
        {
            UpdateConditions(_machine.GetStartConditions());

            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
            OnPropertyChanged("CurrentStepText");
            OnPropertyChanged("SelectedMode");
            OnPropertyChanged("CanChangeMode");
            OnPropertyChanged("RecipeName");
            OnPropertyChanged("RecipeVersionText");
            OnPropertyChanged("ProductText");
            OnPropertyChanged("VialSizeText");
            OnPropertyChanged("DoseText");
            OnPropertyChanged("BatchTargetText");
            OnPropertyChanged("BatchNumberText");
            OnPropertyChanged("OperatorText");
            OnPropertyChanged("BlockingCount");
            OnPropertyChanged("HasBlockingConditions");
            OnPropertyChanged("BlockingText");

            // Доступность кнопок пересматривается сразу, а не по следующему
            // действию пользователя: иначе после останова «Пуск» остаётся серым.
            if (_lastState != _machine.State || _lastBlockingCount != BlockingCount)
            {
                _lastState = _machine.State;
                _lastBlockingCount = BlockingCount;
                RelayCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateConditions(IReadOnlyList<StartCondition> conditions)
        {
            StartConditions.Clear();
            foreach (var condition in conditions)
            {
                StartConditions.Add(new StartConditionViewModel(condition));
            }
        }

        private bool CanStart()
        {
            return _machine.CanStart && BlockingCount == 0;
        }

        private void Start()
        {
            _machine.Start();
            Refresh();
        }

        // Стоп доступна всегда, когда установка работает, и подтверждения не требует:
        // подтверждение замедляет остановку (п. 4.4).
        private bool CanStop()
        {
            return _machine.CanStop;
        }

        private void Stop()
        {
            _machine.Stop();
            Refresh();
        }

        private bool CanPause()
        {
            return _machine.CanPause || _machine.State == MachineState.Paused;
        }

        private void Pause()
        {
            _machine.Pause();
            Refresh();
        }

        private bool CanStep()
        {
            return _machine.CanStep;
        }

        private void Step()
        {
            _machine.Step();
            Refresh();
        }

        private bool CanResetAlarm()
        {
            return _machine.CanResetAlarm;
        }

        private void ResetAlarm()
        {
            _machine.ResetAlarm(_machine.Batch.OperatorName);
            Refresh();
        }

        /// <summary>Выбор рецепта недоступен в состояниях «Работа» и «Пауза» (п. 4.1).</summary>
        private bool CanSelectRecipe()
        {
            return _machine.State == MachineState.Stopped || _machine.State == MachineState.Ready;
        }

        private void SelectRecipe()
        {
            if (_dialogs == null) return;
            _dialogs.SelectRecipe();
            Refresh();
        }
    }
}
