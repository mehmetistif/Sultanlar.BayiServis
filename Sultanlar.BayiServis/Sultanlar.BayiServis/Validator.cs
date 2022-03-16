using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.IdentityModel.Tokens;

namespace Sultanlar.BayiServis
{
    public class Validator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new SecurityTokenException("Username and password required");
            
            if (userName != "mistif" && password != "123456")
                throw new FaultException(string.Format("Wrong username ({0}) or password ", userName));

        }
    }
}