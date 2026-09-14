using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// Выгрузка рецепта в файл для резервного копирования и переноса на другую
        /// установку. Формат «ключ=значение» позволяет проверку контрольной суммы (п. 5.6).
        /// </summary>
        public void ExportToFile(string id, string path)
        {
            var document = GetRecipe(id);
            if (document == null) throw new InvalidOperationException("Рецепт не найден: " + id);

            var lines = new List<string>
            {
                "# Рецепт установки автоматизированного дозирования и упаковки",
                "# Выгружен " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                string.Empty,
                "[Идентификация]"
            };

            Put(lines, "Наименование", document.Summary.Name);
            Put(lines, "Версия", document.Summary.Version);
            Put(lines, "Продукт", document.Summary.Product);
            Put(lines, "Состояние", DisplayNames.Of(document.Summary.State));
            Put(lines, "Автор", document.Summary.Author);
            Put(lines, "ДатаСоздания", document.Summary.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"));
            Put(lines, "ДатаИзменения", document.Summary.ModifiedAt.ToString("dd.MM.yyyy HH:mm:ss"));
            Put(lines, "Комментарий", document.Summary.Comment);

            lines.Add(string.Empty);
            lines.Add("[ТараИОснастка]");
            Put(lines, "ТипоразмерФлакона", document.VialSizeMl);
            Put(lines, "ТипРотора", document.RotorType);
            Put(lines, "ЧислоГнёзд", document.NestCount);
            Put(lines, "ВысотаГорловины", document.NeckHeightMm);
            Put(lines, "ДиаметрГорловины", document.NeckDiameterMm);
            Put(lines, "ТипЗатвора", document.ClosureType);

            lines.Add(string.Empty);
            lines.Add("[СвойстваПродукта]");
            Put(lines, "Плотность", document.DensityAtReference);
            Put(lines, "РепернаяТемпература", document.ReferenceTemperature);
            Put(lines, "ТемпературныйКоэффициент", document.TemperatureCoefficient);
            Put(lines, "ТемператураМин", document.ProductTemperatureMin);
            Put(lines, "ТемператураМакс", document.ProductTemperatureMax);
            Put(lines, "ОсобыеУказания", document.ProductNotes);

            lines.Add(string.Empty);
            lines.Add("[Дозирование]");
            Put(lines, "Доза", document.DoseMl);
            Put(lines, "ДопускДозы", document.DoseTolerancePercent);
            Put(lines, "СкоростьОсновнойФазы", document.MainPhaseRate);
            Put(lines, "СкоростьЗавершающейФазы", document.FinalPhaseRate);
            Put(lines, "ТочкаПереходаФаз", document.PhaseSwitchPercent);
            Put(lines, "ВремяСтабилизацииВесов", document.ScaleSettlingSeconds);
            Put(lines, "ЗадержкаОтводаСопла", document.NozzleRetractDelaySeconds);
            Put(lines, "ТаймаутНаполнения", document.FillTimeoutSeconds);

            lines.Add(string.Empty);
            lines.Add("[Укупорка]");
            Put(lines, "МоментЗатяжки", document.CapTorqueNm);
            Put(lines, "ДопускМомента", document.CapTorqueToleranceNm);
            Put(lines, "УголМин", document.CapAngleMinDeg);
            Put(lines, "УголМакс", document.CapAngleMaxDeg);
            Put(lines, "СкоростьЗавинчивания", document.CapSpeedRpm);
            Put(lines, "ВремяУдержания", document.CapHoldSeconds);
            Put(lines, "ЧислоПопыток", document.CapAttempts);
            Put(lines, "ТаймаутУкупорки", document.CapTimeoutSeconds);

            lines.Add(string.Empty);
            lines.Add("[Атмосфера]");
            Put(lines, "РабочийГаз", DisplayNames.Of(document.Gas));
            Put(lines, "ПределO2", document.OxygenLimitPpm);
            Put(lines, "ПределH2O", document.MoistureLimitPpm);
            Put(lines, "УставкаДавления", document.PressureSetpointMbar);
            Put(lines, "ДавлениеНижняяГраница", document.PressureLowMbar);
            Put(lines, "ДавлениеВерхняяГраница", document.PressureHighMbar);
            Put(lines, "ЦиклыПродувки", document.GatePurgeCycles);

            lines.Add(string.Empty);
            lines.Add("[Цикл]");
            Put(lines, "СкоростьПодачи", document.FeedSpeedPercent);
            Put(lines, "ТаймаутПодачи", document.FeedTimeoutSeconds);
            Put(lines, "СкоростьРотора", document.RotorSpeedPercent);
            Put(lines, "ВыдержкаПослеПоворота", document.RotorSettlingSeconds);
            Put(lines, "СкоростьВыгрузки", document.DischargeSpeedPercent);
            Put(lines, "ТаймаутВыгрузки", document.DischargeTimeoutSeconds);
            Put(lines, "ПаузаМеждуЦиклами", document.PauseBetweenCyclesSeconds);

            var rules = new List<string>();
            foreach (var rule in document.SortingRule) rules.Add(DisplayNames.Of(rule));
            Put(lines, "ПравилоСортировки", string.Join("; ", rules.ToArray()));

            lines.Add(string.Empty);
            lines.Add("[Проверка]");
            Put(lines, "КонтрольнаяСумма", document.Summary.Checksum);

            DemoExport.Write(path, lines);
        }

        /// <summary>Загрузка рецепта из файла с проверкой контрольной суммы.</summary>
        public RecipeDocument ImportFromFile(string path)
        {
            var values = new Dictionary<string, string>();

            foreach (var line in DemoExport.ReadLines(path))
            {
                var text = line.Trim();
                if (text.Length == 0 || text.StartsWith("#") || text.StartsWith("[")) continue;

                int separator = text.IndexOf('=');
                if (separator <= 0) continue;

                values[text.Substring(0, separator).Trim()] = text.Substring(separator + 1).Trim();
            }

            var document = CreateNew();
            document.Summary.Name = Text(values, "Наименование", document.Summary.Name);
            document.Summary.Version = Text(values, "Версия", "1");
            document.Summary.Product = Text(values, "Продукт", string.Empty);
            document.Summary.Author = Text(values, "Автор", string.Empty);
            document.Summary.Comment = Text(values, "Комментарий", string.Empty);
            document.Summary.Checksum = Text(values, "КонтрольнаяСумма", "—");

            document.VialSizeMl = Number(values, "ТипоразмерФлакона", 20);
            document.Summary.VialSizeMl = document.VialSizeMl;
            document.RotorType = Text(values, "ТипРотора", string.Empty);
            document.NestCount = (int)Number(values, "ЧислоГнёзд", 24);
            document.NeckHeightMm = Number(values, "ВысотаГорловины", 0);
            document.NeckDiameterMm = Number(values, "ДиаметрГорловины", 0);
            document.ClosureType = Text(values, "ТипЗатвора", string.Empty);

            document.DensityAtReference = Number(values, "Плотность", 1);
            document.ReferenceTemperature = Number(values, "РепернаяТемпература", 20);
            document.TemperatureCoefficient = Number(values, "ТемпературныйКоэффициент", 0);
            document.ProductTemperatureMin = Number(values, "ТемператураМин", 0);
            document.ProductTemperatureMax = Number(values, "ТемператураМакс", 0);
            document.ProductNotes = Text(values, "ОсобыеУказания", string.Empty);

            document.DoseMl = Number(values, "Доза", 0);
            document.Summary.DoseMl = document.DoseMl;
            document.DoseTolerancePercent = Number(values, "ДопускДозы", 1);
            document.MainPhaseRate = Number(values, "СкоростьОсновнойФазы", 0);
            document.FinalPhaseRate = Number(values, "СкоростьЗавершающейФазы", 0);
            document.PhaseSwitchPercent = Number(values, "ТочкаПереходаФаз", 0);
            document.ScaleSettlingSeconds = Number(values, "ВремяСтабилизацииВесов", 0);
            document.NozzleRetractDelaySeconds = Number(values, "ЗадержкаОтводаСопла", 0);
            document.FillTimeoutSeconds = Number(values, "ТаймаутНаполнения", 0);

            document.CapTorqueNm = Number(values, "МоментЗатяжки", 0);
            document.CapTorqueToleranceNm = Number(values, "ДопускМомента", 0);
            document.CapAngleMinDeg = Number(values, "УголМин", 0);
            document.CapAngleMaxDeg = Number(values, "УголМакс", 0);
            document.CapSpeedRpm = Number(values, "СкоростьЗавинчивания", 0);
            document.CapHoldSeconds = Number(values, "ВремяУдержания", 0);
            document.CapAttempts = (int)Number(values, "ЧислоПопыток", 1);
            document.CapTimeoutSeconds = Number(values, "ТаймаутУкупорки", 0);

            document.Gas = Text(values, "РабочийГаз", "Азот") == "Аргон" ? WorkingGas.Argon : WorkingGas.Nitrogen;
            document.OxygenLimitPpm = Number(values, "ПределO2", 10);
            document.MoistureLimitPpm = Number(values, "ПределH2O", 10);
            document.PressureSetpointMbar = Number(values, "УставкаДавления", 0);
            document.PressureLowMbar = Number(values, "ДавлениеНижняяГраница", 0);
            document.PressureHighMbar = Number(values, "ДавлениеВерхняяГраница", 0);
            document.GatePurgeCycles = (int)Number(values, "ЦиклыПродувки", 3);

            document.FeedSpeedPercent = Number(values, "СкоростьПодачи", 0);
            document.FeedTimeoutSeconds = Number(values, "ТаймаутПодачи", 0);
            document.RotorSpeedPercent = Number(values, "СкоростьРотора", 0);
            document.RotorSettlingSeconds = Number(values, "ВыдержкаПослеПоворота", 0);
            document.DischargeSpeedPercent = Number(values, "СкоростьВыгрузки", 0);
            document.DischargeTimeoutSeconds = Number(values, "ТаймаутВыгрузки", 0);
            document.PauseBetweenCyclesSeconds = Number(values, "ПаузаМеждуЦиклами", 0);

            return document;
        }

        private static void Put(List<string> lines, string key, string value)
        {
            lines.Add(key + "=" + (value ?? string.Empty));
        }

        private static void Put(List<string> lines, string key, double value)
        {
            lines.Add(key + "=" + value.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static string Text(IDictionary<string, string> values, string key, string fallback)
        {
            string result;
            return values.TryGetValue(key, out result) && result.Length > 0 ? result : fallback;
        }

        private static double Number(IDictionary<string, string> values, string key, double fallback)
        {
            string text;
            if (!values.TryGetValue(key, out text)) return fallback;

            double result;
            return double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                ? result
                : fallback;
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
