using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace DowloadApp
{
    class DB
    {
        /// <summary>
        /// 获取农场信息
        /// </summary>
        /// <returns>List</returns>
        static public IList<DataItem> GetFarmData()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MySqlConnection myConnnect = new MySqlConnection(config.AppSettings.Settings["connectionString"].Value);
            myConnnect.Open();
            String sql = "select id,name from farm_info";
            IList<DataItem> list = new List<DataItem>();
            using (MySqlCommand myCmd = new MySqlCommand(sql, myConnnect))
            {
                MySqlDataReader dr = myCmd.ExecuteReader();
                while (dr.Read())
                {
                    if (dr.HasRows)
                    {
                        list.Add(new DataItem() { Id = Convert.ToInt32(dr[0]), Name = Convert.ToString(dr[1]) });
                    }
                }
                dr.Close();
            }
            myConnnect.Close();
            return list;
        }
        /// <summary>
        /// 获取地块信息
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns>List</returns>
        static public IList<DataItem> GetPlanData(Int32 farmId)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MySqlConnection myConnnect = new MySqlConnection(config.AppSettings.Settings["connectionString"].Value);
            myConnnect.Open();
            String sql = "select id,code from farm_plan";
            if (farmId > 0)
            {
                sql += " where farm_id=" + farmId;
            }
            IList<DataItem> list = new List<DataItem>();
            using (MySqlCommand myCmd = new MySqlCommand(sql, myConnnect))
            {
                MySqlDataReader dr = myCmd.ExecuteReader();
                list.Add(new DataItem() { Id = 0, Name = "全部地块" });
                while (dr.Read())
                {
                    if (dr.HasRows)
                    {
                        list.Add(new DataItem() { Id = Convert.ToInt32(dr[0]), Name = Convert.ToString(dr[1]) });
                    }
                }
                dr.Close();
            }
            myConnnect.Close();
            return list;
        }
        /// <summary>
        /// 获取作物信息
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="year"></param>
        /// <returns>List</returns>
        static public IList<DataItem> GetCaseData(int planId, String year)
        {
            if (year == "")
            {
                year = Convert.ToString(DateTime.Now.Year);
            }
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long end_year = (long)(Convert.ToDateTime(year + "-01-01 00:00:00").AddYears(1) - startTime).TotalSeconds;
            long start_year = (long)(Convert.ToDateTime(year + "-01-01 00:00:00") - startTime).TotalSeconds;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MySqlConnection myConnnect = new MySqlConnection(config.AppSettings.Settings["connectionString"].Value);
            myConnnect.Open();
            String sql = "select id,variety,sub_species,batch_date from farm_plan_case where create_time>=" + start_year + " and create_time<" + end_year;
            if (planId > 0)
            {
                sql += " and farm_plan_id=" + planId;
            }
            IList<DataItem> list = new List<DataItem>();
            using (MySqlCommand myCmd = new MySqlCommand(sql, myConnnect))
            {
                MySqlDataReader dr = myCmd.ExecuteReader();
                list.Add(new DataItem() { Id = 0, Name = "全部作物" });
                while (dr.Read())
                {
                    if (dr.HasRows)
                    {
                        list.Add(new DataItem() { Id = Convert.ToInt32(dr[0]), Name = Convert.ToString(dr[1]) + "/" + Convert.ToString(dr[2]) + "/" + Convert.ToString(dr[3]) });
                    }
                }
                dr.Close();
            }
            myConnnect.Close();
            return list;
        }
        /// <summary>
        /// 获取年份
        /// </summary>
        /// <returns>List</returns>
        static public IList<DataItem> GetYearData()
        {
            IList<DataItem> list = new List<DataItem>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new DataItem() { Id = 0, Name = Convert.ToString(DateTime.Now.AddYears(0 - i).Year) });
            }
            return list;
        }
        /// <summary>
        /// 获取农场图片
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns>DataTable</returns>
        static public DataTable getPics(int farmId, int planId, int caseId, String year)
        {
            if (year == "")
            {
                year = Convert.ToString(DateTime.Now.Year);
            }
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long end_year = (long)(Convert.ToDateTime(year + "-01-01 00:00:00").AddYears(1) - startTime).TotalSeconds;
            long start_year = (long)(Convert.ToDateTime(year + "-01-01 00:00:00") - startTime).TotalSeconds;
            DataTable dt = new DataTable();
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MySqlConnection myConnnect = new MySqlConnection(config.AppSettings.Settings["connectionString"].Value);
            myConnnect.Open();

            String sql = "select c.create_time,f.code as land, c.id,c.pic,c.variety,c.sub_species,c.batch_date from case_state_pests c left join farm_plan f on c.farm_plan_id =f.id where c.farm_id=" + farmId + " and c.create_time>=" + start_year + " and c.create_time<" + end_year;
            if (planId > 0)
            {
                sql += " and c.farm_plan_id" + planId;
            }
            if (caseId > 0)
            {
                sql += " and c.case_id" + caseId;
            }
            using (MySqlCommand myCmd = new MySqlCommand(sql, myConnnect))
            {
                MySqlDataReader dr = myCmd.ExecuteReader();
                dt.Load(dr);
                dr.Close();
            }
            myConnnect.Close();
            return dt;
        }

    }

    class DataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class ThreadData
    {
        public string Url { get; set; }
        public string FileName { get; set; }
    }
}
