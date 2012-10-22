using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using DAP_Serial.Utilities;
using System.Text.RegularExpressions;

namespace DAP_Serial.Core
{
    public class SerialPortFactory
    {
        #region 变量

        private static readonly SerialPortFactory _instance = new SerialPortFactory();

        private ILogger _logger = LoggerFactory.GetLogger(typeof(SerialPortFactory).FullName);

        private readonly SerialPort _serialPort = new SerialPort();

        private object _lockObject = new object();

        private readonly char[] _addressess = new char[62];

        const double G = 9.80665d;   // 重力加速度

        const int OUTTIME = 100; // 串口通讯超市时间（毫秒）
            
        private AutoResetEvent _receiveResetEvent = new AutoResetEvent(false);

        private string _result = string.Empty;

        private byte[] _buffer = new byte[1024];

        #endregion

        #region 属性

        public static SerialPortFactory Instance { get { return _instance; } }

        public char[] Addresses { get { return _addressess; } }

        /// <summary>
        /// 串口是否打开
        /// </summary>
        public bool IsOpen { get { return _serialPort.IsOpen; } }

        #endregion

        #region 构造函数

        private SerialPortFactory()
        {
            Init();
        }

        ~SerialPortFactory()
        {
            _serialPort.Dispose();
        }

        #endregion

        #region 私有方法

        private void Init()
        {
            _serialPort.PortName = SettingVM.Instance.PortName;
            _serialPort.BaudRate = SettingVM.Instance.iBaudRate;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.Odd;
            _serialPort.StopBits = StopBits.One;
            _serialPort.RtsEnable = true;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;

            InitAddresses();
        }

        private void InitAddresses()
        {
            var index = 0;
            for (int i = index; i < 10; i++)
            {
                _addressess[index + i] = (char)(i + 48);
            }
            index += 10;
            for (int i = 0; i < 26; i++)
            {
                _addressess[index + i] = (char)(i + 97);
            }
            index += 26;
            for (int i = 0; i < 26; i++)
            {
                _addressess[index + i] = (char)(i + 65);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _result = _serialPort.ReadExisting();
            //var size = _serialPort.Read(_buffer, 0, _buffer.Length);
            //_result = Encoding.ASCII.GetString(_buffer, 0, size);
            _receiveResetEvent.Set();
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
        }

        /// <summary>
        /// 自动超时
        /// </summary>
        private Timer AutoTimeout()
        {
            return new Timer(AutoTimeoutCallback, null, OUTTIME, OUTTIME);
        }

        private void AutoTimeoutCallback(object state)
        {
            _result = string.Empty;
            _receiveResetEvent.Set();
        }

        #endregion

        #region 公共方法

        #region 串口打开关闭操作

        public bool Open()
        {
            if (SettingVM.Instance.PortNames.Length == 0)
            {
                MessageBox.Show("在该计算机上没有检测到COM串口.\n请安装COM串口并重新启动应用程序.", "没有安装COM串口", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!SettingVM.Instance.PortNames.Contains(SettingVM.Instance.PortName))
            {
                MessageBox.Show(String.Format("在该计算机上没有检测到{0}串口.\n请安装COM串口并重新启动应用程序.", SettingVM.Instance.PortName), "没有安装COM串口", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                _serialPort.PortName = SettingVM.Instance.PortName;
                _serialPort.BaudRate = SettingVM.Instance.iBaudRate;
                _serialPort.Open();
                return true;
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                MessageBox.Show(unauthorizedAccessException.Message, "串口打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "串口打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void Close()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "串口关闭失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 发送命令并返回结果

        public string ExecuteQuery(string command)
        {
            if (!SerialPortFactory.Instance.IsOpen)
            {
                if (!SerialPortFactory.Instance.Open())
                {
                    return string.Empty;
                }
            }

            lock (_lockObject)
            {
                var timer = AutoTimeout();
                _serialPort.WriteLine(command);
                _receiveResetEvent.WaitOne();
                timer.Dispose();
                return _result;
            }
        }

        public string ExecuteQuery(string command, object arg0)
        {
            return ExecuteQuery(string.Format(command, arg0));
        }

        public string ExecuteQuery(string command, params object[] args)
        {
            return ExecuteQuery(string.Format(command, args));
        }

        /// <summary>
        /// 扫描地址码
        /// </summary>
        /// <returns></returns>
        public string ScanAddress(string address)
        {
            var reply = ExecuteQuery("#{0}A?;", address);
            return AnalyzeAddress(reply);
        }

        /// <summary>
        /// 扫描万能地址码
        /// </summary>
        /// <returns></returns>
        public string ScanUniversalAddress()
        {
            var reply = ExecuteQuery("#%A?;");
            return AnalyzeAddress(reply);
        }


        /// <summary>
        /// 读取压力值
        /// </summary>
        /// <returns></returns>
        public string ReadPressure(string address)
        {
            var reply = ExecuteQuery("#{0}OP;", address);
            return AnalyzeAddress(reply);
        }

        /// <summary>
        /// 读取深度值
        /// </summary>
        /// <returns></returns>
        public double ReadDepth(string address)
        {
            var data = ReadPressure(address);
            var pressure = Converter.ToDouble(data);
            var h = pressure / (1.0d * G);
            return h;
        }

        #endregion

        #region 命令结果解析

        /// <summary>
        /// 根据正则表示解析结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public string Analyze(string reply, string pattern)
        {
            var match = Regex.Match(reply, pattern, RegexOptions.IgnorePatternWhitespace);
            if (match.Success)
            {
                return match.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 解析读取读取地址命令结果
        /// </summary>
        /// <param name="reply">命令返回结果</param>
        /// <returns>地址码</returns>
        public string AnalyzeAddress(string reply)
        {
            return Analyze(reply, "[A-Za-z0-9]");
        }

        /// <summary>
        /// 解析读取读压力值命令结果
        /// </summary>
        /// <param name="reply">命令返回结果</param>
        /// <returns>压力值</returns>
        public string AnalyzePressure(string reply)
        {
            return Analyze(reply, @"[-]?(([\d]+\.[\d]+)|([\d]+))");
        }

        #endregion

        #endregion
    }
}
