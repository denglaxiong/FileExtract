using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using GainTarGz;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Net;

namespace FileExtract
{
    public partial class Form1 : Form
    {
        string confFilePath = System.Windows.Forms.Application.StartupPath+"/FileEXtractConf";//配置文件的地址
        FormEntity formEntity = null;
        string projectName = null;
        string endTime = "2019-12-1";
        string password = "oppoR11";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {   
            if (File.Exists(confFilePath))
            {
                FileStream fs = new FileStream(confFilePath,FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                FormConfEntity fce = (FormConfEntity)bf.Deserialize(fs);
                fs.Close();
                comboBox1.Items.AddRange(fce.TXTPath.ToArray());
                comboBox2.Items.AddRange(fce.Workspace.ToArray());
                comboBox3.Items.AddRange(fce.OutFolder.ToArray());
                comboBox4.Items.AddRange(fce.OutFolderAdd.ToArray());
                comboBox5.Items.AddRange(fce.ReplaceSource.ToArray());
                comboBox6.Items.AddRange(fce.ReplaceTarget.ToArray());
                checkBox1.Checked = fce.GainClass;
                checkBox2.Checked = fce.RetainDirectory;
                checkBox3.Checked = fce.GainTarGz;
            }
            else
            {
                this.comboBox1.Items.Add(@"F:\demo\demo.txt");
                this.comboBox2.Items.Add(@"E:\workspace");
                this.comboBox3.Items.Add(@"F:\demo");
                this.comboBox4.Items.Add(@"2018-4-18");
                this.comboBox5.Items.Add(@"src/");
                this.comboBox6.Items.Add(@"hxbroot\WEB-INF\classes\");
            }
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
            this.comboBox3.SelectedIndex = 0;
            this.comboBox4.SelectedIndex = 0;
            this.comboBox5.SelectedIndex = 0;
            this.comboBox6.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!password.Equals(textBox1.Text)&&!detectionDate()) return;
            //清空消息框
            this.label7.Text = "";
            //封装参数
            formEntity = new FormEntity()
            {
                TXTPath = this.comboBox1.Text,
                Workspace = this.comboBox2.Text,
                OutFolder = this.comboBox3.Text,
                OutFolderAdd = this.comboBox4.Text,
                ReplaceSource = this.comboBox5.Text,
                ReplaceTarget = this.comboBox6.Text,
                GainClass = this.checkBox1.Checked,
                RetainDirectory = this.checkBox2.Checked,
                GainTarGz=this.checkBox3.Checked
            };
            formEntity.processPathOperation();
            //表单验证
            if (!validateFormEntity(formEntity)){
                return;
            }
            DirectoryInfo dirInfo = new DirectoryInfo(integrationFolder(formEntity.OutFolder, formEntity.OutFolderAdd));
            if (dirInfo.Exists)
            {
                delectDirChilds(integrationFolder(formEntity.OutFolder, formEntity.OutFolderAdd));
            }
            //读取txt文件，进行文件拷贝
            if (!readTxtAndCopyFile(formEntity))
            {
                return;
            }
            //压缩
            if (formEntity.GainTarGz && !new Targz().CreatTarGzArchiveEnvironment(integrationFolder(formEntity.OutFolder, formEntity.OutFolderAdd), projectName))
            {
                print("拷贝完成，压缩失败！");
                return;
            }
            //缓存控件内容
            cacheControl();
            print("拷贝完成");
        }

        private void cacheControl()
        {
            foreach (Control control in this.Controls)
            {
                if (control is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)control;
                    string comboBoxText = comboBox.Text;
                    if (comboBox.Items.Contains(comboBoxText))
                    {
                        comboBox.Items.Remove(comboBoxText);
                    }
                    comboBox.Items.Insert(0, comboBoxText);
                    comboBox.SelectedIndex = 0;
                }
            }
        }

        //读取文件并进行文件拷贝
        private bool readTxtAndCopyFile(FormEntity formEntity)
        {
            Boolean boo = true;
            FileStream fs=new FileStream(formEntity.TXTPath, FileMode.Open);
            StreamReader sw = new StreamReader(fs);
            string lineStr = null;
            int i = 0;
            while ((lineStr = sw.ReadLine()) != null) 
            {
                try
                {
                    lineStr = lineStr.Trim();
                    if ("--end".Equals(lineStr) || "end".Equals(lineStr))
                    {
                        break;
                    }
                    else if (lineStr == "" || lineStr.StartsWith("--"))
                    {
                         continue;
                    }
                    lineStr = lineStr.Replace("/", "\\");
                    //是否获取class
                    if (formEntity.GainClass)
                    {
                        lineStr = lineStr.Replace(formEntity.ReplaceSource, formEntity.ReplaceTarget);
                        lineStr = lineStr.Replace(".java", ".class");
                    }
                    string sourceFile = null;
                    if (Regex.IsMatch(lineStr, "^[A-Z:]"))
                    {
                        sourceFile = sourceFile.Replace(formEntity.Workspace, "");
                    }
                    sourceFile = integrationFolder(formEntity.Workspace, lineStr);
                    string tempStr = null;

                    if (lineStr.StartsWith("\\"))
                    {
                        tempStr = lineStr.Substring(1, lineStr.Length - 1);
                    }
                    projectName = tempStr.Substring(0, tempStr.IndexOf("\\"));
                    if (!File.Exists(sourceFile))
                    {
                        print("文件不存在，拷贝异常，" + sourceFile);
                        boo = false;
                        break;
                    }
                    string filePath = lineStr;
                    if (!formEntity.RetainDirectory)
                    {
                        filePath = lineStr.Substring(lineStr.LastIndexOf("\\", lineStr.Length - 1));
                    }
                    string targetFile = integrationFolder(formEntity.OutFolder, formEntity.OutFolderAdd, filePath);
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                    string outFolder = targetFile.Substring(0, targetFile.LastIndexOf("\\"));
                    if (!Directory.Exists(outFolder))
                    {
                        Directory.CreateDirectory(outFolder);
                    }
                    File.Copy(sourceFile, targetFile);
                    if (formEntity.GainClass)
                    {
                        //如果获取class，则copy所有内部类
                        List<string> filePaths = GetFilesByPath(sourceFile);
                        foreach (string fpath in filePaths)
                        {
                            targetFile = targetFile.Substring(0, targetFile.LastIndexOf("\\")) + fpath.Substring(fpath.LastIndexOf("\\"));
                            if (File.Exists(targetFile))
                            {
                                File.Delete(targetFile);
                            }
                            File.Copy(fpath, targetFile);
                        }
                    }
                    i++;
                }
                catch(OverflowException e)
                {
                    print("txt文件内容有误！！");
                    //throw e;
                    //MessageBox.Show(e.StackTrace);
                    sw.Close();
                    fs.Close();
                    return false;
                }
            }
            sw.Close();
            fs.Close();
            if (i < 1)
            {
                print("没有可拷贝的文件");
                boo = false;
            }

            return boo;
            
        }
        //整合路径-进行路径拼接
        private string integrationFolder(params string[] paths )
        {
            string resultStr = "";
            for(int i=0;i<paths.Length;i++)
            {
                string path = paths[i].Trim();
                if (path=="")
                {
                    continue;
                }
                path=path.Replace("/", @"\");
                if (resultStr!="" && !resultStr.EndsWith(@"\"))
                {
                    resultStr += @"\";
                }
                if (path.StartsWith(@"\"))
                {
                    path = path.Substring(1, path.Length - 1);
                }
                    resultStr += path;
            }
            return resultStr;
        }    
        //表单验证
        private Boolean validateFormEntity(FormEntity formEntity)
        {
            Boolean boo = false;
            if (formEntity.TXTPath == "" || !File.Exists(formEntity.TXTPath))
            {
                print("text文件不存在");
            }
            else if (formEntity.Workspace == "" || !Directory.Exists(formEntity.Workspace))
            {
                print("本地工作空间不存在");
            }
            else
            {
                boo = true;
            }
            return boo;
        }
        //打印信息
        private void print(string str)
        {
            this.label7.Text = DateTime.Now.ToString() + "-message:" + str;
        }
        //删除目录下的子文件及目录
        public void delectDirChilds(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        //Process[] pcs = Process.GetProcesses();
                        //foreach (Process p in pcs)
                        //{
                        //    if (p.MainModule.FileName == i.FullName)
                        //    {
                        //        p.Kill();
                        //    }
                        //}
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //当不按目录结构获取时不压缩
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox3.Visible = true;
            }
            else
            {
                checkBox3.Checked = false;
                checkBox3.Visible = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //用配置文件缓存所有控件
            FormConfEntity formConfEntity = new FormConfEntity()
            {
                TXTPath = itemsToList(comboBox1),
                Workspace = itemsToList(comboBox2),
                OutFolder = itemsToList(comboBox3),
                OutFolderAdd = itemsToList(comboBox4),
                ReplaceSource = itemsToList(comboBox5),
                ReplaceTarget = itemsToList(comboBox6),
                GainClass = checkBox1.Checked,
                RetainDirectory = checkBox2.Checked,
                GainTarGz = checkBox3.Checked

            };

            FileStream fs = new FileStream(confFilePath,FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs,formConfEntity);
            fs.Close();
        }
        private List<string> itemsToList(ComboBox comboBox)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (i > 5) break;
                list.Add(comboBox.GetItemText(comboBox.Items[i]));
            }
            return list;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.button1_Click(sender,e);
            }
        }


        private void label8_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = " 请选择TXT文件";
            openFileDialog1.Filter = "TXT文件(*.txt)|*.txt";
            openFileDialog1.Multiselect = false;
            if(!"".Equals(comboBox1.Text)) openFileDialog1.InitialDirectory = comboBox1.Text;
            if (openFileDialog1.ShowDialog()== DialogResult.OK)
            {
                comboBox1.Text = openFileDialog1.FileName;
            }
            
        }

        private void label9_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (!"".Equals(comboBox3.Text)) dialog.SelectedPath = comboBox3.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                comboBox2.Text = dialog.SelectedPath;
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            //dialog.SelectedPath = "F:\\demo";
            if (!"".Equals(comboBox3.Text)) dialog.SelectedPath = comboBox3.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                comboBox3.Text = dialog.SelectedPath;
            }
        }
        public Boolean detectionDate()
        {
            Boolean boo = false;
            string url = "http://www.baidu.com";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url );

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 1000;// 
            httpWebRequest.ReadWriteTimeout = 1000;

            //byte[] btBodys = Encoding.UTF8.GetBytes(body);
            //httpWebRequest.ContentLength = btBodys.Length;
            //httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
            print("正在链接网络...");
            try
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string dateStr = httpWebResponse.GetResponseHeader("date");
                boo = Convert.ToDateTime(dateStr).CompareTo(Convert.ToDateTime(endTime)) < 0;
                if (!boo)
                {
                    print("程序已过期！！，请找开发人员");
                    MessageBox.Show("程序已过期！！，请找开发人员");
                }


                    
            }
            catch (Exception e)
            {
                print("请链接网络");
                MessageBox.Show("请链接网络");
            }
            return boo;
        }
        //获取class的内部内文件路径
        private List<string> GetFilesByPath(string path)
        {

            string dir = path.Substring(0, path.LastIndexOf("\\"));
            string fileName = path.Substring(path.LastIndexOf("\\") + 1);


            DirectoryInfo di = new DirectoryInfo(dir);
            //找到该目录下的文件 
            FileInfo[] fi = di.GetFiles();
            //把FileInfo[]数组转换为List    
            List<string> list = new List<string>();

            for (int i = 0; i < fi.Length; i++)
            {
                string filestr = fi[i].Name;
                if (fi[i].Name.StartsWith(fileName.Replace(".class", "")) && !fi[i].Name.Equals(fileName))
                {
                    list.Add(fi[i].FullName);
                }

            }
            return list;
        }
        private void label11_Click(object sender, EventArgs e)
        {
            textBox1.Visible = !textBox1.Visible;
        }
    }

}
