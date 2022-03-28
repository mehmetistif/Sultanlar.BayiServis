using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;

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

        public string GetData(bool satis, bool servis)
        {
            SqlConnection conn = new SqlConnection("Server=" + server + "; Database=" + database + "; User Id=" + userid + "; Password=" + password + "; Trusted_Connection=False;");
            SqlDataAdapter da = new SqlDataAdapter(satis ? querySatis : queryStok, conn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            string donendeger = Export(ds, satis, servis);

            ev.WriteEntry(donendeger != "" ? "veri gönderildi" : "veri gönderilemedi", EventLogEntryType.Information);

            return donendeger;
        }

        private string Export(DataSet ds, bool satis, bool servis)
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
    }
}
