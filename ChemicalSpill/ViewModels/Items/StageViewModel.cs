using ChemicalSpill.Models;

namespace ChemicalSpill.ViewModels.Items
{
    /// <summary>
    /// Блок стадии мнемосхемы. Помимо цвета выводит текстовое наименование
    /// состояния и графический знак — обязательное требование п. 2.3.
    /// </summary>
    public class StageViewModel : ViewModelBase
    {
        private StageSnapshot _snapshot;

        public StageViewModel(StageSnapshot snapshot, bool isAuxiliary)
        {
            _snapshot = snapshot;
            IsAuxiliary = isAuxiliary;
        }

        /// <summary>Вспомогательный блок (шлюзы, индексация) — рисуется компактнее.</summary>
        public bool IsAuxiliary { get; private set; }

        public StageId Id { get { return _snapshot.Id; } }

        public string Title { get { return DisplayNames.Of(_snapshot.Id); } }

        /// <summary>
        /// Входящая стрелка слева: стадии соединяются в направлении движения флакона.
        /// У первой стадии цепочки стрелки нет.
        /// </summary>
        public bool ShowInboundArrow { get { return !IsAuxiliary && _snapshot.Id != StageId.Feed; } }

        public StageState State { get { return _snapshot.State; } }

        public string StateText { get { return DisplayNames.Of(_snapshot.State); } }

        /// <summary>Графический знак состояния.</summary>
        public string StateGlyph { get { return DisplayNames.GlyphOf(_snapshot.State); } }

        public string KeyCaption { get { return _snapshot.KeyCaption; } }

        public string KeyValue { get { return _snapshot.KeyValue; } }

        /// <summary>Время выполнения текущей операции и заданный таймаут.</summary>
        public string TimeText
        {
            get
            {
                if (_snapshot.TimeoutSeconds <= 0) return "—";
                return _snapshot.ElapsedSeconds.ToString("0.0") + " с из " + _snapshot.TimeoutSeconds.ToString("0") + " с";
            }
        }

        public double TimeProgress
        {
            get
            {
                if (_snapshot.TimeoutSeconds <= 0) return 0;
                double value = _snapshot.ElapsedSeconds / _snapshot.TimeoutSeconds * 100.0;
                return value > 100 ? 100 : value;
            }
        }

        public bool HasTimeout { get { return _snapshot.TimeoutSeconds > 0; } }

        /// <summary>Номер гнезда ротора, обрабатываемого станцией.</summary>
        public string NestText
        {
            get { return _snapshot.NestNumber > 0 ? "гнездо " + _snapshot.NestNumber : "—"; }
        }

        public bool HasNest { get { return _snapshot.NestNumber > 0; } }

        /// <summary>Счётчик обработанных флаконов и счётчик отказов по станции.</summary>
        public string CountersText
        {
            get { return _snapshot.ProcessedCount + " шт / отказов " + _snapshot.FaultCount; }
        }

        public bool HasCounters { get { return !IsAuxiliary; } }

        /// <summary>Причина отказа выводится текстом рядом с блоком.</summary>
        public string FaultReason { get { return _snapshot.FaultReason; } }

        public bool HasFault
        {
            get { return _snapshot.State == StageState.Fault || !string.IsNullOrEmpty(_snapshot.FaultReason); }
        }

        /// <summary>Обновление блока новыми данными без пересоздания элемента списка.</summary>
        public void Update(StageSnapshot snapshot)
        {
            _snapshot = snapshot;
            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
            OnPropertyChanged("StateGlyph");
            OnPropertyChanged("KeyCaption");
            OnPropertyChanged("KeyValue");
            OnPropertyChanged("TimeText");
            OnPropertyChanged("TimeProgress");
            OnPropertyChanged("HasTimeout");
            OnPropertyChanged("NestText");
            OnPropertyChanged("HasNest");
            OnPropertyChanged("CountersText");
            OnPropertyChanged("FaultReason");
            OnPropertyChanged("HasFault");
        }
    }
}
