using System.Collections.Specialized;
using System.Windows.Controls;
using ChemicalSpill.ViewModels.Main;

namespace ChemicalSpill.View.Pages
{
    /// <summary>
    /// Главный рабочий экран. Код элемента содержит только автопрокрутку консоли:
    /// это поведение представления, а не логика установки.
    /// </summary>
    public partial class MainScreenView : UserControl
    {
        private INotifyCollectionChanged _consoleItems;

        public MainScreenView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_consoleItems != null) _consoleItems.CollectionChanged -= OnConsoleChanged;

            var model = DataContext as MainScreenViewModel;
            if (model == null) return;

            _consoleItems = model.Console.Items;
            _consoleItems.CollectionChanged += OnConsoleChanged;
        }

        private void OnConsoleChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var model = DataContext as MainScreenViewModel;
            if (model == null || !model.Console.AutoScroll) return;

            ConsoleScroll.ScrollToEnd();
        }
    }
}
