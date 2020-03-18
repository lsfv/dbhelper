using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace DBUtilityLB
{
    public class DB_MSSql:IDbBase
    {
        private string mConn;
        public DB_MSSql(string conn)
        {
            mConn = conn;
        }

        /// <summary>
        /// 建立SqlCommand对象
        /// </summary>
        /// <returns></returns>
        public DbCommand CreateCommand()
        {
            return new SqlCommand();
        }
        /// <summary>
        /// 建立SqlConnection对象
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection()
        {
            return new SqlConnection();
        }
        /// <summary>
        /// 创建SqlDataAdapter对象
        /// </summary>
        /// <returns></returns>
        public DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }
        /// <summary>
        /// 建立SqlParameter对象
        /// </summary>
        /// <returns></returns>
        public DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public String GetConnection()
        {
            return mConn;
        }

        public int GetCommandTimeOut()
        {
            return 30;//second
        }
    }
}