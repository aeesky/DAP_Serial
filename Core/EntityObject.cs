using System;
using System.ComponentModel;

namespace DAP_Serial.Core
{
    public abstract class EntityObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 实现属性更改通知
        /// </summary>
        /// <param name="info"></param>
        protected void RaisePropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
