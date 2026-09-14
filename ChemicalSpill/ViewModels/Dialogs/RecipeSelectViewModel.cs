using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;
using ChemicalSpill.ViewModels.Sections;

namespace ChemicalSpill.ViewModels.Dialogs
{
    /// <summary>
    /// Модальное окно выбора рецепта. После загрузки установка проверяет
    /// соответствие рецепта фактически установленной оснастке; результат выводится
    /// перечнем выполненных и невыполненных условий (п. 5.5).
    /// </summary>
    public class RecipeSelectViewModel : ViewModelBase
    {
        private readonly IRecipeService _recipes;
        private readonly IMachineService _machine;

        private RecipeItemViewModel _selected;
        private bool _showArchived;
        private bool _checksPerformed;

        public RecipeSelectViewModel(IRecipeService recipes, IMachineService machine)
        {
            _recipes = recipes;
            _machine = machine;

            Items = new ObservableCollection<RecipeItemViewModel>();
            Checks = new ObservableCollection<CheckResultViewModel>();

            CheckCommand = new RelayCommand(Check, HasSelection);
            LoadCommand = new RelayCommand(Load, CanLoad);

            Rebuild();
        }

        public ObservableCollection<RecipeItemViewModel> Items { get; private set; }
        public ObservableCollection<CheckResultViewModel> Checks { get; private set; }

        public ICommand CheckCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }

        /// <summary>Идентификатор загруженного рецепта; null — оператор отказался.</summary>
        public string ResultId { get; private set; }

        /// <summary>Окно следует закрыть с результатом «загружено».</summary>
        public bool CloseRequested { get; private set; }

        public RecipeItemViewModel Selected
        {
            get { return _selected; }
            set
            {
                if (!SetProperty(ref _selected, value)) return;

                _checksPerformed = false;
                Checks.Clear();
                OnPropertyChanged("SummaryText");
                OnPropertyChanged("HasSelectionValue");
                OnPropertyChanged("StateHint");
            }
        }

        /// <summary>Архивные рецепты остаются доступными для просмотра, но не для загрузки.</summary>
        public bool ShowArchived
        {
            get { return _showArchived; }
            set
            {
                if (SetProperty(ref _showArchived, value)) Rebuild();
            }
        }

        public bool HasSelectionValue { get { return _selected != null; } }

        public string CurrentText
        {
            get
            {
                var loaded = _recipes.Loaded;
                return loaded == null
                    ? "Загруженный рецепт: отсутствует"
                    : "Загруженный рецепт: " + loaded.Name + ", версия " + loaded.Version;
            }
        }

        public string SummaryText
        {
            get
            {
                if (_selected == null) return "Рецепт не выбран";

                return "Продукт: " + _selected.Product +
                       "   ·   Типоразмер тары: " + _selected.VialText +
                       "   ·   Доза: " + _selected.DoseText +
                       "   ·   Контрольная сумма: " + _selected.Checksum;
            }
        }

        public string StateHint
        {
            get
            {
                if (_selected == null) return string.Empty;

                switch (_selected.State)
                {
                    case RecipeState.Approved:
                        return "Рецепт утверждён и допущен к выпуску продукции.";
                    case RecipeState.Verified:
                        return "Рецепт проверен, но не утверждён: производственный пуск по нему невозможен.";
                    case RecipeState.Draft:
                        return "Черновик: допускается только наладочный режим.";
                    default:
                        return "Архивный рецепт: загрузка для работы невозможна, доступен просмотр.";
                }
            }
        }

        /// <summary>Смена рецепта в ходе выполнения партии не допускается (п. 4.1).</summary>
        public bool IsChangeAllowed
        {
            get { return _machine.State == MachineState.Stopped || _machine.State == MachineState.Ready; }
        }

        public string ChangeHint
        {
            get
            {
                return IsChangeAllowed
                    ? "Загрузка доступна: установка остановлена."
                    : "Загрузка недоступна: смена рецепта в ходе выполнения партии не допускается.";
            }
        }

        private void Rebuild()
        {
            Items.Clear();
            foreach (var summary in _recipes.GetRecipes())
            {
                if (!_showArchived && summary.State == RecipeState.Archived) continue;
                Items.Add(new RecipeItemViewModel(summary));
            }
        }

        private bool HasSelection()
        {
            return _selected != null;
        }

        private void Check()
        {
            ShowChecks();
            _checksPerformed = true;
            RelayCommand.RaiseCanExecuteChanged();
        }

        private bool CanLoad()
        {
            return _selected != null && IsChangeAllowed && _selected.State != RecipeState.Archived && _checksPerformed;
        }

        private void Load()
        {
            ShowChecks();

            foreach (var check in Checks)
            {
                if (!check.Passed) return;
            }

            ResultId = _selected.Id;
            CloseRequested = true;
            OnPropertyChanged("CloseRequested");
        }

        private void ShowChecks()
        {
            Checks.Clear();
            foreach (var result in _recipes.Load(_selected.Id))
            {
                Checks.Add(new CheckResultViewModel(result));
            }
        }
    }
}
