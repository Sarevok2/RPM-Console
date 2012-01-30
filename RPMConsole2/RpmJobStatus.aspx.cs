using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace RPMConsole2
{
    /// <summary>
    /// This class shows the status of a specific batch job, including all job tasks.
    /// It is a partial page intended to be called from an AJAX request.
    /// </summary>
    public partial class RpmJobStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Reads the request paramenters and calls the getRpmJobStatus method to get the 
        /// job status
        /// </summary>
        /// <returns>String with the resulting HTML</returns>
        public string createJobStatusTable()
        {
            try
            {
                string systemString = Request.QueryString["system"];
                int batchId = Convert.ToInt32(Request.QueryString["batchid"]);

                if (systemString == null) throw new Exception("invalid/Missing parameters");

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

                return getRpmJobStatus(system, batchId);
            }
            catch (Exception ex) { return ex.Message; }
        }

        /// <summary>
        /// Gets the the job status from the RPM page and returns it.  Also calculates the total duration.
        /// </summary>
        /// <param name="system">The RPM system (bdsprod, sms12, etc.)</param>
        /// <param name="batchId">The batch ID of the job</param>
        /// <returns>String with the resulting HTML</returns>
        public string getRpmJobStatus(RpmConnect.RpmSystem system, int batchId)
        {
            string parameters = "BatchTaskId=" + batchId;

            string html = Global.m_rpmConnect.getRpmPage(system, RpmConnect.PageType.jobstatus, parameters);
            html = "<div class='jobstatustable'>" + html + "</div>";

            MatchCollection matches = Regex.Matches(html, @"([0-9]{0,9})\.[0-9]{0,3}secs", RegexOptions.None);
            int totalDuration = 0;
            string totalDurationString;
            try
            {
                foreach (Match match in matches)
                {
                    totalDuration += Convert.ToInt32(match.Groups[1].Value);
                }
                totalDurationString = formatTime(totalDuration);
            }
            catch (FormatException) { totalDurationString = "Error parsing durations"; }

            html += "<p class='duration'>Total Duration: " + totalDurationString +"</p>"; 

            
            return html;
        }

        /// <summary>
        /// Formats a time in seconds to a string with hours, minutes, and seconds.
        /// </summary>
        /// <param name="seconds">Input time in seconds</param>
        /// <returns>Resulting string</returns>
        public string formatTime(int seconds)
        {
	        int hours = seconds / 3600;
	        int minutes = seconds / 60 % 60;
	        seconds = seconds % 60;
	
	        string timeString = seconds + "s ";
	        if (hours > 0) timeString = hours + "h " + minutes + "m " + timeString;
	        else if (minutes > 0) {timeString = minutes + "m " + timeString;}

	        return timeString;
        }

    }
}