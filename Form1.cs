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
        //�ļ�����ע��
        static FileSystemWatcher watcher = new FileSystemWatcher();

        //����INI�ļ���д��������WritePrivateProfileString()
        [DllImport("kernel32")]
        //section=���ýڣ�key=������value=��ֵ��path=·��
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //����INI�ļ��Ķ���������GetPrivateProfileString()
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        #region INI �ļ���д
        public bool WriteConfigIni(string keyvalue, string variablename, string blockname, string configini)
        {
            bool result = false;
            //д��INI �ļ� 
            WritePrivateProfileString(blockname, variablename, keyvalue, configini); //
            result = true;
            return result;
        }

        //����
        public static string MD5Create(string STR) //STRΪ�����ܵ�string
        {
            string pwd = "";
            //pwdΪ���ܽ��
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(STR));
            //�����UTF8�Ǳ��뷽ʽ������Բ�����ϲ���ķ�ʽ���У�����UNcode�ȵ�
            for (int i = 0; i < s.Length; i++)
            {
                pwd = pwd + s[i].ToString();
            }
            return pwd;
        }

        //��INI�ļ�
        public static string ReadConfigINI(string blockname, string variablename, string configini)
        {
            string inivalue = "";


            //��INI �ļ�
            StringBuilder temp1 = new StringBuilder(255);
            int x = Form1.GetPrivateProfileString(blockname, variablename, "", temp1, 255, configini);
            inivalue = temp1.ToString();
            return inivalue;
        }

        //д���ı��ļ�
        public static void WriteFile(String path, String info)
        {
            FileInfo finfo = new FileInfo(path);

            /**/
            ///�ж��ļ��Ƿ�����Լ��Ƿ����10M
            if (finfo.Exists && finfo.Length > 10 * 1024 * 1024)
            {
                /**/
                ///ɾ�����ļ�
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

        #region ��winform�в��ҿؼ� -------------------------------------------------------------
        /// <summary>
        /// ��winform�в��ҿؼ�
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

        //��
        public void LockMac()
        {
            //��
            string[] str = GetMoc();
            string s = Application.StartupPath + "\\config.ini";
            if (System.IO.File.Exists(s))
            {
            }
            else
            {
                MessageBox.Show("������Ҫ�ļ������ڣ�����ϵ���򿪷���Ա��");
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
        /// �޸ĳ�����ע����еļ�ֵ,ʵ�ֿ���������
        /// </summary>  
        /// <param name="isAuto">true:��������,false:����������</param> 
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
                    R_run.DeleteValue("Ӧ������", false);
                    R_run.Close();
                    R_local.Close();
                }

                //GlobalVariant.Instance.UserConfig.AutoStart = isAuto;
            }
            catch (Exception)
            {
                //MessageBoxDlg dlg = new MessageBoxDlg();
                //dlg.InitialData("����Ҫ����ԱȨ���޸�", "��ʾ", MessageBoxButtons.OK, MessageBoxDlgIcon.Error);
                //dlg.ShowDialog();
                MessageBox.Show("����Ҫ����ԱȨ���޸�", "��ʾ");
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

            //���治��Ҫ�ˣ���
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
            
            //��ʱ������
            //System.Timers.Timer t = new System.Timers.Timer();//ʵ����Timer��
            //int intTime = 5000;
            //t.Interval = intTime;//���ü��ʱ�䣬Ϊ���룻
            //t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//����ʱ���ʱ��ִ���¼���
            //t.AutoReset = false;//������ִ��һ�Σ�false������һֱִ��(true)��
            //t.Enabled = true;//�Ƿ�ִ��System.Timers.Timer.Elapsed�¼���
        }
        #endregion readINI

        //�����ļ�
        private void btn_save_Click(object sender, EventArgs e)
        {
            #region XML1
            WriteConfigIni(tb_xml11.Text, "root_version", "XML1", sini);
            WriteConfigIni(tb_xml12.Text, "root_xmlns", "XML1", sini);
            #endregion XML1
            #region XML2
            //���ж��ַ������Ƿ����Ҫ��
            int strcount;
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml21.Text);
            if (strcount > 12)
            {
                MessageBox.Show("part_label����12�����������룡");
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
                MessageBox.Show("section_label����12�����������룡");
                return;
            }
            WriteConfigIni(tb_xml31.Text, "section_label", "XML3", sini);

            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml32.Text);
            if (strcount > 32)
            {
                MessageBox.Show("section description����32�����������룡");
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
            //�ж��ֽڳ����Ƿ����Ҫ��
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml41.Text);
            if (strcount > 12)
            {
                MessageBox.Show("station_label����12�����������룡");
                return;
            }
            WriteConfigIni(tb_xml41.Text, "station_label", "XML4", sini);
            strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml42.Text);
            if (strcount > 32)
            {
                MessageBox.Show("station description����32�����������룡");
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
            //�ж��ֽڳ����Ƿ����Ҫ��
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml51.Text);
            //if (strcount > 12)
            //{
            //    MessageBox.Show("operations_label����12�����������룡");
            //    return;
            //}
            WriteConfigIni(tb_xml51.Text, "operations_Name", "XML5", sini);
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml52.Text);
            //if (strcount > 32)
            //{
            //    MessageBox.Show("operations_description����32�����������룡");
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
            //    MessageBox.Show("version_name����12�����������룡");
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
            //    MessageBox.Show("version_numberӦ�ô����㣡");
            //}
            #endregion XML5
            #region XML7
            ////�ж��ֽڳ����Ƿ����Ҫ��
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml71.Text);
            //if (strcount > 12)
            //{
            //    MessageBox.Show("feature_label����12�����������룡");
            //    return;
            //}
            //WriteConfigIni(tb_xml71.Text, "feature_label", "XML7", sini);
            //strcount = System.Text.Encoding.GetEncoding("UTF-8").GetByteCount(tb_xml72.Text);
            //if (strcount > 32)
            //{
            //    MessageBox.Show("feature_description����32�����������룡");
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

            //����XML�ļ�����·����2018-11-10
            WriteConfigIni(tbXMLSavePath.Text, "path", "XML8", sini);

            string info = DateTime.Now.ToLocalTime().ToString() + "\n----�������óɹ�----\n\n";
            WriteFile(Runlogpath, info);
            MessageBox.Show("Done!");
        }
        //����TXT�ı���ת��Ϊdatatable
        string txtContent = string.Empty;
        string waitConvertfile, Convertxmlfile;
        bool[] ValueisNum;  //���ݱ��еĸ���ֵ�����ֻ����ַ�

        #region ��ʼ����Ϣ
        //single_part����
        //part
        string part_label = "Engine";
        string part_serial_number = "E4T15B";
        string part_model_number_label = "E4T15B";
        string Operationer = "Ss";
        string Diesel_grade = "20181211";


        //���������һ����ע��Ϣ�������ƺţ�����Ա����Ϣ�ȷŵ�����������INI�ļ���
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
        string operation_result_id = "0";   //�ϴ�������˵PassΪ0��FailΪ������No opΪ-2��UnknowΪ-1��-3
        string operation_order = "0";   //��Ϊ0������
        string operation_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //��ǰʱ��
        string operation_count = "-1";  //��Ϊ��1
        //waveform
        string waveform_label = "1st_Current";  //????
        string wavefrom_description = "1Ignition Current Signal#r0";
        string waveform_result_label = "Pass";   //
        string waveform_result_id = "0";
        string waveform_order = "0";   //��Ϊ0������
        string waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //��ǰʱ��
        string waveform_x_type = "NULL";
        string waveform_x_format = "%g";
        string waveform_x_unit_label = "ms";
        string waveform_x_reference = "-1";
        string waveform_y_type = "NULL";
        string waveform_y_format = "%g";
        string waveform_y_unit_label = "A";
        string waveform_y_reference = "1"; //һ���������һ����������һ������
        //feature
        string feature_label = "CraceNo_1";  //????
        string feature_description = "Crace Numbers CYL#1";
        string feature_result_label = "Pass";   //
        string feature_result_id = "0";
        string feature_order = "0";   //��Ϊ0������
        string feature_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH-mm-ss");  //��ǰʱ��
        string feature_data_type = "float";
        string feature_data_format = "%g";
        string feature_data_unit_label = "A";
        string feature_data_value = "50";
        string feature_analysis_region_type = "9008";
        string feature_analysis_region_is_y_data = "0";

        //binary_catalog����
        string item_label = "1st_Current";
        string item_reference = "1";
        string item_type = "NULL";
        string item_index = "0";
        string item_size = "1";
        string item_data_x_label = "Time";
        string item_data_y_label = "Current";  //���������
        string item_data_x_start = "0";
        string item_data_x_interval = "0.01"; //���������
        string item_data_datapoint_ydata = "0.574791"; //��ֵ����datapoint
        string item_data_datapoint_order = "1";
        #endregion ��ʼ����Ϣ
        string[] _txtFiles = null;
        DataTable _txtDt = null;//��ʼ��һ��datatable
        string[] _head = null;
        string[] _unit = null;
        string _station = null;//������
        string _MacSN = null;//���к�
        string _MacNum = null;//���ͺ�
        string _interval = null;//���
        //��ť����¼�
        private void btn_insert_Click(object sender, EventArgs e)
        {
            ////�ж�·���Ƿ����
            //if (!Directory.Exists(xmlpath))
            //{
            //    MessageBox.Show("�������õ�·�������ڣ����������ã�");
            //    return;
            //}
            ////1.��ȡ�ı��ļ����ڵ�Ŀ¼
            //string directory = "D:/Data/data";
            ////2.��ȡĿ¼�е������ı��ļ�
            //var txtFiles = Directory.GetFiles(directory, "*.txt");
            ////3.ѭ���ı��ļ����ѵ�ǰ�ļ��е��ı���ȡ��datatable��
            //_txtDt = new DataTable();
            //foreach (var txtFile in txtFiles)
            //{
            //    using (var reader = new StreamReader(txtFile, Encoding.UTF8))
            //    {
            //        var txtStr = reader.ReadToEnd();
            //        //ת���ַ�����datatable
            //        AddDataToTxtDt(txtStr);
            //    }
            //}
            //ShowProgress(10, "�ļ����ݶ�����ɣ���������XML�ļ�...");
            //SaveXml();//���ɲ�����                            
            //btn_insert.Enabled = true;
            //btn_save.Enabled = true;
            ////timer1.Stop();//��ʱ���ر�                                              
        }
                    
        //���TXT�ļ���ȡ                        
        private void AddDataToTxtDt(string txtStr)
        {
            //��ť���ã���ת�������ã���������ʾ   2018-11-10
            //btn_insert.Enabled = false;
            //btn_save.Enabled = false;
            //ShowProgress(1, "��׼������ļ���ʽ...");

            txtStr = txtStr.Replace("\r\n", "@");
            var nodes = txtStr.Split('@');
            var interval = GetInterval(nodes).ToString();
            _interval = interval;
            var head = nodes[1].Split('\t');
            var unit = nodes[2].Split('\t');
            //todo:��֤����(��ͷ����λ)

            //ShowProgress(5, "�ļ���ʽ�����ɣ����ڶ����ļ�����...");

            //������ͷ
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
            //ȥ�������е��ͺţ���ͷ����λ
            List<string> lineList = nodes.ToList();
            lineList.RemoveRange(0, 3);
            //var line1Datas = lineList[0].Split('\t');
            //var line1Time = Convert.ToDateTime(line1Datas[0]+" "+line1Datas[1]);
            //var line2Datas = lineList[1].Split('\t');
            //var line2Time = Convert.ToDateTime(line2Datas[0]+" "+line2Datas[1]);
            //var interval = (line2Time - line1Time).TotalSeconds.ToString();
           
            //���������
            for (int i = 0; i < lineList.Count; i++)
            {
                var line = lineList[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] str = line.Split('\t');
                //todo:������֤(����)
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
        //��ʾ��չ
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
        
   
        //���湹��xml
        public void SaveXml()
        {
            //ʵ����һ������
            XmlDocument xmlDoc = new XmlDocument();
            //�������������ڵ�  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "", "");
            xmlDoc.AppendChild(node);
            //�������ڵ�
            XmlNode Root = xmlDoc.CreateElement("root");//���ڵ�������ռ� xmlns��ô���
            XmlAttribute a = xmlDoc.CreateAttribute("xmlns");
            a.Value = "http://www.sciemetric.com/namespace";
            Root.Attributes.Append(a);
            //part�е�label��serial_numberΪʲô���һ��//�����
            #region part
            //�����ӽڵ�
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
            //�����ӽڵ�
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
            //�����ӽڵ�
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
            //DataView view = new DataView(dt);                    //��һ����
            //DataTable dtOpera = view.ToTable(true, strOperaCol); //����������������
            //for (int i = 0; i < dtOpera.Rows.Count; i++)         //����ÿ�ֲ���������
            //{
                //�����鲿��                
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
                operation_result_id = "0";   //�ϴ�������˵PassΪ0��FailΪ������No opΪ-2��UnknowΪ-1��-3
                operation_order = "0";   //��Ϊ0������
                DataRow dtr=_txtDt.Rows[_txtDt.Rows.Count-1];
                string[] A = dtr[0].ToString().Replace("/", "-").Split('-');
                string B = dtr[1].ToString().Replace(":", "-");
                operation_date_stamp = A[2]+"-"+A[1]+"-"+A[0] + "T" + B;  //���һ�����ݵĲ���ʱ��
                operation_count = "-1";  //����Ϊ-1ʱ�������ϴ����ݿ��Զ��ۼƼ�����


                #region operation
                //�����ӽڵ�           
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
                    if (ValueisNum[idtc])   //�������Ϊ����
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
                        //���β���
                        if (_head[idtc].Length > 10)//��ȡdescription�е�ǰ12���ַ���������Ʋ����ظ������Ȳ�ѯ�����ظ��������ظ�����ȡǰ10���ַ���������λ˳�����
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
                        waveform_order = "0";   //��Ϊ0������
                        //string[] A = dtr[0].ToString().Replace("/", "-").Split('-');
                        //string B = dtr[1].ToString().Replace(":", "-");
                        //operation_date_stamp = A[2] + "-" + A[1] + "-" + A[0] + "T" + B;
                        waveform_date_stamp = A[2] + "-" + A[1] + "-" + A[0] + "T" + B;
                        //waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //todo:����ʱ��
                        waveform_x_type = "NULL";
                        waveform_x_format = "%g";
                        waveform_x_unit_label = "second";
                        waveform_x_reference = "-1";
                        waveform_y_type = "PortableMData";
                        waveform_y_format = "%g";
                        waveform_y_unit_label = _unit[idtc].ToString();
                        waveform_y_reference = iwcout.ToString(); //����ڲ����������ֲ�����������                                     
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
                        item_data_y_label = _head[idtc];  //���������
                        item_data_x_start = "0";
                        item_data_x_interval = _interval; //������̶�
                        int iorder = 0;
                        DataTable datapoint = new DataTable();
                        datapoint.Columns.Add("item_data_datapoint_ydata", typeof(string));
                        datapoint.Columns.Add("item_data_datapoint_order", typeof(string));
                        DataRow ds;
                        for (int s = 0; s < _txtDt.Rows.Count; s++)  // yֵϵ�У� 
                        {

                            iorder++;
                            ds = datapoint.NewRow();
                            item_data_datapoint_ydata = _txtDt.Rows[s][idtc].ToString(); //��ֵ����datapoint
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
                        //ѭ���Ḳ��ԭ���

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



            //ShowProgress(15, "XML�ļ�Part,Section,Station��Group�����������");
            
            //����ֵ�����ӣ����ѵ�һ��
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
            //        ShowProgress(20 + (int)(ii * 80.0 / grCount), "XML�ļ�feature����������" + (ii * intervel).ToString() + "/" + drCount.ToString() + "��");
            //        ii++;
            //    }
            //    for (int n = 2; n < head.Length; n++)
            //    {
            //        XmlNode childNode51 = xmlDoc.CreateNode(XmlNodeType.Element, "features", null);
            //        XmlNode childNode52 = xmlDoc.CreateNode(XmlNodeType.Element, "feature", null);
            //        XmlNode childNode53 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            //        //childNode53.InnerText = tb_xml71.Text;
            //        XmlNode childNode54 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);

            //        //��������ȥ���ı������ǰ���Ѿ������ˣ�Ч�ʵ͡�2018-11-11
            //        //xml5result = int.Parse(ReadConfigINI("XML7", "result_label", sini));
            //        string vname = "rb_xml7" + (xml5result).ToString();
            //        //��������ȥ���ؼ������ǰ���Ѿ��й�����ֵ�ˣ�Ч�ʵ͡�2018-11-11
            //        RadioButton vb = (RadioButton)findControl(groupBox6, vname);
            //        CreateNode1(xmlDoc, childNode54, "label", vb.Text);
            //        CreateNode1(xmlDoc, childNode54, "id", tb_xml73.Text);
            //        //XmlNode childNode55 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
            //        XmlNode childNode56 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
            //        //string datachange = dt.Rows[i][0].ToString();
            //        //string[] datachange1 = datachange.Split('/');
            //        //childNode56.InnerText = datachange1[2] + "-" + datachange1[1] + "-" + datachange1[0] + "T" + dr.Rows[i][1].ToString();
            //        XmlNode childNode57 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);
            //        //�˴���type���󲻶ԣ�Ӧ���ǵ���GetDataType�ķ���ֵ�������룬2018-11-11   ���
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
            //        //orderԤ��
            //        childNode52.AppendChild(childNode56);
            //        childNode52.AppendChild(childNode57);
            //        childNode52.AppendChild(childNode61);
            //        childNode51.AppendChild(childNode52);
            //       // Root.AppendChild(childNode51);
            //    }
            //}
            //ShowProgress(95, "XML�ļ�feature����������" + drCount.ToString() + "/" + drCount.ToString() + "��");
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
                //·������������ 2018-11-10
                string sfile =" C:\\Users\\Zhang Shuo\\Desktop "+ "\\" + _MacSN + "+"+DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".xml";
                if (File.Exists(sfile))
                {
                    MessageBox.Show(sfile + "�ļ��Ѵ��ڣ����Ժ����ԣ�");
                    return;
                }
                xmlDoc.Save(sfile);//���浽ָ��λ��
                Convertxmlfile = sfile;
                //MessageBox.Show("����ɹ�");
                //ShowProgress(100, "�ļ�ת�����,��Ϊ" + sfile + "\n@@@@@\n");
                string info = DateTime.Now.ToLocalTime().ToString() + "��" + waitConvertfile + "�ɹ�תΪ" + Convertxmlfile;
                WriteFile(Runlogpath, info);
            }
            catch (System.Exception ex)
            {
                string info = DateTime.Now.ToLocalTime().ToString() + "��" + waitConvertfile + "���ܳɹ�תΪ" + Convertxmlfile;
                WriteFile(Runlogpath, info);
                info = DateTime.Now.ToLocalTime().ToString() + "\n----" + ex.ToString() + "----\n\n";
                WriteFile(Errlogpath, info);
            }
            #endregion save
        }

        //���湹��xml
        /*public void SaveXml(DataTable dt, string[] head, string[] unit, string[] nodes)
        {
            //ʵ����һ������
            XmlDocument xmlDoc = new XmlDocument();
            //�������������ڵ�  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "", "");
            xmlDoc.AppendChild(node);
            //�������ڵ�
            XmlNode Root = xmlDoc.CreateElement("root");//���ڵ�������ռ� xmlns��ô���
            XmlAttribute a = xmlDoc.CreateAttribute("xmlns");
            a.Value = "http://www.sciemetric.com/namespace";
            Root.Attributes.Append(a);
            //part�е�label��serial_numberΪʲô���һ��//�����
            #region part
            //�����ӽڵ�
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
            //�����ӽڵ�
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
            //�����ӽڵ�
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
            ////INI�ļ��д����������������������������������
            ////int OpeLine = ReadConfigINI();
            ////����һ�ֲ��ҵķ���
            //for (int iCol = 0; iCol < dt.Columns.Count; iCol++)
            //{
            //    //if (dt.Rows[1][iCol].ToString() == "1" && dt.Rows[0][iCol].ToString() == strOperaCol)
            //    if (unit[iCol] == "1" && head[iCol].ToString() == strOperaCol)//Columns[iCol].ColumnName //�ٶ������������еĵ�λ���ڶ��У���ֵΪ1�Լ�����һ�У�ָ������
            //    {
            //        iOpera = iCol;
            //        break;
            //    }
            //}
            //if (iOpera == -1)
            //{
            //    MessageBox.Show("�޷��ҵ������������У������¼���ת���ļ���");
            //    return;
            //}

            XmlNode childNode41 = xmlDoc.CreateNode(XmlNodeType.Element, "operations", null);
            //DataView view = new DataView(dt);                    //��һ����
            //DataTable dtOpera = view.ToTable(true, strOperaCol); //����������������
            //for (int i = 0; i < dtOpera.Rows.Count; i++)         //����ÿ�ֲ���������
            //{
            //�����鲿��                
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
            //DataRow[] rows = dt.Select(strOperaCol + "='" + dtOpera.Rows[i][0].ToString() + "'"); // ��dt �в�ѯ����������������ļ�¼��
            //DataRow[] rows = dt.Select("Operation= 'group1'");
            operation_label = "Operation1";
            operation_description = "Operation";
            operation_result_label = "Pass";   //
            operation_result_id = "0";   //�ϴ�������˵PassΪ0��FailΪ������No opΪ-2��UnknowΪ-1��-3
            operation_order = "0";   //��Ϊ0������
            operation_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //��ǰʱ��
            operation_count = "-1";  //����Ϊ-1ʱ�������ϴ����ݿ��Զ��ۼƼ�����


            #region operation
            //�����ӽڵ�           
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
                if (ValueisNum[idtc])   //�������Ϊ����
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
                    //���β���
                    waveform_label = head[idtc].ToString();  //????  ��ȡdescription�е�ǰ12���ַ���������Ʋ����ظ������Ȳ�ѯ�����ظ��������ظ�����ȡǰ10���ַ���������λ˳�����
                    wavefrom_description = unit[idtc].ToString();
                    waveform_result_label = "Pass";   //
                    waveform_result_id = "0";
                    waveform_order = "0";   //��Ϊ0������
                    waveform_date_stamp = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToLocalTime().ToString("HH:mm:ss");  //��ǰʱ��
                    waveform_x_type = "NULL";
                    waveform_x_format = "%g";
                    waveform_x_unit_label = "second";
                    waveform_x_reference = "-1";
                    waveform_y_type = "PortableMData";
                    waveform_y_format = "%g";
                    waveform_y_unit_label = unit[idtc].ToString();
                    waveform_y_reference = iwcout.ToString(); //����ڲ����������ֲ�����������                                     
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
                    item_data_y_label = head[idtc];  //���������
                    item_data_x_start = "0";
                    item_data_x_interval = XinTerval; //����ӱ��ж�ȡ
                    int iorder = 0;
                    DataTable datapoint = new DataTable();
                    datapoint.Columns.Add("item_data_datapoint_ydata", typeof(string));
                    datapoint.Columns.Add("item_data_datapoint_order", typeof(string));
                    DataRow ds;
                    for (int s = 0; s < dt.Rows.Count; s++)  // yֵϵ�У� 
                    {

                        iorder++;
                        ds = datapoint.NewRow();
                        item_data_datapoint_ydata = dt.Rows[s][idtc].ToString(); //��ֵ����datapoint
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
                    //ѭ���Ḳ��ԭ���

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



            ShowProgress(15, "XML�ļ�Part,Section,Station��Group�����������");

            //����ֵ�����ӣ����ѵ�һ��
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
            //        ShowProgress(20 + (int)(ii * 80.0 / grCount), "XML�ļ�feature����������" + (ii * intervel).ToString() + "/" + drCount.ToString() + "��");
            //        ii++;
            //    }
            //    for (int n = 2; n < head.Length; n++)
            //    {
            //        XmlNode childNode51 = xmlDoc.CreateNode(XmlNodeType.Element, "features", null);
            //        XmlNode childNode52 = xmlDoc.CreateNode(XmlNodeType.Element, "feature", null);
            //        XmlNode childNode53 = xmlDoc.CreateNode(XmlNodeType.Element, "label", null);
            //        //childNode53.InnerText = tb_xml71.Text;
            //        XmlNode childNode54 = xmlDoc.CreateNode(XmlNodeType.Element, "result", null);

            //        //��������ȥ���ı������ǰ���Ѿ������ˣ�Ч�ʵ͡�2018-11-11
            //        //xml5result = int.Parse(ReadConfigINI("XML7", "result_label", sini));
            //        string vname = "rb_xml7" + (xml5result).ToString();
            //        //��������ȥ���ؼ������ǰ���Ѿ��й�����ֵ�ˣ�Ч�ʵ͡�2018-11-11
            //        RadioButton vb = (RadioButton)findControl(groupBox6, vname);
            //        CreateNode1(xmlDoc, childNode54, "label", vb.Text);
            //        CreateNode1(xmlDoc, childNode54, "id", tb_xml73.Text);
            //        //XmlNode childNode55 = xmlDoc.CreateNode(XmlNodeType.Element, "order", null);
            //        XmlNode childNode56 = xmlDoc.CreateNode(XmlNodeType.Element, "date_stamp", null);
            //        //string datachange = dt.Rows[i][0].ToString();
            //        //string[] datachange1 = datachange.Split('/');
            //        //childNode56.InnerText = datachange1[2] + "-" + datachange1[1] + "-" + datachange1[0] + "T" + dr.Rows[i][1].ToString();
            //        XmlNode childNode57 = xmlDoc.CreateNode(XmlNodeType.Element, "data", null);
            //        //�˴���type���󲻶ԣ�Ӧ���ǵ���GetDataType�ķ���ֵ�������룬2018-11-11   ���
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
            //        //orderԤ��
            //        childNode52.AppendChild(childNode56);
            //        childNode52.AppendChild(childNode57);
            //        childNode52.AppendChild(childNode61);
            //        childNode51.AppendChild(childNode52);
            //       // Root.AppendChild(childNode51);
            //    }
            //}
            //ShowProgress(95, "XML�ļ�feature����������" + drCount.ToString() + "/" + drCount.ToString() + "��");
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
                //·������������ 2018-11-10
                string sfile = xmlpath + "\\" + partlabel + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".xml";
                if (File.Exists(sfile))
                {
                    MessageBox.Show(sfile + "�ļ��Ѵ��ڣ����Ժ����ԣ�");
                    return;
                }
                xmlDoc.Save(sfile);//���浽ָ��λ��
                Convertxmlfile = sfile;
                //MessageBox.Show("����ɹ�");
                ShowProgress(100, "�ļ�ת�����,��Ϊ" + sfile + "\n@@@@@\n");
                string info = DateTime.Now.ToLocalTime().ToString() + "��" + waitConvertfile + "�ɹ�תΪ" + Convertxmlfile;
                WriteFile(Runlogpath, info);
            }
            catch (System.Exception ex)
            {
                string info = DateTime.Now.ToLocalTime().ToString() + "��" + waitConvertfile + "���ܳɹ�תΪ" + Convertxmlfile;
                WriteFile(Runlogpath, info);
                info = DateTime.Now.ToLocalTime().ToString() + "\n----" + ex.ToString() + "----\n\n";
                WriteFile(Errlogpath, info);
            }
            #endregion save
        }*/


        /// <summary>    
        /// �����ڵ�    
        /// </summary>    
        /// <param name="xmldoc"></param>  xml�ĵ�  
        /// <param name="parentnode"></param>���ڵ�    
        /// <param name="name"></param>  �ڵ���  
        /// <param name="value"></param>  �ڵ�ֵ  
        ///   
        private void CreateNode1(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        //�������ļ����·��
        private void btnPathSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            xmlpath = path.SelectedPath;
            tbXMLSavePath.Text = xmlpath;
        }



        //�ж�ǰ����
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

        //�ж�ʱ����
        private int GetInterval(string[] nodes)
        {
            //ȡ��������
            string[] First = nodes[3].Split('\t');
            string[] Second = nodes[4].Split('\t');
            string[] Third = nodes[5].Split('\t');
            string[] Forth = nodes[6].Split('\t');
            //���С������������
            string[] FirstSecond = First[1].Split(':');
            string[] SecondSecond = Second[1].Split(':');
            string[] ThirdSecond = Third[1].Split(':');
            string[] ForthSecond = Forth[1].Split(':');
            var FirstTime = int.Parse(FirstSecond[2].ToString());
            var SecondTime = int.Parse(SecondSecond[2].ToString());
            var ThirdTime = int.Parse(ThirdSecond[2].ToString());
            var ForthTime = int.Parse(ForthSecond[2].ToString());
            //���ܳ��ֺ����ǰ��С
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

        #region �ļ��м���

        string _watcherDir = "d:\\data\\watcher";
        string _processDir = "d:\\data\\process";
        string _fileNumber = "d:\\data\\watcher\\fileNumber.txt";
        //�ļ��м��
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void CheckChanged()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = _watcherDir;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                    | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //ֻ�������ı��ļ��ı仯
            watcher.Filter = "*.txt";
            watcher.EnableRaisingEvents = true;

            //ע���¼�
            watcher.Created += new FileSystemEventHandler(OnCreated);
        }

        public void OnCreated(object source, FileSystemEventArgs e)
        {
            //����fileNumber.txt�Ƿ����
            if (!File.Exists(_fileNumber))
            {
                return;
            }
            //��ȡ�ļ�����
            var fileNumberStr = getString(_fileNumber, Encoding.UTF8);
            var fileNumber = 0;
            if (string.IsNullOrEmpty(fileNumberStr)
                || !int.TryParse(fileNumberStr, out fileNumber))
            {
                return;
            }
            //δճ����ϣ��ȴ�ճ�����
            var txtFiles = Directory.GetFiles(_watcherDir, "*.txt");
            if (txtFiles.Count() != fileNumber + 1)
            {
                return;
            }
            //ճ�����
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
            //3.ѭ���ı��ļ����ѵ�ǰ�ļ��е��ı���ȡ��datatable��
            _txtDt = new DataTable();
            foreach (var txtFile in _txtFiles)
            {
                using (var reader = new StreamReader(txtFile, Encoding.UTF8))
                {
                    var txtStr = reader.ReadToEnd();
                    //ת���ַ�����datatable
                    AddDataToTxtDt(txtStr);
                    //��Ȼ�ᱨreader�Ѿ��򿪵Ĵ���
                    reader.Close();
                }
            }
            int HalfRowCount = _txtDt.Rows.Count / 2; //ȡ�м���һ�������ж�
            ValueisNum = new bool[_txtDt.Columns.Count];  //�ж��е���Ϣ�Ƿ�Ϊ��ֵ�ͣ����ͻ򸡵��ͣ�
            for (int iCol = 0; iCol < _txtDt.Columns.Count; iCol++)
            {
                string stttr = _txtDt.Rows[HalfRowCount][iCol].ToString();
                Regex regex = new Regex(@"^\d+$");
                Regex reg = new Regex(@"^\d+\.\d+$");
                if (regex.IsMatch(stttr) || reg.IsMatch(stttr))
                    ValueisNum[iCol] = true;        //������С��
                else
                    ValueisNum[iCol] = false;      //�ַ���Ӧ��Ϊ�������������������������Ҫ��ѯ���Ƶ��ظ��ԣ��˴���Ҫ�����������ȹ���һ�Σ�������                             
            }
            SaveXml();
        }
    }
}