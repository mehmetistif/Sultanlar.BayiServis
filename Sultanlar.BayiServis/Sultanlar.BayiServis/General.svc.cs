using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.Xml;
using System.ServiceModel.Web;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Sultanlar.BayiServis
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "General" in code, svc and config file together.
    public class General : IGeneral
    {
        private string getClientIP()
        {
            //WebOperationContext webContext = WebOperationContext.Current;

            OperationContext context = OperationContext.Current;

            System.ServiceModel.Channels.MessageProperties messageProperties = context.IncomingMessageProperties;

            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpointProperty =

                messageProperties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name]

                as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
            return endpointProperty.Address;
        }

        public string Test()
        {
            return "Sultanlar WCF çalışıyör. " + getClientIP();
        }

        public XmlDocument GetView(string Server, string Database, string User, string Password, string ViewName, string ParamNames, string ParamValues)
        {
            XmlDocument donendeger = new XmlDocument();

            DataSet ds = new DataSet("Views");
            DataTable dt = new DataTable(ViewName);
            string connString = "Server=" + Server + "; Database=" + Database + "; User Id=" + User + "; Password=" + Password + "; Trusted_Connection=False;";

            ArrayList paramn = new ArrayList();
            ArrayList paramv = new ArrayList();
            if (ParamNames != string.Empty)
            {
                string[] paramN = ParamNames.Split(new string[] { ";" }, StringSplitOptions.None);
                string[] paramV = ParamValues.Split(new string[] { ";" }, StringSplitOptions.None);
                for (int i = 0; i < paramN.Length; i++)
                {
                    paramn.Add(paramN[i]);
                    paramv.Add(paramV[i]);
                }
            }

            dt = WCFdata(connString, "SELECT * FROM [" + ViewName + "] ", paramn, paramv);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (dt.Columns[j].DataType == typeof(string) && dt.Rows[i][j].ToString() == "")
                        dt.Rows[i][j] = "";
                }
            }
            ds.Tables.Add(dt);
            donendeger.LoadXml(ds.GetXml());

            return donendeger;
        }

        public DataTable WCFdata(string ConnectionString, string CommandText, ArrayList ParameterNames, ArrayList Parameters)
        {
            DataTable dt = new DataTable();

            string where = string.Empty;
            bool var = false;
            for (int i = 0; i < ParameterNames.Count; i++)
            {
                if (ParameterNames[i].ToString().Length > 0)
                {
                    var = true;
                    if (i == 0)
                        where = " WHERE ";
                    where += "[" + ParameterNames[i] + "] = @" + ParameterNames[i] + " AND ";
                }
            }
            where = var ? where.Substring(0, where.Length - 5) : where;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(CommandText + where, conn);
                da.SelectCommand.CommandTimeout = 1000;
                for (int i = 0; i < Parameters.Count; i++)
                    da.SelectCommand.Parameters.AddWithValue(ParameterNames[i].ToString(), Parameters[i].ToString());
                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                catch (SqlException ex)
                {
                    EventLog.WriteEntry("sultanlar bayiservis", ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return dt;
        }
    }

    public class ServiceAuthenticator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("username");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            if (userName != "deneme" && password != "deneme")
                throw new SecurityTokenException("Kullanıcı adı-parola yanlış.");
        }
    }
}
