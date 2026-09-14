using ChemicalSpill.Models;

namespace ChemicalSpill.ViewModels.Items
{
    /// <summary>
    /// Кривая графика с признаком выбора. Линии аварийных уставок выводятся,
    /// чтобы запас до порога был виден сразу (п. 2.1 перечня параметров).
    /// </summary>
    public class TrendSeriesViewModel : ViewModelBase
    {
        private bool _isSelected;
        private TrendSeries _series;

        public TrendSeriesViewModel(string key, string caption, string unit, bool isSelected)
        {
            Key = key;
            Caption = caption;
            Unit = unit;
            _isSelected = isSelected;
        }

        public string Key { get; private set; }
        public string Caption { get; private set; }
        public string Unit { get; private set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>Данные кривой; заполняются сервисом графиков.</summary>
        public TrendSeries Series
        {
            get { return _series; }
            set { SetProperty(ref _series, value); }
        }

        public string LimitText
        {
            get
            {
                if (_series == null) return string.Empty;
                if (_series.HighLimit.HasValue && _series.LowLimit.HasValue)
                    return "границы " + _series.LowLimit.Value.ToString("0.##") + " … " + _series.HighLimit.Value.ToString("0.##");
                if (_series.HighLimit.HasValue) return "порог " + _series.HighLimit.Value.ToString("0.##");
                if (_series.LowLimit.HasValue) return "порог " + _series.LowLimit.Value.ToString("0.##");
                return "порог не задан";
            }
        }

        /// <summary>Единица измерения и порог одной строкой для списка кривых.</summary>
        public string SubtitleText
        {
            get
            {
                var limit = LimitText;
                if (string.IsNullOrEmpty(Unit)) return limit;
                return string.IsNullOrEmpty(limit) ? Unit : Unit + " · " + limit;
            }
        }

        public string LastValueText
        {
            get
            {
                if (_series == null || _series.Points.Count == 0) return "—";
                return _series.Points[_series.Points.Count - 1].Value.ToString("0.##");
            }
        }
    }
}
