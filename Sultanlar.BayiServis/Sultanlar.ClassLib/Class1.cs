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
using System.Security.Cryptography;
using Newtonsoft.Json;

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

        #region crypt
        private byte[] key = new byte[8] { 8, 2, 3, 4, 9, 6, 5, 7 };
        private byte[] iv = new byte[8] { 5, 9, 1, 7, 5, 6, 2, 8 };
        public string Enrypt(string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }
        public string Decrypt(string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }
        #endregion

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

        public bool KaanStokGonder()
        {
            Kaan.Service1 scl = new Kaan.Service1();
            scl.Timeout = 1200000;
            Kaan.Authentication auth = new Kaan.Authentication();
            auth.username = "sultanlar";
            auth.password = "Sn80C3REN";
            Kaan.resultB2BDepoRapor stok = scl.SultanlarDepoDurum(auth);

            Kaan.Urun[] depostok = new Kaan.Urun[stok.MerkezDepo.Length/* + stok.IadeDepo.Length*/];
            for (int i = 0; i < stok.MerkezDepo.Length; i++)
            {
                depostok[i] = stok.MerkezDepo[i];
            }
            /*for (int i = 0; i < stok.IadeDepo.Length; i++)
            {
                depostok[stok.MerkezDepo.Length + i] = stok.IadeDepo[i];
            }*/

            DataTable dt = CopyGenericToDataTable(depostok, new ArrayList() { "ExtensionData" });
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            yilad = "YIL";
            yil = DateTime.Now.Year;
            ayad = "AY";
            ay = DateTime.Now.Month;
            string yazildi = Export(ds, false);

            //ev.WriteEntry(DateTime.Now.ToString() + (yazildi != "" ? " Kaan Gıda stok verisi gönderildi." : " Kaan Gıda satış stok gönderilemedi."), EventLogEntryType.Information);

            return true;
        }

        public bool PekerGonder()
        {
            int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            string AY = dtBas.Month.ToString().Length == 1 ? "0" + dtBas.Month.ToString() : dtBas.Month.ToString();

            string sURL = "https://pekerticaret.ws.dia.com.tr/api/v3/scf/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string jsonWS = @"
                {""scf_fatura_listele_ayrintili"" :
                    {""session_id"": """ + PekerSession() + @""",
                     ""firma_kodu"": 1,
                     ""donem_kodu"": 7,
                     ""filters"":[{""field"": ""kartozelkodu2"", ""operator"": ""="", ""value"": ""SULTANLAR GRUP""},{ ""field"": ""tarih"", ""operator"": "">="", ""value"": """ + dtBas.Year.ToString() + @"-" + AY + @"-01 00:00:00.00""}],
                     ""sorts"": """",
                     ""params"": {
                        ""selectedcolumns"": [""turu"", ""turuack"", ""kartozelkodu2"", ""satiselemani"", ""carikodu"", ""unvan"", ""sevkadresi"", ""tarih"", ""belgeno"", ""belgeno2"", ""kartkodu"", ""kartaciklama"", ""kdv"", ""fatanabirimi"", ""anamiktar"", ""birimfiyati"", ""indirimtoplam"", ""sonbirimfiyati"", ""kdvtutari"", ""toplamtutar"", ""kdvdurumu"", ""iptal""]
	                 },
                     ""limit"": 0,
                     ""offset"": 0
                    }
                }
                ";

                streamWriter.Write(jsonWS);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse(); 
            DataTable dt;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd().Replace("\"msg\": \"\",", "").Replace("\"code\": \"200\",", "").Replace("\"result\": ", "");
                result = result.Substring(1, result.Length - 2);
                dt = (DataTable)JsonConvert.DeserializeObject(result, (typeof(DataTable)));
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);


            yilad = "YIL";
            yil = dtBas.Year;
            ayad = "AY";
            ay = dtBas.Month;
            string yazildi = Export(ds, true);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Peker Gıda satış verisi gönderildi." : " Peker Gıda satış verisi gönderilemedi."), EventLogEntryType.Information);

            return true;
        }

        public bool PekerStokGonder()
        {
            int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            string sURL = "https://pekerticaret.ws.dia.com.tr/api/v3/scf/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string jsonWS = @"
                {""scf_stokkart_listele"" :
                    {
                         ""session_id"": """ + PekerSession() + @""",
                         ""firma_kodu"": 1,
                         ""donem_kodu"": 7,
                         ""filters"":[{ ""field"": ""ozelkod2kodu"", ""operator"": ""="", ""value"": ""SULTANLAR GRUP""}],
                         ""sorts"": [],
                         ""params"": {
                            ""_key"": 4597817,
	                        ""_key_sis_depo_filtre"": 0,
	                        ""tarih"": ""2099-12-31"",
                            ""selectedcolumns"": [""aciklama"", ""stokkartkodu"", ""fiili_stok"", ""fiili_stok_irs"", ""gercek_stok"", ""gercek_stok_fat"", ""gercek_stok_irs"", ""b2c_depomiktari"", ""b2b_depomiktari""]
	                     },
                         ""limit"": 0,
                         ""offset"": 0
                    }
                }
                ";

                streamWriter.Write(jsonWS);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            DataTable dt;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd().Replace("\"msg\": \"\",", "").Replace("\"code\": \"200\",", "").Replace("\"result\": ", "");
                result = result.Substring(1, result.Length - 2);
                dt = (DataTable)JsonConvert.DeserializeObject(result, (typeof(DataTable)));
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);


            yilad = "YIL";
            yil = dtBas.Year;
            ayad = "AY";
            ay = dtBas.Month;
            string yazildi = Export(ds, false);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Peker Gıda stok verisi gönderildi." : " Peker Gıda stok verisi gönderilemedi."), EventLogEntryType.Information);

            return true;
        }

        public string PekerSession()
        {
            string sURL = "https://pekerticaret.ws.dia.com.tr/api/v3/sis/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                // SESSION ID MANUEL YAZILMALI
                string jsonWS = @"
                {""login"" :
                    {
                       ""username"": ""ws2"",
                       ""password"": ""111222"",
                       ""disconnect_same_user"": true,
                       ""params"": { ""apikey"": ""72862570-46db-450e-870b-4563605a984d""}
                    }
                }";

                streamWriter.Write(jsonWS);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var result = string.Empty;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd().Replace("{", "").Replace("}", "").Replace("\"msg\": \"", "").Replace("\",", "").Replace("\"code\": \"200", "").Replace("\"warnings\": []", "").Trim();
            }

            return result;
        }

        public string Export(DataSet ds, bool satis)
        {
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://www.ittihadteknoloji.com.tr/wcf/bayiservis.svc/web/Post?bayikod=" + bayikod + 
                    "&satis=" + (satis ? "Satis" : "Stok") + 
                    "&yilad=" + (satis ? yilad : "") +
                    "&yil=" + yil.ToString() +
                    "&ayad=" + (satis ? ayad : "") +
                    "&ay=" + ay.ToString());
                wr.Method = "POST";
                wr.ContentType = "text/xml; encoding='utf-8'";
                wr.Timeout = 600000;
                wr.ReadWriteTimeout = 600000;
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
