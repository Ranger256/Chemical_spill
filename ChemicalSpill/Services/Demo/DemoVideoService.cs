using System;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Демонстрационная реализация <see cref="IVideoService"/>. Кадров не отдаёт:
    /// представление показывает заглушку «нет сигнала» до подключения ядра.
    /// </summary>
    public class DemoVideoService : IVideoService
    {
        public bool IsStreaming { get; private set; }
        public bool IsLightOn { get; private set; }
        public bool IsRecording { get; private set; }
        public int Fps { get { return IsStreaming ? 25 : 0; } }
        public int LatencyMs { get { return IsStreaming ? 120 : 0; } }

        public DemoVideoService()
        {
            IsStreaming = true;
            IsLightOn = true;
            IsRecording = false;
        }

        public event EventHandler FrameReady;

        public byte[] GetCurrentFrame()
        {
            return null;
        }

        public void SetLight(bool on)
        {
            IsLightOn = on;
            Notify();
        }

        public void TakeSnapshot(string path)
        {
        }

        public void StartStream()
        {
            IsStreaming = true;
            Notify();
        }

        public void StopStream()
        {
            IsStreaming = false;
            Notify();
        }

        private void Notify()
        {
            var handler = FrameReady;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
