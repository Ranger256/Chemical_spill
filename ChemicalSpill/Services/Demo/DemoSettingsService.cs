using System;
using System.Collections.Generic;
using System.Globalization;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Демонстрационная реализация <see cref="ISettingsService"/>.
    /// Перечень настроек полностью соответствует таблице п. 3.2 документа
    /// «Функциональный интерфейс управления установкой».
    /// </summary>
    public class DemoSettingsService : ISettingsService
    {
        /// <summary>Разделы перечня настроек.</summary>
        public const string GroupCommon = "Общие параметры цикла";
        public const string GroupFeed = "Стадия 1. Подача флакона";
        public const string GroupIndex = "Индексация ротора";
        public const string GroupFill = "Стадия 2. Наполнение";
        public const string GroupCap = "Стадия 3. Укупорка";
        public const string GroupDischarge = "Стадия 4. Выдача флакона";

        private readonly List<SettingDefinition> _settings = new List<SettingDefinition>();

        public DemoSettingsService()
        {
            var mode = Item("mode", GroupCommon, "Режим работы", null, null, null, null, false,
                "Автомат — непрерывное выполнение цикла. Шаг — одна стадия за одно нажатие, для проверки настроек. Наладка — ручное управление отдельными механизмами при открытой камере.",
                "Автомат");
            mode.Options.Add("Автомат");
            mode.Options.Add("Шаг");
            mode.Options.Add("Наладка");
            _settings.Add(mode);

            _settings.Add(Item("batchTarget", GroupCommon, "Задание на партию", "шт", 1, 10000, 500, false,
                "Количество флаконов, после которого цикл штатно завершается.", "500"));
            _settings.Add(Item("cyclePause", GroupCommon, "Пауза между циклами", "с", 0, 60, 0, true,
                "Выдержка перед началом следующего цикла. Используется при отладке и при работе с пенящимися жидкостями.", "0"));

            _settings.Add(Item("feedSpeed", GroupFeed, "Скорость подачи", "%", 10, 100, 60, true,
                "Темп переноса флакона из накопителя в гнездо. Снижение уменьшает риск опрокидывания и образования частиц.", "60"));
            _settings.Add(Item("feedTimeout", GroupFeed, "Таймаут подачи", "с", 1, 30, 8, false,
                "Предельное время операции. По истечении — отказ станции и остановка цикла. Защищает от работы с пустым накопителем и от заклинивания.", "8"));

            _settings.Add(Item("rotorSpeed", GroupIndex, "Скорость поворота", "%", 10, 100, 55, true,
                "Темп перемещения ротора между позициями. Ограничен риском расплёскивания уже наполненных флаконов.", "55"));
            _settings.Add(Item("rotorSettling", GroupIndex, "Время выдержки после поворота", "с", 0, 5, 1.5, true,
                "Пауза для затухания колебаний жидкости и механизма. Прямо влияет на точность взвешивания.", "1,5"));

            _settings.Add(Item("dose", GroupFill, "Доза", "мл", 5, 50, 20, false,
                "Заданный объём продукта на флакон. Проверяется на соответствие типоразмеру тары.", "20,00"));
            _settings.Add(Item("mainRate", GroupFill, "Скорость налива, основная фаза", "мл/с", 0.5, 50, 14, true,
                "Темп подачи продукта на быстром участке дозы. Определяет производительность.", "14,0"));
            _settings.Add(Item("finalRate", GroupFill, "Скорость налива, завершающая фаза", "мл/с", 0.1, 5, 1.2, true,
                "Темп на медленном участке в конце дозы. Определяет точность.", "1,2"));
            _settings.Add(Item("phaseSwitch", GroupFill, "Точка перехода фаз", "% дозы", 50, 99, 88, true,
                "Доля дозы, после которой налив переключается на медленную фазу. Основной инструмент поиска компромисса между скоростью и точностью.", "88"));
            _settings.Add(Item("settling", GroupFill, "Время стабилизации весов", "с", 0.1, 5, 1.2, true,
                "Выдержка перед снятием показания массы. Слишком малое значение даёт ложное отклонение дозы, слишком большое снижает производительность.", "1,2"));
            _settings.Add(Item("nozzleDelay", GroupFill, "Задержка отвода сопла", "с", 0, 3, 0.4, true,
                "Пауза после отсечки потока до подъёма сопла. Предотвращает падение капли мимо флакона.", "0,4"));
            _settings.Add(Item("fillTimeout", GroupFill, "Таймаут наполнения", "с", 1, 120, 60, false,
                "Предельное время стадии. Защищает от работы при пустой питающей ёмкости и при засорении тракта.", "60"));

            _settings.Add(Item("torque", GroupCap, "Момент затяжки", "Н·м", 0.1, 5, 1.4, false,
                "Требуемое усилие затяжки крышки.", "1,40"));
            _settings.Add(Item("capSpeed", GroupCap, "Скорость завинчивания", "об/мин", 10, 300, 120, true,
                "Темп вращения укупорочной головки.", "120"));
            _settings.Add(Item("capHold", GroupCap, "Время удержания", "с", 0, 3, 0.5, true,
                "Выдержка под моментом после его достижения. Обеспечивает стабильное обжатие уплотнения крышки.", "0,5"));
            _settings.Add(Item("capAttempts", GroupCap, "Число попыток", "шт", 1, 3, 3, true,
                "Количество повторов при неудачной укупорке до отбраковки флакона.", "3"));
            _settings.Add(Item("capTimeout", GroupCap, "Таймаут укупорки", "с", 1, 30, 20, false,
                "Предельное время стадии.", "20"));

            _settings.Add(Item("dischargeSpeed", GroupDischarge, "Скорость выгрузки", "%", 10, 100, 70, true,
                "Темп снятия флакона с ротора и переноса в приёмник.", "70"));
            _settings.Add(Item("dischargeTimeout", GroupDischarge, "Таймаут выгрузки", "с", 1, 30, 15, false,
                "Предельное время стадии.", "15"));

            var sorting = Item("sorting", GroupDischarge, "Правило сортировки", null, null, null, null, false,
                "Условия направления флакона в приёмник негодной продукции: отклонение дозы, отказ укупорки, ошибка подачи.",
                "Отклонение дозы; отказ укупорки; ошибка подачи");
            sorting.Options.Add("Отклонение дозы");
            sorting.Options.Add("Отказ укупорки");
            sorting.Options.Add("Ошибка подачи");
            _settings.Add(sorting);
        }

        public event EventHandler SettingsChanged;

        public IReadOnlyList<SettingDefinition> GetSettings()
        {
            return _settings;
        }

        public CheckResult TryApply(string key, string value, string operatorName)
        {
            SettingDefinition setting = null;
            foreach (var item in _settings)
            {
                if (item.Key == key) { setting = item; break; }
            }

            if (setting == null)
            {
                return new CheckResult("Настройка не найдена", false, key);
            }

            if (setting.Options.Count == 0 && setting.Minimum.HasValue && setting.Maximum.HasValue)
            {
                double parsed;
                if (!TryParse(value, out parsed))
                {
                    return new CheckResult(setting.Caption, false, "Значение не является числом");
                }

                if (parsed < setting.Minimum.Value || parsed > setting.Maximum.Value)
                {
                    return new CheckResult(setting.Caption, false,
                        "Значение вне диапазона " + Format(setting.Minimum.Value) + "–" + Format(setting.Maximum.Value) +
                        (string.IsNullOrEmpty(setting.Unit) ? string.Empty : " " + setting.Unit));
                }
            }

            setting.Value = value;
            var handler = SettingsChanged;
            if (handler != null) handler(this, EventArgs.Empty);
            return new CheckResult(setting.Caption, true, "Принято: " + value);
        }

        public CycleTimeEstimate Estimate(IEnumerable<SettingDefinition> settings)
        {
            var estimate = new CycleTimeEstimate();
            estimate.StageDurations[StageId.Feed] = 2.6;
            estimate.StageDurations[StageId.Fill] = 6.4;
            estimate.StageDurations[StageId.Cap] = 3.4;
            estimate.StageDurations[StageId.Discharge] = 2.2;
            estimate.IndexingSeconds = 1.9;

            double longest = 0;
            var limiting = StageId.Fill;
            foreach (var pair in estimate.StageDurations)
            {
                if (pair.Value > longest)
                {
                    longest = pair.Value;
                    limiting = pair.Key;
                }
            }

            estimate.LimitingStage = limiting;
            estimate.TactSeconds = longest + estimate.IndexingSeconds;
            estimate.Throughput = estimate.TactSeconds > 0 ? 60.0 / estimate.TactSeconds : 0;
            return estimate;
        }

        private static bool TryParse(string text, out double value)
        {
            if (text == null) { value = 0; return false; }
            text = text.Replace(',', '.').Trim();
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static string Format(double value)
        {
            return value.ToString("0.###");
        }

        private static SettingDefinition Item(string key, string group, string caption, string unit,
            double? min, double? max, double? defaultValue, bool changeableWhileRunning, string purpose, string value)
        {
            return new SettingDefinition
            {
                Key = key,
                Group = group,
                Caption = caption,
                Unit = unit,
                Minimum = min,
                Maximum = max,
                DefaultValue = defaultValue,
                ChangeableWhileRunning = changeableWhileRunning,
                Purpose = purpose,
                Value = value
            };
        }
    }
}
