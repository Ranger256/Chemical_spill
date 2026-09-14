using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ChemicalSpill.Models;

namespace ChemicalSpill.View.Controls
{
    /// <summary>
    /// График одного параметра во времени. По одному мгновенному числу нельзя
    /// отличить стабильную камеру от камеры, в которой значение медленно растёт,
    /// поэтому величины выводятся кривыми (п. 2.1 перечня параметров).
    /// Линии аварийных уставок показывают запас до порога.
    /// </summary>
    public partial class TrendChart : UserControl
    {
        private const double LeftAxis = 48;
        private const double BottomAxis = 18;
        private const double TopPadding = 10;
        private const double RightPadding = 10;

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(
            "Series", typeof(TrendSeries), typeof(TrendChart),
            new PropertyMetadata(null, OnSeriesChanged));

        /// <summary>Цвет кривой; задаётся из разметки, чтобы графики отличались.</summary>
        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
            "LineBrush", typeof(Brush), typeof(TrendChart),
            new PropertyMetadata(null, OnSeriesChanged));

        public TrendChart()
        {
            InitializeComponent();

            SizeChanged += delegate { Render(); };
            Loaded += delegate { Render(); };
        }

        public TrendSeries Series
        {
            get { return (TrendSeries)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        private static void OnSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = d as TrendChart;
            if (chart != null) chart.Render();
        }

        private void Render()
        {
            PlotCanvas.Children.Clear();

            var series = Series;
            if (series == null)
            {
                CaptionText.Text = string.Empty;
                LastValueText.Text = string.Empty;
                return;
            }

            CaptionText.Text = series.Caption;
            LastValueText.Text = series.Points.Count > 0
                ? series.Points[series.Points.Count - 1].Value.ToString("0.##") + " " + series.Unit
                : "—";

            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            if (width < 40 || height < 30) return;

            double plotWidth = width - LeftAxis - RightPadding;
            double plotHeight = height - TopPadding - BottomAxis;
            if (plotWidth <= 0 || plotHeight <= 0) return;

            double min, max;
            GetScale(series, out min, out max);
            if (Math.Abs(max - min) < 1e-9) max = min + 1;

            DrawGrid(min, max, plotWidth, plotHeight);
            DrawLimits(series, min, max, plotWidth, plotHeight);
            DrawSeries(series, min, max, plotWidth, plotHeight);
            DrawTimeAxis(series, plotWidth, plotHeight);
        }

        private static void GetScale(TrendSeries series, out double min, out double max)
        {
            if (series.ScaleMinimum.HasValue && series.ScaleMaximum.HasValue)
            {
                min = series.ScaleMinimum.Value;
                max = series.ScaleMaximum.Value;
                return;
            }

            min = double.MaxValue;
            max = double.MinValue;

            foreach (var point in series.Points)
            {
                if (point.Value < min) min = point.Value;
                if (point.Value > max) max = point.Value;
            }

            if (series.HighLimit.HasValue && series.HighLimit.Value > max) max = series.HighLimit.Value;
            if (series.LowLimit.HasValue && series.LowLimit.Value < min) min = series.LowLimit.Value;

            if (min > max)
            {
                min = 0;
                max = 1;
            }

            double margin = (max - min) * 0.1;
            min -= margin;
            max += margin;
        }

        private void DrawGrid(double min, double max, double plotWidth, double plotHeight)
        {
            var gridBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0xEC, 0xF1));
            var labelBrush = TryBrush("TextMutedBrush", Brushes.Gray);

            const int lines = 4;
            for (int i = 0; i <= lines; i++)
            {
                double y = TopPadding + plotHeight * i / lines;

                var line = new Line
                {
                    X1 = LeftAxis,
                    X2 = LeftAxis + plotWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    SnapsToDevicePixels = true
                };
                PlotCanvas.Children.Add(line);

                double value = max - (max - min) * i / lines;
                var label = new TextBlock
                {
                    Text = value.ToString("0.##"),
                    FontSize = 10,
                    FontFamily = new FontFamily("Segoe UI"),
                    Foreground = labelBrush,
                    TextAlignment = TextAlignment.Right,
                    Width = LeftAxis - 6
                };

                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, y - 7);
                PlotCanvas.Children.Add(label);
            }

            var axis = new Line
            {
                X1 = LeftAxis,
                X2 = LeftAxis,
                Y1 = TopPadding,
                Y2 = TopPadding + plotHeight,
                Stroke = TryBrush("BorderBrushLight", Brushes.LightGray),
                StrokeThickness = 1
            };
            PlotCanvas.Children.Add(axis);
        }

        private void DrawLimits(TrendSeries series, double min, double max, double plotWidth, double plotHeight)
        {
            var alarmBrush = TryBrush("AlarmBrush", Brushes.Firebrick);

            if (series.HighLimit.HasValue)
            {
                AddLimitLine(series.HighLimit.Value, min, max, plotWidth, plotHeight, alarmBrush, "порог");
            }

            if (series.LowLimit.HasValue)
            {
                AddLimitLine(series.LowLimit.Value, min, max, plotWidth, plotHeight, alarmBrush, "порог");
            }
        }

        private void AddLimitLine(double value, double min, double max, double plotWidth, double plotHeight,
            Brush brush, string caption)
        {
            if (value < min || value > max) return;

            double y = TopPadding + plotHeight * (1 - (value - min) / (max - min));

            var line = new Line
            {
                X1 = LeftAxis,
                X2 = LeftAxis + plotWidth,
                Y1 = y,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            };
            PlotCanvas.Children.Add(line);

            var label = new TextBlock
            {
                Text = caption + " " + value.ToString("0.##"),
                FontSize = 9,
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = brush
            };

            Canvas.SetLeft(label, LeftAxis + 4);
            Canvas.SetTop(label, y - 12);
            PlotCanvas.Children.Add(label);
        }

        private void DrawSeries(TrendSeries series, double min, double max, double plotWidth, double plotHeight)
        {
            if (series.Points.Count < 2) return;

            var points = new PointCollection();
            int count = series.Points.Count;

            for (int i = 0; i < count; i++)
            {
                double x = LeftAxis + plotWidth * i / (count - 1);
                double normalized = (series.Points[i].Value - min) / (max - min);
                if (normalized < 0) normalized = 0;
                if (normalized > 1) normalized = 1;

                double y = TopPadding + plotHeight * (1 - normalized);
                points.Add(new Point(x, y));
            }

            var polyline = new Polyline
            {
                Points = points,
                Stroke = LineBrush ?? TryBrush("AccentBrush", Brushes.SteelBlue),
                StrokeThickness = 1.6,
                StrokeLineJoin = PenLineJoin.Round
            };

            PlotCanvas.Children.Add(polyline);
        }

        private void DrawTimeAxis(TrendSeries series, double plotWidth, double plotHeight)
        {
            if (series.Points.Count == 0) return;

            var labelBrush = TryBrush("TextMutedBrush", Brushes.Gray);
            double y = TopPadding + plotHeight + 2;

            AddTimeLabel(series.Points[0].Time, LeftAxis, y, labelBrush, TextAlignment.Left);
            AddTimeLabel(series.Points[series.Points.Count - 1].Time, LeftAxis + plotWidth - 44, y, labelBrush,
                TextAlignment.Right);
        }

        private void AddTimeLabel(DateTime time, double left, double top, Brush brush, TextAlignment alignment)
        {
            var label = new TextBlock
            {
                Text = time.ToString("HH:mm:ss"),
                FontSize = 10,
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = brush,
                Width = 48,
                TextAlignment = alignment
            };

            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            PlotCanvas.Children.Add(label);
        }

        private static Brush TryBrush(string key, Brush fallback)
        {
            if (Application.Current == null) return fallback;

            var brush = Application.Current.TryFindResource(key) as Brush;
            return brush ?? fallback;
        }
    }
}
