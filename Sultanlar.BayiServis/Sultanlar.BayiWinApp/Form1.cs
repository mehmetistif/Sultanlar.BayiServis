using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sultanlar.ClassLib;

namespace Sultanlar.BayiWinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        XmlDocument config;
        EventLog ev;
        private void Form1_Load(object sender, EventArgs e)
        {
            ev = new EventLog();
            ev.Source = "Sultanlar Bayi App";
            /*Process prs = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format(@"sc create \"servicename\" \"{0}\"", filepath);
            startInfo.Verb = "runas";
            prs.StartInfo = startInfo;
            prs.Start();*/

            foreach (Control control in this.Controls)
                if (control.Name != "panel1")
                    control.Enabled = false;
            panel1.Dock = DockStyle.Fill;
            panel1.BringToFront();


            config = new XmlDocument();
            config.Load("config.xml");
            textBox1.Text = config.GetElementsByTagName("bayikod")[0].InnerText;
            textBox2.Text = config.GetElementsByTagName("server")[0].InnerText;
            textBox3.Text = config.GetElementsByTagName("database")[0].InnerText;
            textBox4.Text = config.GetElementsByTagName("userid")[0].InnerText;
            textBox5.Text = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password")[0].InnerText);
            textBox14.Text = config.GetElementsByTagName("server1")[0].InnerText;
            textBox15.Text = config.GetElementsByTagName("database1")[0].InnerText;
            textBox16.Text = config.GetElementsByTagName("userid1")[0].InnerText;
            textBox17.Text = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password1")[0].InnerText);
            textBox6.Text = config.GetElementsByTagName("querySatis")[0].InnerText;
            textBox7.Text = config.GetElementsByTagName("queryStok")[0].InnerText;
            textBox10.Text = config.GetElementsByTagName("yilad")[0].InnerText;
            textBox8.Text = config.GetElementsByTagName("yil")[0].InnerText;
            textBox11.Text = config.GetElementsByTagName("ayad")[0].InnerText;
            textBox9.Text = config.GetElementsByTagName("ay")[0].InnerText;
            checkBox1.Checked = Convert.ToBoolean(config.GetElementsByTagName("https")[0].InnerText);
            label8.Text = "Son gönderim: " + config.GetElementsByTagName("lastSent")[0].InnerText;

            if (config.GetElementsByTagName("db")[0].InnerText == "sql")
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
            else
            {
                radioButton2.Checked = true;
                radioButton1.Checked = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            config.GetElementsByTagName("bayikod")[0].InnerText = textBox1.Text.Trim();
            config.GetElementsByTagName("server")[0].InnerText = textBox2.Text.Trim();
            config.GetElementsByTagName("database")[0].InnerText = textBox3.Text.Trim();
            config.GetElementsByTagName("userid")[0].InnerText = textBox4.Text.Trim();
            config.GetElementsByTagName("password")[0].InnerText = new Class1(ev, "").Enrypt(textBox5.Text);
            config.GetElementsByTagName("server1")[0].InnerText = textBox14.Text.Trim();
            config.GetElementsByTagName("database1")[0].InnerText = textBox15.Text.Trim();
            config.GetElementsByTagName("userid1")[0].InnerText = textBox16.Text.Trim();
            config.GetElementsByTagName("password1")[0].InnerText = new Class1(ev, "").Enrypt(textBox17.Text);
            config.GetElementsByTagName("querySatis")[0].InnerText = textBox6.Text.Trim();
            config.GetElementsByTagName("queryStok")[0].InnerText = textBox7.Text.Trim();
            config.GetElementsByTagName("yilad")[0].InnerText = textBox10.Text.Trim();
            config.GetElementsByTagName("yil")[0].InnerText = textBox8.Text.Trim();
            config.GetElementsByTagName("ayad")[0].InnerText = textBox11.Text.Trim();
            config.GetElementsByTagName("ay")[0].InnerText = textBox9.Text.Trim();
            config.GetElementsByTagName("https")[0].InnerText = checkBox1.Checked ? "true" : "false";
            config.GetElementsByTagName("db")[0].InnerText = radioButton1.Checked ? "sql" : "firebird";
            config.Save("config.xml");
            MessageBox.Show("Kaydedildi");
        }

        #region service install
        public static void InstallService(string serviceName, Assembly assembly)
        {
            if (IsServiceInstalled(serviceName))
            {
                if (MessageBox.Show("Servis zaten kurulmuş. Kaldırıp tekrar kurmak istiyor musunuz?", "Uyarı", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Install(assembly, false);
                }
                else
                {
                    return;
                }
            }

            if (Install(assembly, true))
                MessageBox.Show("İşlem başarılı.");
        }
        public static void UnInstallService(string serviceName, Assembly assembly, bool uyariverme)
        {
            if (IsServiceInstalled(serviceName))
            {
                if (uyariverme || MessageBox.Show("Servisi kaldırmak istediğinize emin misiniz?", "Uyarı", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (UnInstall(assembly))
                        MessageBox.Show("İşlem başarılı.");
                }
                else
                {
                    return;
                }
            }
        }

        public static bool Install(Assembly assembly, bool install)
        {
            using (AssemblyInstaller installer = GetInstaller(assembly))
            {
                IDictionary state = new Hashtable();
                try
                {
                    if (install)
                    {
                        installer.Install(state);
                        installer.Commit(state);
                    }
                    else
                    {
                        installer.Uninstall(state);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch { }
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static bool UnInstall(Assembly assembly)
        {
            using (AssemblyInstaller installer = GetInstaller(assembly))
            {
                IDictionary state = new Hashtable();
                try
                {
                    installer.Uninstall(state);
                }
                catch (Exception ex)
                {
                    try
                    {
                        installer.Rollback(state);
                    }
                    catch { }
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            using (ServiceController controller = new ServiceController(serviceName))
            {
                try
                {
                    ServiceControllerStatus status = controller.Status;
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            }
        }

        private static AssemblyInstaller GetInstaller(Assembly assembly)
        {
            AssemblyInstaller installer = new AssemblyInstaller(assembly, null);
            installer.UseNewContext = true;

            return installer;
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            string query = textBox6.Text.Trim();
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(),
                textBox14.Text.Trim(), textBox15.Text.Trim(), textBox16.Text.Trim(), textBox17.Text.Trim(), query, textBox7.Text.Trim(),
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), Convert.ToInt32(textBox18.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()), Convert.ToInt32(textBox13.Text.Trim()), checkBox1.Checked, radioButton1.Checked ? "sql" : "firebird");
            MessageBox.Show(cls.GetData(true));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string query = textBox7.Text.Trim();
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(),
                textBox14.Text.Trim(), textBox15.Text.Trim(), textBox16.Text.Trim(), textBox17.Text.Trim(), textBox6.Text.Trim(), query,
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), Convert.ToInt32(textBox18.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()), Convert.ToInt32(textBox13.Text.Trim()), checkBox1.Checked, radioButton1.Checked ? "sql" : "firebird");
            MessageBox.Show(cls.GetData(false));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom("Sultanlar.BayiWinServis.exe");
            InstallService("Sultanlar Bayi Servis", assembly);

            /*Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/k C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\InstallUtil.exe Sultanlar.BayiWinServis.exe";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();*/
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceController service = new ServiceController("Sultanlar Bayi Servis");
                if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending)
                {
                    service.Start();
                    MessageBox.Show("Başlatıldı.");
                }
                else
                {
                    MessageBox.Show("Servis durumu: " + service.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceController service = new ServiceController("Sultanlar Bayi Servis");
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    MessageBox.Show("Durduruldu.");
                }
                else
                {
                    MessageBox.Show("Servis durumu: " + service.Status.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Class1 cls = new Class1(ev, "1071593");
            cls.KaanGonder();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Class1 cls = new Class1(ev, "1071593");
            cls.KaanStokGonder();
        }

        private void textBox12_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox12.Text == "sulbayi")
                {
                    panel1.Visible = false;
                    foreach (Control control in this.Controls)
                        control.Enabled = true;
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Class1 cls = new Class1(ev, "1052689");
            cls.PekerGonder();


            /*string configPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            XmlDocument config = new XmlDocument();
            config.Load(configPath + "\\config.xml");

            string bayikod = config.GetElementsByTagName("bayikod")[0].InnerText;
            string server = config.GetElementsByTagName("server")[0].InnerText;
            string database = config.GetElementsByTagName("database")[0].InnerText;
            string userid = config.GetElementsByTagName("userid")[0].InnerText;
            string password = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password")[0].InnerText);
            string server1 = config.GetElementsByTagName("server1")[0].InnerText;
            string database1 = config.GetElementsByTagName("database1")[0].InnerText;
            string userid1 = config.GetElementsByTagName("userid1")[0].InnerText;
            string password1 = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password1")[0].InnerText);
            string querySatis = config.GetElementsByTagName("querySatis")[0].InnerText;
            string queryStok = config.GetElementsByTagName("queryStok")[0].InnerText;
            string yilAd = config.GetElementsByTagName("yilad")[0].InnerText;
            //yil = config.GetElementsByTagName("yil")[0].InnerText;
            string ayAd = config.GetElementsByTagName("ayad")[0].InnerText;
            //ay = config.GetElementsByTagName("ay")[0].InnerText;
            bool https = Convert.ToBoolean(config.GetElementsByTagName("https")[0].InnerText);
            string db = config.GetElementsByTagName("db")[0].InnerText;

            ev = new EventLog();
            ev.Source = "Sultanlar Bayi Servis";
            Class1 cls = new Class1(ev, bayikod, server, database, userid, password, server1, database1, userid1, password1, querySatis, queryStok, yilAd, DateTime.Now.Year, ayAd, DateTime.Now.Month, https, db);

            string satis = cls.GetData(true);*/
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string query = textBox6.Text.Trim();
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(),
                textBox14.Text.Trim(), textBox15.Text.Trim(), textBox16.Text.Trim(), textBox17.Text.Trim(), query, textBox7.Text.Trim(),
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), Convert.ToInt32(textBox18.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()), Convert.ToInt32(textBox13.Text.Trim()), checkBox1.Checked, radioButton1.Checked ? "sql" : "firebird");
            DataSet ds = new DataSet();
            string sonuc = cls.GetDataFromSource(ds, true);
            if (sonuc == "")
            {
                Form2 frm = new Form2(ds);
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show(sonuc);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string query = textBox7.Text.Trim();
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(),
                textBox14.Text.Trim(), textBox15.Text.Trim(), textBox16.Text.Trim(), textBox17.Text.Trim(), textBox6.Text.Trim(), query,
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), Convert.ToInt32(textBox18.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()), Convert.ToInt32(textBox13.Text.Trim()), checkBox1.Checked, radioButton1.Checked ? "sql" : "firebird");
            DataSet ds = new DataSet();
            string sonuc = cls.GetDataFromSource(ds, false);
            if (sonuc == "")
            {
                Form2 frm = new Form2(ds);
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show(sonuc);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                textBox5.Text = "masterkey";
                textBox17.Text = "masterkey";
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                textBox5.Text = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password")[0].InnerText);
                textBox17.Text = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password1")[0].InnerText);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom("Sultanlar.BayiWinServis.exe");
            UnInstallService("Sultanlar Bayii Servis", assembly, true);
            Assembly assembly1 = Assembly.LoadFrom("Sultanlar.BayiWinServis.exe");
            UnInstallService("Sultanlar Bayi Servis", assembly1, false);
        }

        private void button13_Click(object sender, EventArgs e)
        {
        }
    }
}
