using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DAP_Serial.Core;

namespace DAP_Serial.Views
{
    /// <summary>
    /// RealtimeCurvePage.xaml 的交互逻辑
    /// </summary>
    public partial class RealtimeCurvePage : Page
    {
        private readonly static RealtimeCurvePage _instance = new RealtimeCurvePage();
        public static RealtimeCurvePage Instance { get { return _instance; } }
        private RealtimeCurvePage()
        {
            InitializeComponent();

            this.DataContext = RealtimeCurveVM.Instance;
            this.Loaded += RealtimeCurvePage_Loaded;
            this.Unloaded += RealtimeCurvePage_Unloaded;
            RealtimeCurveCanvas.SizeChanged += RealtimeCurveCanvas_SizeChanged;
            RealtimeCurveVM.Instance.PropertyChanged += Instance_PropertyChanged;
            SettingVM.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void RealtimeCurvePage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void RealtimeCurvePage_Unloaded(object sender, RoutedEventArgs e)
        {
            RealtimeCurveVM.Instance.Stop();
        }

        private const double X_WIDTH = 18d;
        private const double Y_WIDTH = 50d;
        private const double TOP = 84d;
        private const double BOTTOM = 72d;
        private const double LEFT = 78d;
        private const double RIGHT = 78d;
        private double _vAxisCount;
        private double _hAxisCount;
        private double _hAxisWidth;
        private double _vAxisHeight;
        private object _lockObject = new object();

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "iRealtimeCurveInterval":
                case "iRealtimeCurveCelling":
                case "iRealtimeCurveFloor":
                    PaintCalibration();
                    PaintData();
                    if (SettingVM.Instance.SaveCommand.CanExecute(null))
                    {
                        SettingVM.Instance.DoSave();
                    }
                    break;
                case "RealtimeCurveColor":
                    if (SettingVM.Instance.SaveCommand.CanExecute(null))
                    {
                        SettingVM.Instance.DoSave();
                    }
                    break;
                case "Update":
                    PaintData();
                    break;
            }
        }

        private void RealtimeCurveCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Paint();
        }

        private void Paint()
        {
            PaintBackground();
            PaintCalibration();
            PaintData();
        }

        private void PaintBackground()
        {
            var width = RealtimeCurveCanvas.ActualWidth;
            var height = RealtimeCurveCanvas.ActualHeight;
            HAxisTitleTextBlock.SetValue(Canvas.TopProperty, (height - TOP - BOTTOM + HAxisTitleTextBlock.ActualHeight) / 2d + TOP);
        }

        /// <summary>
        /// 绘制刻度
        /// </summary>
        private void PaintCalibration()
        {
            CalibrationCanvas.Children.Clear();

            var width = RealtimeCurveCanvas.ActualWidth;
            var height = RealtimeCurveCanvas.ActualHeight;
            var drawingWidth = width - LEFT - RIGHT;
            var drawingHeight = height - TOP - BOTTOM;
            var top = height - BOTTOM;
            var intval = SettingVM.Instance.iRealtimeCurveInterval;
            var celling = SettingVM.Instance.iRealtimeCurveCelling;
            var floor = SettingVM.Instance.iRealtimeCurveFloor;
            var decimalPlaces = Math.Max(DecimalPlaces(celling), DecimalPlaces(floor));

            _hAxisCount = Math.Floor(drawingWidth / X_WIDTH);
            _hAxisWidth = drawingWidth / _hAxisCount;
            _vAxisCount = Math.Floor(drawingHeight / Y_WIDTH);
            _vAxisHeight = drawingHeight / _vAxisCount;
            var hAxisWidth = _hAxisWidth;
            var hAxisCount = _hAxisCount;
            var avg = (celling - floor) / _vAxisCount;

            var brush = Brushes.Black;

            // 绘制横坐标刻度线
            for (int i = 0; i <= _hAxisCount; i++)
            {
                var line = new Line();
                line.Stroke = brush;
                line.StrokeThickness = 1d;
                line.X1 = line.X2 = i * _hAxisWidth + LEFT;
                line.Y1 = TOP;
                line.Y2 = height - BOTTOM + 8d;
                CalibrationCanvas.Children.Add(line);
            }

            // 绘制纵坐标刻度线和刻度文本
            for (int i = 0; i <= _vAxisCount; i++)
            {
                var line = new Line();
                line.Stroke = brush;
                line.StrokeThickness = 1d;
                line.X1 = LEFT - 8d;
                line.X2 = width - RIGHT + 8d;
                line.Y1 = line.Y2 = i * _vAxisHeight + TOP;
                CalibrationCanvas.Children.Add(line);

                var text = new TextBlock();
                text.Text = Math.Round(celling - i * avg, decimalPlaces).ToString();
                text.TextAlignment = TextAlignment.Right;
                text.Height = text.FontSize + 5d;
                text.Width = 100;
                text.SetValue(Canvas.LeftProperty, LEFT - text.Width - 15d);
                text.SetValue(Canvas.TopProperty, line.Y1 - text.Height / 2d);
                CalibrationCanvas.Children.Add(text);
            }

            // 绘制横坐标刻度文本
            for (int i = 0; i < hAxisCount; i++)
            {
                var text = new TextBlock();
                text.Text = (i * intval).ToString();
                text.TextAlignment = TextAlignment.Center;
                text.Height = text.FontSize + 5d;
                text.Width = 100;
                text.SetValue(Canvas.LeftProperty, (i + 0.5d) * hAxisWidth + LEFT - text.Width / 2d);
                text.SetValue(Canvas.TopProperty, top);
                CalibrationCanvas.Children.Add(text);
            }

        }

        /// <summary>
        /// 获取小数点位数方法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int DecimalPlaces(double value)
        {
            if (!double.IsInfinity(value) &&
                !double.IsNaN(value))
            {
                var strValue = value.ToString();
                var index = strValue.ToString().IndexOf('.');
                if (index < 0)
                {
                    return 0;
                }
                return strValue.Length - index - 1;
            }
            return 0;
        }

        /// <summary>
        /// 绘制数据区域
        /// </summary>
        private void PaintData()
        {
            HAxisCanvas.Children.Clear();
            var source = RealtimeCurveVM.Instance.Items.ToList();
            var width = RealtimeCurveCanvas.ActualWidth;
            var height = RealtimeCurveCanvas.ActualHeight;
            var drawingWidth = width - LEFT - RIGHT;
            var drawingHeight = height - TOP - BOTTOM;
            var hAxisWidth = _hAxisWidth;
            var hAxisCount = _hAxisCount;
            var intval = SettingVM.Instance.iRealtimeCurveInterval;
            var celling = SettingVM.Instance.iRealtimeCurveCelling;
            var floor = SettingVM.Instance.iRealtimeCurveFloor;

            var index = source.Count - (int)hAxisCount;
            if (index < 0)
            {
                index = 0;
            }

            var points = new PointCollection();

            if (source.Count > 0)
            {
                for (int i = index; i < source.Count; i++)
                {
                    var value = source[i].Value;
                    if (value > celling)
                    {
                        value = celling;
                    }
                    if (value < floor)
                    {
                        value = floor;
                    }
                    var x = LEFT + (i - index + 0.5d) * hAxisWidth;
                    var y = TOP + (drawingHeight - drawingHeight * (value - floor) / (celling - floor));
                    points.Add(new Point(x, y));
                }

                DepthPolyline.Dispatcher.Invoke(new Action(() =>
                {
                    DepthPolyline.Points = points;
                }));
            }
        }
    }
}
