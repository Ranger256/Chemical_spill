using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSpill.Models;
using ChemicalSpill.Services;

namespace ChemicalSpill.ViewModels.Sections
{
    /// <summary>Строка журнала.</summary>
    public class JournalItemViewModel : ViewModelBase
    {
        private readonly JournalRecord _record;

        public JournalItemViewModel(JournalRecord record)
        {
            _record = record;
        }

        public string TimeText { get { return _record.Time.ToString("dd.MM.yyyy HH:mm:ss"); } }
        public JournalCategory Category { get { return _record.Category; } }
        public string CategoryText { get { return DisplayNames.Of(_record.Category); } }
        public string Event { get { return _record.Event; } }
        public string OldValue { get { return _record.OldValue; } }
        public string NewValue { get { return _record.NewValue; } }
        public string OperatorName { get { return _record.OperatorName; } }
        public string StateText { get { return DisplayNames.Of(_record.MachineState); } }
        public string BatchNumber { get { return _record.BatchNumber; } }
    }

    /// <summary>
    /// Раздел «Журнал и отчёты»: выборка записей и формирование отчёта о партии.
    /// </summary>
    public class JournalViewModel : ViewModelBase
    {
        private readonly IJournalService _journal;
        private readonly IDialogService _dialogs;

        private DateTime _from = DateTime.Today;
        private DateTime _to = DateTime.Today.AddDays(1);
        private object _selectedCategory;
        private string _search = string.Empty;
        private string _selectedBatch;

        public JournalViewModel(IJournalService journal, IDialogService dialogs)
        {
            _journal = journal;
            _dialogs = dialogs;

            Items = new ObservableCollection<JournalItemViewModel>();
            Categories = new ObservableCollection<object> { AllCategories };
            Batches = new ObservableCollection<string>();

            foreach (JournalCategory value in Enum.GetValues(typeof(JournalCategory)))
            {
                Categories.Add(value);
            }

            foreach (var batch in _journal.GetBatchNumbers()) Batches.Add(batch);
            if (Batches.Count > 0) _selectedBatch = Batches[0];

            _selectedCategory = AllCategories;

            RefreshCommand = new RelayCommand(Refresh);
            ExportCommand = new RelayCommand(Export);
            BuildReportCommand = new RelayCommand(BuildReport);

            _journal.RecordAdded += delegate { Refresh(); };
            Refresh();
        }

        /// <summary>Значение фильтра «все категории».</summary>
        public const string AllCategories = "Все категории";

        public string Title { get { return "Журнал и отчёты"; } }

        public ObservableCollection<JournalItemViewModel> Items { get; private set; }
        public ObservableCollection<object> Categories { get; private set; }
        public ObservableCollection<string> Batches { get; private set; }

        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand BuildReportCommand { get; private set; }

        public DateTime From
        {
            get { return _from; }
            set { if (SetProperty(ref _from, value)) Refresh(); }
        }

        public DateTime To
        {
            get { return _to; }
            set { if (SetProperty(ref _to, value)) Refresh(); }
        }

        public object SelectedCategory
        {
            get { return _selectedCategory; }
            set { if (SetProperty(ref _selectedCategory, value)) Refresh(); }
        }

        public string Search
        {
            get { return _search; }
            set { if (SetProperty(ref _search, value)) Refresh(); }
        }

        public string SelectedBatch
        {
            get { return _selectedBatch; }
            set { SetProperty(ref _selectedBatch, value); }
        }

        public string CountText { get { return "записей: " + Items.Count; } }

        /// <summary>Заполнение памяти журнала установки; при 80 % выводится предупреждение.</summary>
        public int MemoryPercent { get { return _journal.MemoryUsedPercent; } }

        public bool MemoryWarning { get { return _journal.MemoryUsedPercent >= 80; } }

        public string MemoryText
        {
            get
            {
                return "Память журнала установки заполнена на " + _journal.MemoryUsedPercent +
                       " %. Запись кольцевая: при заполнении старые записи замещаются новыми.";
            }
        }

        /// <summary>Состав отчёта о партии, предусмотренный техническим заданием.</summary>
        public ObservableCollection<string> ReportContents
        {
            get
            {
                return new ObservableCollection<string>
                {
                    "Идентификация партии и продукта",
                    "Использованная оснастка и типоразмер тары",
                    "Рецепт: наименование, версия, контрольная сумма",
                    "Уставки дозирования и укупорки",
                    "Статистика по годным и забракованным флаконам с причинами",
                    "Сводка по параметрам атмосферы за время партии",
                    "Перечень возникших аварий"
                };
            }
        }

        private void Refresh()
        {
            JournalCategory? category = null;
            if (_selectedCategory is JournalCategory) category = (JournalCategory)_selectedCategory;

            Items.Clear();
            foreach (var record in _journal.Query(_from, _to, category, _search))
            {
                Items.Add(new JournalItemViewModel(record));
            }

            OnPropertyChanged("CountText");
            OnPropertyChanged("MemoryPercent");
            OnPropertyChanged("MemoryWarning");
            OnPropertyChanged("MemoryText");
        }

        private void Export()
        {
            if (_dialogs == null) return;

            var path = _dialogs.SaveFile("Выгрузка журнала", "CSV (*.csv)|*.csv", "journal.csv");
            if (path == null) return;

            var records = _journal.Query(_from, _to, null, _search);
            _journal.Export(records, path);
        }

        private void BuildReport()
        {
            if (_dialogs == null || _selectedBatch == null) return;

            _journal.BuildReport(_selectedBatch);
            _dialogs.ShowMessage("Отчёт о партии",
                "Отчёт по партии " + _selectedBatch + " сформирован. " +
                "Отчёт ссылается на рецепт, его версию и контрольную сумму.");
        }
    }
}
