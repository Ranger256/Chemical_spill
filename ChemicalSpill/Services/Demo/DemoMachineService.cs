using System;
using System.Collections.Generic;
using System.Windows.Threading;
using ChemicalSpill.Models;

namespace ChemicalSpill.Services.Demo
{
    /// <summary>
    /// Демонстрационная реализация <see cref="IMachineService"/>.
    /// Нужна только для того, чтобы фасад запускался и отображал правдоподобные
    /// значения до подключения ядра. Ядро заменяет этот класс своей реализацией —
    /// править представление при этом не требуется.
    /// </summary>
    public class DemoMachineService : IMachineService
    {
        private readonly Random _random = new Random(20250914);
        private readonly List<ConsoleRecord> _console = new List<ConsoleRecord>();
        private readonly List<AlarmRecord> _alarms = new List<AlarmRecord>();
        private readonly Dictionary<string, ParameterSnapshot> _parameters =
            new Dictionary<string, ParameterSnapshot>();

        private readonly DispatcherTimer _timer;
        private double _fillElapsed = 1.8;
        private double _capElapsed = 0.6;

        /// <summary>Имитация невыполненного разрешающего условия — для показа блокировки пуска.</summary>
        private bool _startBlocked;

        public DemoMachineService()
        {
            State = MachineState.Running;
            Mode = OperatingMode.Auto;
            CurrentStepName = "Дозирование";
            SelectedVialNest = 7;
            PowerWatts = 1265;

            Rotor = new RotorInfo
            {
                RotorType = "РТ-24-20 (24 гнезда, 20 мл)",
                NestCount = 24,
                CurrentPosition = 7,
                VialSizeMl = 20,
                InputMagazine = 186,
                GoodBin = 128,
                RejectBin = 3,
                CapMagazine = 412
            };

            Batch = new BatchInfo
            {
                Number = "П-2026-0914-03",
                Product = "Ацетонитрил, о. ч.",
                OperatorName = "Иванов И. И.",
                Target = 500,
                Good = 128,
                StartedAt = DateTime.Now.AddMinutes(-18),
                Elapsed = TimeSpan.FromMinutes(18),
                Remaining = TimeSpan.FromMinutes(52),
                Throughput = 7.2
            };
            Batch.Rejects[RejectReason.DoseDeviation] = 2;
            Batch.Rejects[RejectReason.CapFailure] = 1;

            LoadedRecipe = new RecipeSummary
            {
                Id = "R-014",
                Name = "Ацетонитрил 20 мл",
                Version = "3",
                Product = "Ацетонитрил, о. ч.",
                State = RecipeState.Approved,
                Author = "Петров П. П.",
                CreatedAt = DateTime.Now.AddDays(-64),
                ModifiedBy = "Петров П. П.",
                ModifiedAt = DateTime.Now.AddDays(-12),
                Checksum = "8F3A-21C4-90BE-7715",
                VialSizeMl = 20,
                DoseMl = 20,
                RotorType = "РТ-24-20",
                Comment = "Базовый рецепт для растворителей группы 1."
            };

            BuildParameters();
            BuildConsole();

            _alarms.Add(new AlarmRecord
            {
                Time = DateTime.Now.AddMinutes(-4),
                Severity = AlarmSeverity.Warning,
                Code = "W-214",
                Text = "Остаток крышек в магазине ниже 500 шт",
                Source = "Укупорщик",
                Acknowledged = false
            });
            _alarms.Add(new AlarmRecord
            {
                Time = DateTime.Now.AddMinutes(-11),
                Severity = AlarmSeverity.Warning,
                Code = "W-108",
                Text = "Отклонение дозы приблизилось к границе допуска (0,78 %)",
                Source = "Дозирование",
                Acknowledged = true
            });

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        public MachineState State { get; private set; }
        public OperatingMode Mode { get; private set; }
        public string CurrentStepName { get; private set; }
        public BatchInfo Batch { get; private set; }
        public RotorInfo Rotor { get; private set; }
        public RecipeSummary LoadedRecipe { get; private set; }
        public double PowerWatts { get; private set; }
        public int SelectedVialNest { get; private set; }

        public bool CanStart { get { return State == MachineState.Ready || State == MachineState.Paused; } }
        public bool CanStop { get { return State == MachineState.Running || State == MachineState.Paused; } }
        public bool CanPause { get { return State == MachineState.Running; } }
        public bool CanStep { get { return Mode == OperatingMode.Step && State != MachineState.Alarm; } }
        public bool CanResetAlarm { get { return State == MachineState.Alarm; } }

        public event EventHandler Updated;
        public event EventHandler<AlarmRecord> AlarmRaised;
        public event EventHandler<ConsoleRecord> ConsoleLineAdded;

        public IReadOnlyList<StageSnapshot> GetStages(MnemonicMode mode)
        {
            var list = new List<StageSnapshot>();

            if (mode == MnemonicMode.ByStation)
            {
                list.Add(Stage(StageId.Feed, StageState.Running, "Флакон в позиции", "есть", 0.9, 8, 7, 131, 0, null));
                list.Add(Stage(StageId.Fill, StageState.Running, "Доза", "14,62 из 20,00 мл", _fillElapsed, 60, 6, 129, 2, null));
                list.Add(Stage(StageId.Cap, StageState.Waiting, "Момент затяжки", "ожидание индексации", _capElapsed, 20, 5, 128, 1, null));
                list.Add(Stage(StageId.Discharge, StageState.Inactive, "Флакон в позиции", "нет", 0, 15, 4, 128, 0, null));

                list.Add(Stage(StageId.InputGate, StageState.Completed, "Состояние", "закрыт, продут", 0, 0, 0, 0, 0, null));
                list.Add(Stage(StageId.RotorIndex, StageState.Waiting, "Позиция", "7 из 24", 0.4, 3, 7, 0, 0, null));
                list.Add(Stage(StageId.OutputGate, StageState.Running, "Состояние", "продувка, цикл 2 из 3", 4.1, 30, 0, 0, 0, null));
            }
            else
            {
                list.Add(Stage(StageId.Feed, StageState.Completed, "Флакон в позиции", "перенесён в гнездо 7", 2.4, 8, 7, 131, 0, null));
                list.Add(Stage(StageId.Fill, StageState.Running, "Доза", "14,62 из 20,00 мл", _fillElapsed, 60, 7, 129, 2, null));
                list.Add(Stage(StageId.Cap, StageState.Inactive, "Момент затяжки", "—", 0, 20, 7, 128, 1, null));
                list.Add(Stage(StageId.Discharge, StageState.Inactive, "Флакон в позиции", "—", 0, 15, 7, 128, 0, null));

                list.Add(Stage(StageId.InputGate, StageState.Completed, "Состояние", "пройден", 0, 0, 0, 0, 0, null));
                list.Add(Stage(StageId.RotorIndex, StageState.Completed, "Позиция", "гнездо 7", 0, 3, 7, 0, 0, null));
                list.Add(Stage(StageId.OutputGate, StageState.Inactive, "Состояние", "не пройден", 0, 30, 0, 0, 0, null));
            }

            return list;
        }

        public IReadOnlyList<ParameterSnapshot> GetParameters(string group)
        {
            var list = new List<ParameterSnapshot>();
            foreach (var p in _parameters.Values)
            {
                if (string.IsNullOrEmpty(group) || p.Group == group) list.Add(p);
            }
            return list;
        }

        public IReadOnlyList<StartCondition> GetStartConditions()
        {
            bool alarmsClear = !HasUnacknowledgedAlarm();

            return new List<StartCondition>
            {
                Condition("Содержание O₂ и H₂O ниже заданных порогов", true, "O₂ 3,2 ppm; H₂O 1,8 ppm"),
                Condition("Избыточное давление в камере и давление газа на входе в норме", true, "4,2 мбар; 5,4 бар"),
                Condition("Двери рабочей камеры и шлюзов закрыты и заблокированы", true, "все положения подтверждены"),
                Condition("Тип установленного ротора подтверждён и соответствует рецепту", true, "РТ-24-20"),
                Condition("Загружен рецепт, введены номер партии и наименование продукта", true, "Ацетонитрил 20 мл, в. 3"),
                Condition("В магазине укупорщика есть крышки, во входном накопителе есть флаконы", true, "412 крышек; 186 флаконов"),
                Condition("Продукт в питающей ёмкости присутствует, температура в допуске", !_startBlocked,
                    _startBlocked
                        ? "температура продукта 21,8 °C при реперной 20,0 °C — выдержка не завершена"
                        : "остаток 1840 мл; температура 21,8 °C в допуске 18–24 °C"),
                Condition("Отсутствуют неквитированные аварии, кнопка аварийной остановки разблокирована", alarmsClear,
                    alarmsClear ? "аварий нет, кнопка разблокирована" : "есть неквитированная авария")
            };
        }

        /// <summary>Есть ли неквитированная авария. Предупреждения пуску не препятствуют.</summary>
        private bool HasUnacknowledgedAlarm()
        {
            foreach (var alarm in _alarms)
            {
                if (alarm.Severity == AlarmSeverity.Alarm && !alarm.Acknowledged) return true;
            }
            return false;
        }

        /// <summary>
        /// После штатного останова и после квитирования аварии установка возвращается
        /// в состояние «Готова», если все разрешающие условия выполнены.
        /// Без этого повторный пуск был бы невозможен.
        /// </summary>
        private void UpdateReadiness()
        {
            if (State != MachineState.Stopped) return;
            if (_startBlocked || HasUnacknowledgedAlarm()) return;

            State = MachineState.Ready;
            CurrentStepName = "Ожидание пуска";
            Log(ConsoleLineKind.Info, "Разрешающие условия выполнены: установка готова к пуску");
        }

        /// <summary>
        /// Имитация невыполненного разрешающего условия: показывает перечень того,
        /// что именно препятствует пуску. Только для просмотра фасада.
        /// </summary>
        public void ToggleStartBlock()
        {
            _startBlocked = !_startBlocked;

            if (_startBlocked)
            {
                if (State == MachineState.Ready) State = MachineState.Stopped;
                Log(ConsoleLineKind.Warning, "Имитация: температура продукта вне допуска, пуск заблокирован");
            }
            else
            {
                Log(ConsoleLineKind.Info, "Имитация: разрешающее условие восстановлено");
                UpdateReadiness();
            }

            Raise();
        }

        public IReadOnlyList<AlarmRecord> GetActiveAlarms()
        {
            return _alarms;
        }

        public IReadOnlyList<ConsoleRecord> GetConsoleHistory()
        {
            return _console;
        }

        public void Start()
        {
            State = MachineState.Running;
            CurrentStepName = "Дозирование";
            Log(ConsoleLineKind.Command, "Пуск цикла в режиме «" + DisplayNames.Of(Mode) + "»");
            Raise();
        }

        public void Stop()
        {
            State = MachineState.Stopped;
            CurrentStepName = "Цикл остановлен";
            Log(ConsoleLineKind.Command, "Штатный останов: операции над флаконами в роторе завершаются");

            // Инертная атмосфера и давление сохраняются, поэтому после завершения
            // останова установка снова готова к пуску.
            UpdateReadiness();
            Raise();
        }

        public void Pause()
        {
            State = State == MachineState.Paused ? MachineState.Running : MachineState.Paused;
            Log(ConsoleLineKind.Command, State == MachineState.Paused ? "Цикл приостановлен" : "Цикл возобновлён");
            Raise();
        }

        public void Step()
        {
            Log(ConsoleLineKind.Command, "Выполнена одна стадия цикла");
            Raise();
        }

        public void ResetAlarm(string operatorName)
        {
            State = MachineState.Stopped;
            CurrentStepName = "Цикл остановлен";
            foreach (var alarm in _alarms) alarm.Acknowledged = true;
            Log(ConsoleLineKind.Command, "Авария квитирована оператором: " + operatorName);

            // Автоматическое возобновление прерванного цикла не допускается:
            // установка переводится в «Стоп» и далее, при выполненных условиях, в «Готова».
            UpdateReadiness();
            Raise();
        }

        public void SetMode(OperatingMode mode)
        {
            Mode = mode;
            Log(ConsoleLineKind.Info, "Режим работы: " + DisplayNames.Of(mode));
            Raise();
        }

        public void SelectVial(int nestNumber)
        {
            SelectedVialNest = nestNumber;
            Raise();
        }

        /// <summary>Имитация аварии — используется для просмотра блокирующей панели.</summary>
        public void SimulateAlarm()
        {
            State = MachineState.Alarm;
            var alarm = new AlarmRecord
            {
                Time = DateTime.Now,
                Severity = AlarmSeverity.Alarm,
                Code = "A-311",
                Text = "Истёк таймаут укупорки на гнезде 5. Цикл остановлен.",
                Source = "Станция 3. Укупорка",
                Acknowledged = false
            };
            _alarms.Insert(0, alarm);
            Log(ConsoleLineKind.Error, "АВАРИЯ A-311: " + alarm.Text);

            var handler = AlarmRaised;
            if (handler != null) handler(this, alarm);
            Raise();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (State == MachineState.Running)
            {
                _fillElapsed += 1;
                if (_fillElapsed > 9) _fillElapsed = 0.4;
                _capElapsed += 1;
                if (_capElapsed > 6) _capElapsed = 0.2;

                Batch.Elapsed = Batch.Elapsed.Add(TimeSpan.FromSeconds(1));
                Jitter("o2", 0.08, 0, 12);
                Jitter("h2o", 0.05, 0, 12);
                Jitter("pressure", 0.06, 2, 8);
                Jitter("chamberTemp", 0.03, 20, 27);
                Jitter("productTemp", 0.02, 19, 25);
                Jitter("throughput", 0.05, 6.2, 8.4);
                Jitter("power", 8, 900, 1800);
            }
            else
            {
                UpdateReadiness();
            }

            Raise();
        }

        private void Jitter(string key, double amplitude, double min, double max)
        {
            ParameterSnapshot p;
            if (!_parameters.TryGetValue(key, out p) || !p.Value.HasValue) return;

            double next = p.Value.Value + (_random.NextDouble() - 0.5) * 2 * amplitude;
            if (next < min) next = min;
            if (next > max) next = max;

            p.Value = next;
            p.ValueText = next.ToString(DecimalsOf(key));
        }

        private static string DecimalsOf(string key)
        {
            if (key == "power") return "F0";
            if (key == "o2" || key == "h2o" || key == "throughput") return "F1";
            return "F2";
        }

        private void Raise()
        {
            var handler = Updated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void Log(ConsoleLineKind kind, string text)
        {
            var record = new ConsoleRecord { Time = DateTime.Now, Kind = kind, Text = text };
            _console.Add(record);
            var handler = ConsoleLineAdded;
            if (handler != null) handler(this, record);
        }

        private static StageSnapshot Stage(StageId id, StageState state, string keyCaption, string keyValue,
            double elapsed, double timeout, int nest, int processed, int faults, string faultReason)
        {
            return new StageSnapshot
            {
                Id = id,
                State = state,
                KeyCaption = keyCaption,
                KeyValue = keyValue,
                ElapsedSeconds = elapsed,
                TimeoutSeconds = timeout,
                NestNumber = nest,
                ProcessedCount = processed,
                FaultCount = faults,
                FaultReason = faultReason
            };
        }

        private static StartCondition Condition(string text, bool fulfilled, string detail)
        {
            return new StartCondition { Text = text, Fulfilled = fulfilled, Detail = detail };
        }

        private void Add(string key, string group, string caption, string unit, double? value, string valueText,
            string range, string limit, ParameterKind kind, ValueStatus status, string description)
        {
            _parameters[key] = new ParameterSnapshot
            {
                Key = key,
                Group = group,
                Caption = caption,
                Unit = unit,
                Value = value,
                ValueText = valueText,
                RangeText = range,
                LimitText = limit,
                Kind = kind,
                Status = status,
                Description = description
            };
        }

        private void BuildParameters()
        {
            const string atmosphere = "Атмосфера рабочей камеры";
            const string rotor = "Транспортировка тары и ротор";
            const string dosing = "Дозирование и розлив";
            const string capping = "Укупорка";
            const string gas = "Инертная среда и шлюзы";
            const string machine = "Состояние установки и партия";

            Add("o2", atmosphere, "Содержание кислорода O₂", "ppm", 3.2, "3,2", "0–1000", "порог 10 ppm",
                ParameterKind.Indication, ValueStatus.Normal,
                "Остаточный кислород в камере. Основной показатель готовности камеры к работе и главный источник окисления продукта.");
            Add("o2purge", atmosphere, "O₂, диапазон продувки", "об. %", 0.02, "0,02", "0–25", null,
                ParameterKind.Indication, ValueStatus.Normal,
                "Второй диапазон того же измерения. Нужен после открытия камеры или шлюза, когда прецизионный датчик зашкаливает.");
            Add("o2limit", atmosphere, "Предельное содержание O₂", "ppm", 10, "10", "1–1000", null,
                ParameterKind.AlarmLimit, ValueStatus.Normal, "Порог автоматической остановки, задаётся под конкретный продукт.");
            Add("h2o", atmosphere, "Содержание влаги H₂O", "ppm", 1.8, "1,8", "0–1000", "порог 10 ppm",
                ParameterKind.Indication, ValueStatus.Normal,
                "Остаточная вода в атмосфере камеры. Критична для гигроскопичных и водочувствительных веществ.");
            Add("dewpoint", atmosphere, "Точка росы", "°C", -68.4, "−68,4", "−80…+20", null,
                ParameterKind.Calculated, ValueStatus.Normal, "Альтернативная форма представления влажности, пересчитывается из содержания влаги.");
            Add("h2olimit", atmosphere, "Предельное содержание H₂O", "ppm", 10, "10", "1–1000", null,
                ParameterKind.AlarmLimit, ValueStatus.Normal, "Порог автоматической остановки по влаге.");
            Add("pressure", atmosphere, "Избыточное давление в камере", "мбар", 4.2, "4,2", "−20…+20", "уставка 4,0; границы −5…+15",
                ParameterKind.Indication, ValueStatus.Normal,
                "Разность давлений между камерой и помещением. Только избыточное давление гарантирует, что при неплотности наружный воздух не пойдёт внутрь.");
            Add("pressureSet", atmosphere, "Уставка избыточного давления", "мбар", 4.0, "4,0", "0…+10", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Значение, которое поддерживает система регулирования.");
            Add("leak", atmosphere, "Скорость утечки", "л/ч", 0.42, "0,42", "0–5", "требование ТЗ: не более 1 л/ч",
                ParameterKind.Calculated, ValueStatus.Normal, "Расход газа для поддержания давления при закрытых шлюзах. Прямая проверка требования ТЗ.");
            Add("chamberTemp", atmosphere, "Температура в камере", "°C", 23.4, "23,4", "0–60", null,
                ParameterKind.Indication, ValueStatus.Normal, "Температура газовой среды рабочей зоны. Не заменяет измерения температуры продукта.");
            Add("vapour", atmosphere, "Содержание паров растворителя", "ppm", 62, "62", "0–1000", "порог 200 ppm",
                ParameterKind.Indication, ValueStatus.Normal, "Накопление паров продукта в атмосфере камеры; показатель взрывобезопасности.");

            Add("rotorType", rotor, "Тип установленного ротора", null, null, "РТ-24-20", "по перечню оснастки", null,
                ParameterKind.Indication, ValueStatus.Normal, "Идентификатор сменной кассеты. Участвует в блокировке запуска.");
            Add("vialSize", rotor, "Типоразмер флакона", "мл", 20, "20", "5–50", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Объём тары, под который настроен цикл. Ограничивает допустимую дозу.");
            Add("nests", rotor, "Количество гнёзд", "шт", 24, "24", "1–100", null,
                ParameterKind.Indication, ValueStatus.Normal, "Число посадочных мест в установленном роторе.");
            Add("position", rotor, "Текущая позиция ротора", "номер", 7, "7", "1…24", null,
                ParameterKind.Indication, ValueStatus.Normal, "Номер гнезда в рабочей позиции. Позволяет связать брак с конкретным гнездом.");
            Add("throughput", rotor, "Фактическая производительность", "шт/мин", 7.2, "7,2", "0–20", "требование ТЗ: не менее 6",
                ParameterKind.Calculated, ValueStatus.Normal, "Реальный темп выпуска готовой тары за последнюю минуту.");
            Add("good", rotor, "Счётчик годных флаконов", "шт", 128, "128", "0…", null,
                ParameterKind.Indication, ValueStatus.Normal, "Флаконы, прошедшие все операции без замечаний.");
            Add("rejects", rotor, "Счётчик брака по причинам", "шт", 3, "3", "0…", "доза 2 / укупорка 1",
                ParameterKind.Indication, ValueStatus.Warning, "Отбракованные флаконы с разделением по причине.");
            Add("rejectShare", rotor, "Доля брака", "%", 2.3, "2,3", "0–100", null,
                ParameterKind.Calculated, ValueStatus.Normal, "Отношение брака к общему числу обработанных флаконов.");
            Add("magazines", rotor, "Состояние накопителей", "шт", null, "вход 186 / годн. 128 / негодн. 3", "0…", null,
                ParameterKind.Indication, ValueStatus.Normal, "Показывает оператору, когда требуется вмешательство.");

            Add("doseSet", dosing, "Уставка дозы, объём", "мл", 20, "20,00", "5–50", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Заданный объём продукта на один флакон.");
            Add("doseMass", dosing, "Уставка дозы, масса", "г", 15.68, "15,68", "расчётный", null,
                ParameterKind.Calculated, ValueStatus.Normal, "Та же доза, выраженная в массе. Основная рабочая величина при весовом дозировании.");
            Add("productTemp", dosing, "Температура продукта", "°C", 21.8, "21,8", "0–60", "допуск 18–24 °C",
                ParameterKind.Indication, ValueStatus.Normal, "Температура разливаемой жидкости — исходная величина для температурной поправки плотности.");
            Add("density0", dosing, "Плотность при реперной температуре", "г/см³", 0.786, "0,786", "0,5–2,0", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Справочная плотность вещества, коэффициент пересчёта массы в объём.");
            Add("refTemp", dosing, "Реперная температура плотности", "°C", 20, "20,0", "0–40", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Температура, при которой справедливо введённое значение плотности.");
            Add("tempCoef", dosing, "Температурный коэффициент плотности", "%/°C", 0.14, "0,14", "0–0,2", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Для ацетонитрила около 0,14 %/°C: отклонение 7 °C исчерпывает весь допуск 1 об. %.");
            Add("density", dosing, "Плотность при текущей температуре", "г/см³", 0.784, "0,784", "0,5–2,0", null,
                ParameterKind.Calculated, ValueStatus.Normal, "Плотность, фактически применяемая установкой. Выводится, чтобы поправка была видимой и проверяемой.");
            Add("tempDeviation", dosing, "Отклонение температуры продукта", "°C", 1.8, "+1,8", "−20…+20", "границы ±4,0",
                ParameterKind.AlarmLimit, ValueStatus.Normal, "Разность между текущей и реперной температурой.");
            Add("doseActual", dosing, "Фактическая доза последнего флакона", "мл", 20.06, "20,06 (15,72 г)", "0–60", null,
                ParameterKind.Indication, ValueStatus.Normal, "Реально отпущенное количество продукта. Основной контрольный параметр цикла.");
            Add("doseDeviation", dosing, "Отклонение дозы", "%", 0.3, "+0,30", "−5…+5", "допуск ±1,00 % (ТЗ)",
                ParameterKind.Calculated, ValueStatus.Normal, "Разность между фактической и заданной дозой. Выход за допуск — причина отбраковки.");
            Add("fillRate", dosing, "Скорость налива", "мл/с", 12.4, "12,4", "0–50", "уставка 14,0",
                ParameterKind.Indication, ValueStatus.Normal, "Темп подачи продукта. Влияет на производительность, пенообразование и разбрызгивание.");
            Add("productLeft", dosing, "Остаток продукта в питающей ёмкости", "мл", 1840, "1840 (≈ 92 фл.)", "0…", null,
                ParameterKind.Indication, ValueStatus.Normal, "Даёт оператору время подготовить замену ёмкости, не прерывая партию аварийно.");
            Add("nozzle", dosing, "Состояние сопла", null, null, "норма", "норма / каплеобразование", null,
                ParameterKind.Indication, ValueStatus.Normal, "Признак срабатывания отсечки потока и отсутствия капель после дозы.");

            Add("capTime", capping, "Время укупорки флакона", "с", 3.4, "3,4", "0–10", null,
                ParameterKind.Indication, ValueStatus.Normal, "Определяет, не является ли укупорщик узким местом по отношению к 6 шт/мин.");
            Add("torque", capping, "Момент затяжки", "Н·м", 1.42, "1,42", "0–5", "уставка 1,40 ± 0,15",
                ParameterKind.Indication, ValueStatus.Normal, "Недостаточный момент — негерметичный флакон, избыточный — срыв резьбы.");
            Add("angle", capping, "Угол затяжки", "°", 412, "412", "0–720", "допуск 360–480",
                ParameterKind.Indication, ValueStatus.Normal, "Позволяет отличить нормальную затяжку от перекоса и закусывания резьбы.");
            Add("caps", capping, "Остаток крышек в магазине", "шт", 412, "412", "0…", "предупреждение ниже 500",
                ParameterKind.Indication, ValueStatus.Warning, "Предупреждение выдаётся заранее, чтобы пополнение шло без остановки партии.");
            Add("capFails", capping, "Счётчик неудачных укупорок", "шт", 1, "1", "0…", null,
                ParameterKind.Indication, ValueStatus.Normal, "Показатель состояния укупорочной головки и качества партии крышек.");

            Add("gasPressure", gas, "Давление газа на входе", "бар", 5.4, "5,4", "0–10", "норма 4,0–6,0",
                ParameterKind.Indication, ValueStatus.Normal, "Падение ниже нормы означает, что установка не сможет поддержать давление и продуть шлюзы.");
            Add("gasFlow", gas, "Расход газа", "л/мин", 2.8, "2,8", "0–50", null,
                ParameterKind.Indication, ValueStatus.Normal, "Резкий рост при закрытых шлюзах указывает на нарушение герметичности.");
            Add("gasTotal", gas, "Расход газа за партию", "л", 48.6, "48,6", "0…", null,
                ParameterKind.Calculated, ValueStatus.Normal, "Используется для расчёта себестоимости фасовки и планирования запаса газа.");
            Add("inGate", gas, "Состояние входного шлюза", null, null, "закрыт", "открыт / закрыт / продувка", null,
                ParameterKind.Indication, ValueStatus.Normal, "Пока шлюз не приведён в требуемое состояние, передача флаконов не разрешается.");
            Add("outGate", gas, "Состояние выходного шлюза", null, null, "продувка", "открыт / закрыт / продувка", null,
                ParameterKind.Indication, ValueStatus.Normal, "То же для выгрузочной камеры.");
            Add("purge", gas, "Прогресс продувки шлюза", "%", 66, "66 (цикл 2 из 3)", "0–100", null,
                ParameterKind.Indication, ValueStatus.Normal, "Отвечает на вопрос оператора «сколько ещё ждать».");
            Add("purgeCycles", gas, "Число циклов продувки", "шт", 3, "3", "1–10", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Больше циклов — чище атмосфера, но дольше и дороже по газу.");
            Add("workingGas", gas, "Рабочий газ", null, null, "Азот", "азот / аргон", null,
                ParameterKind.Setpoint, ValueStatus.Normal, "Влияет на градуировку датчиков, расчёт расхода и записи в отчёте о партии.");

            Add("power", machine, "Потребляемая мощность", "Вт", 1265, "1265", "0–3000", "не более 3000 Вт (ТЗ)",
                ParameterKind.Indication, ValueStatus.Normal, "Контроль соответствия требованию ТЗ и косвенный признак механических неисправностей.");
        }

        private void BuildConsole()
        {
            var start = DateTime.Now.AddMinutes(-18);
            _console.Add(new ConsoleRecord { Time = start, Kind = ConsoleLineKind.Command, Text = "Загружен рецепт «Ацетонитрил 20 мл», версия 3, к/сумма 8F3A-21C4" });
            _console.Add(new ConsoleRecord { Time = start.AddSeconds(6), Kind = ConsoleLineKind.Info, Text = "Проверка оснастки: ротор РТ-24-20 подтверждён" });
            _console.Add(new ConsoleRecord { Time = start.AddSeconds(14), Kind = ConsoleLineKind.Info, Text = "Продувка шлюза завершена, 3 цикла" });
            _console.Add(new ConsoleRecord { Time = start.AddSeconds(22), Kind = ConsoleLineKind.Command, Text = "Пуск цикла, партия П-2026-0914-03, задание 500 шт" });
            _console.Add(new ConsoleRecord { Time = start.AddMinutes(7), Kind = ConsoleLineKind.Warning, Text = "Флакон 64 отбракован: отклонение дозы +1,12 %" });
            _console.Add(new ConsoleRecord { Time = start.AddMinutes(9), Kind = ConsoleLineKind.Warning, Text = "Флакон 81 отбракован: момент затяжки не достигнут за 3 попытки" });
            _console.Add(new ConsoleRecord { Time = start.AddMinutes(14), Kind = ConsoleLineKind.Info, Text = "Соединение с ПК установлено: WS-TECH-04 (192.168.10.25)" });
            _console.Add(new ConsoleRecord { Time = start.AddMinutes(16), Kind = ConsoleLineKind.Warning, Text = "W-214: остаток крышек в магазине ниже 500 шт" });
        }
    }
}
