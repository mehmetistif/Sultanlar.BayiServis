using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.ServiceModel.Web;
using System.IO;
using System.Drawing;
using System.Data;

namespace Sultanlar.BayiServis
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IGeneral" in both code and config file together.
    [ServiceContract]
    public interface IGeneral
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Test")]
        string Test();

        //
        [OperationContract, XmlSerializerFormat]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/Get?server={Server}&database={Database}&user={User}&password={Password}&viewname={ViewName}&paramn={ParamNames}&paramv={ParamValues}")]
        XmlDocument GetView(string Server, string Database, string User, string Password, string ViewName, string ParamNames, string ParamValues);
    }
}
