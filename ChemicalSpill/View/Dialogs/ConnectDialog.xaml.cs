using System;
using System.Windows;
using ChemicalSpill.ViewModels.Dialogs;

namespace ChemicalSpill.View.Dialogs
{
    /// <summary>
    /// Мастер подключения к установке. Ключ 1 передаётся в модель представления
    /// напрямую из поля ввода и нигде не сохраняется.
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private readonly ConnectViewModel _model;
        private bool _isClosing;

        public ConnectDialog(ConnectViewModel model)
        {
            InitializeComponent();

            _model = model;
            DataContext = model;

            _model.CloseRequested += OnCloseRequested;
        }

        /// <summary>Соединение установлено.</summary>
        public bool IsEstablished
        {
            get { return _model.IsEstablished; }
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            if (_isClosing) return;

            _isClosing = true;
            DialogResult = _model.IsEstablished;
            Close();
        }

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            _model.Connect(Key1Box.Password);
            Key1Box.Clear();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            _model.ConfirmOnPanel();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            // Закрытие выполняет сама кнопка (IsCancel), поэтому подписку снимаем,
            // чтобы результат не задавался дважды.
            _model.CloseRequested -= OnCloseRequested;
            _model.Cancel();
        }
    }
}
