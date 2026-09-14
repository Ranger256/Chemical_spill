using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Sections;

namespace ChemicalSpill.ViewModels.Main
{
    /// <summary>
    /// Главный рабочий экран. Раскладка блоков:
    /// слева вверху — основные параметры, слева внизу — изображение с камеры,
    /// в центре вверху — управление, в центре внизу — мнемосхема,
    /// справа вверху — подключение к установке, справа в середине — аварии,
    /// справа внизу — консоль.
    /// </summary>
    public class MainScreenViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;

        public MainScreenViewModel(IMachineService machine, INetworkService network,
            IDialogService dialogs, VideoViewModel video)
        {
            _machine = machine;

            Parameters = new KeyParametersViewModel(machine);
            Camera = video;
            Control = new ControlPanelViewModel(machine, dialogs);
            Mnemonic = new MnemonicViewModel(machine);
            Connection = new ConnectionPanelViewModel(network, dialogs);
            Alarms = new AlarmPanelViewModel(machine);
            Console = new ConsolePanelViewModel(machine);

            _machine.Updated += delegate { Refresh(); };
            _machine.AlarmRaised += delegate { Alarms.Refresh(); };
        }

        public string Title { get { return "Главный экран"; } }

        public KeyParametersViewModel Parameters { get; private set; }
        public VideoViewModel Camera { get; private set; }
        public ControlPanelViewModel Control { get; private set; }
        public MnemonicViewModel Mnemonic { get; private set; }
        public ConnectionPanelViewModel Connection { get; private set; }
        public AlarmPanelViewModel Alarms { get; private set; }
        public ConsolePanelViewModel Console { get; private set; }

        /// <summary>
        /// Экран заблокирован аварией: поверх рабочей области выводится красная панель,
        /// органы управления недоступны до квитирования оператором.
        /// </summary>
        public bool IsLocked { get { return Alarms.IsLocked; } }

        public void Refresh()
        {
            Parameters.Refresh();
            Control.Refresh();
            Mnemonic.Refresh();
            Connection.Refresh();
            Alarms.Refresh();

            OnPropertyChanged("IsLocked");
        }
    }
}
