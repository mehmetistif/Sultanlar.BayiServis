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
            XmlDocument config = new XmlDocument();
            config.Load("config.xml");

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

            cls = new Class1("Sultanlar - Bayi Servis", bayikod, server, database, userid, password, querySatis, queryStok, yilAd, DateTime.Now.Year, ayAd, DateTime.Now.Month);
            EventLog.WriteEntry(cls.logHead, "başladı");

            tmr = new Timer(3600000);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Enabled = true;
            tmr.Start();
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 10 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 18 || DateTime.Now.Hour == 20)
            {
                cls.GetData(true, true);
                cls.GetData(false, true);
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry(cls.logHead, "durduruldu");
        }
    }
}
