<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RPMConsole2.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">


<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>RPM Console</title>
    <link rel="stylesheet" type="text/css" href="/Content/Site.css" />
    <!--[if IE 6]><link rel="stylesheet" type="text/css" href="/Content/SiteIE6.css" /><![endif]-->
    <link type="text/css" href="/Content/jquery-ui-1.8.11.custom.css" rel="Stylesheet" />	
    <script type="text/javascript" src="/Scripts/jquery-1.4.1.min.js"></script>
    <script type="text/javascript" src="/Scripts/jquery-ui-1.8.11.custom.min.js"></script>
    <script src="/Scripts/Scripts.js" type="text/javascript"></script>
</head>
<body onload="init()">
    <div id="searchDialog" title="Search">
        <form id="controlsForm" action="" onsubmit="return(search(this));">

            <table style="width: 100%">
                <tr>
                    <td>Search:</td>
                    <td><input type="text" name="searchbox" id="tx_searchbox" /></td>
                </tr>
                <tr>
                    <td>Start Page:</td>
                    <td><input type="text" name="startpage" id="tx_startpage" value="1" /></td>
                </tr>
                <tr>
                    <td>Max Pages: </td>
                    <td>
                        <select name="maxpages">
                            <option value="5">5</option>
                            <option value="10">10</option>
                            <option value="20">20</option>
                            <option value="50">50</option>
                        </select>
                    </td>
                </tr>

                <tr><td><input type="submit" value="Search"/></td></tr>
            </table>
        </form>
    </div>
    <div id="batchJobStatusDialog" title="Status"></div>
    
    <div>
        
        <div class="topHeader">
            <div class="topHeaderItem">
                <div class="maintitle">RPM Console</div>
            </div>  
                     
            <div class="topHeaderItem">    
                <table class="checkBoxTable">
                    <tr>
                        <td><input type="checkbox" id="ck_complete" />Complete</td>
                        <td><input type="checkbox" id="ck_submitted" />Submitted</td>
                    </tr>
                    <tr>
                        <td><input type="checkbox" id="ck_aborted" />Aborted</td>
                        <td><input type="checkbox" id="ck_running" />Running</td>
                    </tr>
                    <tr>
                        <td><input type="checkbox" id="ck_deleted" />Deleted</td>
                    </tr>
                </table>
            </div>  
            
            <div class="topHeaderItem">  
                <button onclick="refresh(0)" class="divButton topButton">Refresh</button><br />
                <button onclick="refreshAll()" class="divButton topButton">Refresh All</button><br />
                <button onclick="$('#searchDialog').dialog('open')" class="divButton topButton">Search</button>
            </div>   
        </div>
        <div class="bottomHeader">
             
            <ul>
                <li class="headerLeft"></li>
                <li class="headerMid">
                    <input class="bottomBarItem" type="radio" id="rd_batch" name="pagetype" value="batch" onclick="showTab()" checked="checked" />Batch
                    <input class="bottomBarItem" type="radio" id="rd_report" name="pagetype" value="report" onclick="showTab()" />Report
                </li>
                <li class="headerMid">
                    
                    <input type="text" class="bottomBarItem" id="tx_gotopage" style="width: 50px" />
                    <button onclick="gotoPage()" class="bottomBarItem divButton">Go To</button>
                    <span id="currentPage" class="bottomBarItem pageNum">Page: 1</span>
                </li>
                <li class="headerMid">         
                    <button onclick="loadRpmTable(1)" class="bottomBarItem divButton"><< First</button>
                    <button onclick="refresh(-1)" class="bottomBarItem divButton">< Back</button>
                    <button onclick="refresh(1)" class="bottomBarItem divButton">Next ></button>
                </li>
                <li class="headerRight"></li>
            </ul>            
        </div>

        <div class="contentArea">
            <ul class="tabs" id="systemtabs">
                <li><a id="a_bdsprod" href="#bdsprod">BDS Prod</a></li>
                <li><a id="a_smsprod" href="#smsprod">SMS Prod</a></li>
                <li><a id="a_bds12" href="#bds12">BDS 12</a></li>
                <li><a id="a_sms12" href="#sms12">SMS 12</a></li>
                <li><a id="a_bds13" href="#bds13">BDS 13</a></li>
                <li><a id="a_sms13" href="#sms13">SMS 13</a></li>
                <li><a id="a_bds01" href="#bds01">BDS 01</a></li>
                <li><a id="a_sms01" href="#sms01">SMS 01</a></li>
            </ul>
            
            <div class="systemTabContent" id="bdsprodbatch">
                <h2>BDS Prod Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bdsprodreport">
                <h2>BDS Prod Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="smsprodbatch">
                <h2>SMS prod batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="smsprodreport">
                <h2>SMS prod Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds12batch">
                <h2>BDS 12 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds12report">
                <h2>BDS 12 Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms12batch">
                <h2>SMS 12 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms12report">
                <h2>SMS 12 Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds13batch">
                <h2>BDS 13 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds13report">
                <h2>BDS 13 Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms13batch">
                <h2>SMS 13 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms13report">
                <h2>SMS 13 Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds01batch">
                <h2>BDS 01 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="bds01report">
                <h2>BDS 01 Report - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms01batch">
                <h2>SMS 01 Batch - Click refresh to update</h2>
            </div>
            <div class="systemTabContent" id="sms01report">
                <h2>SMS 01 Report - Click refresh to update</h2>
            </div>
        </div> 
    </div>
        
</body>
</html>
