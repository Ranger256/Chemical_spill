using System;
using System.Collections.Generic;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Демонстрационная реализация <see cref="IRecipeService"/>: несколько рецептов
    /// во всех четырёх состояниях и типовые перечни проверок.
    /// </summary>
    public class DemoRecipeService : IRecipeService
    {
        private readonly List<RecipeSummary> _recipes = new List<RecipeSummary>();

        public DemoRecipeService()
        {
            _recipes.Add(Make("R-014", "Ацетонитрил 20 мл", "3", "Ацетонитрил, о. ч.", RecipeState.Approved,
                20, 20, "8F3A-21C4-90BE-7715", -12, "Базовый рецепт для растворителей группы 1."));
            _recipes.Add(Make("R-013", "Ацетонитрил 20 мл", "2", "Ацетонитрил, о. ч.", RecipeState.Archived,
                20, 20, "4C11-7E09-1A55-3320", -64, "Заменён версией 3: уточнена точка перехода фаз."));
            _recipes.Add(Make("R-021", "Гексан 10 мл", "1", "н-Гексан, о. ч.", RecipeState.Approved,
                10, 10, "B207-55D1-0C48-9931", -30, "Пониженная скорость налива из-за пенообразования."));
            _recipes.Add(Make("R-028", "Изопропанол 50 мл", "2", "Изопропанол, ос. ч.", RecipeState.Verified,
                50, 45, "D5E8-3B77-2290-6614", -3, "Пробный прогон 20 флаконов выполнен, ожидает утверждения."));
            _recipes.Add(Make("R-030", "Толуол 20 мл", "1", "Толуол, о. ч.", RecipeState.Draft,
                20, 20, "—", -1, "Черновик: не заполнен температурный коэффициент плотности."));

            Loaded = _recipes[0];
        }

        public RecipeSummary Loaded { get; private set; }

        public event EventHandler RecipesChanged;

        public IReadOnlyList<RecipeSummary> GetRecipes()
        {
            return _recipes;
        }

        public RecipeDocument GetRecipe(string id)
        {
            var summary = Find(id);
            if (summary == null) return null;

            var document = new RecipeDocument
            {
                Summary = summary,
                VialSizeMl = summary.VialSizeMl,
                RotorType = summary.RotorType,
                NestCount = 24,
                NeckHeightMm = 42.0,
                NeckDiameterMm = 18.0,
                ClosureType = "Винтовая крышка ПП, GL-18",

                DensityAtReference = 0.786,
                ReferenceTemperature = 20,
                TemperatureCoefficient = 0.14,
                ProductTemperatureMin = 18,
                ProductTemperatureMax = 24,
                ProductNotes = "Горюч, гигроскопичен, чувствителен к кислороду.",

                DoseMl = summary.DoseMl,
                DoseTolerancePercent = 1.0,
                MainPhaseRate = 14.0,
                FinalPhaseRate = 1.2,
                PhaseSwitchPercent = 88,
                ScaleSettlingSeconds = 1.2,
                NozzleRetractDelaySeconds = 0.4,
                FillTimeoutSeconds = 60,

                CapTorqueNm = 1.4,
                CapTorqueToleranceNm = 0.15,
                CapAngleMinDeg = 360,
                CapAngleMaxDeg = 480,
                CapSpeedRpm = 120,
                CapHoldSeconds = 0.5,
                CapAttempts = 3,
                CapTimeoutSeconds = 20,

                Gas = WorkingGas.Nitrogen,
                OxygenLimitPpm = 10,
                MoistureLimitPpm = 10,
                PressureSetpointMbar = 4.0,
                PressureLowMbar = -5,
                PressureHighMbar = 15,
                GatePurgeCycles = 3,

                FeedSpeedPercent = 60,
                FeedTimeoutSeconds = 8,
                RotorSpeedPercent = 55,
                RotorSettlingSeconds = 1.5,
                DischargeSpeedPercent = 70,
                DischargeTimeoutSeconds = 15,
                PauseBetweenCyclesSeconds = 0
            };

            document.SortingRule.Add(RejectReason.DoseDeviation);
            document.SortingRule.Add(RejectReason.CapFailure);
            document.SortingRule.Add(RejectReason.NotFilled);
            return document;
        }

        public IReadOnlyList<CheckResult> Load(string id)
        {
            var summary = Find(id);
            var results = new List<CheckResult>
            {
                new CheckResult("Установленный ротор и гнёзда соответствуют указанным в рецепте", true, "РТ-24-20, 24 гнезда"),
                new CheckResult("Установленный модуль укупорщика соответствует типу затвора", true, "GL-18, винтовая крышка"),
                new CheckResult("Рецепт находится в состоянии «утверждён»", summary != null && summary.State == RecipeState.Approved,
                    summary == null ? "рецепт не найден" : "состояние: " + DisplayNames.Of(summary.State)),
                new CheckResult("Контрольная сумма совпадает с сохранённой", summary != null && summary.State != RecipeState.Draft,
                    summary == null ? null : summary.Checksum)
            };

            if (summary != null && summary.State == RecipeState.Approved)
            {
                Loaded = summary;
                var handler = RecipesChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            return results;
        }

        public IReadOnlyList<CheckResult> Validate(RecipeDocument document)
        {
            return new List<CheckResult>
            {
                new CheckResult("Заполнены все обязательные параметры", true, null),
                new CheckResult("Доза не выходит за пределы выбранного типоразмера тары", true, "20,00 мл при типоразмере 20 мл"),
                new CheckResult("Допуск отклонения дозы не шире требования ТЗ", true, "1,00 % при требовании 1 об. %"),
                new CheckResult("Завершающая скорость налива не превышает основную", true, "1,2 мл/с против 14,0 мл/с"),
                new CheckResult("Таймауты стадий больше расчётной длительности операций", true, "наполнение: 60 с при расчётных 6,4 с"),
                new CheckResult("Пороги O₂ и H₂O лежат в пределах диапазона измерения датчиков", true, "10 ppm при диапазоне 0–1000 ppm"),
                new CheckResult("Границы давления не противоречат уставке", true, "уставка 4,0 мбар в границах −5…+15 мбар")
            };
        }

        public RecipeDocument CreateNew()
        {
            var document = new RecipeDocument();
            document.Summary = Make("R-NEW", "Новый рецепт", "1", string.Empty, RecipeState.Draft, 20, 0, "—", 0, string.Empty);
            return document;
        }

        public RecipeDocument Copy(string id)
        {
            var document = GetRecipe(id);
            if (document == null) return CreateNew();

            var source = document.Summary;
            document.Summary = Make("R-COPY", source.Name + " (копия)", "1", source.Product, RecipeState.Draft,
                source.VialSizeMl, source.DoseMl, "—", 0, "Создан копированием рецепта " + source.Name + ", в. " + source.Version);
            return document;
        }

        public IReadOnlyList<CheckResult> SaveAsDraft(RecipeDocument document)
        {
            return Validate(document);
        }

        public void MarkVerified(string id)
        {
            var summary = Find(id);
            if (summary != null) summary.State = RecipeState.Verified;
            Notify();
        }

        public void Approve(string id, string approvedBy)
        {
            var summary = Find(id);
            if (summary != null) summary.State = RecipeState.Approved;
            Notify();
        }

        public void Archive(string id)
        {
            var summary = Find(id);
            if (summary != null) summary.State = RecipeState.Archived;
            Notify();
        }

        public void ExportToFile(string id, string path)
        {
        }

        public RecipeDocument ImportFromFile(string path)
        {
            return CreateNew();
        }

        private void Notify()
        {
            var handler = RecipesChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private RecipeSummary Find(string id)
        {
            foreach (var recipe in _recipes)
            {
                if (recipe.Id == id) return recipe;
            }
            return null;
        }

        private static RecipeSummary Make(string id, string name, string version, string product, RecipeState state,
            double vial, double dose, string checksum, int modifiedDaysAgo, string comment)
        {
            return new RecipeSummary
            {
                Id = id,
                Name = name,
                Version = version,
                Product = product,
                State = state,
                Author = "Петров П. П.",
                CreatedAt = DateTime.Now.AddDays(modifiedDaysAgo - 20),
                ModifiedBy = "Петров П. П.",
                ModifiedAt = DateTime.Now.AddDays(modifiedDaysAgo),
                Checksum = checksum,
                VialSizeMl = vial,
                DoseMl = dose,
                RotorType = "РТ-24-" + ((int)vial),
                Comment = comment
            };
        }
    }
}
