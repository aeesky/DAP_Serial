using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DAP_Serial.Controls
{
    [TemplatePart(Name="SelectedRectangle",Type=typeof(Rectangle))]
    [TemplatePart(Name="TransparentButton", Type=typeof(Button))]
    [TemplatePart(Name="OthersButton", Type=typeof(Button))]
    public class ColorPicker : ComboBox
    {
        #region Static

        private static IEnumerable<CommonColor> _CommonColors;

        static ColorPicker()
        {
            _CommonColors = InitCommonColors();
        }

        private static IEnumerable<CommonColor> InitCommonColors()
        {
            yield return new CommonColor() { DisplayName = "黑色", Value = Brushes.Black };
            yield return new CommonColor() { DisplayName = "藏青", Value = Brushes.DarkBlue };
            yield return new CommonColor() { DisplayName = "墨绿", Value = Brushes.DarkGreen };
            yield return new CommonColor() { DisplayName = "生青", Value = Brushes.Teal };
            yield return new CommonColor() { DisplayName = "红褐", Value = Brushes.Maroon };
            yield return new CommonColor() { DisplayName = "紫红", Value = Brushes.Purple };
            yield return new CommonColor() { DisplayName = "褐绿", Value = Brushes.Olive };

            yield return new CommonColor() { DisplayName = "浅灰", Value = Brushes.Silver };
            yield return new CommonColor() { DisplayName = "灰色", Value = Brushes.Gray };
            yield return new CommonColor() { DisplayName = "蓝色", Value = Brushes.Blue };
            yield return new CommonColor() { DisplayName = "绿色", Value = Brushes.Green };
            yield return new CommonColor() { DisplayName = "艳青", Value = Brushes.Aqua };
            yield return new CommonColor() { DisplayName = "红色", Value = Brushes.Red };
            yield return new CommonColor() { DisplayName = "品红", Value = Brushes.Fuchsia };

            yield return new CommonColor() { DisplayName = "黄色", Value = Brushes.Yellow };
            yield return new CommonColor() { DisplayName = "白色", Value = Brushes.White };
            yield return new CommonColor() { DisplayName = "蓝灰", Value = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0xff)) };
            yield return new CommonColor() { DisplayName = "藏蓝", Value = new SolidColorBrush(Color.FromRgb(0x58, 0x30, 0xe0)) };
            yield return new CommonColor() { DisplayName = "嫩绿", Value = new SolidColorBrush(Color.FromRgb(0x80, 0xe0, 0x00)) };
            yield return new CommonColor() { DisplayName = "青绿", Value = new SolidColorBrush(Color.FromRgb(0x00, 0xe0, 0x80)) };
            yield return new CommonColor() { DisplayName = "黄褐", Value = new SolidColorBrush(Color.FromRgb(0xC0, 0x60, 0x00)) };

            yield return new CommonColor() { DisplayName = "粉红", Value = new SolidColorBrush(Color.FromRgb(0xFF, 0xA8, 0xFF)) };
            yield return new CommonColor() { DisplayName = "嫩黄", Value = new SolidColorBrush(Color.FromRgb(0xD8, 0xD8, 0x00)) };
            yield return new CommonColor() { DisplayName = "银白", Value = new SolidColorBrush(Color.FromRgb(0xec, 0xec, 0xec)) };
            yield return new CommonColor() { DisplayName = "紫色", Value = new SolidColorBrush(Color.FromRgb(0x90, 0x00, 0xff)) };
            yield return new CommonColor() { DisplayName = "天蓝", Value = new SolidColorBrush(Color.FromRgb(0x00, 0x88, 0xff)) };
            yield return new CommonColor() { DisplayName = "灰绿", Value = new SolidColorBrush(Color.FromRgb(0x80, 0xa0, 0x80)) };
            yield return new CommonColor() { DisplayName = "青蓝", Value = new SolidColorBrush(Color.FromRgb(0x00, 0x60, 0xc0)) };

            yield return new CommonColor() { DisplayName = "橙黄", Value = new SolidColorBrush(Color.FromRgb(0xFF, 0x80, 0x00)) };
            yield return new CommonColor() { DisplayName = "桃红", Value = new SolidColorBrush(Color.FromRgb(0xFF, 0x50, 0x80)) };
            yield return new CommonColor() { DisplayName = "芙红", Value = new SolidColorBrush(Color.FromRgb(0xFF, 0x80, 0xc0)) };
            yield return new CommonColor() { DisplayName = "深灰", Value = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60)) };
        }

        class CommonColor
        {
            public string DisplayName { get; set; }
            public SolidColorBrush Value { get; set; }
        }

        #endregion

        #region 变量

        private Rectangle _selectedRectangle;
        private Button _transparentButton;
        private Button _othersButton;

        #endregion

        #region 属性

        #region 选择属性

        public static readonly RoutedEvent SelectedBrushChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedBrushChanged",
                RoutingStrategy.Bubble,
                typeof(SelectionChangedEventHandler),
                typeof(ColorPicker));

        public event SelectionChangedEventHandler SelectedBrushChanged
        {
            add { AddHandler(SelectedBrushChangedEvent, value); }
            remove { RemoveHandler(SelectedBrushChangedEvent, value); }
        }

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(SolidColorBrush),
                typeof(ColorPicker),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedBrushPropertyChanged));

        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        private static void OnSelectedBrushPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = o as ColorPicker;
            if (null != colorPicker)
            {
                colorPicker.OnSelectedBrushChanged(new SelectionChangedEventArgs(SelectedBrushChangedEvent, new object[] { e.OldValue }, new object[] { e.NewValue }));
            }
        }

        protected virtual void OnSelectedBrushChanged(SelectionChangedEventArgs e)
        {
            if (null != _selectedRectangle && e.AddedItems.Count == 1)
            {
                _selectedRectangle.Fill = e.AddedItems[0] as SolidColorBrush;
            }

            RaiseEvent(e);
        }

        #endregion

        #endregion

        #region 构造函数

        public ColorPicker()
        {
            //base.DefaultStyleKey = typeof(ColorPicker);
            this.ItemsSource = _CommonColors;
        }

        #endregion

        #region 重载

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count == 1)
            {
                var comonColor = e.AddedItems[0] as CommonColor;
                if (null != comonColor)
                {
                    SelectedBrush = comonColor.Value;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            _selectedRectangle = base.GetTemplateChild("SelectedRectangle") as Rectangle;
            _transparentButton = base.GetTemplateChild("TransparentButton") as Button;
            _othersButton = base.GetTemplateChild("OthersButton") as Button;

            _selectedRectangle.Fill = SelectedBrush;
            _transparentButton.Click += TransparentButton_Click;
            _othersButton.Click += OthersButton_Click;
        }

        #endregion

        #region 私有方法

        private void TransparentButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedBrush = Brushes.Transparent;
            IsDropDownOpen = false;
            SelectedIndex = -1;
        }

        private void OthersButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = colorDialog.Color;
                var wpfColor = Color.FromArgb(color.A, color.R, color.G, color.B);
                SelectedBrush = new SolidColorBrush(wpfColor);

                var commonColor = _CommonColors.SingleOrDefault(c => c.Value.Equals(wpfColor));
                SelectedValue = commonColor;
            }
        }

        #endregion

    }
}
