using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FineSlider.Net.Utility;

namespace FineSlider.Net
{
    public partial class FineSlider : UserControl, INotifyPropertyChanged
    {
        private const double GoodTickFreq = 40;
        private const double MarginLabel = 10;
        private Point initMousePoint;
        private double initValue;
        private double initSpan;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsActive { get; set; }
        public string ValueString
        {
            get
            {
                return Value.NominalToString(Unit, digitsHard: 2 + AdditionalDigits(Value));
            }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueString)));
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(FineSlider), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ValuePropertyChangedCallback)));
        private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FineSlider input = (FineSlider)d;
            if (!input.IsActive)
            {
                input.RedrawLimb();
                input.PropertyChanged?.Invoke(input, new PropertyChangedEventArgs(nameof(ValueString)));
            }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(FineSlider), new PropertyMetadata(double.NaN, new PropertyChangedCallback(EdgePropertyesChangedCallback)));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(FineSlider), new PropertyMetadata(double.NaN, new PropertyChangedCallback(EdgePropertyesChangedCallback)));


        private static void EdgePropertyesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FineSlider input = (FineSlider)d;
            input.RedrawLimb();
        }

        public double Span
        {
            get { return (double)GetValue(SpanProperty); }
            set { SetValue(SpanProperty, value); }
        }

        public static readonly DependencyProperty SpanProperty =
            DependencyProperty.Register("Span", typeof(double), typeof(FineSlider), new PropertyMetadata(1.0, new PropertyChangedCallback(SpanPropertyChangedCallback)));

        private static void SpanPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FineSlider input = (FineSlider)d;
            //input.RedrawSpanValue();
            input.RedrawLimb();
        }

        public double MinimumSpan
        {
            get { return (double)GetValue(MinimumSpanProperty); }
            set { SetValue(MinimumSpanProperty, value); }
        }

        public static readonly DependencyProperty MinimumSpanProperty =
            DependencyProperty.Register("MinimumSpan", typeof(double), typeof(FineSlider), new PropertyMetadata(double.NaN));

        public double MaximumSpan
        {
            get { return (double)GetValue(MaximumSpanProperty); }
            set { SetValue(MaximumSpanProperty, value); }
        }

        public static readonly DependencyProperty MaximumSpanProperty =
            DependencyProperty.Register("MaximumSpan", typeof(double), typeof(FineSlider), new PropertyMetadata(double.NaN));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(FineSlider), new PropertyMetadata(string.Empty, new PropertyChangedCallback(UnitPropertyChangedCallback)));

        private static void UnitPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FineSlider input = (FineSlider)d;
            input.PropertyChanged?.Invoke(input, new PropertyChangedEventArgs(nameof(ValueString)));
            //input.RedrawSpanValue();
            input.RedrawLimb();
        }

        public bool SmoothSpan
        {
            get { return (bool)GetValue(SmoothSpanProperty); }
            set { SetValue(SmoothSpanProperty, value); }
        }

        public static readonly DependencyProperty SmoothSpanProperty =
            DependencyProperty.Register("SmoothSpan", typeof(bool), typeof(FineSlider), new PropertyMetadata(true));

        public FineSlider()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawLimb();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueString)));
            RedrawLimb();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Mouse.Capture(this as IInputElement))
            {
                initMousePoint = e.GetPosition(this as IInputElement);
                initValue = Value;
                IsActive = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
                e.Handled = true;
            }
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsActive = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
                Mouse.Capture(null);
                e.Handled = true;
            }
            base.OnPreviewMouseUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (!AreAnyTouchesCaptured && IsActive && e.LeftButton == MouseButtonState.Pressed)
            {
                double deltapx = initMousePoint.X - e.GetPosition(this as IInputElement).X;
                OffsetValue(deltapx);
                RedrawLimb();
                e.Handled = true;
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta < 0) OffsetValue(-1);
                else OffsetValue(1);
                initMousePoint = e.GetPosition(this as IInputElement);
                initValue = Value;
            }
            else
            {
                double newSpan;
                if (SmoothSpan)
                {
                    if (e.Delta < 0) newSpan = Span * 1.1;
                    else newSpan = Span / 1.1;
                }
                else
                {
                    if (e.Delta < 0) newSpan = Near125(Span * 2);
                    else newSpan = Near125(Span / 2);
                }
                SetNewSpan(newSpan);
            }
            RedrawLimb();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueString)));
            e.Handled = true;
            base.OnPreviewMouseWheel(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            initValue = Value;
            initSpan = Span;
            IsActive = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            e.Handled = true;
            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            IsActive = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            e.Handled = true;
            base.OnManipulationCompleted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (Math.Abs(e.CumulativeManipulation.Scale.X - 1) > 0.01)
            {
                if (SmoothSpan) SetNewSpan(initSpan / e.CumulativeManipulation.Scale.X);
                else SetNewSpan(Near125(initSpan / e.CumulativeManipulation.Scale.X));
                RedrawLimb();
            }
            else
            {
                OffsetValue(-e.CumulativeManipulation.Translation.X);
                RedrawLimb();
            }
            e.Handled = true;
            base.OnManipulationDelta(e);
        }

        private void OffsetValue(double deltapx)
        {
            double w = ActualWidth;
            double span = Span;
            double valuePerPx = span / w;

            double deltaval = deltapx * valuePerPx;
            double newval = initValue + deltaval;
            SetNewVal(newval);
        }

        private void OffsetValue(int subticks)
        {
            double w = ActualWidth;
            double span = Span;
            int ticksQty = TickQty(w);
            double ticksStep = span / ticksQty;
            double ticksSubStep = ticksStep / 10;

            double deltaval = subticks * ticksSubStep;
            double newval = Value + deltaval;
            SetNewVal(newval);
        }

        private void SetNewVal(double newval)
        {
            double w = ActualWidth;
            double span = Span;
            int ticksQty = TickQty(w);
            double ticksStep = span / ticksQty;
            double ticksSubStep = ticksStep / 100;

            double nearSubTick = Math.Round(newval / ticksSubStep) * ticksSubStep;
            newval = nearSubTick;

            if (!double.IsNaN(Maximum) && newval > Maximum) Value = Maximum;
            else if (!double.IsNaN(Minimum) && newval < Minimum) Value = Minimum;
            else Value = newval;
        }

        private void SetNewSpan(double newSpan)
        {
            if ((double.IsNaN(MinimumSpan) || newSpan >= MinimumSpan) && (double.IsNaN(MaximumSpan) || newSpan <= MaximumSpan))
            {
                Span = newSpan;
            }
            if (!double.IsNaN(MinimumSpan) && newSpan < MinimumSpan) Span = MinimumSpan;
            if (!double.IsNaN(MaximumSpan) && newSpan > MaximumSpan) Span = MaximumSpan;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueString)));
        }

        private int TickQty(double w)
        {
            double targetTicksDiv = w / GoodTickFreq;
            int ticksQty = (int)Near125(targetTicksDiv);
            return ticksQty;
        }

        private int AdditionalDigits(double value)
        {
            double span = Span;
            int majorValue = Math.Abs(value) < double.Epsilon ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(value)));
            int majorSpan = Math.Abs(span) < double.Epsilon ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(span)));
            return majorValue - majorSpan < 0 ? 0 : majorValue - majorSpan;
        }

        private void RedrawLimb()
        {
            double labelValueWidth = LabelValue.ActualWidth;
            //double labelSpanWidth = LabelSpanValueRight.ActualWidth;
            double w = ActualWidth;
            double span = Span;
            double value = Value;
            double maximum = Maximum;
            double minimum = Minimum;
            double valuePerPx = span / w;

            int ticksQty = TickQty(w);
            double ticksStep = span / ticksQty;
            ticksStep = Near125(ticksStep);
            ticksQty = (int)(span / ticksStep);
            double nearTick = Math.Round(value / ticksStep) * ticksStep;
            double leftTick = nearTick - (ticksQty / 2) * ticksStep;
            double rightTick = nearTick + (ticksQty / 2) * ticksStep;

            CanvasTicks.Children.Clear();
            TicksLabel.Children.Clear();

            double convert(double v)
            {
                return w / 2 + (v - value) / valuePerPx;
            }

            //Measure label width for decimation
            TextBlock test = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = leftTick.NominalToString(Unit, digitsHard: 2 + AdditionalDigits(leftTick) - 1),
            };
            test.Measure(new Size(w, 100));
            double maxSizeTick = test.DesiredSize.Width;
            test = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = rightTick.NominalToString(Unit, digitsHard: 2 + AdditionalDigits(rightTick) - 1),
            };
            test.Measure(new Size(w, 100));
            maxSizeTick = test.DesiredSize.Width > maxSizeTick ? test.DesiredSize.Width : maxSizeTick;
            test = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = nearTick.NominalToString(Unit, digitsHard: 2 + AdditionalDigits(nearTick) - 1),
            };
            test.Measure(new Size(w, 100));
            maxSizeTick = test.DesiredSize.Width > maxSizeTick ? test.DesiredSize.Width : maxSizeTick;
            double ticksStepPx = ticksStep / valuePerPx;
            int decimation = (int)Math.Ceiling((maxSizeTick + MarginLabel) / ticksStepPx);
            if (decimation <= 0) decimation = 1;

            for (int t = -ticksQty / 2 - 1; t <= ticksQty / 2 + 1; t++)
            {
                double tickVal = nearTick + t * ticksStep;
                int tFromZero = (int)Math.Round(tickVal / ticksStep);
                double px = convert(tickVal);

                if (px > 0 && px < w)
                {
                    TextBlock tb = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xAC, 0xAC, 0xAC)),
                        Text = tickVal.NominalToString(Unit, digitsHard: 2 + AdditionalDigits(tickVal) - 1),
                        RenderTransform = new TranslateTransform(px - w / 2, 0),
                    };

                    tb.Measure(new Size(w, 100));
                    double leftEdgeLabel = px - tb.DesiredSize.Width / 2;
                    double rightEdgeLabel = px + tb.DesiredSize.Width / 2;
                    double leftEdgeValue = w / 2 - labelValueWidth / 2 - MarginLabel;
                    double rightEdgeValue = w / 2 + labelValueWidth / 2 + MarginLabel;
                    if (
                        leftEdgeLabel > 0 + MarginLabel && rightEdgeLabel < w - MarginLabel
                        //leftEdgeLabel > labelSpanWidth + MarginLabel && rightEdgeLabel < w - labelSpanWidth - MarginLabel
                        && (leftEdgeLabel < leftEdgeValue || leftEdgeLabel > rightEdgeValue)
                        && (rightEdgeLabel < leftEdgeValue || rightEdgeLabel > rightEdgeValue)
                        && tFromZero % decimation == 0) TicksLabel.Children.Add(tb);

                    Line tick = new Line
                    {
                        X1 = px,
                        Y1 = 6,
                        X2 = px,
                        Y2 = 6 + 5,
                        Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xAC, 0xAC, 0xAC)),
                        StrokeThickness = 1,
                        SnapsToDevicePixels = true,
                    };
                    if (tFromZero % decimation == 0) CanvasTicks.Children.Add(tick);

                }
            }

            double rwall = convert(maximum);
            if (rwall > 0 && rwall < w) RightWall.Width = w - rwall;
            else RightWall.Width = 0;

            double lwall = convert(minimum);
            if (lwall > 0 && lwall < w) Canvas.SetLeft(LeftWall, -(w - lwall)); //LeftWall.canvas = lwall;
            else Canvas.SetLeft(LeftWall, w);
        }
    }
}
