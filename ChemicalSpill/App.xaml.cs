using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ChemicalSpill.Services;
using ChemicalSpill.Services.Demo;
using ChemicalSpill.View;
using ChemicalSpill.View.Services;
using ChemicalSpill.ViewModels;

namespace ChemicalSpill
{
    /// <summary>
    /// Точка сборки приложения.
    ///
    /// Здесь и только здесь выбирается, какие реализации сервисов получает
    /// интерфейс. Сейчас подставлены демонстрационные реализации, чтобы фасад
    /// запускался без ядра. Когда появятся ядро и контроллер, достаточно
    /// заменить объекты Demo* на рабочие: представление и модели представления
    /// править не требуется.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplyRussianCulture();

            IMachineService machine = new DemoMachineService();
            IRecipeService recipes = new DemoRecipeService();
            ISettingsService settings = new DemoSettingsService();
            INetworkService network = new DemoNetworkService();
            IJournalService journal = new DemoJournalService();
            ITrendService trends = new DemoTrendService();
            IVideoService video = new DemoVideoService();

            IDialogService dialogs = new DialogService(recipes, machine, network);

            var shell = new ShellViewModel(machine, recipes, settings, network, journal, trends, video, dialogs);

            var window = new MainWindow { DataContext = shell };
            MainWindow = window;
            window.Show();
        }

        /// <summary>
        /// Русские форматы чисел и дат: десятичный разделитель — запятая,
        /// как в документах на установку.
        /// </summary>
        private static void ApplyRussianCulture()
        {
            var culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }
    }
}
