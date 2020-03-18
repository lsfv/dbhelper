using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace DBUtilityLB
{
    //文件关系
    //有数据工具的基本接口和各种数据库的基本实现.idbbase.  db_xxx
    //这个是实现之一,实现的是mssql ,并且进一步封装了常用的方法.
    public  class MSSqlHelper
    {
        public IDbBase MyDatabase = null;

        public MSSqlHelper(string connectionStr)
        {
            MyDatabase= DBhelper.CreateMsSql(DBhelper.GetConnectionStr(connectionStr));
        }


        public  int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            DbCommand cmd = MyDatabase.CreateCommand();
            using (DbConnection conn = MyDatabase.CreateConnection())
            {
                conn.ConnectionString = MyDatabase.GetConnection();
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        //执行语句，返回表格(查询)
        public  DataTable ExecuteTable(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            DbCommand cmd = MyDatabase.CreateCommand();

            using (DbConnection connection = MyDatabase.CreateConnection())
            {
                connection.ConnectionString = MyDatabase.GetConnection();
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                DbDataAdapter ap = MyDatabase.CreateDataAdapter();
                ap.SelectCommand = cmd;
                DataSet st = new DataSet();
                ap.Fill(st, "Result");
                cmd.Parameters.Clear();
                return st.Tables["Result"];
            }
        }

        //执行存储过程，返回表格(查询)
        public  DataTable ExecuteTable(string spname, params DbParameter[] commandParameters)
        {
            DbCommand cmd = MyDatabase.CreateCommand();

            using (DbConnection connection = MyDatabase.CreateConnection())
            {
                connection.ConnectionString = MyDatabase.GetConnection();
                PrepareCommand(cmd, connection, null, CommandType.StoredProcedure, spname, commandParameters);
                DbDataAdapter ap = MyDatabase.CreateDataAdapter();
                ap.SelectCommand = cmd;
                DataSet st = new DataSet();
                ap.Fill(st, "Result");
                cmd.Parameters.Clear();
                return st.Tables["Result"];
            }
        }


     
         public  object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] commandParameters)
        {
            DbCommand cmd = MyDatabase.CreateCommand();

            using (DbConnection connection = MyDatabase.CreateConnection())
            {
                connection.ConnectionString = MyDatabase.GetConnection();
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        //分页查询
        /// <summary>
        /// 执行有自定义排序的分页的查询
        /// </summary>
        /// <param name="connectionString">SQL数据库连接字符串</param>
        /// <param name="SqlAllFields">查询字段，如果是多表查询，请将必要的表名或别名加上，如:a.id,a.name,b.score</param>
        /// <param name="SqlTablesAndWhere">查询的表如果包含查询条件，也将条件带上，但不要包含order by子句，也不要包含"from"关键字，如:students a inner join achievement b on a.... where ....</param>
        /// <param name="IndexField">用以分页的不能重复的索引字段名，最好是主表的自增长字段，如果是多表查询，请带上表名或别名，如:a.id</param>
        /// <param name="OrderASC">排序方式,如果为true则按升序排序,false则按降序排</param>
        /// <param name="OrderFields">排序字段以及方式如：a.OrderID desc,CnName desc</OrderFields>
        /// <param name="PageIndex">当前页的页码</param>
        /// <param name="PageSize">每页记录数</param>
        /// <param name="RecordCount">输出参数，返回查询的总记录条数</param>
        /// <param name="PageCount">输出参数，返回查询的总页数</param>
        /// <returns>返回查询结果</returns>
        public DataTable ExecutePage(string SqlAllFields, string SqlTablesAndWhere, string IndexField, string OrderFields, int PageIndex, int PageSize, out int RecordCount, out int PageCount, params DbParameter[] commandParameters)
        {
            using (DbConnection connection = MyDatabase.CreateConnection())
            {
                connection.ConnectionString = MyDatabase.GetConnection();
                connection.Open();
                DbCommand cmd = MyDatabase.CreateCommand();
                PrepareCommand(cmd, connection, null, CommandType.Text, "", commandParameters);
                string Sql = GetPageSql(connection, cmd, SqlAllFields, SqlTablesAndWhere, IndexField, OrderFields, PageIndex, PageSize, out RecordCount, out PageCount);
                cmd.CommandText = Sql;
                DbDataAdapter ap = MyDatabase.CreateDataAdapter();
                ap.SelectCommand = cmd;
                DataSet st = new DataSet();
                ap.Fill(st, "PageResult");
                cmd.Parameters.Clear();
                return st.Tables["PageResult"];
            }
        }


        private  string GetPageSql(DbConnection connection, DbCommand cmd, string SqlAllFields, string SqlTablesAndWhere, string IndexField, string OrderFields, int PageIndex, int PageSize, out int RecordCount, out int PageCount)
        {
            RecordCount = 0;
            PageCount = 0;
            if (PageSize <= 0)
            {
                PageSize = 10;
            }
            string SqlCount = "select count(" + IndexField + ") from " + SqlTablesAndWhere;
            cmd.CommandText = SqlCount;
            RecordCount = (int)cmd.ExecuteScalar();
            if (RecordCount % PageSize == 0)
            {
                PageCount = RecordCount / PageSize;
            }
            else
            {
                PageCount = RecordCount / PageSize + 1;
            }
            if (PageIndex > PageCount)
                PageIndex = PageCount;
            if (PageIndex < 1)
                PageIndex = 1;
            string Sql = null;
            if (PageIndex == 1)
            {
                Sql = "select top " + PageSize + " " + SqlAllFields + " from " + SqlTablesAndWhere + " " + OrderFields;
            }
            else
            {
                Sql = "select top " + PageSize + " " + SqlAllFields + " from ";
                if (SqlTablesAndWhere.ToLower().IndexOf(" where ") > 0)
                {
                    string _where = Regex.Replace(SqlTablesAndWhere, @"\ where\ ", " where (", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    int i = Regex.Split(_where, @"\ where\ ", RegexOptions.IgnoreCase | RegexOptions.Compiled).Length - 1;
                    Sql += _where;
                    for (int j = 0; j < i; j++)
                    {
                        Sql += ")";
                    }
                    Sql += " and (";
                }
                else
                {
                    Sql += SqlTablesAndWhere + " where (";
                }
                Sql += IndexField + " not in (select top " + (PageIndex - 1) * PageSize + " " + IndexField + " from " + SqlTablesAndWhere + " " + OrderFields;
                Sql += ")) " + OrderFields;
            }
            return Sql;
        }


       
        private  void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType, string cmdText, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;
            cmd.CommandTimeout = MyDatabase.GetCommandTimeOut();
            if (cmdParms != null)
            {
                foreach (DbParameter parm in cmdParms)
                    if (parm != null)
                    {
                        if ((parm.Direction == ParameterDirection.InputOutput || parm.Direction == ParameterDirection.Input) &&(parm.Value == null))
                        {
                            parm.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(parm);
                    }
            }
        }
    }
}