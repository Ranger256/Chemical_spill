using System.ComponentModel;
using System.Windows;
using ChemicalSpill.ViewModels.Dialogs;


namespace ChemicalSpill.View.Dialogs
{
    /// <summary>
    /// Модальное окно выбора рецепта. Закрывается с положительным результатом,
    /// когда рецепт успешно загружен в установку.
    /// </summary>
    public partial class RecipeSelectDialog : Window
    {
        private readonly RecipeSelectViewModel _model;

        public RecipeSelectDialog(RecipeSelectViewModel model)
        {
            InitializeComponent();

            _model = model;
            DataContext = model;

            _model.PropertyChanged += OnModelPropertyChanged;
        }

        /// <summary>Идентификатор загруженного рецепта или null.</summary>
        public string ResultId
        {
            get { return _model.ResultId; }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CloseRequested" || !_model.CloseRequested) return;

            DialogResult = true;
            Close();
        }
    }
}
