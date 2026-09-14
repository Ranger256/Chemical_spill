using System;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Связь с установкой по Ethernet и процедура подключения двумя ключами.
    /// Источник: «Подключение установки к персональному компьютеру».
    /// </summary>
    public interface INetworkService
    {
        /// <summary>Текущее состояние канала и соединения.</summary>
        NetworkStatus Status { get; }

        /// <summary>
        /// Шаг 2–3 процедуры: указание адреса установки и передача Ключа 1.
        /// Ключ 1 не передаётся по сети в открытом виде. При верном ключе установка
        /// переходит в состояние «ожидание подтверждения» и запрашивает Ключ 2 на панели.
        /// </summary>
        ConnectResult Connect(string address, int port, string key1);

        /// <summary>
        /// Ожидание ввода Ключа 2 оператором у панели. Время ожидания ограничено
        /// (предварительно 60 с), по истечении запрос отклоняется.
        /// </summary>
        ConnectResult WaitForPanelConfirmation();

        /// <summary>Разрыв соединения. Оператор у панели может сделать это в любой момент.</summary>
        void Disconnect();

        /// <summary>Задание способа получения адреса и сетевых параметров.</summary>
        void ApplyAddressing(AddressMode mode, string ip, string mask, string gateway, int port);

        /// <summary>Состояние сети изменилось.</summary>
        event EventHandler StatusChanged;

        /// <summary>Связь разорвана: отображается и записывается в журнал, но аварией не является.</summary>
        event EventHandler LinkLost;
    }
}
