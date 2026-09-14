using System.Windows;
using ChemicalSpill.Services;
using ChemicalSpill.View.Dialogs;
using ChemicalSpill.ViewModels.Dialogs;

namespace ChemicalSpill.View.Services
{
    /// <summary>
    /// Реализация <see cref="IDialogService"/> на модальных окнах WPF.
    /// Это единственное место, где модели представления соприкасаются с окнами,
    /// поэтому ядро и контроллер остаются независимыми от WPF.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IRecipeService _recipes;
        private readonly IMachineService _machine;
        private readonly INetworkService _network;

        public DialogService(IRecipeService recipes, IMachineService machine, INetworkService network)
        {
            _recipes = recipes;
            _machine = machine;
            _network = network;
        }

        public string SelectRecipe()
        {
            var model = new RecipeSelectViewModel(_recipes, _machine);
            var dialog = new RecipeSelectDialog(model);
            SetOwner(dialog);

            var result = dialog.ShowDialog();
            return result == true ? dialog.ResultId : null;
        }

        public bool ShowConnection()
        {
            var model = new ConnectViewModel(_network);
            var dialog = new ConnectDialog(model);
            SetOwner(dialog);

            return dialog.ShowDialog() == true;
        }

        public bool Confirm(string title, string message)
        {
            var result = MessageBox.Show(Owner(), message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return result == MessageBoxResult.OK;
        }

        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(Owner(), message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public string OpenFile(string title, string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Title = title, Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string SaveFile(string title, string filter, string defaultFileName)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private static void SetOwner(Window dialog)
        {
            var owner = Owner();
            if (owner != null && !ReferenceEquals(owner, dialog)) dialog.Owner = owner;
        }

        private static Window Owner()
        {
            return Application.Current == null ? null : Application.Current.MainWindow;
        }
    }
}
