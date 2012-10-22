using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAP_Serial.Core
{
    /// <summary>
    /// 采集器模型
    /// </summary>
    public class DeviceModel : EntityObject
    {
        #region 变量

        private string _address; // 探测器

        private DateTime _time;   // 时间

        private string _depth; // 深度

        #endregion

        #region 属性

        /// <summary>
        /// 设备地址码(0-9a-zA-Z)
        /// </summary>
        public string Address { get { return _address; } set { _address = value; RaisePropertyChanged("Address"); } }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime Time { get { return _time; } set { _time = value; RaisePropertyChanged("Time"); } }

        /// <summary>
        /// 深度值
        /// </summary>
        public string Depth { get { return _depth; } set { _depth = value; RaisePropertyChanged("Depth"); } }

        #endregion

        #region 构造函数

        public DeviceModel()
        {

        }

        #endregion

        #region 重载方法

        public override bool Equals(object obj)
        {
            var device = obj as DeviceModel;
            if (null != obj &&
                null != device._depth)
            {
                return device._depth.Equals(_depth);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region 私有方法

        #endregion

        #region 公共方法

        #endregion
    }
}
