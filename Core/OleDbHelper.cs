using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using DAP_Serial.Utilities;
using System.IO;
using System.Threading.Tasks;

namespace DAP_Serial.Core
{
   public static class OleDbHelper
    {
       private readonly static string _connectString =
           string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\DAP502_97.mdb", AppDomain.CurrentDomain.BaseDirectory);

       private static readonly ILogger _logger = new Logger(typeof(OleDbHelper).FullName);

       /// <summary>
       /// 新增一条记录
       /// </summary>
       /// <param name="address">采集器</param>
       /// <param name="depth">深度</param>
       public static void WriteToAccess(string address, string depth)
       {
           Task.Factory.StartNew(() =>
           {
               try
               {
                   var cmdText = "INSERT INTO LineData (模块地址, 采样时刻, 压力值) VALUES (@模块地址, now(), @压力值)";
                   using (var conn = new OleDbConnection(_connectString))
                   {
                       conn.Open();
                       using (OleDbCommand cmd = new OleDbCommand(cmdText, conn))
                       {
                           cmd.Parameters.Add(new OleDbParameter("@模块地址", address));
                           cmd.Parameters.Add(new OleDbParameter("@压力值", depth));
                           cmd.ExecuteNonQuery();
                       }
                   }
               }
               catch (OleDbException oleDbException)
               {
                   _logger.Error("[WritetoAccess] OleDbException : {0}", oleDbException.Message);
               }
               catch (Exception ex)
               {
                   _logger.Error("[WritetoAccess] Exception : {0}", ex.Message);
               }
           });
       }
    }
}
