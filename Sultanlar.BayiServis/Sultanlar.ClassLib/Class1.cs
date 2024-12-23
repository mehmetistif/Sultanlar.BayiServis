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
using FirebirdSql.Data.FirebirdClient;

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
        private string server1;
        private string database1;
        private string userid1;
        private string password1;
        private string querySatis;
        private string queryStok;
        private string queryCari;
        private string yilad;
        private int basyil;
        private int bityil;
        private string ayad;
        private int basay;
        private int bitay;
        private bool https;
        private string db;
        public int BasYil { get { return basyil; } set { basyil = value; } }
        public int BitYil { get { return bityil; } set { bityil = value; } }
        public int BasAy { get { return basay; } set { basay = value; } }
        public int BitAy { get { return bitay; } set { bitay = value; } }

        public Class1(EventLog Ev, string Bayikod)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.Expect100Continue = true;
            ev = Ev;
            bayikod = Bayikod;
        }

        /*public Class1(EventLog Ev, string Bayikod, string Server, string Database, string Userid, string Password, string Server1, string Database1, string Userid1, string Password1, string QuerySatis, string QueryStok, string YilAd, int Yil, string AyAd, int Ay, bool Https, string Db)
        {
            ev = Ev;

            bayikod = Bayikod;
            server = Server;
            database = Database;
            userid = Userid;
            password = Password;
            server1 = Server1;
            database1 = Database1;
            userid1 = Userid1;
            password1 = Password1;

            queryStok = QueryStok;
            yilad = YilAd;
            yil = Yil;
            ayad = AyAd;
            ay = Ay;
            https = Https;
            db = Db;

            DateTime baslangic = Convert.ToDateTime("01." + Ay + "." + Yil).AddMonths(-3);
            querySatis = db == "sql" ? 
                QuerySatis + " WHERE CONVERT(datetime,CONVERT(nvarchar(50)," + AyAd + ") + '.01.' + CONVERT(nvarchar(50)," + YilAd + ")) >= '" + baslangic.Month + ".01." + baslangic.Year + "'"
            :
                QuerySatis + " WHERE CAST(CAST(" + AyAd + " AS VARCHAR(100)) || '.01.' || CAST(" + YilAd + " AS VARCHAR(100)) AS DATE) >= '" + baslangic.Month + ".01." + baslangic.Year + "'";
        }*/

        public Class1(EventLog Ev, string Bayikod, string Server, string Database, string Userid, string Password, string Server1, string Database1, string Userid1, string Password1, string QuerySatis, string QueryStok, string QueryCari, string YilAd, int BasYil, int BitYil, string AyAd, int BasAy, int BitAy, bool Https, string Db)
        {
            ev = Ev;

            bayikod = Bayikod;
            server = Server;
            database = Database;
            userid = Userid;
            password = Password;
            server1 = Server1;
            database1 = Database1;
            userid1 = Userid1;
            password1 = Password1;

            queryStok = QueryStok;
            queryCari = QueryCari;

            yilad = YilAd;
            basyil = BasYil;
            bityil = BitYil;
            ayad = AyAd;
            basay = BasAy;
            bitay = BitAy;
            https = Https;
            db = Db;

            DateTime baslangic = Convert.ToDateTime("01." + BasAy + "." + BasYil);
            DateTime bitis = Convert.ToDateTime("01." + BitAy + "." + BitYil).AddMonths(1);

            try
            {
                baslangic = DateTime.ParseExact(BasYil + "-" + (BasAy > 9 ? BasAy.ToString() : "0" + BasAy.ToString()) + "-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture); //Convert.ToDateTime("01." + BasAy + "." + BasYil);
                bitis = DateTime.ParseExact(BitYil + "-" + (BitAy > 9 ? BitAy.ToString() : "0" + BitAy.ToString()) + "-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).AddMonths(1); ; //Convert.ToDateTime("01." + BitAy + "." + BitYil).AddMonths(1);
            }
            catch (Exception ex)
            {
                ev.WriteEntry("[Hata] " + BasYil + "-" + (BasAy > 9 ? BasAy.ToString() : "0" + BasAy.ToString()) + "-01 - " + BitYil + "-" + (BitAy > 9 ? BitAy.ToString() : "0" + BitAy.ToString()) + "-01 : " + ex.Message, EventLogEntryType.Information);
            }
            

            ev.WriteEntry("[Bilgi] Gönderilecek verinin aralık tarihi = yıl:" + baslangic.Year + " ay:" + baslangic.Month + " gün:" + baslangic.Day + 
                " <= veri < " +
                "yıl:" +  bitis.Year + " ay:" + bitis.Month + " gün:" + bitis.Day, EventLogEntryType.Information);

            querySatis = db == "sql" ?
                QuerySatis + " WHERE CONVERT(datetime,CONVERT(nvarchar(50)," + AyAd + ") + '.01.' + CONVERT(nvarchar(50)," + YilAd + ")) >= '" + baslangic.Month + ".01." + baslangic.Year +
                "' AND CONVERT(datetime,CONVERT(nvarchar(50)," + AyAd + ") + '.01.' + CONVERT(nvarchar(50)," + YilAd + ")) < '" + bitis.Month + ".01." + bitis.Year + "'"
            :
                QuerySatis + " WHERE CAST(CAST(" + AyAd + " AS VARCHAR(100)) || '.01.' || CAST(" + YilAd + " AS VARCHAR(100)) AS DATE) >= '" + baslangic.Month + ".01." + baslangic.Year +
                "' AND CAST(CAST(" + AyAd + " AS VARCHAR(100)) || '.01.' || CAST(" + YilAd + " AS VARCHAR(100)) AS DATE) < '" + bitis.Month + ".01." + bitis.Year + "'";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tur">1: satis, 2: stok, 3: cari</param>
        /// <returns></returns>
        public string GetData(int tur)
        {
            DataSet ds = new DataSet();
            string sonuc = GetDataFromSource(ds, tur);
            if (sonuc != "")
                return sonuc;

            string donendeger = Export(ds, tur);

            string sat = tur == 1 ? "Satış" : tur == 2 ? "Stok" : tur == 3 ? "Cari" : "";
            ev.WriteEntry(donendeger != "" ? sat + " verisi gönderildi." : sat + " verisi gönderilemedi.", EventLogEntryType.Information);

            return donendeger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tur">1: satis, 2: stok, 3: cari</param>
        /// <returns></returns>
        public string GetDataFromSource(DataSet ds, int tur)
        {
            string Server = tur == 1 ? server : server1;
            string Database = tur == 1 ? database : database1;
            string Userid = tur == 1 ? userid : userid1;
            string Password = tur == 1 ? password : password1;

            try
            {
                if (db == "sql")
                {
                    SqlConnection conn = new SqlConnection("Server=" + Server + "; Database=" + Database + "; User Id=" + Userid + "; Password=" + Password + "; Trusted_Connection=False;");
                    SqlDataAdapter da = new SqlDataAdapter(tur == 1 ? querySatis : tur == 2 ? queryStok : tur == 3 ? queryCari : "", conn);
                    da.SelectCommand.CommandTimeout = 600;
                    da.Fill(ds);
                }
                else
                {
                    FbConnection conn = new FbConnection("User=" + Userid + ";Password=" + Password + ";Database=" + Database + ";DataSource=" + Server + ";Port=3050;Dialect=3;Charset=WIN1254;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType = 0;");
                    FbDataAdapter da = new FbDataAdapter(tur == 1 ? querySatis : tur == 2 ? queryStok : tur == 3 ? queryCari : "", conn);
                    da.SelectCommand.CommandTimeout = 600;
                    da.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                ev.WriteEntry((tur == 1 ? "Satış" : tur == 2 ? "Stok" : tur == 3 ? "Cari" : "") + ": " + ex.Message, EventLogEntryType.Information);
                return ex.Message;
            }

            return "";
        }

        #region Kaan
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
            bityil = dtBas.Year;
            ayad = "AY";
            bitay = dtBas.Month;
            string yazildi = Export(ds, 1);

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
            bityil = DateTime.Now.Year;
            ayad = "AY";
            bitay = DateTime.Now.Month;
            string yazildi = Export(ds, 2);

            //ev.WriteEntry(DateTime.Now.ToString() + (yazildi != "" ? " Kaan Gıda stok verisi gönderildi." : " Kaan Gıda satış stok gönderilemedi."), EventLogEntryType.Information);

            return true;
        }
        #endregion

        #region Peker
        public bool PekerGonder()
        {
            //int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = DateTime.Now.AddMonths(-3); //Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            //string AY = dtBas.Month.ToString().Length == 1 ? "0" + dtBas.Month.ToString() : dtBas.Month.ToString();

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
                     ""donem_kodu"": 9,
                     ""filters"":[{""field"": ""kartozelkodu2"", ""operator"": ""IN"", ""value"": ""SULTANLAR GRUP,BANDUFF""},{ ""field"": ""tarih"", ""operator"": "">="", ""value"": """ + dtBas.Year.ToString() + @"-" + (dtBas.Month.ToString().Length == 1 ? "0" : "") + dtBas.Month.ToString() + @"-01 00:00:00.00""}],
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
            basyil = dtBas.Year;
            bityil = dtBit.Year;
            ayad = "AY";
            basay = dtBas.Month;
            bitay = dtBit.Month;
            string yazildi = Export(ds, 1);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Peker Gıda satış verisi gönderildi." : " Peker Gıda satış verisi gönderilemedi."), EventLogEntryType.Information);

            return true;
        }
        public bool PekerStokGonder()
        {
            //int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = DateTime.Now.AddMonths(-3); //Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            //string AY = dtBas.Month.ToString().Length == 1 ? "0" + dtBas.Month.ToString() : dtBas.Month.ToString();

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
                         ""filters"":[{ ""field"": ""ozelkod2kodu"", ""operator"": ""IN"", ""value"": ""SULTANLAR GRUP,BANDUFF""}],
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

                var o = JsonConvert.DeserializeObject<List<DiaStok>>(result);

                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

                //dt = (DataTable)JsonConvert.DeserializeObject(o.ToString(), (typeof(DataTable)));

                dt = CopyGenericToDataTable(o.ToArray(), new ArrayList() { "ExtensionData" });
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);


            yilad = "YIL";
            basyil = dtBas.Year;
            bityil = dtBit.Year;
            ayad = "AY";
            basay = dtBas.Month;
            bitay = dtBit.Month;
            string yazildi = Export(ds, 2);

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
                       ""username"": ""Ws2"",
                       ""password"": ""12345..0"",
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
        #endregion

        #region Yilmaz
        public bool YilmazGonder()
        {
            //int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = DateTime.Now.AddMonths(-3); //Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            //string AY = dtBas.Month.ToString().Length == 1 ? "0" + dtBas.Month.ToString() : dtBas.Month.ToString();

            string sURL = "https://yilmazmesrubat.ws.dia.com.tr/api/v3/scf/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string jsonWS = @"
                {""scf_fatura_listele_ayrintili"" :
                    {""session_id"": """ + YilmazSession() + @""",
                     ""firma_kodu"": 1,
                     ""donem_kodu"": 4,
                     ""filters"":[{""field"": ""stokkartmarka"", ""operator"": ""IN"", ""value"": ""BANDUFF,SULTANLAR""},{ ""field"": ""tarih"", ""operator"": "">="", ""value"": """ + dtBas.Year.ToString() + @"-" + (dtBas.Month.ToString().Length == 1 ? "0" : "") + dtBas.Month.ToString() + @"-01 00:00:00.00""}],
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
            basyil = dtBas.Year;
            bityil = dtBit.Year;
            ayad = "AY";
            basay = dtBas.Month;
            bitay = dtBit.Month;
            string yazildi = Export(ds, 1);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Yılmaz meşrubat satış verisi gönderildi." : " Yılmaz meşrubat satış verisi gönderilemedi."), EventLogEntryType.Information);

            return true;
        }
        public bool YilmazStokGonder()
        {
            //int ikiayonce = DateTime.Now.AddMonths(-3).Month;
            DateTime dtBas = DateTime.Now.AddMonths(-3); //Convert.ToDateTime(DateTime.Now.Year.ToString() + "." + (ikiayonce < 1 ? "1" : ikiayonce.ToString()) + ".1");
            DateTime dtBit = DateTime.Now;

            //string AY = dtBas.Month.ToString().Length == 1 ? "0" + dtBas.Month.ToString() : dtBas.Month.ToString();

            string sURL = "https://yilmazmesrubat.ws.dia.com.tr/api/v3/scf/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string jsonWS = @"
                {""scf_stokkart_listele"" :
                    {
                         ""session_id"": """ + YilmazSession() + @""",
                         ""firma_kodu"": 1,
                         ""donem_kodu"": 3,
                         ""filters"":[{ ""field"": ""marka"", ""operator"": ""IN"", ""value"": ""BANDUFF,SULTANLAR""}],
                         ""sorts"": [],
                         ""params"": {
                            ""_key"": 0,
	                        ""_key_sis_depo_filtre"": 1165932,
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

                var o = JsonConvert.DeserializeObject<List<DiaStok>>(result);

                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

                //dt = (DataTable)JsonConvert.DeserializeObject(o.ToString(), (typeof(DataTable)));

                dt = CopyGenericToDataTable(o.ToArray(), new ArrayList() { "ExtensionData" });
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);


            yilad = "YIL";
            basyil = dtBas.Year;
            bityil = dtBit.Year;
            ayad = "AY";
            basay = dtBas.Month;
            bitay = dtBit.Month;
            string yazildi = Export(ds, 2);

            ev.WriteEntry(dtBas.Year.ToString() + "-" + dtBas.Month.ToString() + (yazildi != "" ? " Yılmaz Meşrubat stok verisi gönderildi." : " Yılmaz Meşrubat stok verisi gönderilemedi."), EventLogEntryType.Information);

            return true;
        }
        public string YilmazSession()
        {
            string sURL = "https://yilmazmesrubat.ws.dia.com.tr/api/v3/sis/json";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                // SESSION ID MANUEL YAZILMALI d7283da0-d7db-4294-9bd7-9786892ee005
                string jsonWS = @"
                {""login"" :
                    {
                       ""username"": ""ws-1"",
                       ""password"": ""1234"",
                       ""disconnect_same_user"": true,
                       ""params"": { ""apikey"": ""8a2d19c4-90c6-40ee-9f62-7f650155bfac""}
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
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tur">1: satis, 2: stok, 3: cari</param>
        /// <returns></returns>
        public string Export(DataSet ds, int tur)
        {
            try
            {
                HttpWebRequest wr; //(HttpWebRequest)WebRequest.CreateDefault(new Uri(""))

                if (https)
                {
                    wr = (HttpWebRequest)WebRequest.Create("https://www.ittihadteknoloji.com.tr/dis/bayiservis/SatisStok2/" + bayikod +
                    "/" + (tur == 1 ? "Satis" : tur == 2 ? "Stok" : tur == 3 ? "Cari" : "") +
                    "/" + (tur == 1 ? yilad : "-") +
                    "/" + basyil.ToString() +
                    "/" + bityil.ToString() +
                    "/" + (tur == 1 ? ayad : "-") +
                    "/" + basay.ToString() +
                    "/" + bitay.ToString());
                }
                else
                {
                    wr = (HttpWebRequest)WebRequest.Create("http://www.ittihadteknoloji.com.tr/wcf/bayiservis.svc/web/Post2?bayikod=" + bayikod +
                    "&satis=" + (tur == 1 ? "Satis" : tur == 2 ? "Stok" : tur == 3 ? "Cari" : "") +
                    "&yilad=" + (tur == 1 ? yilad : "") +
                    "&basyil=" + basyil.ToString() +
                    "&bityil=" + bityil.ToString() +
                    "&ayad=" + (tur == 1 ? ayad : "") +
                    "&basay=" + basay.ToString() +
                    "&bitay=" + bitay.ToString());
                }

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
                ev.WriteEntry((tur == 1 ? "Satış" : tur == 2 ? "Stok" : tur == 3 ? "Cari" : "") + ": " + ex.Message, EventLogEntryType.Information);
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

    public class DiaStok
    {
        [JsonIgnore]
        public string __cellcolor { get { return ""; } }
        public double fiili_stok_irs { get; set; }
        public double fiili_stok { get; set; }
        public string stokkartkodu { get; set; }
        public double gercek_stok_fat { get; set; }
        [JsonIgnore]
        public string __format { get { return ""; } }
        public double b2c_depomiktari { get; set; }
        public double gercek_stok_irs { get; set; }
        public double b2b_depomiktari { get; set; }
        public double gercek_stok { get; set; }
        public string aciklama { get; set; }
    }
}
