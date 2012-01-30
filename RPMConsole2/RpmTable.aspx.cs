using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Net;

namespace RPMConsole2
{
    /// <summary>
    /// This page shows a table of the RPM console.  It is a partial page intended to 
    /// be called from an AJAX request
    /// </summary>
    public partial class RpmTable : System.Web.UI.Page
    {
        

        /// <summary>
        /// Constructor
        /// </summary>
        public RpmTable()
        {
            
        }

        /// <summary>
        /// Called when page loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string systemString = Request.QueryString["system"];
                string pageTypeString = Request.QueryString["pagetype"];
                int pagesBack = Convert.ToInt32(Request.QueryString["pagesback"]);
                bool submitted = Convert.ToBoolean(Request.QueryString["submitted"]);
                bool running = Convert.ToBoolean(Request.QueryString["running"]);
                bool complete = Convert.ToBoolean(Request.QueryString["complete"]);
                bool aborted = Convert.ToBoolean(Request.QueryString["aborted"]);
                bool deleted = Convert.ToBoolean(Request.QueryString["deleted"]);
                int maxPages = Convert.ToInt32(Request.QueryString["maxpages"]);
                string searchString = Request.QueryString["searchstring"];

                if (systemString == null || pageTypeString == null) throw new Exception("Invalid/Missing parameters");
                
                RpmConnect.RpmSystem system;
                if (systemString.Equals("bdsprod")) system = RpmConnect.RpmSystem.bdsprod;
                else if (systemString.Equals("bds12")) system = RpmConnect.RpmSystem.bds12;
                else if (systemString.Equals("bds13")) system = RpmConnect.RpmSystem.bds13;
                else if (systemString.Equals("smsprod")) system = RpmConnect.RpmSystem.smsprod;
                else if (systemString.Equals("sms12")) system = RpmConnect.RpmSystem.sms12;
                else if (systemString.Equals("sms13")) system = RpmConnect.RpmSystem.sms13;
                else if (systemString.Equals("bds01")) system = RpmConnect.RpmSystem.bds01;
                else if (systemString.Equals("sms01")) system = RpmConnect.RpmSystem.sms01;
                else throw new Exception("Invalid/Missing parameters");


                RpmConnect.PageType pageType;
                if (pageTypeString.Equals("batch")) pageType = RpmConnect.PageType.batch;
                else if (pageTypeString.Equals("report")) pageType = RpmConnect.PageType.report;
                else throw new Exception("Invalid/Missing parameters");


                string result;
                if (searchString == null) result = getRpmTable(system, pageType, pagesBack, submitted, running, complete, aborted, deleted);
                else result = search(searchString, maxPages, system, pageType, pagesBack, submitted, running, complete, aborted, deleted);
                Response.Write(result);
            }
            catch (Exception ex) { Response.Write(ex.Message); }
            
        }

        /// <summary>
        /// Gets the table from an RPM page and returns the HTML
        /// </summary>
        /// <param name="system">The system (bdsprod, sms12, etc.)</param>
        /// <param name="pageType">Page type (report, batch, or jobstatus)</param>
        /// <param name="pagesBack">Page number (1 is newest)</param>
        /// <param name="submitted">submitted checkbox</param>
        /// <param name="running">running checkbox</param>
        /// <param name="complete">complete checkbox</param>
        /// <param name="aborted">aborted checkbox</param>
        /// <param name="deleted">deleted checkbox</param>
        /// <returns>HTML of the table</returns>
        public string getRpmTable(RpmConnect.RpmSystem system, RpmConnect.PageType pageType, int pagesBack, bool submitted, bool running, 
            bool complete, bool aborted, bool deleted)
        {
            string postData = "pageNo=" + pagesBack;
            if (submitted) postData += "&scheduled=on";
            if (running) postData += "&running=on";
            if (complete) postData += "&complete=on";
            if (aborted) postData += "&aborted=on";
            if (deleted) postData += "&deleted=on";

            string html = Global.m_rpmConnect.getRpmPage(system, pageType, postData);
            html = html.Replace("href=\"batchsubtask?BatchTaskId=", "onclick=\"showJobStatusDialog(this);\" href=\"#");

            return html;
        }

        /// <summary>
        /// Searches for the search string.  It searches the entire page, not just job name.
        /// </summary>
        /// <param name="searchString">The string to search for.  Note that it will search for this exact string, not each word</param>
        /// <param name="maxPages">The maximum number of pages to search</param>
        /// <param name="system">The system (bdsprod, sms12, etc.)</param>
        /// <param name="pageType">Page type (report, batch, or jobstatus)</param>
        /// <param name="pagesBack">The page number to start at</param>
        /// <param name="submitted">submitted checkbox</param>
        /// <param name="running">running checkbox</param>
        /// <param name="complete">complete checkbox</param>
        /// <param name="aborted">aborted checkbox</param>
        /// <param name="deleted">deleted checkbox</param>
        /// <returns>HTML of the page in which the result was found, or a message saying it was not found</returns>
        public string search(string searchString, int maxPages, RpmConnect.RpmSystem system, RpmConnect.PageType pageType, int pagesBack, 
                    bool submitted, bool running, bool complete, bool aborted, bool deleted)
        {
            for (int i = pagesBack; i <= pagesBack + maxPages; i++)
            {
                string html = getRpmTable(system, pageType, i, submitted, running, complete, aborted, deleted);

                if (html.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) return "<pageno>" + i + "</pageno>" + html;

            }
            return "<pageno>1</pageno><p>Job Not Found</p>";
        }
    }
}