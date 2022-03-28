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
            ev.Source = "Sultanlar Bayii App";
            /*Process prs = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format(@"sc create \"servicename\" \"{0}\"", filepath);
            startInfo.Verb = "runas";
            prs.StartInfo = startInfo;
            prs.Start();*/

            config = new XmlDocument();
            config.Load("config.xml");
            textBox1.Text = config.GetElementsByTagName("bayikod")[0].InnerText;
            textBox2.Text = config.GetElementsByTagName("server")[0].InnerText;
            textBox3.Text = config.GetElementsByTagName("database")[0].InnerText;
            textBox4.Text = config.GetElementsByTagName("userid")[0].InnerText;
            textBox5.Text = config.GetElementsByTagName("password")[0].InnerText;
            textBox6.Text = config.GetElementsByTagName("querySatis")[0].InnerText;
            textBox7.Text = config.GetElementsByTagName("queryStok")[0].InnerText;
            textBox10.Text = config.GetElementsByTagName("yilad")[0].InnerText;
            textBox8.Text = config.GetElementsByTagName("yil")[0].InnerText;
            textBox11.Text = config.GetElementsByTagName("ayad")[0].InnerText;
            textBox9.Text = config.GetElementsByTagName("ay")[0].InnerText;
            label8.Text = "Son gönderim: " + config.GetElementsByTagName("lastSent")[0].InnerText;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            config.GetElementsByTagName("bayikod")[0].InnerText = textBox1.Text.Trim();
            config.GetElementsByTagName("server")[0].InnerText = textBox2.Text.Trim();
            config.GetElementsByTagName("database")[0].InnerText = textBox3.Text.Trim();
            config.GetElementsByTagName("userid")[0].InnerText = textBox4.Text.Trim();
            config.GetElementsByTagName("password")[0].InnerText = textBox5.Text.Trim();
            config.GetElementsByTagName("querySatis")[0].InnerText = textBox6.Text.Trim();
            config.GetElementsByTagName("queryStok")[0].InnerText = textBox7.Text.Trim();
            config.GetElementsByTagName("yilad")[0].InnerText = textBox10.Text.Trim();
            config.GetElementsByTagName("yil")[0].InnerText = textBox8.Text.Trim();
            config.GetElementsByTagName("ayad")[0].InnerText = textBox11.Text.Trim();
            config.GetElementsByTagName("ay")[0].InnerText = textBox9.Text.Trim();
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
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(), query, textBox7.Text.Trim(),
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()));
            MessageBox.Show(cls.GetData(true));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string query = textBox7.Text.Trim();
            Class1 cls = new Class1(ev, textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(), textBox6.Text.Trim(), query,
                textBox10.Text.Trim(), Convert.ToInt32(textBox8.Text.Trim()), textBox11.Text.Trim(), Convert.ToInt32(textBox9.Text.Trim()));
            MessageBox.Show(cls.GetData(false));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom("Sultanlar.BayiWinServis.exe");
            InstallService("Sultanlar Bayii Servis", assembly);

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
                ServiceController service = new ServiceController("Sultanlar Bayii Servis");
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
                ServiceController service = new ServiceController("Sultanlar Bayii Servis");
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
    }
}
