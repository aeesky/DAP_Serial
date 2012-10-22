using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;

using DAP_Serial.Utilities;

namespace DAP_Serial.Core
{
    public class SettingVM : EntityObject
    {
        #region 变量

        static SettingVM _instance = new SettingVM();

        private ILogger _logger = LoggerFactory.GetLogger(typeof(SettingVM).FullName);

        private readonly string _configPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.xml");

        private XElement _xElement = null;

        private string _appTitle = Properties.Resources.AppTitle;

        private string _about = Properties.Resources.About;

        private string _portName = Properties.Resources.PortName;

        private readonly string[] _portNames = SerialPort.GetPortNames();

        private string _baudRate = Properties.Resources.BaudRate;

        private readonly string[] _baudRates = new[] { "1200", "2400", "4800", "9600", "19200", "38400" };

        private string _phone = Properties.Resources.Phone;

        private string _website = Properties.Resources.WebSite;

        private string _dataAcquisitionInterval = Properties.Resources.DataAcquisitionInterval; // 数据采样时间

        private int _idataAcquisitionInterval;

        private string _dataAcquisitionSaveInterval = Properties.Resources.DataAcquisitionInterval; // 数据保存周期

        private int _idataAcquisitionSaveInterval;

        private string _realtimeCurveTitle = Properties.Resources.RealtimeCurveTitle;

        private string _realtimeCurveInterval = Properties.Resources.RealtimeCurveInterval; // 数据采样时间

        private int _irealtimeCurveInterval;

        private string _realtimeCurveCelling = Properties.Resources.RealtimeCurveCelling; // 上限

        private double _irealtimeCurveCelling;

        private string _realtimeCurveFloor = Properties.Resources.RealtimeCurveFloor; // 下限

        private double _irealtimeCurveFloor;

        private SolidColorBrush _realtimeCurveColor = new SolidColorBrush(Converter.ToColor(Properties.Resources.RealtimeCurveColor));

        private readonly DelegateCommand _saveCommand;

        #endregion

        #region 属性

        public static SettingVM Instance { get { return _instance; } }

        public string AppTitle
        {
            get { return _appTitle; }
            set
            {
                if (_appTitle != value)
                {
                    _appTitle = value;
                    RaisePropertyChanged("AppTitle");
                    _saveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string About
        {
#if TEST
            get { return _about + "\n\n\n\t\t[测试版本 本版本采用虚拟数据(非真实串口数据)]"; }
#else
            get { return _about; }
#endif
            set
            {
                if (_about != value)
                {
                    _about = value;
                    RaisePropertyChanged("About");
                    _saveCommand.RaiseCanExecuteChanged();
                    //return;
                }
            }
        }

        public string PortName
        {
            get { return _portName; }
            set
            {
                if (_portName != value)
                {
                    _portName = value;
                    RaisePropertyChanged("PortName");
                    _saveCommand.RaiseCanExecuteChanged();
                    return;
                }
            }
        }

        public string[] PortNames { get { return _portNames; } }

        public string BaudRate
        {
            get { return _baudRate; }
            set
            {
                if (_baudRate != value
                    && value != null)
                {
                    var baudRate = Converter.ToInt(value);
                    if (baudRate > 0)
                    {
                        _baudRate = value;
                        RaisePropertyChanged("BaudRate");
                        _saveCommand.RaiseCanExecuteChanged();
                        return;
                    }
                }
            }
        }

        public int iBaudRate
        {
            get { return Converter.ToInt(_baudRate); }
        }

        public string[] BaudRates { get { return _baudRates; } }

        public string Phone
        {
            get { return _phone; }
            set
            {
                if (_phone != value
                    && value != null)
                {
                    _phone = value;
                    RaisePropertyChanged("Phone");
                    _saveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string WebSite
        {
            get { return _website; }
            set
            {
                if (_website != value
                    && value != null)
                {
                    _website = value;
                    RaisePropertyChanged("WebSite");
                    _saveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string DataAcquisitionInterval
        {
            get { return _dataAcquisitionInterval; }
            set
            {
                if (_dataAcquisitionInterval != value
                    && value != null)
                {
                    var dataAcquisitionInterval = Converter.ToInt(value);
                    if (dataAcquisitionInterval > 0)
                    {
                        _dataAcquisitionInterval = value;
                        _idataAcquisitionInterval = dataAcquisitionInterval;
                        RaisePropertyChanged("DataAcquisitionInterval");
                        RaisePropertyChanged("iDataAcquisitionInterval");
                        _saveCommand.RaiseCanExecuteChanged();
                        return;
                    }
                }
            }
        }

        public int iDataAcquisitionInterval
        {
            get { return _idataAcquisitionInterval < 1 ? 1 : _idataAcquisitionInterval; }
        }

        public string DataAcquisitionSaveInterval
        {
            get { return _dataAcquisitionSaveInterval; }
            set
            {
                if (_dataAcquisitionSaveInterval != value
                    && value != null)
                {
                    var dataAcquisitionSaveInterval = Converter.ToInt(value);
                    if (dataAcquisitionSaveInterval > 0)
                    {
                        _dataAcquisitionSaveInterval = value;
                        _idataAcquisitionSaveInterval = dataAcquisitionSaveInterval;
                        RaisePropertyChanged("DataAcquisitionSaveInterval");
                        RaisePropertyChanged("iDataAcquisitionSaveInterval");
                        _saveCommand.RaiseCanExecuteChanged();
                        return;
                    }
                }
            }
        }

        public int iDataAcquisitionSaveInterval
        {
            get { return _idataAcquisitionSaveInterval < 1 ? 3 : _idataAcquisitionSaveInterval; }
        }

        public string RealtimeCurveTitle
        {
            get { return _realtimeCurveTitle; }
            set
            {
                if (_realtimeCurveTitle != value)
                {
                    _realtimeCurveTitle = value;
                    RaisePropertyChanged("RealtimeCurveTitle");
                    _saveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string RealtimeCurveInterval
        {
            get { return _realtimeCurveInterval; }
            set
            {
                if (_realtimeCurveInterval != value
                    && value != null)
                {
                    var realtimeCurveInterval = Converter.ToInt(value);
                    if (realtimeCurveInterval > 0)
                    {
                        _realtimeCurveInterval = value;
                        _irealtimeCurveInterval = realtimeCurveInterval;
                        RaisePropertyChanged("RealtimeCurveInterval");
                        RaisePropertyChanged("iRealtimeCurveInterval");
                        _saveCommand.RaiseCanExecuteChanged();
                        return;
                    }
                }
            }
        }

        public int iRealtimeCurveInterval
        {
            get { return _irealtimeCurveInterval < 1 ? 1 : _irealtimeCurveInterval; }
        }

        public string RealtimeCurveCelling
        {
            get { return _realtimeCurveCelling; }
            set
            {
                if (_realtimeCurveCelling != value
                    && value != null)
                {
                    var realtimeCurveCelling = Converter.ToDouble(value);
                    if (realtimeCurveCelling > 0)
                    {
                        _realtimeCurveCelling = value;
                        _irealtimeCurveCelling = realtimeCurveCelling;
                        RaisePropertyChanged("RealtimeCurveCelling");
                        RaisePropertyChanged("iRealtimeCurveCelling");
                        _saveCommand.RaiseCanExecuteChanged();
                        return;
                    }
                }
            }
        }

        public double iRealtimeCurveCelling
        {
            get { return _irealtimeCurveCelling; }
        }

        public string RealtimeCurveFloor
        {
            get { return _realtimeCurveFloor; }
            set
            {
                if (_realtimeCurveFloor != value
                    && value != null)
                {
                    var realtimeCurveFloor = Converter.ToDouble(value);
                    if (realtimeCurveFloor > 0)
                    {
                        _realtimeCurveFloor = value;
                        _irealtimeCurveFloor = realtimeCurveFloor;
                        RaisePropertyChanged("RealtimeCurveFloor");
                        RaisePropertyChanged("iRealtimeCurveFloor");
                        _saveCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public double iRealtimeCurveFloor
        {
            get { return _irealtimeCurveFloor; }
        }

        public SolidColorBrush RealtimeCurveColor
        {
            get { return _realtimeCurveColor; }
            set
            {
                _realtimeCurveColor = value;
                RaisePropertyChanged("RealtimeCurveColor");
                _saveCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand SaveCommand { get { return _saveCommand; } }

        #endregion

        #region 构造方法

        private SettingVM()
        {
            _saveCommand = new DelegateCommand(Save, CanSave);

            ReadConfig();
        }

        private void ReadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    _xElement = XElement.Load(_configPath);
                    if (null != _xElement)
                    {
                        _appTitle = GetValue("//AppTitle", _appTitle);
                        _about = GetValue("//About", _about);
                        _portName = GetValue("//PortName", _portName);
                        _baudRate = GetValue("//BaudRate", _baudRate);
                        _phone = GetValue("//Phone", _phone);
                        _website = GetValue("//WebSite", _website);
                        _dataAcquisitionInterval = GetValue("//DataAcquisitionInterval", _dataAcquisitionInterval);
                        _dataAcquisitionSaveInterval = GetValue("//DataAcquisitionSaveInterval", _dataAcquisitionSaveInterval);
                        _realtimeCurveTitle = GetValue("//RealtimeCurveTitle", _realtimeCurveTitle);
                        _realtimeCurveColor = new SolidColorBrush(Converter.ToColor(GetValue("//RealtimeCurveColor", _realtimeCurveColor.ToString())));
                        _realtimeCurveInterval = GetValue("//RealtimeCurveInterval", _realtimeCurveInterval);
                        _realtimeCurveCelling = GetValue("//RealtimeCurveCelling", _realtimeCurveCelling);
                        _realtimeCurveFloor = GetValue("//RealtimeCurveFloor", _realtimeCurveFloor);
                    }
                }
                else
                {
                    _appTitle = Properties.Resources.AppTitle;
                    _about = Properties.Resources.About;
                    _portName = Properties.Resources.PortName;
                    _baudRate = Properties.Resources.BaudRate;
                    _phone = Properties.Resources.Phone;
                    _website = Properties.Resources.WebSite;
                    _dataAcquisitionInterval = Properties.Resources.DataAcquisitionInterval;
                    _dataAcquisitionSaveInterval = Properties.Resources.DataAcquisitionSaveInterval;
                    _realtimeCurveTitle = Properties.Resources.RealtimeCurveTitle;
                    _realtimeCurveColor = new SolidColorBrush(Converter.ToColor(Properties.Resources.RealtimeCurveColor));
                    _realtimeCurveInterval = Properties.Resources.RealtimeCurveInterval;
                    _realtimeCurveCelling = Properties.Resources.RealtimeCurveCelling;
                    _realtimeCurveFloor = Properties.Resources.RealtimeCurveFloor;

                    File.WriteAllText(_configPath, "<?xml version=\"1.0\" encoding=\"utf-8\" ?>");

                }

                _idataAcquisitionInterval = Converter.ToInt(_dataAcquisitionInterval);
                _idataAcquisitionSaveInterval = Converter.ToInt(_dataAcquisitionSaveInterval);
                _irealtimeCurveInterval = Converter.ToInt(_realtimeCurveInterval);
                _irealtimeCurveCelling = Converter.ToDouble(_realtimeCurveCelling);
                _irealtimeCurveFloor = Converter.ToDouble(_realtimeCurveFloor);

                RaisePropertyChanged("AppTitle");
                RaisePropertyChanged("About");
                RaisePropertyChanged("PortName");
                RaisePropertyChanged("BaudRate");
                RaisePropertyChanged("Phone");
                RaisePropertyChanged("WebSite");
                RaisePropertyChanged("DataAcquisitionInterval");
                RaisePropertyChanged("DataAcquisitionSaveInterval");
                RaisePropertyChanged("iDataAcquisitionInterval");
                RaisePropertyChanged("iDataAcquisitionSaveInterval");
                RaisePropertyChanged("RealtimeCurveTitle");
                RaisePropertyChanged("RealtimeCurveColor");
                RaisePropertyChanged("RealtimeCurveInterval");
                RaisePropertyChanged("RealtimeCurveCelling");
                RaisePropertyChanged("RealtimeCurveFloor");

                _saveCommand.RaiseCanExecuteChanged(); // 恢复按钮状态
            }
            catch (Exception ex)
            {
                _logger.Error("[ReadConfig]Exception : {0}", ex.Message);
            }
        }

        #endregion

        #region 私有方法

        private string GetValue(string expression, string defaultValue = null)
        {
            var value = _xElement.XPathSelectElement(expression);
            if (null != value)
            {
                return value.Value;
            }
            return defaultValue;
        }

        private void Save()
        {
            if (DoSave())
            {

                _saveCommand.RaiseCanExecuteChanged();  // 恢复按钮状态

                MainWindow.Instance.ShowMessage("设置保存成功！");
            }
        }

        public bool DoSave()
        {
            try
            {
                if (null == _xElement)
                {
                    _xElement = new XElement("Config");
                }

                _xElement.SetElementValue("AppTitle", _appTitle);
                _xElement.SetElementValue("About", _about);
                _xElement.SetElementValue("PortName", _portName);
                _xElement.SetElementValue("BaudRate", _baudRate);
                _xElement.SetElementValue("Phone", _phone);
                _xElement.SetElementValue("WebSite", _website);
                _xElement.SetElementValue("DataAcquisitionInterval", _dataAcquisitionInterval);
                _xElement.SetElementValue("DataAcquisitionSaveInterval", _dataAcquisitionSaveInterval);
                _xElement.SetElementValue("RealtimeCurveTitle", _realtimeCurveTitle);
                _xElement.SetElementValue("RealtimeCurveTitle", _realtimeCurveTitle);
                _xElement.SetElementValue("DataAcquisitionInterval", _dataAcquisitionInterval);
                _xElement.SetElementValue("DataAcquisitionSaveInterval", _dataAcquisitionSaveInterval);
                _xElement.SetElementValue("RealtimeCurveTitle", _realtimeCurveTitle);
                _xElement.SetElementValue("RealtimeCurveColor", _realtimeCurveColor);
                _xElement.SetElementValue("RealtimeCurveInterval", _realtimeCurveInterval);
                _xElement.SetElementValue("RealtimeCurveCelling", _realtimeCurveCelling);
                _xElement.SetElementValue("RealtimeCurveFloor", _realtimeCurveFloor);
                _xElement.Save(_configPath);
                return true;
            }
            catch (IOException ioException)
            {
                _logger.Error("[Save] IOException: {0}", ioException.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("[Save] Exception: {0}", ex.Message);
            }
            return false;
        }

        private bool CanSave()
        {
            string appTitle = string.Empty;
            string about = string.Empty;
            string portName = string.Empty;
            string baudRate = string.Empty;
            string phone = string.Empty;
            string website = string.Empty;
            string dataAcquisitionInterval = string.Empty;
            string dataAcquisitionSaveInterval = string.Empty;
            string realtimeCurveTitle = string.Empty;
            Brush realtimeCurveColor = default(Brush);
            string realtimeCurveInterval = string.Empty;
            string realtimeCurveCelling = string.Empty;
            string realtimeCurveFloor = string.Empty;
            if (File.Exists(_configPath))
            {
                _xElement = XElement.Load(_configPath);
                if (null != _xElement)
                {
                    appTitle = GetValue("//AppTitle", _appTitle);
                    about = GetValue("//About", _about);
                    portName = GetValue("//PortName", _portName);
                    baudRate = GetValue("//BaudRate", _baudRate);
                    phone = GetValue("//Phone", _phone);
                    website = GetValue("//WebSite", _website);
                    dataAcquisitionInterval = GetValue("//DataAcquisitionInterval", _dataAcquisitionInterval);
                    dataAcquisitionSaveInterval = GetValue("//DataAcquisitionSaveInterval", _dataAcquisitionSaveInterval);
                    realtimeCurveTitle = GetValue("//RealtimeCurveTitle", _realtimeCurveTitle);
                    realtimeCurveColor = new SolidColorBrush(Converter.ToColor(GetValue("//RealtimeCurveColor", _realtimeCurveColor.ToString())));
                    realtimeCurveInterval = GetValue("//RealtimeCurveInterval", _realtimeCurveInterval);
                    realtimeCurveCelling = GetValue("//RealtimeCurveCelling", _realtimeCurveCelling);
                    realtimeCurveFloor = GetValue("//RealtimeCurveFloor", _realtimeCurveFloor);
                }
            }
            else
            {
                appTitle = Properties.Resources.AppTitle;
                about = Properties.Resources.About;
                portName = Properties.Resources.PortName;
                baudRate = Properties.Resources.BaudRate;
                phone = Properties.Resources.Phone;
                website = Properties.Resources.WebSite;
                dataAcquisitionInterval = Properties.Resources.DataAcquisitionInterval;
                dataAcquisitionSaveInterval = Properties.Resources.DataAcquisitionSaveInterval;
                realtimeCurveTitle = Properties.Resources.RealtimeCurveTitle;
                realtimeCurveColor = new SolidColorBrush(Converter.ToColor(Properties.Resources.RealtimeCurveColor));
                realtimeCurveInterval = Properties.Resources.RealtimeCurveInterval;
                realtimeCurveCelling = Properties.Resources.RealtimeCurveCelling;
                realtimeCurveFloor = Properties.Resources.RealtimeCurveFloor;
            }

            return
                appTitle != _appTitle
                || about != _about
                || portName != _portName
                || baudRate != _baudRate
                || phone != _phone
                || website != _website
                || dataAcquisitionInterval != _dataAcquisitionInterval
                || dataAcquisitionSaveInterval != _dataAcquisitionSaveInterval
                || realtimeCurveColor.ToString() != _realtimeCurveColor.ToString()
                || realtimeCurveTitle != _realtimeCurveTitle
                || realtimeCurveInterval != _realtimeCurveInterval
                || realtimeCurveCelling != _realtimeCurveCelling
                || realtimeCurveFloor != _realtimeCurveFloor;
        }

        #endregion

        #region 公共方法

        public void Cancel()
        {
            ReadConfig();
        }

        #endregion
    }
}
