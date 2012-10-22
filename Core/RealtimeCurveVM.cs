using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DAP_Serial.Utilities;

namespace DAP_Serial.Core
{
    public class RealtimeCurveVM : EntityObject
    {
        #region 变量

        private static readonly RealtimeCurveVM _instance = new RealtimeCurveVM();

        private ILogger _logger = LoggerFactory.GetLogger(typeof(RealtimeCurveVM).FullName);

        private readonly Timer _timer;

        private readonly DelegateCommand _scanCommand;

        private readonly DelegateCommand _startCommand;

        private readonly DelegateCommand _stopCommand;

        private readonly ObservableCollection<Data> _items = new ObservableCollection<Data>();

#if TEST
        private string _device = "1";
#else
        private string _device = string.Empty;
#endif

        private bool _isRunning;

        private bool _isScaning;

        private int _count = 0; // 保存周期计数器

#if TEST
        private int _secend = 0;

        private int t = 0;
#endif
        #endregion

        #region 属性

        public static RealtimeCurveVM Instance { get { return _instance; } }

        public SettingVM Setting { get { return SettingVM.Instance; } }

        public ObservableCollection<Data> Items { get { return _items; } }

        /// <summary>
        /// 扫描
        /// </summary>
        public DelegateCommand ScanCommand { get { return _scanCommand; } }

        /// <summary>
        /// 开始采集
        /// </summary>
        public DelegateCommand StartCommand { get { return _startCommand; } }

        /// <summary>
        /// 停止采集
        /// </summary>
        public DelegateCommand StopCommand { get { return _stopCommand; } }

        /// <summary>
        /// 搜索到的设备地址码
        /// </summary>
        public string Device
        {
            get { return _device; }
            set
            {
                if (!_device.Equals(value))
                {
                    _device = value;
                    RaisePropertyChanged("Device");

                    // 如果没有扫描到设备，则停止采集
                    if (string.IsNullOrEmpty(value))
                    {
                        if (_isRunning)
                        {
                            Stop();
                            return;
                        }
                    }

                    // 通知按钮重新设置按钮状态
                    _startCommand.RaiseCanExecuteChanged();
                    _stopCommand.RaiseCanExecuteChanged();
                }
            }
        }
        /// <summary>
        /// 是否在采集
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                RaisePropertyChanged("IsRunning");
                _startCommand.RaiseCanExecuteChanged();
                _stopCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 是否在扫描
        /// </summary>
        public bool IsScaning
        {
            get { return _isScaning; }
            set
            {
                _isScaning = value;
                RaisePropertyChanged("IsScaning");
                _scanCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region 构造函数

        private RealtimeCurveVM()
        {
            _scanCommand = new DelegateCommand(Scan);
            _startCommand = new DelegateCommand(Start, CanStart);
            _stopCommand = new DelegateCommand(Stop, CanStop);

            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            Setting.PropertyChanged += Setting_PropertyChanged;
        }

        public class Data
        {
            public int Timeline { get; set; }
            public double Value { get; set; }

            public Data() { }

            public Data(int timeLine, double value)
            {
                this.Timeline = timeLine;
                this.Value = value;
            }
        }

        #endregion

#if TEST
        Random _random = new Random();
#endif
        #region 私有方法

        private void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "iRealtimeCurveInterval"
                && _isRunning)
            {
                _timer.Change(0, Setting.iRealtimeCurveInterval * 1000);
            }
        }

        private void TimerCallback(object state)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // 删除100个
                if (_items.Count > 100)
                {
                    _items.RemoveAt(0);
                }

#if TEST
                Interlocked.Exchange(ref _secend, _secend + Setting.iRealtimeCurveInterval);
                var value = _random.NextDouble() * 50d;
                _items.Add(new Data(_secend, value));
#else
                var value = SerialPortFactory.Instance.ReadDepth(_device);
                _items.Add(new Data(0, value));

                Interlocked.Increment(ref _count);

                if (_count % SettingVM.Instance.iDataAcquisitionSaveInterval == 0) // 保存周期
                {
                    OleDbHelper.WriteToAccess(_device, value.ToString());

                    Interlocked.Exchange(ref _count, 0);    // 计数器清零 防止溢出
                }
#endif
                RaisePropertyChanged("Update"); // 实时绘制图形
            }));
        }

        private void Scan()
        {
#if !TEST
            IsScaning = true;
            MainVM.Instance.IsBusying = true;
            Task.Factory.StartNew(() =>
            {
                var divice = SerialPortFactory.Instance.ScanUniversalAddress();

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Device = divice;
                    MainVM.Instance.IsBusying = false;
                    IsScaning = false;
                }));
            });
#endif
        }

        private bool CanScan()
        {
            return !_isScaning;
        }

        private void Start()
        {
            if (CanStart())
            {
#if TEST
                Interlocked.Exchange(ref _secend, 0);
                Interlocked.Exchange(ref t, 0);
#endif
                _items.Clear();
                _timer.Change(0, Setting.iRealtimeCurveInterval * 1000);
                IsRunning = true;
            }
            else
            {
                IsRunning = false;
            }
        }

        private bool CanStart()
        {
            return !string.IsNullOrEmpty(_device) && !_isRunning;
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
        }

        private bool CanStop()
        {
            return _isRunning;
        }

        #endregion

        #region 公共方法

        #endregion
    }
}
