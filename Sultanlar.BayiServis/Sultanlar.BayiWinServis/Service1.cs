using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Xml;
using Sultanlar.ClassLib;

namespace Sultanlar.BayiWinServis
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Timer tmr;
        Class1 cls;
        EventLog ev;
        XmlDocument config;
        string configPath;

        string bayikod;
        string server;
        string database;
        string userid;
        string password;
        string querySatis;
        string queryStok;
        string yilAd;
        //string yil;
        string ayAd;
        //string ay;

        protected override void OnStart(string[] args)
        {
            configPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            config = new XmlDocument();
            config.Load(configPath + "\\config.xml");

            bayikod = config.GetElementsByTagName("bayikod")[0].InnerText;
            server = config.GetElementsByTagName("server")[0].InnerText;
            database = config.GetElementsByTagName("database")[0].InnerText;
            userid = config.GetElementsByTagName("userid")[0].InnerText;
            password = config.GetElementsByTagName("password")[0].InnerText;
            querySatis = config.GetElementsByTagName("querySatis")[0].InnerText;
            queryStok = config.GetElementsByTagName("queryStok")[0].InnerText;
            yilAd = config.GetElementsByTagName("yilad")[0].InnerText;
            //yil = config.GetElementsByTagName("yil")[0].InnerText;
            ayAd = config.GetElementsByTagName("ayad")[0].InnerText;
            //ay = config.GetElementsByTagName("ay")[0].InnerText;

            ev = new EventLog();
            ev.Source = "Sultanlar Bayii Servis";
            cls = new Class1(ev, bayikod, server, database, userid, password, querySatis, queryStok, yilAd, DateTime.Now.Year, ayAd, DateTime.Now.Month);

            tmr = new Timer(300000);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Enabled = true;
            tmr.Start();
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime sonGonderim = Convert.ToDateTime(config.GetElementsByTagName("lastSent")[0].InnerText);
            if (sonGonderim.ToShortDateString() == DateTime.Now.ToShortDateString()) // bugün gönderilmişse
            {
                if ((DateTime.Now.Hour == 10 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 18 || DateTime.Now.Hour == 20)
                    && sonGonderim.Hour != DateTime.Now.Hour)
                {
                    Gonder();
                }
            }
            else
            {
                Gonder();
            }
        }

        private void Gonder()
        {
            string satis = cls.GetData(true);
            string stok = cls.GetData(false);
            if (satis == string.Empty && stok == string.Empty) // satış ve stok gönderilemedi
            {
                return;
            }

            // ikisinden birisi gönderildiyse
            config.GetElementsByTagName("lastSent")[0].InnerText = DateTime.Now.ToString();
            config.Save(configPath + "\\config.xml");
        }

        protected override void OnStop()
        {

        }
    }
}
