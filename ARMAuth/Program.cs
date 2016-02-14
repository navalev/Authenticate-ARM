using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ARMAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("enter tenant id: ");
            string tenantId = Console.ReadLine();
            Console.WriteLine("enter user name: ");
            string username = Console.ReadLine();
            Console.WriteLine("enter application info file full path: ");
            string appInfoFile = Console.ReadLine();

            AuthenticationResult result = getToken(tenantId, username);
            string authToken = "Bearer " + result.AccessToken;
            string appId = createApplication(tenantId, authToken, appInfoFile);
            JObject serPrinv = createServicePrinciple(tenantId, appId, authToken);
            string resourceId = serPrinv.GetValue("objectId").ToString();
            string roleId = getAdminRoleId(serPrinv);
            assignServicePrinciple(tenantId, result.UserInfo.UniqueId, resourceId, roleId, authToken);                     
        }

        private static string getAdminRoleId(JObject serPrinv)
        {
            foreach (JObject role in serPrinv["appRoles"])
            {
                if (role["value"].ToString().Equals("Admin"))
                {
                    return role["id"].ToString();
                }
            }
            return "";
        }

        private static void assignServicePrinciple(string tenantId, string uniqueId, string resourceId, string roleId, string authToken)
        {
            string uri = String.Format("https://graph.windows.net/{0}/users/{1}/appRoleAssignments?api-version=1.6", tenantId, uniqueId);
             WebRequest serviceRequest = WebRequest.Create(uri);            
            string body = @"{""id"" : """ + roleId + @""", ""principalId"" : """ + uniqueId + @""", ""resourceId"" : """ + resourceId + @"""}";            

            serviceRequest.Method = "POST";

            serviceRequest.ContentType = "application/json";
            serviceRequest.Headers.Add("Authorization", authToken);

            Stream requestStream = serviceRequest.GetRequestStream();

            requestStream.Write(Encoding.ASCII.GetBytes(body), 0, body.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)serviceRequest.GetResponse();

            string res = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.WriteLine(res);
            Debug.WriteLine(response.StatusCode);
        }

        public static AuthenticationResult getToken(string tenantId, string userName)
        {                 
            var authContext = new AuthenticationContext(String.Format("https://login.windows.net/{0}", tenantId));
            var user = new UserIdentifier(userName, UserIdentifierType.OptionalDisplayableId);
            // get a token for - using the powershell clientid (1950a258-227b-4e31-a9cf-717495945fc2)
            var result = authContext.AcquireToken("https://graph.windows.net/", "1950a258-227b-4e31-a9cf-717495945fc2", new Uri("urn:ietf:wg:oauth:2.0:oob"), PromptBehavior.Auto, user);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain token");
            }

            return result;            
        }

        public static string createApplication(string tenanatId, string authToken, string appInfoFile)
        {
            // create an ad application
            string json = File.ReadAllText(appInfoFile);
            WebRequest theRequest = WebRequest.Create(String.Format("https://graph.windows.net/{0}/applications?api-version=1.6", tenanatId));
            theRequest.Method = "POST";

            theRequest.ContentType = "application/json";
            theRequest.Headers.Add("Authorization", authToken);

            Stream requestStream = theRequest.GetRequestStream();

            requestStream.Write(Encoding.ASCII.GetBytes(json), 0, json.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)theRequest.GetResponse();

            string application = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.WriteLine(application);
            Debug.WriteLine(response.StatusCode);

            var objects = JObject.Parse(application);
            var appId = objects.GetValue("appId");

            return appId.ToString();
        }

        public static JObject createServicePrinciple(string tenantId, string appId, string authToken)
        {
            WebRequest serviceRequest = WebRequest.Create(String.Format("https://graph.windows.net/{0}/servicePrincipals?api-version=1.6", tenantId));
            string body = @"{""appId"" : """ + appId + @"""}";

            serviceRequest.Method = "POST";

            serviceRequest.ContentType = "application/json";
            serviceRequest.Headers.Add("Authorization", authToken);

            Stream requestStream = serviceRequest.GetRequestStream();

            requestStream.Write(Encoding.ASCII.GetBytes(body), 0, body.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)serviceRequest.GetResponse();

            string res = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.WriteLine(res);
            Debug.WriteLine(response.StatusCode);

            var objects = JObject.Parse(res);
            return objects;
        }

    }

    

}
