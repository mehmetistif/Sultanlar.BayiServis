using Sultanlar.DatabaseObject.Internet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace Sultanlar.WinServis
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Timer tmr;

        protected override void OnStart(string[] args)
        {
            tmr = new Timer(3600000);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Enabled = true;
            tmr.Start();
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 10 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 18 || DateTime.Now.Hour == 20)
            {
                KaanGonder();
            }
        }

        private bool KaanGonder()
        {
            DateTime baslangic = DateTime.Now;
            string Bayikod = "1071593";

            int ikiayonce = DateTime.Now.AddMonths(-2).Month;
            DateTime dtBas = Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            KaanGida.Service1SoapClient scl = new KaanGida.Service1SoapClient();
            KaanGida.Authentication auth = new KaanGida.Authentication();
            auth.username = "sultanlar";
            auth.password = "Sn80C3REN";
            KaanGida.resultB2BSatisRapor report = scl.SultanlarRapor(auth, dtBas, dtBit);

            DataTable dt = CopyGenericToDataTable(report.Items, new ArrayList() { "ExtensionData" });

            string tabloadi = "tbl_" + Bayikod + "_Satis";

            if (!DisVeri.ExecNQwp("DELETE FROM " + tabloadi + " WHERE CONVERT(datetime, FATURATARIHI, 104) >= @FATURATARIHI", new ArrayList() { "FATURATARIHI" }, new object[] { dtBas }))
                return false;

            bool yazildi = DisVeri.TabloYaz(tabloadi, dt, "", "", "", "", false);

            SAPs.BayiLogYaz("bayi dis servis Satis", yazildi, "1071593 nolu bayi " + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + " dönemi. Gelen satır: " + report.Items.Length.ToString(), baslangic, DateTime.Now);

            return yazildi;
        }

        private DataTable CopyGenericToDataTable<T>(IEnumerable<T> items, ArrayList excludeProps)
        {
            var properties = typeof(T).GetProperties();
            var result = new DataTable();

            //Build the columns
            foreach (var prop in properties)
            {
                for (int i = 0; i < excludeProps.Count; i++)
                {
                    if (prop.Name != excludeProps[i].ToString())
                    {
                        result.Columns.Add(prop.Name); //, prop.PropertyType
                    }
                }
            }

            //Fill the DataTable
            foreach (var item in items)
            {
                var row = result.NewRow();

                foreach (var prop in properties)
                {
                    for (int i = 0; i < excludeProps.Count; i++)
                    {
                        if (prop.Name != excludeProps[i].ToString())
                        {
                            var itemValue = prop.GetValue(item, new object[] { });
                            row[prop.Name] = itemValue;
                        }
                    }
                }

                result.Rows.Add(row);
            }

            return result;
        }

        protected override void OnStop()
        {
        }
    }
}
