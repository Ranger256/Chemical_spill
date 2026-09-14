using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>Строка списка рецептов.</summary>
    public class RecipeItemViewModel : ViewModelBase
    {
        private readonly RecipeSummary _summary;

        public RecipeItemViewModel(RecipeSummary summary)
        {
            _summary = summary;
        }

        public RecipeSummary Summary { get { return _summary; } }

        public string Id { get { return _summary.Id; } }
        public string Name { get { return _summary.Name; } }
        public string Version { get { return _summary.Version; } }
        public string Product { get { return _summary.Product; } }
        public RecipeState State { get { return _summary.State; } }
        public string StateText { get { return DisplayNames.Of(_summary.State); } }
        public string Checksum { get { return _summary.Checksum; } }
        public string ModifiedText { get { return _summary.ModifiedAt.ToString("dd.MM.yyyy HH:mm"); } }
        public string ModifiedBy { get { return _summary.ModifiedBy; } }
        public string VialText { get { return _summary.VialSizeMl.ToString("0") + " мл"; } }
        public string DoseText { get { return _summary.DoseMl.ToString("0.00") + " мл"; } }
        public string Comment { get { return _summary.Comment; } }

        public void RefreshState()
        {
            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
        }
    }

    /// <summary>
    /// Раздел «Рецепты»: список, редактор блоков 1–7, состояния и версионность,
    /// проверки при сохранении и загрузке (раздел 5 функционального интерфейса).
    /// </summary>
    public class RecipesViewModel : ViewModelBase
    {
        private readonly IRecipeService _recipes;
        private readonly IMachineService _machine;
        private readonly IDialogService _dialogs;

        private RecipeItemViewModel _selected;
        private RecipeDocument _document;

        public RecipesViewModel(IRecipeService recipes, IMachineService machine, IDialogService dialogs)
        {
            _recipes = recipes;
            _machine = machine;
            _dialogs = dialogs;

            Items = new ObservableCollection<RecipeItemViewModel>();
            Checks = new ObservableCollection<CheckResultViewModel>();

            CreateCommand = new RelayCommand(Create);
            CopyCommand = new RelayCommand(Copy, HasSelection);
            SaveCommand = new RelayCommand(Save, CanEdit);
            VerifyCommand = new RelayCommand(Verify, CanVerify);
            ApproveCommand = new RelayCommand(Approve, CanApprove);
            ArchiveCommand = new RelayCommand(Archive, CanArchive);
            LoadCommand = new RelayCommand(Load, CanLoad);
            ExportCommand = new RelayCommand(Export, HasSelection);
            ImportCommand = new RelayCommand(Import);

            Reload();
        }

        public string Title { get { return "Рецепты"; } }

        public ObservableCollection<RecipeItemViewModel> Items { get; private set; }

        /// <summary>Результаты проверок: перечнем выполненных и невыполненных условий (п. 5.5).</summary>
        public ObservableCollection<CheckResultViewModel> Checks { get; private set; }

        public ICommand CreateCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand VerifyCommand { get; private set; }
        public ICommand ApproveCommand { get; private set; }
        public ICommand ArchiveCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }

        public RecipeItemViewModel Selected
        {
            get { return _selected; }
            set
            {
                if (!SetProperty(ref _selected, value)) return;

                Document = _selected == null ? null : _recipes.GetRecipe(_selected.Id);
                Checks.Clear();
                OnPropertyChanged("IsEditable");
                OnPropertyChanged("EditabilityText");
                OnPropertyChanged("HeaderText");
            }
        }

        /// <summary>Полный состав выбранного рецепта для редактора.</summary>
        public RecipeDocument Document
        {
            get { return _document; }
            private set { SetProperty(ref _document, value); }
        }

        public string HeaderText
        {
            get
            {
                if (_selected == null) return "Рецепт не выбран";
                return _selected.Name + " · версия " + _selected.Version + " · " + _selected.StateText;
            }
        }

        /// <summary>Редактирование допускается только для черновика.</summary>
        public bool IsEditable
        {
            get { return _selected != null && _selected.State == RecipeState.Draft; }
        }

        public string EditabilityText
        {
            get
            {
                if (_selected == null) return string.Empty;

                switch (_selected.State)
                {
                    case RecipeState.Draft:
                        return "Черновик: редактирование разрешено. Запуск производственного цикла запрещён, допускается наладочный режим.";
                    case RecipeState.Verified:
                        return "Проверен: пробный прогон выполнен, рецепт ожидает утверждения.";
                    case RecipeState.Approved:
                        return "Утверждён: редактирование не допускается. Изменение выполняется созданием новой версии.";
                    default:
                        return "Архивный: загрузка для работы невозможна, доступен только просмотр.";
                }
            }
        }

        public string LoadedText
        {
            get
            {
                var loaded = _recipes.Loaded;
                return loaded == null ? "не загружен" : loaded.Name + ", версия " + loaded.Version;
            }
        }

        private void Reload()
        {
            var previous = _selected == null ? null : _selected.Id;

            Items.Clear();
            foreach (var summary in _recipes.GetRecipes())
            {
                Items.Add(new RecipeItemViewModel(summary));
            }

            foreach (var item in Items)
            {
                if (item.Id == previous) Selected = item;
            }

            if (Selected == null && Items.Count > 0) Selected = Items[0];
            OnPropertyChanged("LoadedText");
        }

        private bool HasSelection()
        {
            return _selected != null;
        }

        private bool CanEdit()
        {
            return IsEditable;
        }

        private bool CanVerify()
        {
            return _selected != null && _selected.State == RecipeState.Draft;
        }

        private bool CanApprove()
        {
            return _selected != null && _selected.State == RecipeState.Verified;
        }

        private bool CanArchive()
        {
            return _selected != null && _selected.State != RecipeState.Archived;
        }

        /// <summary>Загрузка недоступна в состояниях «Работа» и «Пауза» (п. 4.1).</summary>
        private bool CanLoad()
        {
            if (_selected == null) return false;
            if (_selected.State == RecipeState.Archived) return false;
            return _machine.State == MachineState.Stopped || _machine.State == MachineState.Ready;
        }

        private void Create()
        {
            Document = _recipes.CreateNew();
            Checks.Clear();
            OnPropertyChanged("HeaderText");
        }

        private void Copy()
        {
            Document = _recipes.Copy(_selected.Id);
            Checks.Clear();
            OnPropertyChanged("HeaderText");
        }

        private void Save()
        {
            ShowChecks(_recipes.SaveAsDraft(Document));
            Reload();
        }

        private void Verify()
        {
            _recipes.MarkVerified(_selected.Id);
            _selected.RefreshState();
            OnPropertyChanged("IsEditable");
            OnPropertyChanged("EditabilityText");
            OnPropertyChanged("HeaderText");
        }

        private void Approve()
        {
            _recipes.Approve(_selected.Id, _machine.Batch.OperatorName);
            _selected.RefreshState();
            OnPropertyChanged("IsEditable");
            OnPropertyChanged("EditabilityText");
            OnPropertyChanged("HeaderText");
        }

        private void Archive()
        {
            if (_dialogs != null &&
                !_dialogs.Confirm("Перевод в архив",
                    "Рецепт будет выведен из применения. Загрузка станет невозможной, просмотр сохранится, " +
                    "поскольку на рецепт ссылаются отчёты ранее выпущенных партий. Продолжить?"))
            {
                return;
            }

            _recipes.Archive(_selected.Id);
            _selected.RefreshState();
            OnPropertyChanged("EditabilityText");
        }

        private void Load()
        {
            ShowChecks(_recipes.Load(_selected.Id));
            OnPropertyChanged("LoadedText");
        }

        private void Export()
        {
            if (_dialogs == null) return;

            var path = _dialogs.SaveFile("Выгрузка рецепта в файл", "Рецепт (*.json)|*.json",
                _selected.Name + "_v" + _selected.Version + ".json");

            if (path != null) _recipes.ExportToFile(_selected.Id, path);
        }

        private void Import()
        {
            if (_dialogs == null) return;

            var path = _dialogs.OpenFile("Загрузка рецепта из файла", "Рецепт (*.json)|*.json");
            if (path == null) return;

            Document = _recipes.ImportFromFile(path);
            OnPropertyChanged("HeaderText");
        }

        private void ShowChecks(System.Collections.Generic.IReadOnlyList<CheckResult> results)
        {
            Checks.Clear();
            foreach (var result in results)
            {
                Checks.Add(new CheckResultViewModel(result));
            }
        }
    }
}
