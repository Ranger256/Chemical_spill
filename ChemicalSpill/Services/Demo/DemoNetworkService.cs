using System;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Демонстрационная реализация <see cref="INetworkService"/>.
    /// Воспроизводит последовательность п. 4.2: Ключ 1 с ПК, затем ожидание
    /// подтверждения Ключом 2 на панели оператора.
    /// </summary>
    public class DemoNetworkService : INetworkService
    {
        private readonly NetworkStatus _status;

        public DemoNetworkService()
        {
            _status = new NetworkStatus
            {
                LinkUp = true,
                SpeedMbits = 1000,
                Duplex = DuplexMode.Full,
                AddressMode = AddressMode.Manual,
                IpAddress = "192.168.10.10",
                SubnetMask = "255.255.255.0",
                Gateway = "192.168.10.1",
                MacAddress = "00:1B:44:11:3A:B7",
                Port = 5020,
                LinkState = PcLinkState.Established,
                ClientName = "WS-TECH-04",
                ClientAddress = "192.168.10.25",
                ConnectedSince = DateTime.Now.AddMinutes(-14),
                LatencyMs = 4,
                ErrorCount = 0,
                LostPacketCount = 0,
                LastDisconnect = DateTime.Now.AddHours(-6),
                JournalMemoryPercent = 42,
                FailedAttempts = 0,
                TemporarilyBlocked = false,
                ConfirmationTimeoutSeconds = 60
            };
        }

        public NetworkStatus Status { get { return _status; } }

        public event EventHandler StatusChanged;
        public event EventHandler LinkLost;

        public ConnectResult Connect(string address, int port, string key1)
        {
            if (string.IsNullOrEmpty(key1))
            {
                _status.FailedAttempts++;
                Notify();
                return new ConnectResult(false, PcLinkState.None, "Ключ 1 не введён. Попытка отклонена.");
            }

            _status.LinkState = PcLinkState.AwaitingConfirmation;
            Notify();
            return new ConnectResult(true, PcLinkState.AwaitingConfirmation,
                "Ключ 1 принят. Запрос выведен на панель установки: требуется ввод Ключа 2 оператором у машины.");
        }

        public ConnectResult WaitForPanelConfirmation()
        {
            _status.LinkState = PcLinkState.Established;
            _status.ClientName = Environment.MachineName;
            _status.ClientAddress = "192.168.10.25";
            _status.ConnectedSince = DateTime.Now;
            Notify();
            return new ConnectResult(true, PcLinkState.Established, "Соединение установлено.");
        }

        public void Disconnect()
        {
            _status.LinkState = PcLinkState.None;
            _status.ClientName = null;
            _status.ClientAddress = null;
            _status.ConnectedSince = null;
            _status.LastDisconnect = DateTime.Now;
            Notify();

            var handler = LinkLost;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void ApplyAddressing(AddressMode mode, string ip, string mask, string gateway, int port)
        {
            _status.AddressMode = mode;
            _status.IpAddress = ip;
            _status.SubnetMask = mask;
            _status.Gateway = gateway;
            _status.Port = port;
            Notify();
        }

        private void Notify()
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
