using System;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Краткий признак связи с установкой на главном экране.
    /// Подробные сведения открываются в разделе «Сеть и подключение» (п. 3 документа
    /// «Подключение установки к персональному компьютеру»).
    /// </summary>
    public class ConnectionPanelViewModel : ViewModelBase
    {
        private readonly INetworkService _network;
        private readonly IDialogService _dialogs;

        public ConnectionPanelViewModel(INetworkService network, IDialogService dialogs)
        {
            _network = network;
            _dialogs = dialogs;

            ConnectCommand = new RelayCommand(Connect, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);

            _network.StatusChanged += delegate { Refresh(); };
        }

        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }

        private NetworkStatus Status { get { return _network.Status; } }

        public PcLinkState LinkState { get { return Status.LinkState; } }
        public string LinkStateText { get { return DisplayNames.Of(Status.LinkState); } }
        public bool IsConnected { get { return Status.LinkState == PcLinkState.Established; } }

        /// <summary>Наличие физического соединения проверяется в первую очередь.</summary>
        public bool LinkUp { get { return Status.LinkUp; } }

        public string PhysicalText
        {
            get { return Status.LinkUp ? "есть" : "нет"; }
        }

        public string SpeedText
        {
            get { return Status.LinkUp ? Status.SpeedMbits + " Мбит/с, " + DisplayNames.Of(Status.Duplex) : "—"; }
        }

        public string AddressText
        {
            get { return Status.IpAddress + " / " + Status.SubnetMask; }
        }

        public string AddressModeText { get { return DisplayNames.Of(Status.AddressMode); } }

        public string MacText { get { return Status.MacAddress; } }

        public string ClientText
        {
            get
            {
                if (Status.LinkState != PcLinkState.Established) return "—";
                return Status.ClientName + " (" + Status.ClientAddress + ")";
            }
        }

        public string ConnectedSinceText
        {
            get
            {
                if (!Status.ConnectedSince.HasValue) return "—";
                return Status.ConnectedSince.Value.ToString("HH:mm:ss") + ", " +
                       FormatDuration(DateTime.Now - Status.ConnectedSince.Value);
            }
        }

        public string LatencyText { get { return Status.LatencyMs + " мс"; } }

        public string ErrorsText
        {
            get { return "ошибок " + Status.ErrorCount + " / потерь " + Status.LostPacketCount; }
        }

        public string LastDisconnectText
        {
            get { return Status.LastDisconnect.HasValue ? Status.LastDisconnect.Value.ToString("dd.MM HH:mm") : "—"; }
        }

        /// <summary>Заполнение памяти журнала установки; при 80 % выводится предупреждение.</summary>
        public int MemoryPercent { get { return Status.JournalMemoryPercent; } }

        public bool MemoryWarning { get { return Status.JournalMemoryPercent >= 80; } }

        public void Refresh()
        {
            OnPropertyChanged("LinkState");
            OnPropertyChanged("LinkStateText");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("LinkUp");
            OnPropertyChanged("PhysicalText");
            OnPropertyChanged("SpeedText");
            OnPropertyChanged("AddressText");
            OnPropertyChanged("AddressModeText");
            OnPropertyChanged("MacText");
            OnPropertyChanged("ClientText");
            OnPropertyChanged("ConnectedSinceText");
            OnPropertyChanged("LatencyText");
            OnPropertyChanged("ErrorsText");
            OnPropertyChanged("LastDisconnectText");
            OnPropertyChanged("MemoryPercent");
            OnPropertyChanged("MemoryWarning");
        }

        private static string FormatDuration(TimeSpan value)
        {
            if (value.TotalHours >= 1) return ((int)value.TotalHours) + " ч " + value.Minutes + " мин";
            if (value.TotalMinutes >= 1) return value.Minutes + " мин";
            return value.Seconds + " с";
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
    }
}
