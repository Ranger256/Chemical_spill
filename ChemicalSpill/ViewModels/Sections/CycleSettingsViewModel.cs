using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>Одна настройка цикла в редакторе параметров.</summary>
    public class SettingItemViewModel : ViewModelBase
    {
        private readonly SettingDefinition _definition;
        private readonly ISettingsService _settings;
        private readonly CycleSettingsViewModel _owner;

        private string _value;
        private string _message;
        private bool _hasError;

        public SettingItemViewModel(SettingDefinition definition, ISettingsService settings, CycleSettingsViewModel owner)
        {
            _definition = definition;
            _settings = settings;
            _owner = owner;
            _value = definition.Value;
        }

        public string Key { get { return _definition.Key; } }
        public string Caption { get { return _definition.Caption; } }
        public string Unit { get { return _definition.Unit; } }
        public bool HasUnit { get { return !string.IsNullOrEmpty(_definition.Unit); } }
        public string Purpose { get { return _definition.Purpose; } }

        /// <summary>Диапазон допустимых значений; ввод вне диапазона отклоняется.</summary>
        public string RangeText
        {
            get
            {
                if (_definition.Options.Count > 0) return "по перечню";
                if (!_definition.Minimum.HasValue || !_definition.Maximum.HasValue) return "—";
                return _definition.Minimum.Value.ToString("0.###") + " – " + _definition.Maximum.Value.ToString("0.###");
            }
        }

        public string DefaultText
        {
            get { return _definition.DefaultValue.HasValue ? _definition.DefaultValue.Value.ToString("0.###") : "—"; }
        }

        /// <summary>Смена в работе: да — изменяется на ходу, нет — требует останова цикла.</summary>
        public bool ChangeableWhileRunning { get { return _definition.ChangeableWhileRunning; } }

        public string ChangeableText { get { return _definition.ChangeableWhileRunning ? "в работе" : "только на стопе"; } }

        public bool IsChoice { get { return _definition.Options.Count > 0; } }

        public IList<string> Options { get { return _definition.Options; } }

        public string Value
        {
            get { return _value; }
            set
            {
                if (!SetProperty(ref _value, value)) return;
                Apply();
            }
        }

        /// <summary>Настройка недоступна, если требует останова, а цикл выполняется.</summary>
        public bool IsEditable
        {
            get { return _definition.ChangeableWhileRunning || !_owner.IsRunning; }
        }

        public string Message
        {
            get { return _message; }
            private set { SetProperty(ref _message, value); }
        }

        public bool HasError
        {
            get { return _hasError; }
            private set { SetProperty(ref _hasError, value); }
        }

        public void RefreshAvailability()
        {
            OnPropertyChanged("IsEditable");
        }

        private void Apply()
        {
            var result = _settings.TryApply(_definition.Key, _value, _owner.OperatorName);
            HasError = !result.Passed;
            Message = result.Detail;
            _owner.Recalculate();
        }
    }

    /// <summary>
    /// Раздел «Настройки цикла»: перечень настроек по стадиям с диапазонами
    /// и расчёт времени такта (раздел 3 функционального интерфейса).
    /// </summary>
    public class CycleSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settings;
        private readonly IMachineService _machine;
        private readonly IDialogService _dialogs;

        private CycleTimeEstimate _estimate;

        public CycleSettingsViewModel(ISettingsService settings, IMachineService machine, IDialogService dialogs)
        {
            _settings = settings;
            _machine = machine;
            _dialogs = dialogs;

            Groups = new ObservableCollection<SettingGroupViewModel>();
            Build();

            SaveToRecipeCommand = new RelayCommand(SaveToRecipe);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);

            _machine.Updated += delegate { RefreshAvailability(); };
            Recalculate();
        }

        public string Title { get { return "Настройки цикла"; } }

        public ObservableCollection<SettingGroupViewModel> Groups { get; private set; }

        public ICommand SaveToRecipeCommand { get; private set; }
        public ICommand ResetToDefaultsCommand { get; private set; }

        public string OperatorName { get { return _machine.Batch.OperatorName; } }

        /// <summary>Цикл выполняется: настройки, требующие останова, недоступны.</summary>
        public bool IsRunning
        {
            get { return _machine.State == MachineState.Running || _machine.State == MachineState.Paused; }
        }

        public string AvailabilityText
        {
            get
            {
                return IsRunning
                    ? "Цикл выполняется: доступны только настройки с признаком «в работе». Остальные требуют останова."
                    : "Цикл остановлен: доступны все настройки.";
            }
        }

        public string RecipeText
        {
            get
            {
                var recipe = _machine.LoadedRecipe;
                if (recipe == null) return "рецепт не загружен";
                return recipe.Name + ", версия " + recipe.Version + " (" + DisplayNames.Of(recipe.State) + ")";
            }
        }

        /// <summary>Редактирование утверждённого рецепта не допускается (п. 5.4).</summary>
        public bool IsRecipeReadOnly
        {
            get { return _machine.LoadedRecipe != null && _machine.LoadedRecipe.State == RecipeState.Approved; }
        }

        // --- Расчёт времени такта (п. 3.3) -----------------------------------

        public string FeedDurationText { get { return Duration(StageId.Feed); } }
        public string FillDurationText { get { return Duration(StageId.Fill); } }
        public string CapDurationText { get { return Duration(StageId.Cap); } }
        public string DischargeDurationText { get { return Duration(StageId.Discharge); } }

        public string IndexingText
        {
            get { return _estimate == null ? "—" : _estimate.IndexingSeconds.ToString("0.0") + " с"; }
        }

        public string TactText
        {
            get { return _estimate == null ? "—" : _estimate.TactSeconds.ToString("0.0") + " с"; }
        }

        public string ThroughputText
        {
            get { return _estimate == null ? "—" : _estimate.Throughput.ToString("0.0") + " шт/мин"; }
        }

        public string RequirementText
        {
            get
            {
                if (_estimate == null) return "—";
                return _estimate.MeetsRequirement
                    ? "Требование ТЗ выполнено: не менее 6 шт/мин"
                    : "Ниже требования ТЗ: не менее 6 шт/мин";
            }
        }

        public bool MeetsRequirement { get { return _estimate != null && _estimate.MeetsRequirement; } }

        public string LimitingStageText
        {
            get { return _estimate == null ? "—" : DisplayNames.Of(_estimate.LimitingStage); }
        }

        public string TactExplanation
        {
            get
            {
                return "Для роторной схемы время такта определяется самой длительной стадией " +
                       "плюс время индексации, а не суммой всех стадий.";
            }
        }

        public void Recalculate()
        {
            _estimate = _settings.Estimate(_settings.GetSettings());

            OnPropertyChanged("FeedDurationText");
            OnPropertyChanged("FillDurationText");
            OnPropertyChanged("CapDurationText");
            OnPropertyChanged("DischargeDurationText");
            OnPropertyChanged("IndexingText");
            OnPropertyChanged("TactText");
            OnPropertyChanged("ThroughputText");
            OnPropertyChanged("RequirementText");
            OnPropertyChanged("MeetsRequirement");
            OnPropertyChanged("LimitingStageText");
        }

        private void RefreshAvailability()
        {
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("AvailabilityText");
            OnPropertyChanged("RecipeText");
            OnPropertyChanged("IsRecipeReadOnly");

            foreach (var group in Groups)
            {
                foreach (var item in group.Items) item.RefreshAvailability();
            }
        }

        private string Duration(StageId stage)
        {
            if (_estimate == null) return "—";

            double value;
            return _estimate.StageDurations.TryGetValue(stage, out value) ? value.ToString("0.0") + " с" : "—";
        }

        private void Build()
        {
            SettingGroupViewModel current = null;

            foreach (var definition in _settings.GetSettings())
            {
                if (current == null || current.Title != definition.Group)
                {
                    current = new SettingGroupViewModel(definition.Group);
                    Groups.Add(current);
                }

                current.Items.Add(new SettingItemViewModel(definition, _settings, this));
            }
        }

        private void SaveToRecipe()
        {
            if (_dialogs == null) return;

            _dialogs.ShowMessage("Сохранение в рецепт",
                "Изменение утверждённого рецепта не допускается: будет создана новая версия, " +
                "предыдущая сохранится без изменений.");
        }

        private void ResetToDefaults()
        {
            // Значения по умолчанию подставляет ядро: фасад только запрашивает действие
            // и перечитывает расчёт такта.
            Recalculate();
        }
    }

    /// <summary>Раздел перечня настроек: общие параметры, стадии, индексация.</summary>
    public class SettingGroupViewModel : ViewModelBase
    {
        public SettingGroupViewModel(string title)
        {
            Title = title;
            Items = new ObservableCollection<SettingItemViewModel>();
        }

        public string Title { get; private set; }

        public ObservableCollection<SettingItemViewModel> Items { get; private set; }
    }
}
