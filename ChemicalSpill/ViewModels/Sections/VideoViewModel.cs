using System.Windows.Input;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>
    /// Видеонаблюдение рабочей камеры (раздел 8 перечня параметров).
    /// Одна модель представления обслуживает и блок на главном экране,
    /// и полноразмерный раздел.
    /// </summary>
    public class VideoViewModel : ViewModelBase
    {
        private readonly IVideoService _video;

        public VideoViewModel(IVideoService video)
        {
            _video = video;

            ToggleLightCommand = new RelayCommand(ToggleLight);
            SnapshotCommand = new RelayCommand(Snapshot);
            ToggleStreamCommand = new RelayCommand(ToggleStream);

            _video.FrameReady += delegate { Refresh(); };
        }

        public string Title { get { return "Видеонаблюдение"; } }

        public ICommand ToggleLightCommand { get; private set; }
        public ICommand SnapshotCommand { get; private set; }
        public ICommand ToggleStreamCommand { get; private set; }

        public bool IsStreaming { get { return _video.IsStreaming; } }

        /// <summary>Кадр отсутствует: выводится заглушка «нет сигнала».</summary>
        public bool HasFrame { get { return _video.GetCurrentFrame() != null; } }

        public byte[] CurrentFrame { get { return _video.GetCurrentFrame(); } }

        public bool IsLightOn { get { return _video.IsLightOn; } }

        public string LightText { get { return _video.IsLightOn ? "Освещение включено" : "Освещение выключено"; } }

        public bool IsRecording { get { return _video.IsRecording; } }

        public string RecordingText { get { return _video.IsRecording ? "запись ведётся" : "только трансляция"; } }

        public string StreamText
        {
            get
            {
                return _video.IsStreaming
                    ? _video.Fps + " кадр/с · задержка " + _video.LatencyMs + " мс"
                    : "трансляция остановлена";
            }
        }

        public string PlaceholderText
        {
            get { return _video.IsStreaming ? "Ожидание кадра от установки" : "Трансляция остановлена"; }
        }

        public void Refresh()
        {
            OnPropertyChanged("IsStreaming");
            OnPropertyChanged("HasFrame");
            OnPropertyChanged("CurrentFrame");
            OnPropertyChanged("IsLightOn");
            OnPropertyChanged("LightText");
            OnPropertyChanged("IsRecording");
            OnPropertyChanged("RecordingText");
            OnPropertyChanged("StreamText");
            OnPropertyChanged("PlaceholderText");
        }

        private void ToggleLight()
        {
            _video.SetLight(!_video.IsLightOn);
            Refresh();
        }

        private void Snapshot()
        {
            _video.TakeSnapshot(null);
        }

        private void ToggleStream()
        {
            if (_video.IsStreaming) _video.StopStream();
            else _video.StartStream();
            Refresh();
        }
    }
}
