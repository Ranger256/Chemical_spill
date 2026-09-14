using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;
using ChemicalSpill.ViewModels.Items;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>
    /// Раздел «Параметры и графики»: полный перечень величин, выводимых оператору,
    /// и графики во времени с линиями аварийных уставок.
    /// </summary>
    public class ParametersViewModel : ViewModelBase
    {
        private readonly IMachineService _machine;
        private readonly ITrendService _trends;

        private string _selectedGroup;
        private TrendWindow _window = TrendWindow.LastTenMinutes;
        private bool _isPaused;

        public ParametersViewModel(IMachineService machine, ITrendService trends, IDialogService dialogs)
        {
            _machine = machine;
            _trends = trends;

            Groups = new ObservableCollection<string>();
            Parameters = new ObservableCollection<ParameterItemViewModel>();
            Series = new ObservableCollection<TrendSeriesViewModel>();
            Windows = new ObservableCollection<TrendWindow>
            {
                TrendWindow.LastTenMinutes,
                TrendWindow.LastHour,
                TrendWindow.CurrentBatch
            };

            Series.Add(new TrendSeriesViewModel("o2", "Содержание кислорода O₂", "ppm", true));
            Series.Add(new TrendSeriesViewModel("h2o", "Содержание влаги H₂O", "ppm", true));
            Series.Add(new TrendSeriesViewModel("pressure", "Избыточное давление в камере", "мбар", true));
            Series.Add(new TrendSeriesViewModel("productTemp", "Температура продукта", "°C", true));
            Series.Add(new TrendSeriesViewModel("chamberTemp", "Температура в камере", "°C", false));
            Series.Add(new TrendSeriesViewModel("vapour", "Содержание паров растворителя", "ppm", false));
            Series.Add(new TrendSeriesViewModel("doseDeviation", "Отклонение дозы", "%", false));
            Series.Add(new TrendSeriesViewModel("throughput", "Фактическая производительность", "шт/мин", false));

            foreach (var item in Series)
            {
                item.PropertyChanged += OnSeriesPropertyChanged;
            }

            var groups = new List<string>();
            foreach (var parameter in _machine.GetParameters(null))
            {
                if (!groups.Contains(parameter.Group)) groups.Add(parameter.Group);
            }

            Groups.Add(AllGroups);
            foreach (var group in groups) Groups.Add(group);
            _selectedGroup = AllGroups;

            ExportCommand = new RelayCommand(delegate
            {
                if (dialogs != null) dialogs.SaveFile("Выгрузка данных графиков", "CSV (*.csv)|*.csv", "trends.csv");
            });

            TogglePauseCommand = new RelayCommand(delegate { IsPaused = !IsPaused; });

            _machine.Updated += delegate { Refresh(); };

            RebuildParameters();
            ReloadSeries();
        }

        /// <summary>Строка «все группы» в фильтре.</summary>
        public const string AllGroups = "Все группы";

        public string Title { get { return "Параметры и графики"; } }

        public ObservableCollection<string> Groups { get; private set; }
        public ObservableCollection<ParameterItemViewModel> Parameters { get; private set; }
        public ObservableCollection<TrendSeriesViewModel> Series { get; private set; }
        public ObservableCollection<TrendWindow> Windows { get; private set; }

        public ICommand ExportCommand { get; private set; }
        public ICommand TogglePauseCommand { get; private set; }

        public string SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (SetProperty(ref _selectedGroup, value)) RebuildParameters();
            }
        }

        /// <summary>Окно наблюдения: 10 минут, 1 час, время текущей партии.</summary>
        public TrendWindow SelectedWindow
        {
            get { return _window; }
            set
            {
                if (SetProperty(ref _window, value)) ReloadSeries();
            }
        }

        /// <summary>Обновление приостановлено — удобно при разборе картины.</summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (SetProperty(ref _isPaused, value)) OnPropertyChanged("PauseText");
            }
        }

        public string PauseText { get { return _isPaused ? "Возобновить обновление" : "Приостановить обновление"; } }

        /// <summary>Данные выбранных кривых для элемента графика.</summary>
        public ObservableCollection<TrendSeries> PlottedSeries { get; private set; }

        public void Refresh()
        {
            if (_isPaused) return;

            foreach (var snapshot in _machine.GetParameters(null))
            {
                foreach (var item in Parameters)
                {
                    if (item.Key == snapshot.Key) item.Update(snapshot);
                }
            }
        }

        private void RebuildParameters()
        {
            Parameters.Clear();
            var group = _selectedGroup == AllGroups ? null : _selectedGroup;

            foreach (var snapshot in _machine.GetParameters(group))
            {
                Parameters.Add(new ParameterItemViewModel(snapshot));
            }
        }

        /// <summary>Перезагружает кривые только при изменении набора выбранных параметров.</summary>
        private void OnSeriesPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected") ReloadSeries();
        }

        private void ReloadSeries()
        {
            var keys = new List<string>();
            foreach (var item in Series)
            {
                if (item.IsSelected) keys.Add(item.Key);
            }

            var loaded = _trends.GetSeries(keys, _window);

            if (PlottedSeries == null)
            {
                PlottedSeries = new ObservableCollection<TrendSeries>();
                OnPropertyChanged("PlottedSeries");
            }

            PlottedSeries.Clear();
            foreach (var series in loaded)
            {
                PlottedSeries.Add(series);

                foreach (var item in Series)
                {
                    if (item.Key == series.Key) item.Series = series;
                }
            }
        }
    }
}
