using System;
using System.Windows.Threading;
using ChemicalSpill.Models;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Dialogs
{
    /// <summary>
    /// Мастер подключения к установке. Порядок по п. 4.2: адрес и Ключ 1 вводятся
    /// на ПК, затем оператор у панели вводит Ключ 2. Время ожидания Ключа 2
    /// ограничено, по истечении процедуру требуется начать заново.
    /// </summary>
    public class ConnectViewModel : ViewModelBase
    {
        private readonly INetworkService _network;
        private readonly DispatcherTimer _timer;

        private string _address;
        private string _port;
        private string _message;
        private bool _isError;
        private int _remainingSeconds;

        public ConnectViewModel(INetworkService network)
        {
            _network = network;

            _address = network.Status.IpAddress;
            _port = network.Status.Port.ToString();
            _message = "Укажите адрес установки и введите Ключ 1.";

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTick;
        }

        /// <summary>Соединение установлено — окно закрывается с положительным результатом.</summary>
        public bool IsEstablished { get; private set; }

        /// <summary>Окно следует закрыть.</summary>
        public event EventHandler CloseRequested;

        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        public string Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }

        public PcLinkState State { get { return _network.Status.LinkState; } }

        public string StateText { get { return DisplayNames.Of(_network.Status.LinkState); } }

        /// <summary>Идёт ожидание ввода Ключа 2 на панели оператора.</summary>
        public bool IsWaiting
        {
            get { return _network.Status.LinkState == PcLinkState.AwaitingConfirmation; }
        }

        public bool CanEnterKey
        {
            get { return _network.Status.LinkState == PcLinkState.None; }
        }

        public string Message
        {
            get { return _message; }
            private set { SetProperty(ref _message, value); }
        }

        public bool IsError
        {
            get { return _isError; }
            private set { SetProperty(ref _isError, value); }
        }

        public int RemainingSeconds
        {
            get { return _remainingSeconds; }
            private set
            {
                if (SetProperty(ref _remainingSeconds, value)) OnPropertyChanged("RemainingText");
            }
        }

        public string RemainingText
        {
            get { return "Осталось " + _remainingSeconds + " с"; }
        }

        public int TimeoutSeconds { get { return _network.Status.ConfirmationTimeoutSeconds; } }

        public string Key2Hint
        {
            get
            {
                return "Ключ 2 вводится на панели оператора и подтверждает, что подключение разрешил человек, " +
                       "физически находящийся у установки. По сети он не передаётся.";
            }
        }

        /// <summary>
        /// Шаг 3 процедуры. Ключ 1 передаётся из PasswordBox напрямую, чтобы
        /// не хранить его в свойстве модели представления.
        /// </summary>
        public void Connect(string key1)
        {
            var result = _network.Connect(_address, ParsePort(), key1);

            Message = result.Message;
            IsError = !result.Accepted;
            RaiseStateChanged();

            if (!result.Accepted) return;

            RemainingSeconds = TimeoutSeconds;
            _timer.Start();
        }

        /// <summary>Имитация ввода Ключа 2 оператором у панели — только для просмотра фасада.</summary>
        public void ConfirmOnPanel()
        {
            _timer.Stop();

            var result = _network.WaitForPanelConfirmation();
            Message = result.Message;
            IsError = !result.Accepted;
            IsEstablished = result.Accepted;
            RaiseStateChanged();

            if (result.Accepted) Close();
        }

        public void Cancel()
        {
            _timer.Stop();

            if (_network.Status.LinkState == PcLinkState.AwaitingConfirmation) _network.Disconnect();
            Close();
        }

        private void OnTick(object sender, EventArgs e)
        {
            RemainingSeconds = RemainingSeconds - 1;
            if (RemainingSeconds > 0) return;

            _timer.Stop();
            _network.Disconnect();

            Message = "Время ожидания Ключа 2 истекло. Запрос отклонён, процедуру требуется начать заново.";
            IsError = true;
            RaiseStateChanged();
        }

        private int ParsePort()
        {
            int port;
            return int.TryParse(_port, out port) ? port : _network.Status.Port;
        }

        private void RaiseStateChanged()
        {
            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
            OnPropertyChanged("IsWaiting");
            OnPropertyChanged("CanEnterKey");
        }

        private void Close()
        {
            var handler = CloseRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
