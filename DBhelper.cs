using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

//  </system.web>
//  <connectionStrings>
//    <add name="dbstr" connectionString="Data Source=localhost;Initial Catalog=mycity;User ID=sa;Password=123"/>
//  </connectionStrings>
//</configuration>


namespace DBUtilityLB
{
    public abstract class DBhelper
    {
        //0具体的访问方法都是使用接口来执行。而这里是对接口实例化。
        //static public IDbBase MyDataBaseType = new DbBase_MSSql();//1.创建那个类型的数据库访问基础库。
        //static public string CnString = ConfigurationManager.ConnectionStrings["dbstr"].ConnectionString;//2。默认数据库联接。DBConfig.CnString
        //public static int Timeout = 30;

        public static IDbBase CreateMsSql(string connection)
        {
            return new DB_MSSql(connection);
        }

        public static String GetConnectionStr(string configName)
        {
            return ConfigurationManager.ConnectionStrings[configName].ConnectionString;
        }
    }
}