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
        string server1;
        string database1;
        string userid1;
        string password1;
        string querySatis;
        string queryStok;
        string queryCari;
        string yilAd;
        //string yil;
        string ayAd;
        //string ay;
        bool https;
        string db;

        protected override void OnStart(string[] args)
        {
            configPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            config = new XmlDocument();
            config.Load(configPath + "\\config.xml");

            bayikod = config.GetElementsByTagName("bayikod")[0].InnerText;
            server = config.GetElementsByTagName("server")[0].InnerText;
            database = config.GetElementsByTagName("database")[0].InnerText;
            userid = config.GetElementsByTagName("userid")[0].InnerText;
            password = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password")[0].InnerText);
            server1 = config.GetElementsByTagName("server1")[0].InnerText;
            database1 = config.GetElementsByTagName("database1")[0].InnerText;
            userid1 = config.GetElementsByTagName("userid1")[0].InnerText;
            password1 = new Class1(ev, "").Decrypt(config.GetElementsByTagName("password1")[0].InnerText);
            querySatis = config.GetElementsByTagName("querySatis")[0].InnerText;
            queryStok = config.GetElementsByTagName("queryStok")[0].InnerText;
            queryCari = config.GetElementsByTagName("queryCari")[0].InnerText;
            yilAd = config.GetElementsByTagName("yilad")[0].InnerText;
            //yil = config.GetElementsByTagName("yil")[0].InnerText;
            ayAd = config.GetElementsByTagName("ayad")[0].InnerText;
            //ay = config.GetElementsByTagName("ay")[0].InnerText;
            https = Convert.ToBoolean(config.GetElementsByTagName("https")[0].InnerText);
            db = config.GetElementsByTagName("db")[0].InnerText;

            ev = new EventLog();
            ev.Source = "Sultanlar Bayi Servis";
            DateTime baslangic = DateTime.Now.AddMonths(-3);
            DateTime bitis = DateTime.Now;
            cls = new Class1(ev, bayikod, server, database, userid, password, server1, database1, userid1, password1, querySatis, queryStok, queryCari, yilAd, baslangic.Year, bitis.Year, ayAd, baslangic.Month, bitis.Month, https, db);

            tmr = new Timer(300000);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Enabled = true;
            tmr.Start();

            Gonder();
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime baslangic = DateTime.Now.AddMonths(-3);
            DateTime bitis = DateTime.Now;
            cls = new Class1(ev, bayikod, server, database, userid, password, server1, database1, userid1, password1, querySatis, queryStok, queryCari, yilAd, baslangic.Year, bitis.Year, ayAd, baslangic.Month, bitis.Month, https, db);

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
            DateTime baslangic = DateTime.Now.AddMonths(-3);
            DateTime bitis = DateTime.Now;
            cls.BasYil = baslangic.Year;
            cls.BasAy = baslangic.Month;
            cls.BitYil = bitis.Year;
            cls.BitAy = bitis.Month;
            string satis = cls.GetData(1);
            string stok = cls.GetData(2);
            string cari = queryCari != "" ? cls.GetData(3) : "";
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
