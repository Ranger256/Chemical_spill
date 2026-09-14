using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.Services.Demo;
using ChemicalSpill.ViewModels.Main;
using ChemicalSpill.ViewModels.Sections;

namespace ChemicalSpill.ViewModels
{
    /// <summary>Раздел бокового меню.</summary>
    public class SectionViewModel : ViewModelBase
    {
        public SectionViewModel(string title, string glyph, object content)
        {
            Title = title;
            Glyph = glyph;
            Content = content;
        }

        public string Title { get; private set; }

        /// <summary>Знак раздела в меню.</summary>
        public string Glyph { get; private set; }

        /// <summary>Модель представления раздела; сопоставление с разметкой задаётся DataTemplate.</summary>
        public object Content { get; private set; }
    }

    /// <summary>
    /// Оболочка приложения: верхняя панель с меню, боковое меню разделов,
    /// область содержимого и строка состояния.
    /// Состояние установки, рецепт и признак связи видны постоянно (п. 4.4).
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;
        private readonly INetworkService _network;
        private readonly IJournalService _journal;
        private readonly IDialogService _dialogs;
        private readonly DispatcherTimer _clock;

        private SectionViewModel _currentSection;

        public ShellViewModel(IMachineService machine, IRecipeService recipes, ISettingsService settings,
            INetworkService network, IJournalService journal, ITrendService trends, IVideoService video,
            IDialogService dialogs)
        {
            _machine = machine;
            _network = network;
            _journal = journal;
            _dialogs = dialogs;

            Video = new VideoViewModel(video, dialogs);
            MainScreen = new MainScreenViewModel(machine, network, dialogs, Video);
            Parameters = new ParametersViewModel(machine, trends, dialogs);
            CycleSettings = new CycleSettingsViewModel(settings, machine, dialogs);
            Recipes = new RecipesViewModel(recipes, machine, dialogs);
            Journal = new JournalViewModel(journal, dialogs);
            Network = new NetworkViewModel(network, dialogs);

            Sections = new ObservableCollection<SectionViewModel>
            {
                new SectionViewModel("Главный экран", "▦", MainScreen),
                new SectionViewModel("Параметры и графики", "⌗", Parameters),
                new SectionViewModel("Настройки цикла", "⚙", CycleSettings),
                new SectionViewModel("Рецепты", "☰", Recipes),
                new SectionViewModel("Журнал и отчёты", "≡", Journal),
                new SectionViewModel("Сеть и подключение", "⇄", Network),
                new SectionViewModel("Видеонаблюдение", "▷", Video)
            };

            _currentSection = Sections[0];

            SelectSectionCommand = new RelayCommand(SelectSection, delegate { return true; });
            LoadRecipeCommand = new RelayCommand(LoadRecipe);
            ExportReportCommand = new RelayCommand(ExportReport);
            ExportJournalCommand = new RelayCommand(ExportJournal);
            ConnectCommand = new RelayCommand(Connect, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);
            ToggleMnemonicCommand = new RelayCommand(ToggleMnemonic);
            SimulateAlarmCommand = new RelayCommand(SimulateAlarm, delegate { return IsDemoMode; });
            SimulateStartBlockCommand = new RelayCommand(SimulateStartBlock, delegate { return IsDemoMode; });
            AboutCommand = new RelayCommand(About);

            _machine.Updated += delegate { RefreshStatus(); };
            _network.StatusChanged += delegate { RefreshStatus(); };

            _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clock.Tick += delegate { OnPropertyChanged("ClockText"); };
            _clock.Start();
        }

        public string WindowTitle
        {
            get { return "Установка дозирования и упаковки жидких особо чистых веществ — рабочее место оператора"; }
        }

        public ObservableCollection<SectionViewModel> Sections { get; private set; }

        public MainScreenViewModel MainScreen { get; private set; }
        public ParametersViewModel Parameters { get; private set; }
        public CycleSettingsViewModel CycleSettings { get; private set; }
        public RecipesViewModel Recipes { get; private set; }
        public JournalViewModel Journal { get; private set; }
        public NetworkViewModel Network { get; private set; }
        public VideoViewModel Video { get; private set; }

        public ICommand SelectSectionCommand { get; private set; }
        public ICommand LoadRecipeCommand { get; private set; }
        public ICommand ExportReportCommand { get; private set; }
        public ICommand ExportJournalCommand { get; private set; }
        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand ToggleMnemonicCommand { get; private set; }
        public ICommand SimulateAlarmCommand { get; private set; }
        public ICommand SimulateStartBlockCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }

        public SectionViewModel CurrentSection
        {
            get { return _currentSection; }
            set { SetProperty(ref _currentSection, value); }
        }

        /// <summary>Работа на демонстрационных данных: пункт имитации аварии доступен только в этом режиме.</summary>
        public bool IsDemoMode { get { return _machine is DemoMachineService; } }

        // --- Строка состояния -------------------------------------------------

        public MachineState State { get { return _machine.State; } }
        public string StateText { get { return DisplayNames.Of(_machine.State); } }
        public string ModeText { get { return "Режим: " + DisplayNames.Of(_machine.Mode); } }
        public string StepText { get { return "Шаг цикла: " + _machine.CurrentStepName; } }

        public string RecipeText
        {
            get
            {
                var recipe = _machine.LoadedRecipe;
                return recipe == null
                    ? "Рецепт: не загружен"
                    : "Рецепт: " + recipe.Name + ", в. " + recipe.Version;
            }
        }

        public string BatchText
        {
            get
            {
                return "Партия " + _machine.Batch.Number + ": " + _machine.Batch.Good +
                       " из " + _machine.Batch.Target + " шт";
            }
        }

        public string ElapsedText
        {
            get { return "Наработка: " + _machine.Batch.Elapsed.ToString(@"hh\:mm\:ss"); }
        }

        public string RemainingText
        {
            get
            {
                if (!_machine.Batch.Remaining.HasValue) return "Оценка: —";
                return "До завершения: " + _machine.Batch.Remaining.Value.ToString(@"hh\:mm");
            }
        }

        public string ThroughputText
        {
            get { return _machine.Batch.Throughput.ToString("0.0") + " шт/мин"; }
        }

        public string OperatorText { get { return _machine.Batch.OperatorName; } }

        public string LinkText
        {
            get
            {
                var status = _network.Status;
                if (status.LinkState == PcLinkState.Established) return "Связь: установлена";
                if (status.LinkState == PcLinkState.AwaitingConfirmation) return "Связь: ожидание Ключа 2";
                return status.LinkUp ? "Связь: кабель есть, соединения нет" : "Связь: нет";
            }
        }

        public bool IsLinkOk { get { return _network.Status.LinkState == PcLinkState.Established; } }

        public string MemoryText { get { return "Память журнала: " + _journal.MemoryUsedPercent + " %"; } }

        public string ClockText { get { return DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss"); } }

        private void RefreshStatus()
        {
            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
            OnPropertyChanged("ModeText");
            OnPropertyChanged("StepText");
            OnPropertyChanged("RecipeText");
            OnPropertyChanged("BatchText");
            OnPropertyChanged("ElapsedText");
            OnPropertyChanged("RemainingText");
            OnPropertyChanged("ThroughputText");
            OnPropertyChanged("OperatorText");
            OnPropertyChanged("LinkText");
            OnPropertyChanged("IsLinkOk");
            OnPropertyChanged("MemoryText");
        }

        private void SelectSection(object parameter)
        {
            var section = parameter as SectionViewModel;
            if (section != null) CurrentSection = section;
        }

        private void LoadRecipe()
        {
            if (_dialogs == null) return;
            _dialogs.SelectRecipe();
            RefreshStatus();
            MainScreen.Refresh();
        }

        private void ExportReport()
        {
            CurrentSection = Sections[4];
            Journal.BuildReportCommand.Execute(null);
        }

        private void ExportJournal()
        {
            Journal.ExportCommand.Execute(null);
        }

        private bool CanConnect()
        {
            return _network.Status.LinkState != PcLinkState.Established;
        }

        private void Connect()
        {
            if (_dialogs == null) return;
            _dialogs.ShowConnection();
            RefreshStatus();
        }

        private bool CanDisconnect()
        {
            return _network.Status.LinkState != PcLinkState.None;
        }

        private void Disconnect()
        {
            _network.Disconnect();
            RefreshStatus();
        }

        private void ToggleMnemonic()
        {
            MainScreen.Mnemonic.Mode = MainScreen.Mnemonic.Mode == MnemonicMode.ByStation
                ? MnemonicMode.ByVial
                : MnemonicMode.ByStation;

            CurrentSection = Sections[0];
        }

        private void SimulateAlarm()
        {
            var demo = _machine as DemoMachineService;
            if (demo == null) return;

            demo.SimulateAlarm();
            CurrentSection = Sections[0];
            MainScreen.Refresh();
        }

        private void SimulateStartBlock()
        {
            var demo = _machine as DemoMachineService;
            if (demo == null) return;

            demo.ToggleStartBlock();
            CurrentSection = Sections[0];
            MainScreen.Refresh();
        }

        private void About()
        {
            if (_dialogs == null) return;

            _dialogs.ShowMessage("О программе",
                "Программное обеспечение для ПК под управлением ОС Windows.\n" +
                "Установка автоматизированного дозирования и упаковки жидких особо чистых веществ " +
                "в условиях контролируемой атмосферы.\n\n" +
                "Интерфейс выполнен по документам: «Перечень параметров, выводимых оператору», " +
                "«Функциональный интерфейс управления установкой», " +
                "«Подключение установки к персональному компьютеру».");
        }
    }
}
