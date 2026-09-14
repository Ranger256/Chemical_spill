using System;

namespace ChemicalSpill.Models
{
    /// <summary>
    /// Состояние сети и соединения с установкой.
    /// Источник: «Подключение установки к персональному компьютеру», п. 3.
    /// </summary>
    public class NetworkStatus
    {
        /// <summary>Наличие физического соединения: обнаружен ли сигнал в кабеле.</summary>
        public bool LinkUp { get; set; }

        /// <summary>Согласованная скорость канала, Мбит/с: 10 / 100 / 1000.</summary>
        public int SpeedMbits { get; set; }

        /// <summary>Режим передачи — результат автосогласования.</summary>
        public DuplexMode Duplex { get; set; }

        /// <summary>Способ получения адреса установкой.</summary>
        public AddressMode AddressMode { get; set; }

        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }

        /// <summary>Неизменяемый идентификатор устройства.</summary>
        public string MacAddress { get; set; }

        public int Port { get; set; }

        /// <summary>Стадия процедуры подключения.</summary>
        public PcLinkState LinkState { get; set; }

        /// <summary>Имя подключённого компьютера.</summary>
        public string ClientName { get; set; }

        /// <summary>Адрес подключённого компьютера.</summary>
        public string ClientAddress { get; set; }

        /// <summary>С какого момента клиент подключён.</summary>
        public DateTime? ConnectedSince { get; set; }

        /// <summary>Задержка обмена, мс.</summary>
        public int LatencyMs { get; set; }

        /// <summary>Накопительный счётчик ошибок канала.</summary>
        public long ErrorCount { get; set; }

        /// <summary>Накопительный счётчик потерянных пакетов.</summary>
        public long LostPacketCount { get; set; }

        /// <summary>Время последнего разрыва связи.</summary>
        public DateTime? LastDisconnect { get; set; }

        /// <summary>Заполнение памяти журнала установки, % (предупреждение при 80 %).</summary>
        public int JournalMemoryPercent { get; set; }

        /// <summary>Число неудачных попыток ввода Ключа 1 подряд.</summary>
        public int FailedAttempts { get; set; }

        /// <summary>Подключения временно заблокированы после превышения числа попыток.</summary>
        public bool TemporarilyBlocked { get; set; }

        /// <summary>Остаток времени на ввод Ключа 2 на панели оператора, с (предварительно 60 с).</summary>
        public int ConfirmationTimeoutSeconds { get; set; }
    }

    /// <summary>Результат попытки подключения к установке.</summary>
    public class ConnectResult
    {
        public bool Accepted { get; set; }

        /// <summary>Текст причины отказа для вывода пользователю.</summary>
        public string Message { get; set; }

        /// <summary>Достигнутая стадия процедуры подключения.</summary>
        public PcLinkState State { get; set; }

        public ConnectResult()
        {
        }

        public ConnectResult(bool accepted, PcLinkState state, string message)
        {
            Accepted = accepted;
            State = state;
            Message = message;
        }
    }
}
