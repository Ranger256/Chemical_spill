using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>Шаг процедуры установления соединения (п. 4.2).</summary>
    public class ConnectionStepViewModel : ViewModelBase
    {
        private bool _isDone;
        private bool _isCurrent;

        public ConnectionStepViewModel(int number, string text)
        {
            Number = number;
            Text = text;
        }

        public int Number { get; private set; }
        public string Text { get; private set; }

        public bool IsDone
        {
            get { return _isDone; }
            set { SetProperty(ref _isDone, value); }
        }

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { SetProperty(ref _isCurrent, value); }
        }
    }

    /// <summary>
    /// Раздел «Сеть и подключение»: состояние канала, параметры адресации,
    /// процедура подключения двумя ключами и качество связи.
    /// </summary>
    public class NetworkViewModel : ViewModelBase
    {
        private readonly INetworkService _network;
        private readonly IDialogService _dialogs;

        private AddressMode _addressMode;
        private string _ip;
        private string _mask;
        private string _gateway;
        private string _port;

        public NetworkViewModel(INetworkService network, IDialogService dialogs)
        {
            _network = network;
            _dialogs = dialogs;

            var status = _network.Status;
            _addressMode = status.AddressMode;
            _ip = status.IpAddress;
            _mask = status.SubnetMask;
            _gateway = status.Gateway;
            _port = status.Port.ToString();

            Steps = new ObservableCollection<ConnectionStepViewModel>
            {
                new ConnectionStepViewModel(1, "Кабель подключён; на панели отображается наличие физического соединения и сетевые параметры установки."),
                new ConnectionStepViewModel(2, "Пользователь указывает адрес установки и запускает подключение."),
                new ConnectionStepViewModel(3, "Программа запрашивает Ключ 1 и передаёт запрос установке."),
                new ConnectionStepViewModel(4, "При верном Ключе 1 установка переходит в состояние «ожидание подтверждения»."),
                new ConnectionStepViewModel(5, "Оператор у панели вводит Ключ 2. До этого момента доступ к данным не предоставляется."),
                new ConnectionStepViewModel(6, "Соединение установлено, в журнале делается запись.")
            };

            ConnectCommand = new RelayCommand(Connect, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);
            ApplyAddressingCommand = new RelayCommand(ApplyAddressing);

            _network.StatusChanged += delegate { Refresh(); };
            Refresh();
        }

        public string Title { get { return "Сеть и подключение"; } }

        public ObservableCollection<ConnectionStepViewModel> Steps { get; private set; }

        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand ApplyAddressingCommand { get; private set; }

        private NetworkStatus Status { get { return _network.Status; } }

        // --- Физическое подключение ------------------------------------------

        public bool LinkUp { get { return Status.LinkUp; } }
        public string LinkText { get { return Status.LinkUp ? "есть" : "нет"; } }

        public string SpeedText { get { return Status.SpeedMbits + " Мбит/с"; } }
        public string DuplexText { get { return DisplayNames.Of(Status.Duplex); } }
        public string MacText { get { return Status.MacAddress; } }

        public string PhysicalHint
        {
            get
            {
                return "Ethernet, RJ-45, 10/100/1000 Мбит/с с автосогласованием. Кабель экранированный, " +
                       "не ниже Cat. 5e, проложен отдельно от силовых линий. Отсутствие соединения указывает " +
                       "на обрыв, невставленный разъём или выключенный коммутатор.";
            }
        }

        // --- Адресация --------------------------------------------------------

        public bool IsDhcp
        {
            get { return _addressMode == AddressMode.Dhcp; }
            set
            {
                if (value && _addressMode == AddressMode.Dhcp) return;
                _addressMode = value ? AddressMode.Dhcp : AddressMode.Manual;
                OnPropertyChanged();
                OnPropertyChanged("IsManual");
            }
        }

        public bool IsManual
        {
            get { return _addressMode == AddressMode.Manual; }
            set
            {
                if (value && _addressMode == AddressMode.Manual) return;
                _addressMode = value ? AddressMode.Manual : AddressMode.Dhcp;
                OnPropertyChanged();
                OnPropertyChanged("IsDhcp");
            }
        }

        public string IpAddress
        {
            get { return _ip; }
            set { SetProperty(ref _ip, value); }
        }

        public string SubnetMask
        {
            get { return _mask; }
            set { SetProperty(ref _mask, value); }
        }

        public string Gateway
        {
            get { return _gateway; }
            set { SetProperty(ref _gateway, value); }
        }

        public string Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }

        // --- Соединение с установкой -----------------------------------------

        public PcLinkState LinkState { get { return Status.LinkState; } }
        public string LinkStateText { get { return DisplayNames.Of(Status.LinkState); } }
        public bool IsConnected { get { return Status.LinkState == PcLinkState.Established; } }

        public string ClientText
        {
            get
            {
                if (Status.LinkState != PcLinkState.Established) return "—";
                return Status.ClientName + " · " + Status.ClientAddress;
            }
        }

        public string ConnectedSinceText
        {
            get { return Status.ConnectedSince.HasValue ? Status.ConnectedSince.Value.ToString("dd.MM.yyyy HH:mm:ss") : "—"; }
        }

        // --- Качество канала --------------------------------------------------

        public string LatencyText { get { return Status.LatencyMs + " мс"; } }
        public string ErrorsText { get { return Status.ErrorCount.ToString(); } }
        public string LostText { get { return Status.LostPacketCount.ToString(); } }

        public string LastDisconnectText
        {
            get { return Status.LastDisconnect.HasValue ? Status.LastDisconnect.Value.ToString("dd.MM.yyyy HH:mm:ss") : "—"; }
        }

        public int MemoryPercent { get { return Status.JournalMemoryPercent; } }
        public bool MemoryWarning { get { return Status.JournalMemoryPercent >= 80; } }

        // --- Защита соединения ------------------------------------------------

        public string FailedAttemptsText
        {
            get { return Status.FailedAttempts + " подряд"; }
        }

        public bool IsBlocked { get { return Status.TemporarilyBlocked; } }

        public string SecurityHint
        {
            get
            {
                return "Ключ 1 подтверждает право доступа и вводится на ПК. Ключ 2 подтверждает присутствие " +
                       "человека у установки: он задаётся и изменяется только с панели оператора, по сети не " +
                       "передаётся и проверяется локально. Одновременно допускается только одно активное соединение.";
            }
        }

        public string PriorityHint
        {
            get
            {
                return "Панель оператора имеет приоритет: оператор у установки может в любой момент разорвать " +
                       "соединение или подать команду останова. Аварийная остановка остаётся аппаратной — " +
                       "команда останова с ПК рассматривается как штатный останов.";
            }
        }

        public string LinkLossHint
        {
            get
            {
                return "При разрыве связи установка продолжает выполнение партии автономно. Разрыв отображается " +
                       "и записывается в журнал, но аварией не является. Непереданные данные сохраняются в памяти " +
                       "установки и передаются после восстановления соединения.";
            }
        }

        public void Refresh()
        {
            OnPropertyChanged("LinkUp");
            OnPropertyChanged("LinkText");
            OnPropertyChanged("SpeedText");
            OnPropertyChanged("DuplexText");
            OnPropertyChanged("MacText");
            OnPropertyChanged("LinkState");
            OnPropertyChanged("LinkStateText");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("ClientText");
            OnPropertyChanged("ConnectedSinceText");
            OnPropertyChanged("LatencyText");
            OnPropertyChanged("ErrorsText");
            OnPropertyChanged("LostText");
            OnPropertyChanged("LastDisconnectText");
            OnPropertyChanged("MemoryPercent");
            OnPropertyChanged("MemoryWarning");
            OnPropertyChanged("FailedAttemptsText");
            OnPropertyChanged("IsBlocked");

            UpdateSteps();
        }

        private void UpdateSteps()
        {
            int reached;
            switch (Status.LinkState)
            {
                case PcLinkState.Established: reached = 6; break;
                case PcLinkState.AwaitingConfirmation: reached = 4; break;
                default: reached = Status.LinkUp ? 1 : 0; break;
            }

            foreach (var step in Steps)
            {
                step.IsDone = step.Number <= reached;
                step.IsCurrent = step.Number == reached + 1;
            }
        }

        private bool CanConnect()
        {
            return Status.LinkState != PcLinkState.Established;
        }

        private void Connect()
        {
            if (_dialogs == null) return;
            _dialogs.ShowConnection();
            Refresh();
        }

        private bool CanDisconnect()
        {
            return Status.LinkState != PcLinkState.None;
        }

        private void Disconnect()
        {
            _network.Disconnect();
            Refresh();
        }

        private void ApplyAddressing()
        {
            int port;
            if (!int.TryParse(_port, out port)) port = Status.Port;

            _network.ApplyAddressing(_addressMode, _ip, _mask, _gateway, port);
            Refresh();
        }
    }
}
