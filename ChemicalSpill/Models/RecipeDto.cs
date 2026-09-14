using System;
using System.Collections.Generic;

namespace ChemicalSpill.Models
{
    /// <summary>
    /// Краткие сведения о рецепте для списка выбора.
    /// Источник: «Функциональный интерфейс», п. 5.2, блок 1.
    /// </summary>
    public class RecipeSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>Номер версии, присваивается автоматически при каждом сохранении.</summary>
        public string Version { get; set; }

        public string Product { get; set; }
        public RecipeState State { get; set; }

        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }

        /// <summary>Контрольная сумма по содержимому рецепта (доказательство неизменности).</summary>
        public string Checksum { get; set; }

        /// <summary>Типоразмер флакона, мл.</summary>
        public double VialSizeMl { get; set; }

        /// <summary>Доза, мл.</summary>
        public double DoseMl { get; set; }

        public string RotorType { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>
    /// Полный состав рецепта, блоки 1–7. Источник: «Функциональный интерфейс», п. 5.2.
    /// Ядро наполняет объект при загрузке и принимает его при сохранении.
    /// </summary>
    public class RecipeDocument
    {
        public RecipeSummary Summary { get; set; }

        // Блок 2. Тара и оснастка
        public double VialSizeMl { get; set; }
        public string RotorType { get; set; }
        public int NestCount { get; set; }
        public double NeckHeightMm { get; set; }
        public double NeckDiameterMm { get; set; }
        public string ClosureType { get; set; }

        // Блок 3. Свойства продукта
        public double DensityAtReference { get; set; }
        public double ReferenceTemperature { get; set; }
        public double TemperatureCoefficient { get; set; }
        public double ProductTemperatureMin { get; set; }
        public double ProductTemperatureMax { get; set; }
        public string ProductNotes { get; set; }

        // Блок 4. Дозирование
        public double DoseMl { get; set; }
        public double DoseTolerancePercent { get; set; }
        public double MainPhaseRate { get; set; }
        public double FinalPhaseRate { get; set; }
        public double PhaseSwitchPercent { get; set; }
        public double ScaleSettlingSeconds { get; set; }
        public double NozzleRetractDelaySeconds { get; set; }
        public double FillTimeoutSeconds { get; set; }

        // Блок 5. Укупорка
        public double CapTorqueNm { get; set; }
        public double CapTorqueToleranceNm { get; set; }
        public double CapAngleMinDeg { get; set; }
        public double CapAngleMaxDeg { get; set; }
        public double CapSpeedRpm { get; set; }
        public double CapHoldSeconds { get; set; }
        public int CapAttempts { get; set; }
        public double CapTimeoutSeconds { get; set; }

        // Блок 6. Атмосфера
        public WorkingGas Gas { get; set; }
        public double OxygenLimitPpm { get; set; }
        public double MoistureLimitPpm { get; set; }
        public double PressureSetpointMbar { get; set; }
        public double PressureLowMbar { get; set; }
        public double PressureHighMbar { get; set; }
        public int GatePurgeCycles { get; set; }

        // Блок 7. Цикл
        public double FeedSpeedPercent { get; set; }
        public double FeedTimeoutSeconds { get; set; }
        public double RotorSpeedPercent { get; set; }
        public double RotorSettlingSeconds { get; set; }
        public double DischargeSpeedPercent { get; set; }
        public double DischargeTimeoutSeconds { get; set; }
        public double PauseBetweenCyclesSeconds { get; set; }

        /// <summary>Правило сортировки: условия направления флакона в приёмник негодной продукции.</summary>
        public List<RejectReason> SortingRule { get; set; }

        public RecipeDocument()
        {
            Summary = new RecipeSummary();
            SortingRule = new List<RejectReason>();
        }
    }
}
