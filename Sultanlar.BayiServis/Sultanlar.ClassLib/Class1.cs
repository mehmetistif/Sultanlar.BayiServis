using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Collections;

namespace Sultanlar.ClassLib
{
    public class Class1
    {
        EventLog ev;
        private string bayikod;
        private string server;
        private string database;
        private string userid;
        private string password;
        private string querySatis;
        private string queryStok;
        private string yilad;
        private int yil;
        private string ayad;
        private int ay;

        public Class1(EventLog Ev, string Bayikod)
        {
            ev = Ev;
            bayikod = Bayikod;
        }

        public Class1(EventLog Ev, string Bayikod, string Server, string Database, string Userid, string Password, string QuerySatis, string QueryStok, string YilAd, int Yil, string AyAd, int Ay)
        {
            ev = Ev;

            server = Server;
            bayikod = Bayikod;
            database = Database;
            userid = Userid;
            password = Password;

            queryStok = QueryStok;
            yilad = YilAd;
            yil = Yil;
            ayad = AyAd;
            ay = Ay;

            querySatis = QuerySatis + " WHERE " + YilAd + " = " + Yil + " AND " + AyAd + " >= " + (Ay - 3).ToString();
        }

        public string GetData(bool satis)
        {
            SqlConnection conn = new SqlConnection("Server=" + server + "; Database=" + database + "; User Id=" + userid + "; Password=" + password + "; Trusted_Connection=False;");
            SqlDataAdapter da = new SqlDataAdapter(satis ? querySatis : queryStok, conn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string donendeger = Export(ds, satis);

            string sat = satis ? "Satış" : "Stok";
            ev.WriteEntry(donendeger != "" ? sat + " verisi gönderildi." : sat + " verisi gönderilemedi.", EventLogEntryType.Information);

            return donendeger;
        }

        public bool KaanGonder()
        {
            int ikiayonce = DateTime.Now.AddMonths(-2).Month;
            DateTime dtBas = Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            Kaan.Service1 scl = new Kaan.Service1();
            scl.Timeout = 1200000;
            Kaan.Authentication auth = new Kaan.Authentication();
            auth.username = "sultanlar";
            auth.password = "Sn80C3REN";
            Kaan.resultB2BSatisRapor report = scl.SultanlarRapor(auth, dtBas, dtBit);

            DataTable dt = CopyGenericToDataTable(report.Items, new ArrayList() { "ExtensionData" });
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);


            yilad = "YIL";
            yil = dtBas.Year;
            ayad = "AY";
            ay = dtBas.Month;
            string yazildi = Export(ds, true);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Kaan Gıda satış verisi gönderildi." : " Kaan Gıda satış verisi gönderilemedi."), EventLogEntryType.Information);

            /*
            string tabloadi = "tbl_" + Bayikod + "_Satis";

            if (!DisVeri.ExecNQwp("DELETE FROM " + tabloadi + " WHERE CONVERT(datetime, FATURATARIHI, 104) >= @FATURATARIHI", new ArrayList() { "FATURATARIHI" }, new object[] { dtBas }))
                return false;

            bool yazildi = DisVeri.TabloYaz(tabloadi, dt, "", "", "", "", false);

            SAPs.BayiLogYaz("bayi dis servis Satis", yazildi, "1071593 nolu bayi " + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + " dönemi. Gelen satır: " + report.Items.Length.ToString(), baslangic, DateTime.Now);
            */


            return true;
        }

        public string Export(DataSet ds, bool satis)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://www.ittihadteknoloji.com.tr/wcf/bayiservis.svc/web/Post?bayikod=" + bayikod + 
                    "&satis=" + (satis ? "Satis" : "Stok") + 
                    "&yilad=" + (satis ? yilad : "") + 
                    "&yil=" + (satis ? yil.ToString() : "") + 
                    "&ayad=" + (satis ? ayad : "") + 
                    "&ay=" + (satis ? ay.ToString() : ""));
                wr.Method = "POST";
                wr.ContentType = "text/xml; encoding='utf-8'";
                byte[] bytes = Encoding.UTF8.GetBytes(ds.GetXml());

                Stream requestStream = wr.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return responseStr;
                }
            }
            catch (Exception ex)
            {
                ev.WriteEntry(ex.Message, EventLogEntryType.Information);
                return "";
            }

            return "";
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
    }
}
