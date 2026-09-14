using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Консоль событий главного экрана: поток сообщений установки и введённых команд.
    /// Постоянное хранение обеспечивает журнал, консоль показывает последние события.
    /// </summary>
    public class ConsolePanelViewModel : ViewModelBase
    {
        private const int Capacity = 500;

        private readonly IMachineService _machine;
        private string _input = string.Empty;
        private bool _autoScroll = true;
        private bool _showInfo = true;
        private bool _showWarnings = true;
        private bool _showErrors = true;

        public ConsolePanelViewModel(IMachineService machine)
        {
            _machine = machine;
            Items = new ObservableCollection<ConsoleItemViewModel>();

            SendCommand = new RelayCommand(Send, CanSend);
            ClearCommand = new RelayCommand(Clear);

            foreach (var record in _machine.GetConsoleHistory())
            {
                Items.Add(new ConsoleItemViewModel(record));
            }

            _machine.ConsoleLineAdded += OnConsoleLineAdded;
        }

        public ObservableCollection<ConsoleItemViewModel> Items { get; private set; }

        public ICommand SendCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        /// <summary>Строка ввода команды. Обработку принимает на себя контроллер.</summary>
        public string Input
        {
            get { return _input; }
            set { SetProperty(ref _input, value); }
        }

        public bool AutoScroll
        {
            get { return _autoScroll; }
            set { SetProperty(ref _autoScroll, value); }
        }

        public bool ShowInfo
        {
            get { return _showInfo; }
            set { SetProperty(ref _showInfo, value); }
        }

        public bool ShowWarnings
        {
            get { return _showWarnings; }
            set { SetProperty(ref _showWarnings, value); }
        }

        public bool ShowErrors
        {
            get { return _showErrors; }
            set { SetProperty(ref _showErrors, value); }
        }

        public ConsoleItemViewModel LastItem
        {
            get { return Items.Count > 0 ? Items[Items.Count - 1] : null; }
        }

        private void OnConsoleLineAdded(object sender, ConsoleRecord record)
        {
            Items.Add(new ConsoleItemViewModel(record));
            while (Items.Count > Capacity) Items.RemoveAt(0);
            OnPropertyChanged("LastItem");
        }

        private bool CanSend()
        {
            return !string.IsNullOrWhiteSpace(_input);
        }

        private void Send()
        {
            // Заглушка фасада: команда добавляется в ленту, разбор выполняет контроллер.
            var record = new ConsoleRecord
            {
                Time = DateTime.Now,
                Kind = ConsoleLineKind.Command,
                Text = "> " + _input
            };

            Items.Add(new ConsoleItemViewModel(record));
            Input = string.Empty;
            OnPropertyChanged("LastItem");
        }

        private void Clear()
        {
            Items.Clear();
            OnPropertyChanged("LastItem");
        }
    }
}
