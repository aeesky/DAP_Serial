using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using DAP_Serial.Utilities;

namespace DAP_Serial.Core
{
    /// <summary>
    /// 深度探测 ViewModel
    /// </summary>
    public class DataAcquisitionVM : EntityObject
    {
        #region 变量

        private static readonly DataAcquisitionVM _instance = new DataAcquisitionVM();

        private ILogger _logger = LoggerFactory.GetLogger(typeof(DataAcquisitionVM).FullName);

        private ObservableCollection<DeviceModel> _items = new ObservableCollection<DeviceModel>();

        private readonly DelegateCommand _connectCommand;

        private readonly DelegateCommand _disconnectCommand;

        private readonly DelegateCommand _startCommand;

        private readonly DelegateCommand _stopCommand;

        private bool? _isConnected = false;

        private bool _isRunning;

        private readonly Timer _timer;

        private int _count = 0; // 保存周期计数器

        #endregion

        #region 属性

        public static DataAcquisitionVM Instance { get { return _instance; } }

        public ObservableCollection<DeviceModel> Items { get { return _items; } }

        public DelegateCommand ConnectCommand { get { return _connectCommand; } }

        public DelegateCommand DisconnectCommand { get { return _disconnectCommand; } }

        public DelegateCommand StartCommand { get { return _startCommand; } }

        public DelegateCommand StopCommand { get { return _stopCommand; } }

        public bool? IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged("IsConnected");
                _connectCommand.RaiseCanExecuteChanged();
                _disconnectCommand.RaiseCanExecuteChanged();
                _startCommand.RaiseCanExecuteChanged();
                _stopCommand.RaiseCanExecuteChanged();
            }
        }

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

        #endregion

        #region 构造函数

        private DataAcquisitionVM()
        {
            _connectCommand = new DelegateCommand(Connect, CanConnect);
            _disconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);
            _startCommand = new DelegateCommand(Start, CanStart);
            _stopCommand = new DelegateCommand(Stop, CanStop);

            _timer = new Timer(TimerCallback, null ,Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region 私有方法

        private void TimerCallback(object state)
        {
            DoRead();
        }
#if TEST
        Random _random = new Random();
#endif
        /// <summary>
        /// 读取串口
        /// </summary>
        private void DoRead()
        {
            var items = _items.ToList();
            Task.Factory.StartNew(() => {
                items.ForEach((device) => {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
#if TEST
                        device.Depth = (_random.NextDouble() * 50).ToString();
#else
                        device.Depth = SerialPortFactory.Instance.ReadDepth(device.Address).ToString();

                        Interlocked.Increment(ref _count);

                        if (_count % SettingVM.Instance.iDataAcquisitionSaveInterval == 0) // 保存周期
                        {
                            OleDbHelper.WriteToAccess(device.Address, device.Depth);

                            Interlocked.Exchange(ref _count, 0);    // 计数器清零 防止溢出
                        }
#endif
                    }));
                });
            });
        }

        #region 操作方法

        private void Connect()
        {
            IsConnected = null;
            MainVM.Instance.IsBusying = true;
#if TEST
            _items.Clear();
            ScanAddress();
#else
            if (SerialPortFactory.Instance.Open())
            {
                _items.Clear();
                ScanAddress();
            }
            else
            {
                IsConnected = false;
                MainVM.Instance.IsBusying = false;
            }
#endif
        }

        private bool CanConnect()
        {
            return _isConnected == false;
        }

        public void Disconnect()
        {
            Stop();
            SerialPortFactory.Instance.Close();
            IsConnected = false;
        }

        private bool CanDisconnect()
        {
            return _isConnected == true;
        }

        private void Start()
        {
            _timer.Change(0, 2 * 1000);
            IsRunning = true;
        }

        private bool CanStart()
        {
            return _isConnected == true && !_isRunning;
        }

        private void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            IsRunning = false;
        }

        private bool CanStop()
        {
            return _isConnected == true &&  _isRunning;
        }

        #endregion

        #region 枚举地址码

        /// <summary>
        /// 扫描地址(0-9a-zA-Z)
        /// </summary>
        private void ScanAddress()
        {
            Task.Factory.StartNew(() =>
            {
#if TEST
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    _items.Add(new DeviceModel() { Address = "1", });
                    _items.Add(new DeviceModel() { Address = "2", });
                    _items.Add(new DeviceModel() { Address = "3", });
                }));
#else
                // 一次扫描地址
                foreach (var address in SerialPortFactory.Instance.Addresses)
                {
                    var data = SerialPortFactory.Instance.ScanAddress(address.ToString());
                    if (!string.IsNullOrEmpty(data))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            _items.Add(new DeviceModel() { Address = data, });
                        }));
                    }
                }
#endif
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    IsConnected = true;
                    MainVM.Instance.IsBusying = false;
                    if (_items.Count > 0)
                    {
                        MainWindow.Instance.ShowMessage(string.Format("扫描探测器设备完成，已发现{0}个设备", _items.Count));
                    }
                    else
                    {
                        MainWindow.Instance.ShowMessage("扫描探测器设备完成，没有发现任何设备");
                    }
                }));
            });
        }

        #endregion

        #endregion

        #region 公共方法

        #endregion
    }
}
