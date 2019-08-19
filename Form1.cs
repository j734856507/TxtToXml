using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Permissions;



namespace ToXml
{
    public partial class Form1 : Form
    {
        //文件监视注册
        static FileSystemWatcher watcher = new FileSystemWatcher();

        //申明INI文件的写操作函数WritePrivateProfileString()
        [DllImport("kernel32")]
        //section=配置节，key=键名，value=键值，path=路径
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //申明INI文件的读操作函数GetPrivateProfileString()
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        #region INI 文件读写
        public bool WriteConfigIni(string keyvalue, string variablename, string blockname, string configini)
        {
            bool result = false;
            //写入INI 文件 
            WritePrivateProfileString(blockname, variablename, keyvalue, configini); //
            result = true;
            return result;
        }

        //加密
        public static string MD5Create(string STR) //STR为待加密的string
        {
            string pwd = "";
            //pwd为加密结果
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(STR));
            //这里的UTF8是编码方式，你可以采用你喜欢的方式进行，比如UNcode等等
            for (int i = 0; i < s.Length; i++)
            {
                pwd = pwd + s[i].ToString();
            }
            return pwd;
        }

        //读INI文件
        public static string ReadConfigINI(string blockname, string variablename, string configini)
        {
            string inivalue = "";


            //读INI 文件
            StringBuilder temp1 = new StringBuilder(255);
            int x = Form1.GetPrivateProfileString(blockname, variablename, "", temp1, 255, configini);
            inivalue = temp1.ToString();
            return inivalue;
        }

        //写入文本文件
        public static void WriteFile(String path, String info)
        {
            FileInfo finfo = new FileInfo(path);

            /**/
            ///判断文件是否存在以及是否大于10M
            if (finfo.Exists && finfo.Length > 10 * 1024 * 1024)
            {
                /**/
                ///删除该文件
                finfo.Delete();
            }
            try
            {
                using (FileStream fs = finfo.OpenWrite())
                {
                    StreamWriter w = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    w.BaseStream.Seek(0, SeekOrigin.End);
                    w.WriteLine(info);
                    w.Flush();
                    w.Close();
                }
            }
            catch
            {

            }
        }
        #endregion INI

        #region 在winform中查找控件 -------------------------------------------------------------
        /// <summary>
        /// 在winform中查找控件
        /// </summary>
        /// <param name="control"></param>
        /// <param name="controlName"></param>
        /// <returns></returns>
        private System.Windows.Forms.Control findControl(System.Windows.Forms.Control control, string controlName)
        {
            Control c1;
            foreach (Control c in control.Controls)
            {
                if (c.Name == controlName)
                {
                    return c;
                }
                else if (c.Controls.Count > 0)
                {
                    c1 = findControl(c, controlName);
                    if (c1 != null)
                    {
                        return c1;
                    }
                }
            }
            return null;
        }
        #endregion

        //锁
        public void LockMac()
        {
            //锁
            string[] str = GetMoc();
            string s = Application.StartupPath + "\\config.ini";
            if (System.IO.File.Exists(s))
            {
            }
            else
            {
                MessageBox.Show("程序重要文件不存在，请联系程序开发人员！");
                System.Environment.Exit(0);
            }

            if (MD5Create(str[0]) == ReadConfigINI("Block1", "up", s))  //
            {
            }
            else
            {
                System.Environment.Exit(0);
            }
            //VolumeSerialNumber
            if (MD5Create(str[1]) == ReadConfigINI("Block5", "up", s))   //
            {
            }
            else
            {
                //Application.Exit();
                System.Environment.Exit(0);
            }
            //MacAddress

            if (MD5Create(str[2]) == ReadConfigINI("Block8", "up", s))
            {
            }
            else
            {
                //Application.Exit();
                System.Environment.Exit(0);
            }


        }
        #region LockMac
        private string[] GetMoc()
        {
            string[] str = new string[3];
            ManagementClass mcCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection mocCpu = mcCpu.GetInstances();
            foreach (ManagementObject m in mocCpu)
            {
                str[0] = m["ProcessorId"].ToString();
            }

            ManagementClass mcHD = new ManagementClass("win32_logicaldisk");
            ManagementObjectCollection mocHD = mcHD.GetInstances();
            foreach (ManagementObject m in mocHD)
            {
                if (m["DeviceID"].ToString() == "C:")
                {
                    str[1] = m["VolumeSerialNumber"].ToString();
                    break;
                }
            }

            ManagementClass mcMAC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection mocMAC = mcMAC.GetInstances();
            foreach (ManagementObject m in mocMAC)
            {
                if ((bool)m["IPEnabled"])
                {
                    str[2] = m["MacAddress"].ToString();
                    break;
                }
            }

            return str;
        }


        #endregion
        bool isAuto = true;
        public Form1()
        {
            LockMac();
            InitializeComponent();
            AutoStart(isAuto);
            CheckChanged();
        }
        /// <summary>  
        /// 修改程序在注册表中的键值,实现开机自启动
        /// </summary>  
        /// <param name="isAuto">true:开机启动,false:不开机自启</param> 
        public void AutoStart(bool isAuto)
        {
            try
            {
                if (isAuto == true)
                {
                    RegistryKey R_local = Registry.LocalMachine;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.SetValue("ToXml", Application.ExecutablePath);
                    R_run.Close();
                    R_local.Close();
                }
                else
                {
                    RegistryKey R_local = Registry.LocalMachine;//RegistryKey R_local = Registry.CurrentUser;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                    R_run.DeleteValue("应用名称", false);
                    R_run.Close();
                    R_local.Close();
                }

                //GlobalVariant.Instance.UserConfig.AutoStart = isAuto;
            }
            catch (Exception)
            {
                //MessageBoxDlg dlg = new MessageBoxDlg();
                //dlg.InitialData("您需要管理员权限修改", "提示", MessageBoxButtons.OK, MessageBoxDlgIcon.Error);
                //dlg.ShowDialog();
                MessageBox.Show("您需要管理员权限修改", "提示");
            }
        }
        string Runlogpath = Application.StartupPath + "/Runlog.dat";
        string Errlogpath = Application.StartupPath + "/Errlog.dat";

        #region readINI
        public string sini, xmlpath, partlabel;
        int xml3result;
        int xml4result;
        int xml5result;
        int xml7result;

    
        private void Form1_Load(object sender, EventArgs e)
        {
            sini = Application.StartupPath + "//A.ini";
            tb_xml11.Text = ReadConfigINI("XML1", "root_version", sini);
            tb_xml12.Text = ReadConfigINI("XML1", "root_xmlns", sini);

            partlabel = ReadConfigINI("XML2", "part_label", sini);
            tb_xml21.Text = partlabel;

            tb_xml22.Text = ReadConfigINI("XML2", "serial_number", sini);
            tb_xml23.Text = ReadConfigINI("XML2", "model_number", sini);

            tb_xml31.Text = ReadConfigINI("XML3", "section_label", sini);
            tb_xml32.Text = ReadConfigINI("XML3", "description", sini);
            xml3result = int.Parse(ReadConfigINI("XML3", "result_label", sini));
            string tname = "rb_xml3" + (xml3result).ToString();
            RadioButton rb = (RadioButton)findControl(groupBox3, tname);
            rb.Checked = true;

            tb_xml41.Text = ReadConfigINI("XML4", "station_label", sini);
            tb_xml42.Text = ReadConfigINI("XML4", "description", sini);
            xml4result = int.Parse(ReadConfigINI("XML4", "result_label", sini));
            string xname = "rb_xml4" + (xml4result).ToString();
            RadioButton xb = (RadioButton)findControl(groupBox4, xname);
            xb.Checked = true;

            //后面不需要了？？
            tb_xml51.Text = ReadConfigINI("XML5", "operations_Name", sini);
            tb_xml52.Text = ReadConfigINI("XML5", "operations_Column", sini);
            //xml5result = int.Parse(ReadConfigINI("XML5", "result_label", sini));
            //string sname = "rb_xml5" + (xml5result).ToString();
            //RadioButton sb = (RadioButton)findControl(groupBox5, sname);
            //sb.Checked = true;
            //tb_xml53.Text = ReadConfigINI("XML5", "id", sini);
            //tb_xml54.Text = ReadConfigINI("XML5", "count", sini);
            tb_xml55.Text = ReadConfigINI("XML5", "Operationer", sini);
            tb_xml56.Text = ReadConfigINI("XML5", "Diesel_grade", sini);

            //tb_xml71.Text = ReadConfigINI("XML7", "feature_label", sini);
            //tb_xml72.Text = ReadConfigINI("XML7", "description", sini);
            //xml5result = int.Parse(ReadConfigINI("XML7", "result_label", sini));
            //string vname = "rb_xml7" + (xml5result).ToString();
            //RadioButton vb = (RadioButton)findControl(groupBox6, vname);
            //vb.Checked = true;
            //tb_xml73.Text = ReadConfigINI("XML7", "id", sini);
            //tb_xml74.Text = ReadConfigINI("XML7", "analysis_type", sini);
            //tb_xml75.Text = ReadConfigINI("XML7", "analysis_is_y_data", sini);

            xmlpath = ReadConfigINI("XML8", "path", sini);
            tbXMLSavePath.Text = xmlpath;
            
            //定时器代码
            //System.Timers.Timer t = new System.Timers.Timer();//实例化Timer类
            //int intTime = 5000;
            //t.Interval = intTime;//设置间隔时间，为毫秒；
            //t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            //t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            //t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
        #endregion readINI

        //保存文件
        private void btn_save_Click(object sender, EventArgs e)
        {
            #region XML1
            WriteConfigIni(tb_xml11.Text, "root_version", "XML1", sini);
            WriteConfigIni(tb_xml12.Text, "root_xmlns", "XML1", sini);
            #endregion XML1
            #region XML2
            //先判断字符长度是否符合要求
            int strcount;
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml21.Text);
            if (strcount > 12)
            {
                MessageBox.Show("part_label大于12，请重新输入！");
                return;
            }
            partlabel = tb_xml21.Text;
            WriteConfigIni(tb_xml21.Text, "part_label", "XML2", sini);
            WriteConfigIni(tb_xml22.Text, "serial_number", "XML2", sini);
            WriteConfigIni(tb_xml23.Text, "model_number", "XML2", sini);
            #endregion XML2
            #region XML3
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml31.Text);
            if (strcount > 12)
            {
                MessageBox.Show("section_label大于12，请重新输入！");
                return;
            }
            WriteConfigIni(tb_xml31.Text, "section_label", "XML3", sini);

            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml32.Text);
            if (strcount > 32)
            {
                MessageBox.Show("section description大于32，请重新输入！");
                return;
            }
            WriteConfigIni(tb_xml32.Text, "description", "XML3", sini);
            //XML3 result            
            if (rb_xml31.Checked)
            {
                xml3result = 1;
            }
            else if (rb_xml32.Checked)
            {
                xml3result = 2;
            }
            else if (rb_xml33.Checked)
            {
                xml3result = 3;
            }
            else
            {
                xml3result = 4;
            }
            WriteConfigIni(xml3result.ToString(), "result_label", "XML3", sini);
            #endregion XML3
            #region XML4
            //判断字节长度是否符合要求
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml41.Text);
            if (strcount > 12)
            {
                MessageBox.Show("station_label大于12，请重新输入！");
                return;
            }
            WriteConfigIni(tb_xml41.Text, "station_label", "XML4", sini);
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml42.Text);
            if (strcount > 32)
            {
                MessageBox.Show("station description大于32，请重新输入！");
                return;
            }
            WriteConfigIni(tb_xml42.Text, "description", "XML4", sini);
            //XML4 result            
            if (rb_xml41.Checked)
            {
                xml4result = 1;
            }
            else if (rb_xml42.Checked)
            {
                xml4result = 2;
            }
            else if (rb_xml43.Checked)
            {
                xml4result = 3;
            }
            else
            {
                xml4result = 4;
            }
            WriteConfigIni(xml4result.ToString(), "result_label", "XML4", sini);
            #endregion XML4
            #region XML5
            //判断字节长度是否符合要求
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml51.Text);
            //if (strcount > 12)
            //{
            //    MessageBox.Show("operations_label大于12，请重新输入！");
            //    return;
            //}
            WriteConfigIni(tb_xml51.Text, "operations_Name", "XML5", sini);
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml52.Text);
            //if (strcount > 32)
            //{
            //    MessageBox.Show("operations_description大于32，请重新输入！");
            //    return;
            //}
            WriteConfigIni(tb_xml52.Text, "operations_Column", "XML5", sini);
            ////XML5 result            
            //if (rb_xml51.Checked)
            //{
            //    xml5result = 1;
            //}
            //else if (rb_xml52.Checked)
            //{
            //    xml5result = 2;
            //}
            //else if (rb_xml53.Checked)
            //{
            //    xml5result = 3;
            //}
            //else
            //{
            //    xml5result = 4;
            //}
            //WriteConfigIni(xml5result.ToString(), "result_label", "XML5", sini);
            //WriteConfigIni(tb_xml53.Text, "id", "XML5", sini);
            //WriteConfigIni(tb_xml54.Text, "count", "XML5", sini);
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml55.Text);
            //if (strcount > 12)
            //{
            //    MessageBox.Show("version_name大于12，请重新输入！");
            //    return;
            //}
            //else
            //{
            WriteConfigIni(tb_xml55.Text, "Operationer", "XML5", sini);
            //}
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml56.Text);
            //if (strcount > 0)
            //{
            WriteConfigIni(tb_xml56.Text, "Diesel_grade", "XML5", sini);
            //}
            //else
            //{
            //    MessageBox.Show("version_number应该大于零！");
            //}
            #endregion XML5
            #region XML7
            ////判断字节长度是否符合要求
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml71.Text);
            //if (strcount > 12)
            //{
            //    MessageBox.Show("feature_label大于12，请重新输入！");
            //    return;
            //}
            //WriteConfigIni(tb_xml71.Text, "feature_label", "XML7", sini);
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml72.Text);
            //if (strcount > 32)
            //{
            //    MessageBox.Show("feature_description大于32，请重新输入！");
            //    return;
            //}
            //WriteConfigIni(tb_xml72.Text, "description", "XML7", sini);
            ////XML7 result            
            //if (rb_xml71.Checked)
            //{
            //    xml7result = 1;
            //}
            //else if (rb_xml72.Checked)
            //{
            //    xml7result = 2;
            //}
            //else if (rb_xml73.Checked)
            //{
            //    xml7result = 3;
            //}
            //else
            //{
            //    xml7result = 4;
            //}
            //WriteConfigIni(xml7result.ToString(), "result_label", "XML7", sini);
            //WriteConfigIni(tb_xml73.Text, "id", "XML7", sini);
            //WriteConfigIni(tb_xml74.Text, "analysis_type", "XML7", sini);
            //WriteConfigIni(tb_xml75.Text, "analysis_is_y_data", "XML7", sini);
            #endregion XML7

            //加入XML文件保存路径。2018-11-10
            WriteConfigIni(tbXMLSavePath.Text, "path", "XML8", sini);

            string info = DateTime.Now.ToLocalTime().ToString() + "\n----参数设置成功----\n\n";
            WriteFile(Runlogpath, info);
            MessageBox.Show("Done!");
        }
        //导入TXT文本并转化为datatable
        string txtContent = string.Empty;
        string waitConvertfile, Convertxmlfile;
        bool[] ValueisNum;  //数据表中的各列值是数字还是字符

        #region 初始化信息
        //single_part部分
        //part
        string part_label = "Engine";
        string part_serial_number = "E4T15B";
        string part_model_number_label = "E4T15B";
        string Operationer = "Ss";
        string Diesel_grade = "20181211";


        //这里好象有一个备注信息，机油牌号，操作员等信息等放到这来，存入INI文件中
        //section
        string section_label = "section22";
        string section_description = "section22";
        string section_result_label = "Pass";
        string section_result_id = "0";
        string section_order = "0";
        //station
        string station_label = "Op3240";
        string station_description = "OE-024 Ignition Test";
        string station_result_label = "Pass";
        string station_result_id = "0";
        string station_order = "0";
        //operation
        string operation_label = "CK_Ceramic_T";   //????
        string operation_description = "Cracked Ceramic Test";
        string operation_result_label = "Pass";   //
        string operation_result_id = "0";   //上次在重庆说Pass为0，Fail为正数，No op为-2，Unknow为-1或-3
        string operation_order = "0";   //恒为0？？？
        string operation_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //当前时间
        string operation_count = "-1";  //恒为—1
        //waveform
        string waveform_label = "1st_Current";  //????
        string wavefrom_description = "1Ignition Current Signal#r0";
        string waveform_result_label = "Pass";   //
        string waveform_result_id = "0";
        string waveform_order = "0";   //恒为0？？？
        string waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //当前时间
        string waveform_x_type = "NULL";
        string waveform_x_format = "%g";
        string waveform_x_unit_label = "ms";
        string waveform_x_reference = "-1";
        string waveform_y_type = "NULL";
        string waveform_y_format = "%g";
        string waveform_y_unit_label = "A";
        string waveform_y_reference = "1"; //一个操作组的一个变量就是一个波形
        //feature
        string feature_label = "CraceNo_1";  //????
        string feature_description = "Crace Numbers CYL#1";
        string feature_result_label = "Pass";   //
        string feature_result_id = "0";
        string feature_order = "0";   //恒为0？？？
        string feature_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //当前时间
        string feature_data_type = "float";
        string feature_data_format = "%g";
        string feature_data_unit_label = "A";
        string feature_data_value = "50";
        string feature_analysis_region_type = "9008";
        string feature_analysis_region_is_y_data = "0";

        //binary_catalog部分
        string item_label = "1st_Current";
        string item_reference = "1";
        string item_type = "NULL";
        string item_index = "0";
        string item_size = "1";
        string item_data_x_label = "Time";
        string item_data_y_label = "Current";  //问清楚？？
        string item_data_x_start = "0";
        string item_data_x_interval = "0.01"; //问清楚？？
        string item_data_datapoint_ydata = "0.574791"; //该值放入datapoint
        string item_data_datapoint_order = "1";
        #endregion 初始化信息
        string[] _txtFiles = null;
        DataTable _txtDt = null;//初始化一张datatable
        string[] _head = null;
        string[] _unit = null;
        string _station = null;//机器名
        string _MacSN = null;//序列号
        string _MacNum = null;//机型号
        string _interval = null;//间隔
        //按钮点击事件
        private void btn_insert_Click(object sender, EventArgs e)
        {
            ////判断路径是否存在
            //if (!Directory.Exists(xmlpath))
            //{
            //    MessageBox.Show("参数配置的路径不存在，请重新设置！");
            //    return;
            //}
            ////1.读取文本文件所在的目录
            //string directory = "D:/Data/data";
            ////2.获取目录中的所有文本文件
            //var txtFiles = Directory.GetFiles(directory, "*.txt");
            ////3.循环文本文件，把当前文件中的文本读取到datatable中
            //_txtDt = new DataTable();
            //foreach (var txtFile in txtFiles)
            //{
            //    using (var reader = new StreamReader(txtFile, Encoding.UTF8))
            //    {
            //        var txtStr = reader.ReadToEnd();
            //        //转换字符串到datatable
            //        AddDataToTxtDt(txtStr);
            //    }
            //}
            //ShowProgress(10, "文件内容读入完成，正在生成XML文件...");
            //SaveXml();//生成并保存                            
            //btn_insert.Enabled = true;
            //btn_save.Enabled = true;
            ////timer1.Stop();//定时器关闭                                              
        }
                    
        //多个TXT文件读取                        
        private void AddDataToTxtDt(string txtStr)
        {
            //按钮禁用，等转完再启用，进度条显示   2018-11-10
            //btn_insert.Enabled = false;
            //btn_save.Enabled = false;
            //ShowProgress(1, "正准备检查文件格式...");

            txtStr = txtStr.Replace("\r\n", "@");
            var nodes = txtStr.Split('@');
            var interval = GetInterval(nodes).ToString();
            _interval = interval;
            var head = nodes[1].Split('\t');
            var unit = nodes[2].Split('\t');
            //todo:验证数据(表头、单位)

            //ShowProgress(5, "文件格式检查完成，正在读入文件内容...");

            //创建表头
            if (_txtDt.Columns == null || _txtDt.Columns.Count == 0)
            {
                _head = head;
                _unit = unit;
                for (int n = 0; n < head.Length; n++)
                {
                    _txtDt.Columns.Add(head[n], typeof(string));
                }
                //_txtDt.Columns.Add("Interval", typeof(string));
            }
            //去掉数组中的型号，表头，单位
            List<string> lineList = nodes.ToList();
            lineList.RemoveRange(0, 3);
            //var line1Datas = lineList[0].Split('\t');
            //var line1Time = Convert.ToDateTime(line1Datas[0]+" "+line1Datas[1]);
            //var line2Datas = lineList[1].Split('\t');
            //var line2Time = Convert.ToDateTime(line2Datas[0]+" "+line2Datas[1]);
            //var interval = (line2Time - line1Time).TotalSeconds.ToString();
           
            //填充行内容
            for (int i = 0; i < lineList.Count; i++)
            {
                var line = lineList[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] str = line.Split('\t');
                //todo:数据验证(内容)
                DataRow dr = _txtDt.NewRow();
                for (int a = 0; a < _txtDt.Columns.Count; a++)
                {
                    dr[a] = str[a];
                }
                _txtDt.Rows.Add(dr);
                _station = _txtDt.Rows[0]["M_EN.STATION"].ToString();
                _MacSN = _txtDt.Rows[0]["M_EN.SERIALNUM"].ToString();
                _MacNum = _txtDt.Rows[0]["M_EN.MODNUM"].ToString();

            }
            _txtDt.Columns.Remove("M_EN.STATION");
            _txtDt.Columns.Remove("M_EN.SERIALNUM");
            _txtDt.Columns.Remove("M_EN.MODNUM");
            List<string> headlist = new List<string>(head);
            headlist.RemoveRange(2, 3);
            var head1 = headlist.ToArray();
            _head = head1;
            List<string> unitlist = new List<string>(unit);
            unitlist.RemoveRange(2, 3);
            var unit1 = unitlist.ToArray();
            _unit = unit1;
        }
        //显示进展
        //public void ShowProgress(int nvalue, string str)
        //{
        //    labelP.Text = nvalue.ToString() + "%";
        //    progressBar1.Value = nvalue;
        //    this.textBox1.AppendText("--" + str + "\r\n");
        //    Application.DoEvents();
        //    progressBar1.Update();
        //    progressBar1.Refresh();
        //    this.textBox1.Update();
        //    this.textBox1.Refresh();
        //}
        string _a = null;
        
   
        //保存构造xml
        public void SaveXml()
        {
            //实例化一个对象
            XmlDocument xmlDoc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "", "");
            xmlDoc.AppendChild(node);
            //创建根节点
            XmlNode Root = xmlDoc.CreateElement("root");//根节点的命名空间 xmlns怎么添加
            XmlAttribute a = xmlDoc.CreateAttribute("xmlns");
            a.Value = "http://www.sciemetric.com/namespace";
            Root.Attributes.Append(a);
            //part中的label和serial_number为什么会空一行//不清楚
            #region part
            //创建子节点
            XmlNode childNode11 = xmlDoc.CreateNode(XmlNodeType.Element, "single_part", null);
            XmlNode childNode1 = xmlDoc.CreateNode(XmlNodeType.Element, "binary_catalog", null);

            XmlNode childNode12 = xmlDoc.CreateNode(XmlNodeType.Element, "part", null);
            XmlNode childNode13 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            //string MacName = _txtDt.Rows[1][2].ToString();
            //_MacName = MacName;
            childNode13.InnerText = "Head";
            XmlNode childNode14 = xmlDoc.CreateNode(XmlNodeType.Element, "serial_number", null);
            //string MacSN = _txtDt.Rows[1][3].ToString();
            //_MacSN = MacSN;
            childNode14.InnerText =_MacSN;
            XmlNode childNode15 = xmlDoc.CreateNode(XmlNodeType.Element, "model_number", null);
            //string MacNum = _txtDt.Rows[1][4].ToString();
            //_MacNum = MacNum;
            CreateNode1(xmlDoc, childNode15, "label", _MacNum);
            //XmlNode childNode16 = xmlDoc.CreateNode(XmlNodeType.Element, "Remarks", null);
            //Operationer = tb_xml55.Text;
            //Diesel_grade = tb_xml56.Text;
            //childNode16.InnerText = Operationer + "," + Diesel_grade;
            childNode12.AppendChild(childNode13);
            childNode12.AppendChild(childNode14);
            childNode12.AppendChild(childNode15);
            //childNode12.AppendChild(childNode16);
            childNode11.AppendChild(childNode12);
            //Root.AppendChild(childNode11);
            #endregion part
            #region section
            //创建子节点
            XmlNode childNode21 = xmlDoc.CreateNode(XmlNodeType.Element, "sections", null);
            XmlNode childNode22 = xmlDoc.CreateNode(XmlNodeType.Element, "section", null);
            XmlNode childNode23 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            childNode23.InnerText = "Section36";
            XmlNode childNode24 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
            childNode24.InnerText = "Seciton36";
            XmlNode childNode25 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
            //xml3result = int.Parse(ReadConfigINI("XML3", "result_label", sini));
            //string tname = "rb_xml3" + (xml3result).ToString();
            //RadioButton rb = (RadioButton)findControl(groupBox3, tname);
            CreateNode1(xmlDoc, childNode25, "label", "Pass");
            childNode22.AppendChild(childNode23);
            childNode22.AppendChild(childNode24);
            childNode22.AppendChild(childNode25);
            childNode21.AppendChild(childNode22);
            //Root.AppendChild(childNode21);
            #endregion section
            #region stations
            //创建子节点
            XmlNode childNode31 = xmlDoc.CreateNode(XmlNodeType.Element, "stations", null);
            XmlNode childNode32 = xmlDoc.CreateNode(XmlNodeType.Element, "station", null);
            XmlNode childNode33 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            childNode33.InnerText = _station;
            XmlNode childNode34 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
            childNode34.InnerText = "Performance Test Bench";
            XmlNode childNode35 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
            //xml4result = int.Parse(ReadConfigINI("XML4", "result_label", sini));
            //string xname = "rb_xml4" + (xml4result).ToString();
            //RadioButton xb = (RadioButton)findControl(groupBox4, xname);
            CreateNode1(xmlDoc, childNode35, "label", "Pass");
            childNode32.AppendChild(childNode33);
            childNode32.AppendChild(childNode34);
            childNode32.AppendChild(childNode35);
            childNode31.AppendChild(childNode32);
            //Root.AppendChild(childNode31);
            #endregion stations
           
            XmlNode childNode41 = xmlDoc.CreateNode(XmlNodeType.Element, "operations", null);
            //DataView view = new DataView(dt);                    //新一个表
            //DataTable dtOpera = view.ToTable(true, strOperaCol); //查出操作组各种类型
            //for (int i = 0; i < dtOpera.Rows.Count; i++)         //对于每种操作组类型
            //{
                //操作组部分                
                XmlNode childNode42 = xmlDoc.CreateNode(XmlNodeType.Element, "operation", null);
                XmlNode childNode43 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
                XmlNode childNode44 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
                XmlNode childNode45 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
                XmlNode childNode46 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
                XmlNode childNode47 = xmlDoc.CreateNode(XmlNodeType.Element, "count", null);
                XmlNode childNode48 = xmlDoc.CreateNode(XmlNodeType.Element, "version_name", null);
                XmlNode childNode49 = xmlDoc.CreateNode(XmlNodeType.Element, "version_number", null);
                //XmlNode childNode50 = xmlDoc.CreateNode(XmlNodeType.Element, "version_date", null);

                operation_label = "PerformTest";
                operation_description = "PerformanceTest";
                operation_result_label = "Pass";   //
                operation_result_id = "0";   //上次在重庆说Pass为0，Fail为正数，No op为-2，Unknow为-1或-3
                operation_order = "0";   //恒为0？？？
                DataRow dtr=_txtDt.Rows[_txtDt.Rows.Count-1];
                string[] A = dtr[0].ToString().Replace("/", "-").Split('-');
                string B = dtr[1].ToString().Replace(":", "-");
                operation_date_stamp = A[2]+"-"+A[1]+"-"+A[0] + "T" + B;  //最后一组数据的操作时间
                operation_count = "-1";  //设置为-1时：数据上传数据库自动累计计数。


                #region operation
                //创建子节点           
                childNode43.InnerText = operation_label;
                childNode44.InnerText = operation_description;
                //xml5result = int.Parse(ReadConfigINI("XML5", "result_label", sini));
                //string sname = "rb_xml5" + (xml5result).ToString();
                //RadioButton sb = (RadioButton)findControl(groupBox5, sname);
                CreateNode1(xmlDoc, childNode45, "label", operation_result_label);
                CreateNode1(xmlDoc, childNode45, "id", operation_result_id);
                childNode46.InnerText = operation_date_stamp;
                childNode47.InnerText = operation_count;
                childNode48.InnerText = "MORPHEE";
                childNode49.InnerText = "V2.9.15";          
                //childNode50.InnerText = ReadConfigINI("XML5", "version_date", sini);
                childNode42.AppendChild(childNode43);
                childNode42.AppendChild(childNode44);
                childNode42.AppendChild(childNode45);
                childNode42.AppendChild(childNode46);
                childNode42.AppendChild(childNode47);
                childNode42.AppendChild(childNode48);
                childNode42.AppendChild(childNode49);
                //childNode42.AppendChild(childNode50);
                childNode41.AppendChild(childNode42);
                //Root.AppendChild(childNode41);
                #endregion operation

                XmlNode childNode61 = xmlDoc.CreateNode(XmlNodeType.Element, "waveforms", null);
                int iwcout = 0;
                for (int idtc = 0; idtc < ValueisNum.Length; idtc++) //
                {
                    if (ValueisNum[idtc])   //如果该列为数据
                    {

                        #region waveforms
                        XmlNode childNode62 = xmlDoc.CreateNode(XmlNodeType.Element, "waveform", null);
                        XmlNode childNode63 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
                        //XmlNode childNode666 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
                        XmlNode childNode64 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
                        XmlNode childNode669 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
                        XmlNode childNode65 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
                        XmlNode childNode66 = xmlDoc.CreateNode(XmlNodeType.Element, "x", null);
                        XmlNode childNode67 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                        XmlNode childNode68 = xmlDoc.CreateNode(XmlNodeType.Element, "format", null);
                        XmlNode childNode69 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
                        XmlNode childNode611 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);
                        XmlNode childNode612 = xmlDoc.CreateNode(XmlNodeType.Element, "y", null);
                        XmlNode childNode613 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                        XmlNode childNode614 = xmlDoc.CreateNode(XmlNodeType.Element, "format", null);
                        XmlNode childNode615 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
                        XmlNode childNode616 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);

                        iwcout++;
                        //波形部分
                        if (_head[idtc].Length > 10)//截取description中的前12个字符，如果名称不能重复，则先查询有无重复，如有重复，截取前10个字符，加上两位顺序编码
                        {
                            waveform_label = iwcout.ToString() + _head[idtc].Substring(0, 10).ToString();
                        }
                        else
                        {
                            waveform_label = iwcout.ToString() + _head[idtc].ToString();
                        }
                        //wavefrom_description = _unit[idtc].ToString();
                        waveform_result_label = "Pass";   //
                        waveform_result_id = "0";
                        waveform_order = "0";   //恒为0？？？
                        //string[] A = dtr[0].ToString().Replace("/", "-").Split('-');
                        //string B = dtr[1].ToString().Replace(":", "-");
                        //operation_date_stamp = A[2] + "-" + A[1] + "-" + A[0] + "T" + B;
                        waveform_date_stamp = A[2] + "-" + A[1] + "-" + A[0] + "T" + B;
                        //waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //todo:更改时间
                        waveform_x_type = "NULL";
                        waveform_x_format = "%g";
                        waveform_x_unit_label = "second";
                        waveform_x_reference = "-1";
                        waveform_y_type = "PortableMData";
                        waveform_y_format = "%g";
                        waveform_y_unit_label = _unit[idtc].ToString();
                        waveform_y_reference = iwcout.ToString(); //如何在操作组中区分波形数？？？                                     
                        childNode63.InnerText = waveform_label;
                        //childNode666.InnerText = wavefrom_description;
                        CreateNode1(xmlDoc, childNode64, "label", waveform_result_label);
                        CreateNode1(xmlDoc, childNode64, "id", waveform_result_id);
                        childNode669.InnerText = waveform_order;
                        childNode65.InnerText = waveform_date_stamp;
                        childNode67.InnerText = waveform_x_type;
                        childNode68.InnerText = waveform_x_format;
                        CreateNode1(xmlDoc, childNode69, "label", waveform_x_unit_label);
                        childNode611.InnerText = waveform_x_reference;
                        childNode66.AppendChild(childNode67);
                        childNode66.AppendChild(childNode68);
                        childNode66.AppendChild(childNode69);
                        childNode66.AppendChild(childNode611);
                        childNode613.InnerText = waveform_y_type;
                        childNode614.InnerText = waveform_y_format;
                        CreateNode1(xmlDoc, childNode615, "label", waveform_y_unit_label);
                        childNode616.InnerText = waveform_y_reference;
                        childNode612.AppendChild(childNode613);
                        childNode612.AppendChild(childNode614);
                        childNode612.AppendChild(childNode615);
                        childNode612.AppendChild(childNode616);

                        childNode62.AppendChild(childNode63);
                        //childNode62.AppendChild(childNode666);
                        childNode62.AppendChild(childNode64);
                        childNode62.AppendChild(childNode669);
                        childNode62.AppendChild(childNode65);
                        childNode62.AppendChild(childNode66);
                        childNode62.AppendChild(childNode612);
                        childNode61.AppendChild(childNode62);

                        childNode42.AppendChild(childNode61);
                        #endregion waveforms

                        #region binary_catalog

                        XmlNode childNode101 = xmlDoc.CreateNode(XmlNodeType.Element, "item", null);
                        XmlNode childNode102 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
                        XmlNode childNode103 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);
                        XmlNode childNode104 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                        XmlNode childNode105 = xmlDoc.CreateNode(XmlNodeType.Element, "index", null);
                        XmlNode childNode106 = xmlDoc.CreateNode(XmlNodeType.Element, "size", null);
                        XmlNode childNode107 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);

                        //XmlNode childNode108 = xmlDoc.CreateNode(XmlNodeType.Element, "x_label", null);
                        //XmlNode childNode109 = xmlDoc.CreateNode(XmlNodeType.Element, "y_label", null);
                        //XmlNode childNode110 = xmlDoc.CreateNode(XmlNodeType.Element, "x_start", null);
                        //XmlNode childNode111 = xmlDoc.CreateNode(XmlNodeType.Element, "x_interval", null);                       


                        item_label = waveform_label;
                        item_reference = iwcout.ToString();
                        item_type = "NULL";
                        item_index = "0";
                        item_size = _txtDt.Rows.Count.ToString();
                        item_data_x_label = "Time";
                        item_data_y_label = _head[idtc];  //问清楚？？
                        item_data_x_start = "0";
                        item_data_x_interval = _interval; //间隔不固定
                        int iorder = 0;
                        DataTable datapoint = new DataTable();
                        datapoint.Columns.Add("item_data_datapoint_ydata", typeof(string));
                        datapoint.Columns.Add("item_data_datapoint_order", typeof(string));
                        DataRow ds;
                        for (int s = 0; s < _txtDt.Rows.Count; s++)  // y值系列； 
                        {

                            iorder++;
                            ds = datapoint.NewRow();
                            item_data_datapoint_ydata = _txtDt.Rows[s][idtc].ToString(); //该值放入datapoint
                            item_data_datapoint_order = iorder.ToString();
                            ds["item_data_datapoint_ydata"] = item_data_datapoint_ydata;
                            ds["item_data_datapoint_order"] = item_data_datapoint_order;
                            datapoint.Rows.Add(ds);
                        }

                        //for (int o = 0; o < dt.Rows.Count; o++)
                        //{
                        childNode102.InnerText = item_label;
                        childNode103.InnerText = item_reference;
                        childNode104.InnerText = item_type;
                        childNode105.InnerText = item_index;
                        childNode106.InnerText = item_size;
                        XmlNode childNode108 = xmlDoc.CreateNode(XmlNodeType.Element, "x_label", null);
                        XmlNode childNode109 = xmlDoc.CreateNode(XmlNodeType.Element, "y_label", null);
                        XmlNode childNode110 = xmlDoc.CreateNode(XmlNodeType.Element, "x_start", null);
                        XmlNode childNode111 = xmlDoc.CreateNode(XmlNodeType.Element, "x_interval", null);
                        childNode108.InnerText = item_data_x_label;
                        childNode109.InnerText = item_data_y_label;
                        childNode110.InnerText = item_data_x_start;
                        childNode111.InnerText = item_data_x_interval;
                        childNode107.AppendChild(childNode108);
                        childNode107.AppendChild(childNode109);
                        childNode107.AppendChild(childNode110);
                        childNode107.AppendChild(childNode111);
                        //循环会覆盖原结果

                        for (int b = 0; b < int.Parse(item_size); b++)
                        {
                            XmlNode childNode112 = xmlDoc.CreateNode(XmlNodeType.Element, "datapoint", null);
                            XmlNode childNode113 = xmlDoc.CreateNode(XmlNodeType.Element, "ydata", null);
                            XmlNode childNode114 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);

                            childNode113.InnerText = datapoint.Rows[b][0].ToString();
                            childNode114.InnerText = datapoint.Rows[b][1].ToString();
                            childNode112.AppendChild(childNode113);
                            childNode112.AppendChild(childNode114);

                            childNode107.AppendChild(childNode112);
                            //.InsertAfter(childNode112, null);

                        }

                        childNode101.AppendChild(childNode102);
                        childNode101.AppendChild(childNode103);
                        childNode101.AppendChild(childNode104);
                        childNode101.AppendChild(childNode105);
                        childNode101.AppendChild(childNode106);
                        childNode101.AppendChild(childNode107);
                        childNode1.AppendChild(childNode101);
                        //}

                        #endregion binary_catalog
                    //}

                }
            }



            //ShowProgress(15, "XML文件Part,Section,Station和Group部分生成完毕");
            
            //特征值的增加，最难的一步
            #region feature
            //int drCount = dt.Rows.Count;
            //double intervel = 500.0;
            //double drCountSplit = drCount / intervel;

            //int grCount = (int)Math.Ceiling(drCountSplit);
            //int ii = 1;
            //for (int i = 0; i < drCount; i++)
            //{

            //    if (i == ii * intervel)
            //    {
            //        ShowProgress(20 + (int)(ii * 80.0 / grCount), "XML文件feature数据已生成" + (ii * intervel).ToString() + "/" + drCount.ToString() + "行");
            //        ii++;
            //    }
            //    for (int n = 2; n < head.Length; n++)
            //    {
            //        XmlNode childNode51 = xmlDoc.CreateNode(XmlNodeType.Element, "features", null);
            //        XmlNode childNode52 = xmlDoc.CreateNode(XmlNodeType.Element, "feature", null);
            //        XmlNode childNode53 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            //        //childNode53.InnerText = tb_xml71.Text;
            //        XmlNode childNode54 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);

            //        //不建议再去读文本，因此前面已经读过了，效率低。2018-11-11
            //        //xml5result = int.Parse(ReadConfigINI("XML7", "result_label", sini));
            //        string vname = "rb_xml7" + (xml5result).ToString();
            //        //不建议再去读控件，因此前面已经有过它的值了，效率低。2018-11-11
            //        RadioButton vb = (RadioButton)findControl(groupBox6, vname);
            //        CreateNode1(xmlDoc, childNode54, "label", vb.Text);
            //        CreateNode1(xmlDoc, childNode54, "id", tb_xml73.Text);
            //        //XmlNode childNode55 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
            //        XmlNode childNode56 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
            //        //string datachange = dt.Rows[i][0].ToString();
            //        //string[] datachange1 = datachange.Split('/');
            //        //childNode56.InnerText = datachange1[2] + "-" + datachange1[1] + "-" + datachange1[0] + "T" + dr.Rows[i][1].ToString();
            //        XmlNode childNode57 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);
            //        //此处的type好象不对，应该是调用GetDataType的返回值，来填入，2018-11-11   解决
            //        XmlNode childNode58 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
            //        string type1 = GetDataType(head[n]);
            //        childNode58.InnerText = type1;
            //        XmlNode childNode59 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
            //        CreateNode1(xmlDoc, childNode59, "label", unit[n]);
            //        XmlNode childNode60 = xmlDoc.CreateNode(XmlNodeType.Element, "value", null);
            //        childNode60.InnerText = dt.Rows[i][n].ToString();
            //        //CreateNode1(xmlDoc, childNode57, "type",head[n].ToString());
            //        //CreateNode1(xmlDoc, childNode57, "unit", "<label>"+unit[n].ToString()+"</label>");
            //        //CreateNode1(xmlDoc,childNode57,"value",dr.Rows[i][n].ToString());
            //        XmlNode childNode61 = xmlDoc.CreateNode(XmlNodeType.Element, "analysis_region", null);
            //        CreateNode1(xmlDoc, childNode61, "type", "9030");
            //        CreateNode1(xmlDoc, childNode61, "is_y_data", "0");
            //        childNode57.AppendChild(childNode58);
            //        childNode57.AppendChild(childNode59);
            //        childNode57.AppendChild(childNode60);
            //        childNode52.AppendChild(childNode53);
            //        childNode52.AppendChild(childNode54);
            //        //order预留
            //        childNode52.AppendChild(childNode56);
            //        childNode52.AppendChild(childNode57);
            //        childNode52.AppendChild(childNode61);
            //        childNode51.AppendChild(childNode52);
            //       // Root.AppendChild(childNode51);
            //    }
            //}
            //ShowProgress(95, "XML文件feature数据已生成" + drCount.ToString() + "/" + drCount.ToString() + "行");
            #endregion feature
            

            #region structures
            //childNode42.AppendChild(childNode61);
            //childNode41.AppendChild(childNode42);
            childNode32.AppendChild(childNode41);
            childNode31.AppendChild(childNode32);
            childNode22.AppendChild(childNode31);
            childNode21.AppendChild(childNode22);
            childNode11.AppendChild(childNode12);
            childNode11.AppendChild(childNode21);
            Root.AppendChild(childNode11);
            Root.AppendChild(childNode1);
            #endregion structures

            #region save
            xmlDoc.AppendChild(Root);
            try
            {
                //路径从配置中来 2018-11-10
                string sfile =" C:\\Users\\Zhang Shuo\\Desktop "+ "\\" + _MacSN + "+"+DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".xml";
                if (File.Exists(sfile))
                {
                    MessageBox.Show(sfile + "文件已存在，请稍后再试！");
                    return;
                }
                xmlDoc.Save(sfile);//保存到指定位置
                Convertxmlfile = sfile;
                //MessageBox.Show("保存成功");
                //ShowProgress(100, "文件转换完成,存为" + sfile + "\n@@@@@\n");
                string info = DateTime.Now.ToLocalTime().ToString() + "将" + waitConvertfile + "成功转为" + Convertxmlfile;
                WriteFile(Runlogpath, info);
            }
            catch (System.Exception ex)
            {
                string info = DateTime.Now.ToLocalTime().ToString() + "将" + waitConvertfile + "不能成功转为" + Convertxmlfile;
                WriteFile(Runlogpath, info);
                info = DateTime.Now.ToLocalTime().ToString() + "\n----" + ex.ToString() + "----\n\n";
                WriteFile(Errlogpath, info);
            }
            #endregion save
        }

        //保存构造xml
        /*public void SaveXml(DataTable dt, string[] head, string[] unit, string[] nodes)
        {
            //实例化一个对象
            XmlDocument xmlDoc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "", "");
            xmlDoc.AppendChild(node);
            //创建根节点
            XmlNode Root = xmlDoc.CreateElement("root");//根节点的命名空间 xmlns怎么添加
            XmlAttribute a = xmlDoc.CreateAttribute("xmlns");
            a.Value = "http://www.sciemetric.com/namespace";
            Root.Attributes.Append(a);
            //part中的label和serial_number为什么会空一行//不清楚
            #region part
            //创建子节点
            XmlNode childNode11 = xmlDoc.CreateNode(XmlNodeType.Element, "single_part", null);
            XmlNode childNode1 = xmlDoc.CreateNode(XmlNodeType.Element, "binary_catalog", null);

            XmlNode childNode12 = xmlDoc.CreateNode(XmlNodeType.Element, "part", null);
            XmlNode childNode13 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            childNode13.InnerText = tb_xml21.Text;
            XmlNode childNode14 = xmlDoc.CreateNode(XmlNodeType.Element, "serial_number", null);
            childNode14.InnerText = tb_xml22.Text;
            XmlNode childNode15 = xmlDoc.CreateNode(XmlNodeType.Element, "model_number", null);
            CreateNode1(xmlDoc, childNode15, "label", tb_xml23.Text);
            //XmlNode childNode16 = xmlDoc.CreateNode(XmlNodeType.Element, "Remarks", null);
            //Operationer = tb_xml55.Text;
            //Diesel_grade = tb_xml56.Text;
            //childNode16.InnerText = Operationer + "," + Diesel_grade;
            childNode12.AppendChild(childNode13);
            childNode12.AppendChild(childNode14);
            childNode12.AppendChild(childNode15);
            //childNode12.AppendChild(childNode16);
            childNode11.AppendChild(childNode12);
            //Root.AppendChild(childNode11);
            #endregion part
            #region section
            //创建子节点
            XmlNode childNode21 = xmlDoc.CreateNode(XmlNodeType.Element, "sections", null);
            XmlNode childNode22 = xmlDoc.CreateNode(XmlNodeType.Element, "section", null);
            XmlNode childNode23 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            childNode23.InnerText = tb_xml31.Text;
            XmlNode childNode24 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
            childNode24.InnerText = tb_xml32.Text;
            XmlNode childNode25 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
            xml3result = int.Parse(ReadConfigINI("XML3", "result_label", sini));
            string tname = "rb_xml3" + (xml3result).ToString();
            RadioButton rb = (RadioButton)findControl(groupBox3, tname);
            CreateNode1(xmlDoc, childNode25, "label", rb.Text);
            childNode22.AppendChild(childNode23);
            childNode22.AppendChild(childNode24);
            childNode22.AppendChild(childNode25);
            childNode21.AppendChild(childNode22);
            //Root.AppendChild(childNode21);
            #endregion section
            #region stations
            //创建子节点
            XmlNode childNode31 = xmlDoc.CreateNode(XmlNodeType.Element, "stations", null);
            XmlNode childNode32 = xmlDoc.CreateNode(XmlNodeType.Element, "station", null);
            XmlNode childNode33 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            childNode33.InnerText = tb_xml41.Text;
            XmlNode childNode34 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
            childNode34.InnerText = tb_xml42.Text;
            XmlNode childNode35 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
            xml4result = int.Parse(ReadConfigINI("XML4", "result_label", sini));
            string xname = "rb_xml4" + (xml4result).ToString();
            RadioButton xb = (RadioButton)findControl(groupBox4, xname);
            CreateNode1(xmlDoc, childNode35, "label", xb.Text);
            childNode32.AppendChild(childNode33);
            childNode32.AppendChild(childNode34);
            childNode32.AppendChild(childNode35);
            childNode31.AppendChild(childNode32);
            //Root.AppendChild(childNode31);
            #endregion stations



            //int iOpera = -1;
            //string strOperaCol = tb_xml51.Text;
            ////INI文件中存操作组所在列名？？？？或列数？？？
            ////int OpeLine = ReadConfigINI();
            ////这是一种查找的方法
            //for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
            //{
            //    //if (dt.Rows[1][iCol].ToString() == "1" && dt.Rows[0][iCol].ToString() == strOperaCol)
            //    if (unit[iCol] == "1" && head[iCol].ToString() == strOperaCol)//Columns[iCol].ColumnName //假定操作组所在列的单位（第二行）的值为1以及（第一行）指定列名
            //    {
            //        iOpera = iCol;
            //        break;
            //    }
            //}
            //if (iOpera == -1)
            //{
            //    MessageBox.Show("无法找到操作组所在列，请重新检查待转换文件！");
            //    return;
            //}

            XmlNode childNode41 = xmlDoc.CreateNode(XmlNodeType.Element, "operations", null);
            //DataView view = new DataView(dt);                    //新一个表
            //DataTable dtOpera = view.ToTable(true, strOperaCol); //查出操作组各种类型
            //for (int i = 0; i < dtOpera.Rows.Count; i++)         //对于每种操作组类型
            //{
            //操作组部分                
            XmlNode childNode42 = xmlDoc.CreateNode(XmlNodeType.Element, "operation", null);
            XmlNode childNode43 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            XmlNode childNode44 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
            XmlNode childNode45 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
            XmlNode childNode46 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
            XmlNode childNode47 = xmlDoc.CreateNode(XmlNodeType.Element, "count", null);
            //XmlNode childNode48 = xmlDoc.CreateNode(XmlNodeType.Element, "version_name", null);
            //XmlNode childNode49 = xmlDoc.CreateNode(XmlNodeType.Element, "version_number", null);
            //XmlNode childNode50 = xmlDoc.CreateNode(XmlNodeType.Element, "version_date", null);

            //string strrrr = dtOpera.Rows[i][0].ToString();
            //DataRow[] rows = dt.Select(strOperaCol + "='" + dtOpera.Rows[i][0].ToString() + "'"); // 从dt 中查询符合条件该组操作的记录；
            //DataRow[] rows = dt.Select("Operation= 'group1'");
            operation_label = "Operation1";
            operation_description = "Operation";
            operation_result_label = "Pass";   //
            operation_result_id = "0";   //上次在重庆说Pass为0，Fail为正数，No op为-2，Unknow为-1或-3
            operation_order = "0";   //恒为0？？？
            operation_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //当前时间
            operation_count = "-1";  //设置为-1时：数据上传数据库自动累计计数。


            #region operation
            //创建子节点           
            childNode43.InnerText = operation_label;
            childNode44.InnerText = operation_description;
            //xml5result = int.Parse(ReadConfigINI("XML5", "result_label", sini));
            //string sname = "rb_xml5" + (xml5result).ToString();
            //RadioButton sb = (RadioButton)findControl(groupBox5, sname);
            CreateNode1(xmlDoc, childNode45, "label", operation_result_label);
            CreateNode1(xmlDoc, childNode45, "id", operation_result_id);
            childNode46.InnerText = operation_date_stamp;
            childNode47.InnerText = operation_count;
            //childNode48.InnerText = tb_xml55.Text;         
            //childNode49.InnerText = tb_xml56.Text;          
            //childNode50.InnerText = ReadConfigINI("XML5", "version_date", sini);
            childNode42.AppendChild(childNode43);
            childNode42.AppendChild(childNode44);
            childNode42.AppendChild(childNode45);
            childNode42.AppendChild(childNode46);
            childNode42.AppendChild(childNode47);
            //childNode42.AppendChild(childNode48);
            //childNode42.AppendChild(childNode49);
            //childNode42.AppendChild(childNode50);
            childNode41.AppendChild(childNode42);
            //Root.AppendChild(childNode41);
            #endregion operation

            XmlNode childNode61 = xmlDoc.CreateNode(XmlNodeType.Element, "waveforms", null);
            string XinTerval = GetInterval(nodes).ToString();
            int iwcout = 0;
            for (int idtc = 0; idtc < ValueisNum.Length; idtc++) //
            {
                if (ValueisNum[idtc])   //如果该列为数据
                {

                    #region waveforms
                    XmlNode childNode62 = xmlDoc.CreateNode(XmlNodeType.Element, "waveform", null);
                    XmlNode childNode63 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
                    XmlNode childNode666 = xmlDoc.CreateNode(XmlNodeType.Element, "description", null);
                    XmlNode childNode64 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);
                    XmlNode childNode669 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
                    XmlNode childNode65 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
                    XmlNode childNode66 = xmlDoc.CreateNode(XmlNodeType.Element, "x", null);
                    XmlNode childNode67 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                    XmlNode childNode68 = xmlDoc.CreateNode(XmlNodeType.Element, "format", null);
                    XmlNode childNode69 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
                    XmlNode childNode611 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);
                    XmlNode childNode612 = xmlDoc.CreateNode(XmlNodeType.Element, "y", null);
                    XmlNode childNode613 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                    XmlNode childNode614 = xmlDoc.CreateNode(XmlNodeType.Element, "format", null);
                    XmlNode childNode615 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
                    XmlNode childNode616 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);

                    iwcout++;
                    //波形部分
                    waveform_label = head[idtc].ToString();  //????  截取description中的前12个字符，如果名称不能重复，则先查询有无重复，如有重复，截取前10个字符，加上两位顺序编码
                    wavefrom_description = unit[idtc].ToString();
                    waveform_result_label = "Pass";   //
                    waveform_result_id = "0";
                    waveform_order = "0";   //恒为0？？？
                    waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //当前时间
                    waveform_x_type = "NULL";
                    waveform_x_format = "%g";
                    waveform_x_unit_label = "second";
                    waveform_x_reference = "-1";
                    waveform_y_type = "PortableMData";
                    waveform_y_format = "%g";
                    waveform_y_unit_label = unit[idtc].ToString();
                    waveform_y_reference = iwcout.ToString(); //如何在操作组中区分波形数？？？                                     
                    childNode63.InnerText = waveform_label;
                    childNode666.InnerText = wavefrom_description;
                    CreateNode1(xmlDoc, childNode64, "label", waveform_result_label);
                    CreateNode1(xmlDoc, childNode64, "id", waveform_result_id);
                    childNode669.InnerText = waveform_order;
                    childNode65.InnerText = waveform_date_stamp;
                    childNode67.InnerText = waveform_x_type;
                    childNode68.InnerText = waveform_x_format;
                    CreateNode1(xmlDoc, childNode69, "label", waveform_x_unit_label);
                    childNode611.InnerText = waveform_x_reference;
                    childNode66.AppendChild(childNode67);
                    childNode66.AppendChild(childNode68);
                    childNode66.AppendChild(childNode69);
                    childNode66.AppendChild(childNode611);
                    childNode613.InnerText = waveform_y_type;
                    childNode614.InnerText = waveform_y_format;
                    CreateNode1(xmlDoc, childNode615, "label", waveform_y_unit_label);
                    childNode616.InnerText = waveform_y_reference;
                    childNode612.AppendChild(childNode613);
                    childNode612.AppendChild(childNode614);
                    childNode612.AppendChild(childNode615);
                    childNode612.AppendChild(childNode616);

                    childNode62.AppendChild(childNode63);
                    childNode62.AppendChild(childNode666);
                    childNode62.AppendChild(childNode64);
                    childNode62.AppendChild(childNode669);
                    childNode62.AppendChild(childNode65);
                    childNode62.AppendChild(childNode66);
                    childNode62.AppendChild(childNode612);
                    childNode61.AppendChild(childNode62);

                    childNode42.AppendChild(childNode61);
                    #endregion waveforms

                    #region binary_catalog

                    XmlNode childNode101 = xmlDoc.CreateNode(XmlNodeType.Element, "item", null);
                    XmlNode childNode102 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
                    XmlNode childNode103 = xmlDoc.CreateNode(XmlNodeType.Element, "reference", null);
                    XmlNode childNode104 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
                    XmlNode childNode105 = xmlDoc.CreateNode(XmlNodeType.Element, "index", null);
                    XmlNode childNode106 = xmlDoc.CreateNode(XmlNodeType.Element, "size", null);
                    XmlNode childNode107 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);

                    //XmlNode childNode108 = xmlDoc.CreateNode(XmlNodeType.Element, "x_label", null);
                    //XmlNode childNode109 = xmlDoc.CreateNode(XmlNodeType.Element, "y_label", null);
                    //XmlNode childNode110 = xmlDoc.CreateNode(XmlNodeType.Element, "x_start", null);
                    //XmlNode childNode111 = xmlDoc.CreateNode(XmlNodeType.Element, "x_interval", null);                       


                    item_label = waveform_label;
                    item_reference = iwcout.ToString();
                    item_type = "NULL";
                    item_index = "0";
                    item_size = dt.Rows.Count.ToString();
                    item_data_x_label = "Time";
                    item_data_y_label = head[idtc];  //问清楚？？
                    item_data_x_start = "0";
                    item_data_x_interval = XinTerval; //间隔从表中读取
                    int iorder = 0;
                    DataTable datapoint = new DataTable();
                    datapoint.Columns.Add("item_data_datapoint_ydata", typeof(string));
                    datapoint.Columns.Add("item_data_datapoint_order", typeof(string));
                    DataRow ds;
                    for (int s = 0; s < dt.Rows.Count; s++)  // y值系列； 
                    {

                        iorder++;
                        ds = datapoint.NewRow();
                        item_data_datapoint_ydata = dt.Rows[s][idtc].ToString(); //该值放入datapoint
                        item_data_datapoint_order = iorder.ToString();
                        ds["item_data_datapoint_ydata"] = item_data_datapoint_ydata;
                        ds["item_data_datapoint_order"] = item_data_datapoint_order;
                        datapoint.Rows.Add(ds);
                    }

                    //for (int o = 0; o < dt.Rows.Count; o++)
                    //{
                    childNode102.InnerText = item_label;
                    childNode103.InnerText = item_reference;
                    childNode104.InnerText = item_type;
                    childNode105.InnerText = item_index;
                    childNode106.InnerText = item_size;
                    XmlNode childNode108 = xmlDoc.CreateNode(XmlNodeType.Element, "x_label", null);
                    XmlNode childNode109 = xmlDoc.CreateNode(XmlNodeType.Element, "y_label", null);
                    XmlNode childNode110 = xmlDoc.CreateNode(XmlNodeType.Element, "x_start", null);
                    XmlNode childNode111 = xmlDoc.CreateNode(XmlNodeType.Element, "x_interval", null);
                    childNode108.InnerText = item_data_x_label;
                    childNode109.InnerText = item_data_y_label;
                    childNode110.InnerText = item_data_x_start;
                    childNode111.InnerText = item_data_x_interval;
                    childNode107.AppendChild(childNode108);
                    childNode107.AppendChild(childNode109);
                    childNode107.AppendChild(childNode110);
                    childNode107.AppendChild(childNode111);
                    //循环会覆盖原结果

                    for (int b = 0; b < int.Parse(item_size); b++)
                    {
                        XmlNode childNode112 = xmlDoc.CreateNode(XmlNodeType.Element, "datapoint", null);
                        XmlNode childNode113 = xmlDoc.CreateNode(XmlNodeType.Element, "ydata", null);
                        XmlNode childNode114 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);

                        childNode113.InnerText = datapoint.Rows[b][0].ToString();
                        childNode114.InnerText = datapoint.Rows[b][1].ToString();
                        childNode112.AppendChild(childNode113);
                        childNode112.AppendChild(childNode114);

                        childNode107.AppendChild(childNode112);
                        //.InsertAfter(childNode112, null);

                    }

                    childNode101.AppendChild(childNode102);
                    childNode101.AppendChild(childNode103);
                    childNode101.AppendChild(childNode104);
                    childNode101.AppendChild(childNode105);
                    childNode101.AppendChild(childNode106);
                    childNode101.AppendChild(childNode107);
                    childNode1.AppendChild(childNode101);
                    //}

                    #endregion binary_catalog
                    //}

                }
            }



            ShowProgress(15, "XML文件Part,Section,Station和Group部分生成完毕");

            //特征值的增加，最难的一步
            #region feature
            //int drCount = dt.Rows.Count;
            //double intervel = 500.0;
            //double drCountSplit = drCount / intervel;

            //int grCount = (int)Math.Ceiling(drCountSplit);
            //int ii = 1;
            //for (int i = 0; i < drCount; i++)
            //{

            //    if (i == ii * intervel)
            //    {
            //        ShowProgress(20 + (int)(ii * 80.0 / grCount), "XML文件feature数据已生成" + (ii * intervel).ToString() + "/" + drCount.ToString() + "行");
            //        ii++;
            //    }
            //    for (int n = 2; n < head.Length; n++)
            //    {
            //        XmlNode childNode51 = xmlDoc.CreateNode(XmlNodeType.Element, "features", null);
            //        XmlNode childNode52 = xmlDoc.CreateNode(XmlNodeType.Element, "feature", null);
            //        XmlNode childNode53 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            //        //childNode53.InnerText = tb_xml71.Text;
            //        XmlNode childNode54 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);

            //        //不建议再去读文本，因此前面已经读过了，效率低。2018-11-11
            //        //xml5result = int.Parse(ReadConfigINI("XML7", "result_label", sini));
            //        string vname = "rb_xml7" + (xml5result).ToString();
            //        //不建议再去读控件，因此前面已经有过它的值了，效率低。2018-11-11
            //        RadioButton vb = (RadioButton)findControl(groupBox6, vname);
            //        CreateNode1(xmlDoc, childNode54, "label", vb.Text);
            //        CreateNode1(xmlDoc, childNode54, "id", tb_xml73.Text);
            //        //XmlNode childNode55 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
            //        XmlNode childNode56 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
            //        //string datachange = dt.Rows[i][0].ToString();
            //        //string[] datachange1 = datachange.Split('/');
            //        //childNode56.InnerText = datachange1[2] + "-" + datachange1[1] + "-" + datachange1[0] + "T" + dr.Rows[i][1].ToString();
            //        XmlNode childNode57 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);
            //        //此处的type好象不对，应该是调用GetDataType的返回值，来填入，2018-11-11   解决
            //        XmlNode childNode58 = xmlDoc.CreateNode(XmlNodeType.Element, "type", null);
            //        string type1 = GetDataType(head[n]);
            //        childNode58.InnerText = type1;
            //        XmlNode childNode59 = xmlDoc.CreateNode(XmlNodeType.Element, "unit", null);
            //        CreateNode1(xmlDoc, childNode59, "label", unit[n]);
            //        XmlNode childNode60 = xmlDoc.CreateNode(XmlNodeType.Element, "value", null);
            //        childNode60.InnerText = dt.Rows[i][n].ToString();
            //        //CreateNode1(xmlDoc, childNode57, "type",head[n].ToString());
            //        //CreateNode1(xmlDoc, childNode57, "unit", "<label>"+unit[n].ToString()+"</label>");
            //        //CreateNode1(xmlDoc,childNode57,"value",dr.Rows[i][n].ToString());
            //        XmlNode childNode61 = xmlDoc.CreateNode(XmlNodeType.Element, "analysis_region", null);
            //        CreateNode1(xmlDoc, childNode61, "type", "9030");
            //        CreateNode1(xmlDoc, childNode61, "is_y_data", "0");
            //        childNode57.AppendChild(childNode58);
            //        childNode57.AppendChild(childNode59);
            //        childNode57.AppendChild(childNode60);
            //        childNode52.AppendChild(childNode53);
            //        childNode52.AppendChild(childNode54);
            //        //order预留
            //        childNode52.AppendChild(childNode56);
            //        childNode52.AppendChild(childNode57);
            //        childNode52.AppendChild(childNode61);
            //        childNode51.AppendChild(childNode52);
            //       // Root.AppendChild(childNode51);
            //    }
            //}
            //ShowProgress(95, "XML文件feature数据已生成" + drCount.ToString() + "/" + drCount.ToString() + "行");
            #endregion feature


            #region structures
            //childNode42.AppendChild(childNode61);
            //childNode41.AppendChild(childNode42);
            childNode32.AppendChild(childNode41);
            childNode31.AppendChild(childNode32);
            childNode22.AppendChild(childNode31);
            childNode21.AppendChild(childNode22);
            childNode11.AppendChild(childNode12);
            childNode11.AppendChild(childNode21);
            Root.AppendChild(childNode11);
            Root.AppendChild(childNode1);
            #endregion structures

            #region save
            xmlDoc.AppendChild(Root);
            try
            {
                //路径从配置中来 2018-11-10
                string sfile = xmlpath + "\\" + partlabel + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".xml";
                if (File.Exists(sfile))
                {
                    MessageBox.Show(sfile + "文件已存在，请稍后再试！");
                    return;
                }
                xmlDoc.Save(sfile);//保存到指定位置
                Convertxmlfile = sfile;
                //MessageBox.Show("保存成功");
                ShowProgress(100, "文件转换完成,存为" + sfile + "\n@@@@@\n");
                string info = DateTime.Now.ToLocalTime().ToString() + "将" + waitConvertfile + "成功转为" + Convertxmlfile;
                WriteFile(Runlogpath, info);
            }
            catch (System.Exception ex)
            {
                string info = DateTime.Now.ToLocalTime().ToString() + "将" + waitConvertfile + "不能成功转为" + Convertxmlfile;
                WriteFile(Runlogpath, info);
                info = DateTime.Now.ToLocalTime().ToString() + "\n----" + ex.ToString() + "----\n\n";
                WriteFile(Errlogpath, info);
            }
            #endregion save
        }*/


        /// <summary>    
        /// 创建节点    
        /// </summary>    
        /// <param name="xmldoc"></param>  xml文档  
        /// <param name="parentnode"></param>父节点    
        /// <param name="name"></param>  节点名  
        /// <param name="value"></param>  节点值  
        ///   
        private void CreateNode1(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        //给配置文件添加路径
        private void btnPathSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            xmlpath = path.SelectedPath;
            tbXMLSavePath.Text = xmlpath;
        }



        //判断前五行
        private bool jundgeFive(string[] nodes)
        {
            bool result = false;
            string[] standard = nodes[1].Split('\t');
            if (nodes.Length >= 8)
            {
                string[] one = nodes[3].Split('\t');
                string[] two = nodes[4].Split('\t');
                string[] three = nodes[5].Split('\t');
                string[] four = nodes[6].Split('\t');
                string[] five = nodes[7].Split('\t');
                if (standard.Length == one.Length && standard.Length == two.Length && standard.Length == three.Length && standard.Length == four.Length && standard.Length == five.Length)
                {
                    result = true;
                }
            }
            else if (nodes.Length >= 4 || nodes.Length < 8)
            {
                if (nodes.Length == 4)
                {
                    string[] one = nodes[3].Split('\t');
                    if (standard.Length == one.Length) { result = true; }
                }
                else if (nodes.Length == 5)
                {
                    string[] one = nodes[3].Split('\t');
                    string[] two = nodes[4].Split('\t');
                    if (standard.Length == one.Length && standard.Length == two.Length) { result = true; }
                }
                else if (nodes.Length == 6)
                {
                    string[] one = nodes[3].Split('\t');
                    string[] two = nodes[4].Split('\t');
                    string[] three = nodes[5].Split('\t');
                    if (standard.Length == one.Length && standard.Length == two.Length && standard.Length == three.Length) { result = true; }
                }
                else
                {
                    string[] one = nodes[3].Split('\t');
                    string[] two = nodes[4].Split('\t');
                    string[] three = nodes[5].Split('\t');
                    string[] four = nodes[6].Split('\t');
                    if (standard.Length == one.Length && standard.Length == two.Length && standard.Length == three.Length && standard.Length == four.Length) { result = true; }
                }
            }
            else
            {

            }
            return result;

        }

        //判断时间间隔
        private int GetInterval(string[] nodes)
        {
            //取四组数据
            string[] First = nodes[3].Split('\t');
            string[] Second = nodes[4].Split('\t');
            string[] Third = nodes[5].Split('\t');
            string[] Forth = nodes[6].Split('\t');
            //获得小数点后面的数字
            string[] FirstSecond = First[1].Split(':');
            string[] SecondSecond = Second[1].Split(':');
            string[] ThirdSecond = Third[1].Split(':');
            string[] ForthSecond = Forth[1].Split(':');
            var FirstTime = int.Parse(FirstSecond[2].ToString());
            var SecondTime = int.Parse(SecondSecond[2].ToString());
            var ThirdTime = int.Parse(ThirdSecond[2].ToString());
            var ForthTime = int.Parse(ForthSecond[2].ToString());
            //可能出现后项比前项小
            var FirstInterval = Math.Abs(SecondTime - FirstTime);
            var SecondInterval = Math.Abs(ThirdTime - SecondTime);
            var ThirdInterval = Math.Abs(ForthTime - ThirdTime);
            if (FirstInterval==SecondInterval)
            {
                return FirstInterval;
            }
            else if (FirstInterval==ThirdInterval)
            {
                return FirstInterval;
            }
            else if (SecondInterval==ThirdInterval)
            {
                return SecondInterval;
            }
            else
            {
                return 1;
            }
        }

        #region 文件夹监听

        string _watcherDir = "d:\\data\\watcher";
        string _processDir = "d:\\data\\process";
        string _fileNumber = "d:\\data\\watcher\\fileNumber.txt";
        //文件夹监控
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void CheckChanged()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = _watcherDir;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                    | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //只看其中文本文件的变化
            watcher.Filter = "*.txt";
            watcher.EnableRaisingEvents = true;

            //注册事件
            watcher.Created += new FileSystemEventHandler(OnCreated);
        }

        public void OnCreated(object source, FileSystemEventArgs e)
        {
            //查找fileNumber.txt是否存在
            if (!File.Exists(_fileNumber))
            {
                return;
            }
            //获取文件数量
            var fileNumberStr = getString(_fileNumber, Encoding.UTF8);
            var fileNumber = 0;
            if (string.IsNullOrEmpty(fileNumberStr)
                || !int.TryParse(fileNumberStr, out fileNumber))
            {
                return;
            }
            //未粘贴完毕，等待粘贴完毕
            var txtFiles = Directory.GetFiles(_watcherDir, "*.txt");
            if (txtFiles.Count() != fileNumber + 1)
            {
                return;
            }
            //粘贴完毕
            File.Delete(_fileNumber);
            var processDir = _processDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff");
            Directory.CreateDirectory(processDir);
            foreach (var item in txtFiles)
            {
                if (string.Equals(item, _fileNumber, StringComparison.OrdinalIgnoreCase)) continue;
                File.Move(item, processDir + "\\" + Path.GetFileName(item));
            }
            var files = Directory.GetFiles(processDir);
            ThreadPool.QueueUserWorkItem(obj =>
            {
                new Form1((string[])obj).Start();
            }, files);
        }

        private string getString(string file, Encoding encoding)
        {
            string str = null;
            using (var reader = new StreamReader(file, encoding))
            {
                str = reader.ReadToEnd();
            }
            return str;
        }
        #endregion
        public  Form1(string[] txtFiles) 
        {
            _txtFiles = txtFiles;
        }
        public void Start()
        {
            //3.循环文本文件，把当前文件中的文本读取到datatable中
            _txtDt = new DataTable();
            foreach (var txtFile in _txtFiles)
            {
                using (var reader = new StreamReader(txtFile, Encoding.UTF8))
                {
                    var txtStr = reader.ReadToEnd();
                    //转换字符串到datatable
                    AddDataToTxtDt(txtStr);
                    //不然会报reader已经打开的错误
                    reader.Close();
                }
            }
            int HalfRowCount = _txtDt.Rows.Count / 2; //取中间那一行来做判断
            ValueisNum = new bool[_txtDt.Columns.Count];  //判断行的信息是否为数值型（整型或浮点型）
            for (int iCol = 0; iCol < _txtDt.Columns.Count; iCol++)
            {
                string stttr = _txtDt.Rows[HalfRowCount][iCol].ToString();
                Regex regex = new Regex(@"^\d+$");
                Regex reg = new Regex(@"^\d+\.\d+$");
                if (regex.IsMatch(stttr) || reg.IsMatch(stttr))
                    ValueisNum[iCol] = true;        //整数或小数
                else
                    ValueisNum[iCol] = false;      //字符（应该为常量或操作组变量）如果后面需要查询名称的重复性，此处需要保存下来，先过滤一次？？？？                             
            }
            SaveXml();
        }
    }
}