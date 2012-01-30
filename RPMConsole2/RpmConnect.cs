using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

namespace RPMConsole2
{
    /// <summary>
    /// This class handles the connection to the RPM website, including logging in and downloading
    /// all the pages.
    /// </summary>
    public class RpmConnect
    {
        public enum RpmSystem { bdsprod, smsprod, bds12, sms12, bds13, sms13, bds01, sms01 };
        public enum PageType { batch, report, jobstatus };

        private const string rpmurl = ".cmamdm.enterprise.corp/rpm/console/console-servlet/";
        private const string overviewurl = "overview";
        private const string batchSummaryurl = "batchsummary";
        private const string reporturl = "servicelevels;report";
        private const string batchTaskurl = "batchsubtask";
        private const string rpmUser = "rpm";
        private const string rpmPass = "open";
        private const string cookieJarFileName = "cookiejar.dat";
        private const string logFileName = "D:\\inetpub\\live\\RPMConsole\\rpmconsole.log";
        private const int rpmSessionTimeout = 10;//minutes

        private Dictionary<RpmSystem, string> systemUrls;
        private Dictionary<RpmSystem, DateTime> sessionAges;

        //According to Microsft, the CookieContainer class is guaranteed thread safe only if it's public static
        public static CookieContainer cookieJar;
        private StreamWriter logWriter;
        private static readonly object logLocker = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public RpmConnect()
        {
            systemUrls = new Dictionary<RpmSystem, string>();
            systemUrls.Add(RpmSystem.bdsprod, "http://bdsprod");
            systemUrls.Add(RpmSystem.smsprod, "http://smsprod");
            systemUrls.Add(RpmSystem.bds12, "http://itbds12");
            systemUrls.Add(RpmSystem.sms12, "http://itsms12");
            systemUrls.Add(RpmSystem.bds13, "http://itbds13");
            systemUrls.Add(RpmSystem.sms13, "http://itsms13");
            systemUrls.Add(RpmSystem.bds01, "http://ftbds01");
            systemUrls.Add(RpmSystem.sms01, "http://ftsms01");

            sessionAges = new Dictionary<RpmSystem, DateTime>();
            sessionAges.Add(RpmSystem.bdsprod, new DateTime(0));
            sessionAges.Add(RpmSystem.smsprod, new DateTime(0));
            sessionAges.Add(RpmSystem.bds12, new DateTime(0));
            sessionAges.Add(RpmSystem.sms12, new DateTime(0));
            sessionAges.Add(RpmSystem.bds13, new DateTime(0));
            sessionAges.Add(RpmSystem.sms13, new DateTime(0));
            sessionAges.Add(RpmSystem.bds01, new DateTime(0));
            sessionAges.Add(RpmSystem.sms01, new DateTime(0));

            cookieJar = new CookieContainer();
        }

        /// <summary>
        /// Gets a pages from the RPM website, automatically logging in if necessary
        /// </summary>
        /// <param name="system">The system we're getting the page from (bdsprod, sms12, etc.)</param>
        /// <param name="pageType">The type of page.  This is either batch, report, or jobstatus</param>
        /// <param name="parameters">A string with the parameters for the get or post request</param>
        /// <returns>A string with the HTML of the result</returns>
        public string getRpmPage(RpmSystem system, PageType pageType, string parameters) 
        {
            logit("Loading page " + system + " " + pageType);
            initSession(system);

            string endUrl = "";
            if (pageType == PageType.batch) endUrl = batchSummaryurl;
            else if (pageType == PageType.report) endUrl = reporturl;
            else if (pageType == PageType.jobstatus) endUrl = batchTaskurl;

            string targetUri = systemUrls[system] + rpmurl + endUrl;
           
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    HttpWebResponse response;
                    if (pageType == PageType.jobstatus) response = getRequest(targetUri, parameters);
                    else response = postRequest(targetUri, parameters);

                    System.IO.StreamReader str = new System.IO.StreamReader(response.GetResponseStream());

                    string html = str.ReadToEnd();
                    if (str != null) str.Close();//TODO - should throw exception
                    html = extractDataTable(html);

                    logit("Done Loading " + system + " " + pageType);
                    return html;
                }
                catch (Exception e) 
                {
                    logit("Failed to retrieve page.  Retrying login and load page: " + e.Message);
                    rpmLogin(system); 
                }
            }
            
            return null;
        }

        /// <summary>
        /// Writes a message to log file
        /// </summary>
        /// <param name="text">Text to log</param>
        public void logit(string text)
        {
            lock (logLocker)
            {
                string timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                logWriter = File.AppendText(logFileName);
                logWriter.WriteLine(timestamp + " - " + text);
                logWriter.Close();
            }
        }

        /// <summary>
        /// Logs into the RPM console of the given system
        /// </summary>
        /// <param name="system">The system we're logging in to (bdsprod, sms12, etc.)</param>
        private void rpmLogin(RpmSystem system)
        {
            string targetUri = systemUrls[system] + rpmurl + overviewurl;
            string postData = "Username=" + rpmUser + "&Password=" + rpmPass + "&Start=OK";
            HttpWebResponse response = postRequest(targetUri, postData);

            logit("Logged in " + system + ". HTTP response code is " + response.StatusCode);
        }
            
        
        /// <summary>
        /// Extracts the main table from an RPM page, getting rid of the rest of the page, 
        /// such as the header, etc..
        /// </summary>
        /// <param name="html">The html of the RPM page</param>
        /// <returns>The resulting table in HTML format</returns>
        private string extractDataTable(string html)
        {
            string[] seperator = new string[] { "<TABLE width=\"100%\" border=\"0\" >" };
            string[] seperator2 = new string[] { "</TABLE>" };
            string[] values = html.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            string resultHtml = "<script>var tableArray = new Array();</script>";
            for (int i = 1; i < values.Length; i++)
            {
                string[] values2 = values[i].Split(seperator2, StringSplitOptions.RemoveEmptyEntries);
                resultHtml += "<table class='rpmtable'>" + values2[0] + "</table>";
            }
            return resultHtml;
        }

        /// <summary>
        /// Checks if it's necessary to login (session expired) and if it is, log in.
        /// </summary>
        /// <param name="system">Which system (bdsprod, sms12, etc.)</param>
        private void initSession(RpmSystem system)
        {
            int comparison = DateTime.Compare(DateTime.Now - new TimeSpan(0, rpmSessionTimeout, 0), sessionAges[system]);
            if (comparison > 0) rpmLogin(system);
            else logit("Session still active on " + system + " so no need to login.");
            sessionAges[system] = DateTime.Now;
        }

        /// <summary>
        /// Does an HTML get request
        /// </summary>
        /// <param name="uriString">The URL to request, not including parameters</param>
        /// <param name="parameters">A string with all the parameters of the request</param>
        /// <returns>The HTML of the resulting page</returns>
        private HttpWebResponse getRequest(string uriString, string parameters)
        {
            Uri uri = new Uri(uriString + "?" + parameters);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
            request.CookieContainer = cookieJar;
            request.Method = "GET";

            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Does an HTML post request
        /// </summary>
        /// <param name="uriString">The URL to request</param>
        /// <param name="parameters">A string with all the parameters of the request</param>
        /// <returns>The HTML of the resulting page</returns>
        private HttpWebResponse postRequest(string uriString, string parameters)
        {
            Uri uri = new Uri(uriString);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
            request.CookieContainer = cookieJar;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byteArray = encoding.GetBytes(parameters);
            request.ContentLength = byteArray.Length;

            try
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            catch (Exception e) { logit("Error doing post request: " + e.Message); }

            return (HttpWebResponse)request.GetResponse();
        }

    }
}