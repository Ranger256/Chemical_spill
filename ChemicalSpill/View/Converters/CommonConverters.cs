using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChemicalSpill.Models;

namespace ChemicalSpill.View.Converters
{
    /// <summary>Логическое значение в видимость. Параметр «invert» меняет смысл на обратный.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool && (bool)value;
            if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase)) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>Инверсия логического значения.</summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool && (bool)value);
        }
    }

    /// <summary>Непустая строка или объект — видимость.</summary>
    public class NotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            bool hasValue = text != null ? !string.IsNullOrWhiteSpace(text) : value != null;
            if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase)) hasValue = !hasValue;
            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Сравнение значения с параметром — для переключателей, привязанных к перечислению.</summary>
    public class EqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = value is bool && (bool)value;
            if (!isChecked || parameter == null) return Binding.DoNothing;

            return Enum.IsDefined(targetType, parameter.ToString())
                ? Enum.Parse(targetType, parameter.ToString())
                : Binding.DoNothing;
        }
    }

    /// <summary>Русское наименование значения перечисления.</summary>
    public class EnumDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MachineState) return DisplayNames.Of((MachineState)value);
            if (value is OperatingMode) return DisplayNames.Of((OperatingMode)value);
            if (value is StageState) return DisplayNames.Of((StageState)value);
            if (value is StageId) return DisplayNames.Of((StageId)value);
            if (value is ParameterKind) return DisplayNames.Of((ParameterKind)value);
            if (value is RecipeState) return DisplayNames.Of((RecipeState)value);
            if (value is PcLinkState) return DisplayNames.Of((PcLinkState)value);
            if (value is DuplexMode) return DisplayNames.Of((DuplexMode)value);
            if (value is GateState) return DisplayNames.Of((GateState)value);
            if (value is AlarmSeverity) return DisplayNames.Of((AlarmSeverity)value);
            if (value is RejectReason) return DisplayNames.Of((RejectReason)value);
            if (value is JournalCategory) return DisplayNames.Of((JournalCategory)value);
            if (value is TrendWindow) return DisplayNames.Of((TrendWindow)value);
            if (value is WorkingGas) return DisplayNames.Of((WorkingGas)value);
            if (value is AddressMode) return DisplayNames.Of((AddressMode)value);

            return value == null ? string.Empty : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Кисти состояния стадии. Параметр: «fill», «stroke» или «text».
    /// </summary>
    public class StageStateBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = (parameter as string) ?? "fill";
            var state = value is StageState ? (StageState)value : StageState.Inactive;

            string key;
            switch (state)
            {
                case StageState.Running: key = "StateRunning"; break;
                case StageState.Completed: key = "StateCompleted"; break;
                case StageState.Waiting: key = "StateWaiting"; break;
                case StageState.Fault: key = "StateFault"; break;
                default: key = "StateInactive"; break;
            }

            if (role == "stroke") key += "Stroke";
            else if (role == "text") key += "Text";
            else key += "Fill";

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кисть по оценке значения: норма, предупреждение, авария.</summary>
    public class ValueStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value is ValueStatus ? (ValueStatus)value : ValueStatus.Unknown;
            var soft = string.Equals(parameter as string, "soft", StringComparison.OrdinalIgnoreCase);

            string key;
            switch (status)
            {
                case ValueStatus.Warning: key = soft ? "WarningSoftBrush" : "WarningBrush"; break;
                case ValueStatus.Alarm: key = soft ? "AlarmSoftBrush" : "AlarmBrush"; break;
                case ValueStatus.Normal: key = soft ? "OkSoftBrush" : "OkBrush"; break;
                default: key = soft ? "PanelHeaderBrush" : "TextMutedBrush"; break;
            }

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кисть строки консоли по типу сообщения.</summary>
    public class ConsoleKindBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value is ConsoleLineKind ? (ConsoleLineKind)value : ConsoleLineKind.Info;

            string key;
            switch (kind)
            {
                case ConsoleLineKind.Command: key = "ConsoleCommandBrush"; break;
                case ConsoleLineKind.Warning: key = "ConsoleWarningBrush"; break;
                case ConsoleLineKind.Error: key = "ConsoleErrorBrush"; break;
                default: key = "ConsoleTextBrush"; break;
            }

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Gainsboro;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кисть по логическому признаку: true — норма, false — отклонение.</summary>
    public class BoolBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool && (bool)value;
            var key = flag ? "OkBrush" : "AlarmBrush";

            if (string.Equals(parameter as string, "warning", StringComparison.OrdinalIgnoreCase))
                key = flag ? "OkBrush" : "WarningBrush";

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кисть состояния установки для индикатора в строке состояния.</summary>
    public class MachineStateBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value is MachineState ? (MachineState)value : MachineState.Stopped;

            string key;
            switch (state)
            {
                case MachineState.Running: key = "OkBrush"; break;
                case MachineState.Ready: key = "AccentBrush"; break;
                case MachineState.Paused: key = "WarningBrush"; break;
                case MachineState.Purging: key = "WarningBrush"; break;
                case MachineState.Alarm: key = "AlarmBrush"; break;
                default: key = "TextSecondaryBrush"; break;
            }

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кисть по состоянию рецепта.</summary>
    public class RecipeStateBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value is RecipeState ? (RecipeState)value : RecipeState.Draft;

            string key;
            switch (state)
            {
                case RecipeState.Approved: key = "OkBrush"; break;
                case RecipeState.Verified: key = "AccentBrush"; break;
                case RecipeState.Draft: key = "WarningBrush"; break;
                default: key = "TextMutedBrush"; break;
            }

            var brush = Application.Current != null ? Application.Current.TryFindResource(key) as Brush : null;
            return brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Цвет кривой графика по ключу параметра: один и тот же параметр всегда
    /// рисуется одним цветом на всех экранах.
    /// </summary>
    public class SeriesBrushConverter : IValueConverter
    {
        private static readonly string[] Keys =
        {
            "Series1Brush", "Series2Brush", "Series3Brush",
            "Series4Brush", "Series5Brush", "Series6Brush"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var series = value as TrendSeries;
            var key = series != null ? series.Key : value as string;

            int index = 0;
            if (!string.IsNullOrEmpty(key))
            {
                int sum = 0;
                foreach (var symbol in key) sum += symbol;
                index = Math.Abs(sum) % Keys.Length;
            }

            var brush = Application.Current != null ? Application.Current.TryFindResource(Keys[index]) as Brush : null;
            return brush ?? Brushes.SteelBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>Кадр видеонаблюдения из сжатого изображения.</summary>
    public class FrameToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];
            if (bytes == null || bytes.Length == 0) return null;

            var image = new BitmapImage();
            using (var stream = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
