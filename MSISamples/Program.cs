using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Net;
using System.Web.Script.Serialization;

// Azure Management dependencies
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var port = Environment.GetEnvironmentVariable("MSI_PORT");
            if (new List<string> { tenantId, port, subscriptionId }.Any(i => String.IsNullOrEmpty(i)))
           {
                Console.WriteLine("Please provide ENV vars for AZURE_TENANT_ID, MSI_PORT and AZURE_SUBSCRIPTION_ID.");
           }
           else
           {
                RunSample(tenantId, port, subscriptionId);
           }
        }

        public static void RunSample(string tenantId, string port, string subscriptionId)
        {
            string authority = "https://login.microsoftonline.com/" + tenantId;
            string address = string.Format("http://localhost:{0}/oauth2/token?resource={1}&authority={2}",
               port,
               Uri.EscapeDataString("https://management.azure.com/"),
               Uri.EscapeDataString(authority));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            StreamReader objReader = new StreamReader(request.GetResponse().GetResponseStream());
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, string>>(objReader.ReadLine());

            Write("Access token for ARM");
            Write(dict["access_token"]);


            // Intialize SDK using the token
            var credentials = new TokenCredentials(dict["access_token"]);
            var resourceClient = new ResourceManagementClient(credentials);

            resourceClient.SubscriptionId = subscriptionId;

            // List the resource group where VM has access to 
            Write("Listing resource groups:");
            resourceClient.ResourceGroups.List().ToList().ForEach(rg =>
            {
                Write("\tName: {0}, Id: {1}", rg.Name, rg.Id);
            });
            Write(Environment.NewLine);
            
        }

        private static void Write(string format, params object[] items)
        {
            Console.WriteLine(String.Format(format, items));
        }
    }
}