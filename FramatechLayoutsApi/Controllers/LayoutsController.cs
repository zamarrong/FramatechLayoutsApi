using FramatechLayoutsApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Web.Hosting;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using System.Configuration;

namespace FramatechLayoutsApi.Controllers
{
    public class LayoutsController : ApiController
    {
        // POST api/layouts
        public async Task<HttpResponseMessage> Post([FromBody] LayoutParams layoutParams)
        {
            try
            {
                string path = Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), string.Format("{0}.rpt", layoutParams.DocCode));

                if (!File.Exists(Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), string.Format("{0}.rpt", layoutParams.DocCode))))
                {
                    Layout layout = await GetLayoutAsync(layoutParams.DocCode);
                    using (FileStream oFile = new FileStream(path, FileMode.Create))
                    {
                        oFile.Write(layout.Template.data, 0, layout.Template.data.Length);
                        oFile.Close();
                    }
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(path, OpenReportMethod.OpenReportByTempCopy);

                
                var conexion = new { Driver = Properties.Settings.Default.Driver, DataSource = Properties.Settings.Default.DataSource, InitialCatalog = Properties.Settings.Default.InitialCatalog, UserID = Properties.Settings.Default.UserID, Password = Properties.Settings.Default.Password };
                string strConnection = string.Format("DRIVER={0};UID={1};PWD={2};SERVERNODE={3};DATABASE={4};", "{" + conexion.Driver + "}", conexion.UserID, conexion.Password, conexion.DataSource, conexion.InitialCatalog);
                
                ConnectionInfo crConnectionInfo = new ConnectionInfo();

                crConnectionInfo.ServerName = conexion.DataSource;

                crConnectionInfo.AllowCustomConnection = true;

                crConnectionInfo.DatabaseName = conexion.InitialCatalog;

                crConnectionInfo.UserID = conexion.UserID;

                crConnectionInfo.Password = conexion.Password;

                int connections = rd.DataSourceConnections.Count;
                foreach (Table table in rd.Database.Tables)
                {

                    TableLogOnInfo tableLogonInfo = table.LogOnInfo;

                    tableLogonInfo.ConnectionInfo = crConnectionInfo;

                    table.ApplyLogOnInfo(tableLogonInfo);

                }

                for (int i = 0; i < connections; i++)
                {
                    NameValuePairs2 logon = rd.DataSourceConnections[i].LogonProperties;

                    logon.Set("Provider", conexion.Driver);
                    logon.Set("Server Type", conexion.Driver);
                    logon.Set("Connection String", strConnection);

                    rd.DataSourceConnections[i].SetLogonProperties(logon);
                    rd.DataSourceConnections[i].SetConnection(conexion.DataSource, conexion.InitialCatalog, false);
                }

                foreach (KeyValuePair<string, string> pair in layoutParams.Pairs.ToList())
                {
                    rd.SetParameterValue(pair.Key, pair.Value);
                }

                path = Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), string.Format("{0}-{1}.pdf", layoutParams.DocCode, layoutParams.Pairs.First().Value));

                rd.ExportToDisk(ExportFormatType.PortableDocFormat, path);
                rd.Close();
                rd.Dispose();

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                response.Content = new StreamContent(new FileStream(@path, FileMode.Open, FileAccess.Read));
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = string.Format("{0}.pdf", layoutParams.DocCode);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                return response;
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(ex.Message);

                return response;
            }
        }

        static async Task<Layout> GetLayoutAsync(string docCode)
        {
            HttpClient client = new HttpClient();
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.DefaultRequestHeaders.Add("Authorization", "");
            Layout layout = null;
            HttpResponseMessage response = await client.GetAsync(string.Format("https://api.com/api/hana/layouts?DocCode={0}", docCode));
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                layout = JsonConvert.DeserializeObject<Layout>(jsonString);
            }

            return layout;
        }
    }
}
