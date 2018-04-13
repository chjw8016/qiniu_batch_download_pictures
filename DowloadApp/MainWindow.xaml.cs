using Qiniu.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Threading;
using System.IO;

namespace DowloadApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BindFarm();
            BindYear();
        }

        public void BindFarm()
        {
            cbb_farm.ItemsSource = DB.GetFarmData();
            cbb_farm.DisplayMemberPath = "Name";
            cbb_farm.SelectedValuePath = "Id";
            cbb_farm.SelectedIndex = 0;
        }
        public void BindCase(int planId)
        {
            cbb_case.ItemsSource = DB.GetCaseData(planId, Convert.ToString(cbb_year.Text));
            cbb_case.DisplayMemberPath = "Name";
            cbb_case.SelectedValuePath = "Id";
            cbb_case.SelectedIndex = 0;
        }
        public void BindPlan(int farmId)
        {
            cbb_plan.ItemsSource = DB.GetPlanData(farmId);
            cbb_plan.DisplayMemberPath = "Name";
            cbb_plan.SelectedValuePath = "Id";
            cbb_plan.SelectedIndex = 0;
        }
        public void BindYear()
        {
            cbb_year.ItemsSource = DB.GetYearData();
            cbb_year.DisplayMemberPath = "Name";
            cbb_year.SelectedValuePath = "Id";
            cbb_year.SelectedIndex = 0;
        }
        private void CbbFarm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindPlan(Convert.ToInt32(cbb_farm.SelectedValue));
        }
        private void CbbPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindCase(Convert.ToInt32(cbb_plan.SelectedValue));
        }

        private void CbbCase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void btn_download_Click(object sender, RoutedEventArgs e)
        {
            DataTable list = DB.getPics(Convert.ToInt32(cbb_farm.SelectedValue), Convert.ToInt32(cbb_plan.SelectedValue), Convert.ToInt32(cbb_case.SelectedValue), cbb_year.Text);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String url = config.AppSettings.Settings["qiniu_url"].Value;

            IList<ThreadData> arr = new List<ThreadData>();
            for (int i = 0; i < list.Rows.Count; i++)
            {
                JArray array = (JArray)JsonConvert.DeserializeObject(Convert.ToString(list.Rows[i]["pic"]));
                foreach (var jObject in array)
                {
                    String fileName = cbb_year.Text + "_" + list.Rows[i]["land"] + "_" + list.Rows[i]["variety"] + "_" + list.Rows[i]["sub_species"] + "_" + list.Rows[i]["batch_date"] + "=" + list.Rows[i]["create_time"] + "." + jObject["ext"];
                    //DownloadManager.Download(url + Convert.ToString(jObject["filename"]), fileName);
                    url += Convert.ToString(jObject["filename"]);
                    arr.Add(new ThreadData() { Url = url, FileName = fileName });
                    //ConsoleBox.AppendText("开始下载：" + fileName + "\n");
                }
            }
            ConsoleBox.AppendText("[ " + DateTime.Now.ToString() + " ] 开始下载，总计" + arr.Count + "个文件\n");
            ThreadPool.QueueUserWorkItem(DownloadPic, arr);
            //ConsoleBox.AppendText("下载完成：" + DateTime.Now.ToLocalTime().ToString() + "\n");
        }
        public void DownloadPic(object obj)
        {
            List<ThreadData> list = (List<ThreadData>)obj;
            foreach (ThreadData item in list)
            {
                String path = Environment.CurrentDirectory + "\\" + item.FileName.Replace("_", "\\").Split('=')[0];
                //ConsoleBox.Dispatcher.Invoke(new Action(() => { ConsoleBox.AppendText("下载：" + item.FileName); }));
                if (File.Exists(path + "\\" + item.FileName) == false)
                {
                    DownloadManager.Download(item.Url, item.FileName);
                    ConsoleBox.Dispatcher.Invoke(new Action(() =>
                    {
                        ConsoleBox.AppendText("[ " + DateTime.Now.ToString() + " ] " + item.FileName + "，下载完成！\n");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        File.Move(item.FileName, path + "\\" + item.FileName);
                    }));
                }
                else
                {
                    ConsoleBox.Dispatcher.Invoke(new Action(() =>
                    {
                        ConsoleBox.AppendText("[ " + DateTime.Now.ToString() + " ] " + item.FileName + "，已下载完成！\n");
                    }));
                }

            }
        }
    }

}
