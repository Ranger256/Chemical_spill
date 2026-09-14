using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services
{
    /// <summary>
    /// Работа с рецептами: хранение, версионность, проверки при сохранении и загрузке.
    /// Источник: «Функциональный интерфейс», раздел 5.
    /// </summary>
    public interface IRecipeService
    {
        /// <summary>Все рецепты, доступные для просмотра, включая архивные.</summary>
        IReadOnlyList<RecipeSummary> GetRecipes();

        /// <summary>Полный состав рецепта по идентификатору.</summary>
        RecipeDocument GetRecipe(string id);

        /// <summary>Загруженный в установку рецепт.</summary>
        RecipeSummary Loaded { get; }

        /// <summary>
        /// Загрузка рецепта в установку. Возвращает перечень проверок соответствия
        /// фактическому состоянию установки (п. 5.5): оснастка, модуль укупорщика,
        /// состояние рецепта, контрольная сумма.
        /// </summary>
        IReadOnlyList<CheckResult> Load(string id);

        /// <summary>Проверка внутренней непротиворечивости рецепта перед сохранением (п. 5.5).</summary>
        IReadOnlyList<CheckResult> Validate(RecipeDocument document);

        /// <summary>Создание пустого рецепта.</summary>
        RecipeDocument CreateNew();

        /// <summary>Копирование существующего рецепта — предпочтительный способ создания (п. 5.3).</summary>
        RecipeDocument Copy(string id);

        /// <summary>Сохранение как черновика; версия, автор и дата проставляются автоматически.</summary>
        IReadOnlyList<CheckResult> SaveAsDraft(RecipeDocument document);

        /// <summary>Перевод в состояние «проверен» после пробного прогона.</summary>
        void MarkVerified(string id);

        /// <summary>Утверждение рецепта; с этого момента он допускается к выпуску продукции.</summary>
        void Approve(string id, string approvedBy);

        /// <summary>Перевод в архив. Удаление рецептов, по которым выпускалась продукция, не допускается.</summary>
        void Archive(string id);

        /// <summary>Выгрузка рецепта в файл для резервного копирования и переноса.</summary>
        void ExportToFile(string id, string path);

        /// <summary>Загрузка рецепта из файла с проверкой контрольной суммы.</summary>
        RecipeDocument ImportFromFile(string path);

        /// <summary>Состав рецептов или их состояния изменились.</summary>
        event EventHandler RecipesChanged;
    }
}
