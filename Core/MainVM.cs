using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAP_Serial.Utilities;

namespace DAP_Serial.Core
{
    public class MainVM : EntityObject
    {
        #region 变量

        private static readonly MainVM _instance = new MainVM();

        private ILogger _logger = LoggerFactory.GetLogger(typeof(MainVM).FullName);

        private bool _isBusying;

        #endregion

        #region 属性

        public static MainVM Instance { get { return _instance; } }

        public SettingVM Setting { get { return SettingVM.Instance; } }

        public bool IsBusying
        {
            get { return _isBusying; }
            set
            {
                _isBusying = value;
                RaisePropertyChanged("IsBusying");
            }
        }

        #endregion

        #region 构造函数

        private MainVM()
        {
        }

        #endregion

        #region 私有方法

        #endregion

        #region 公共方法



        #endregion
    }
}
