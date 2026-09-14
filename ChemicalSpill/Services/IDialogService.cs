namespace ChemicalSpill.Services
{
    /// <summary>
    /// Модальные окна. Вынесены в интерфейс, чтобы ViewModel не создавала окна
    /// напрямую: ядро и контроллер остаются независимыми от WPF.
    /// Реализация — ChemicalSpill.View.Services.DialogService.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Окно выбора рецепта. Возвращает идентификатор выбранного рецепта
        /// или null, если оператор отказался от загрузки.
        /// </summary>
        string SelectRecipe();

        /// <summary>
        /// Мастер подключения к установке: ввод Ключа 1 и ожидание подтверждения
        /// Ключом 2 на панели оператора. Возвращает true, если соединение установлено.
        /// </summary>
        bool ShowConnection();

        /// <summary>Запрос подтверждения действия.</summary>
        bool Confirm(string title, string message);

        /// <summary>Сообщение оператору.</summary>
        void ShowMessage(string title, string message);

        /// <summary>Выбор файла для открытия; null — отказ.</summary>
        string OpenFile(string title, string filter);

        /// <summary>Выбор файла для сохранения; null — отказ.</summary>
        string SaveFile(string title, string filter, string defaultFileName);
    }
}
