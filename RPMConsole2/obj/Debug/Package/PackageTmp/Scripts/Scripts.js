var tabLinks = new Array();
var contentDivs = new Array();
var selectedTab = "bdsprod";
var currentPages = new Array();

var tabText = {
    "bdsprod": "BDS Prod",
    "smsprod": "SMS Prod",
    "bds12": "BDS 12",
    "sms12": "SMS 12",
    "bds13": "BDS 13",
    "sms13": "SMS 13",
    "bds01": "BDS 01",
    "sms01": "SMS 01"
};

$(function () {
    $("#searchDialog").dialog({
        autoOpen: false,
        minHeight: 140,
        minWidth: 350,
        modal: true
    });
    $("#batchJobStatusDialog").dialog({
        autoOpen: false,
        minWidth: 850,
        modal: true
    });
});


function init() {
    // Grab the tab links and content divs from the page
    var tabListItems = document.getElementById('systemtabs').childNodes;
    for (var i = 0; i < tabListItems.length; i++) {
        if (tabListItems[i].nodeName == "LI") {
            var tabLink = getFirstChildWithTagName(tabListItems[i], 'A');

            var id = getHash(tabLink.getAttribute('href'));
            tabLinks[id] = tabLink;
            contentDivs[id + "batch"] = document.getElementById(id + "batch");
            contentDivs[id + "report"] = document.getElementById(id + "report");
        }
    }

    // Assign onclick events to the tab links, and highlight the first tab
    var i = 0;
    for (var id in tabLinks) {
        tabLinks[id].onclick = changeTab;
        tabLinks[id].onfocus = function () { this.blur() };
        if (i == 0) tabLinks[id].className = 'selected';
        i++;

        currentPages[id] = new Array();
        currentPages[id]["batch"] = 1;
        currentPages[id]["report"] = 1;
    }

    // Hide all content divs except the first
    var i = 0;
    for (var id in contentDivs) {
        if (i != 0) {
            contentDivs[id].className = 'systemTabContent hide';
        }
        i++;
    }

}

function changeTab() {
    selectedTab = getHash(this.getAttribute('href'));
    showTab();

    // Stop the browser following the link
    return false;
}

function showTab() {
    var suffix;
    if (document.getElementById("rd_batch").checked) { suffix = "batch"; }
    else { suffix = "report"; }

    // Highlight the selected tab, and dim all others.
    for (var tLink in tabLinks) {
        if (tLink == selectedTab) { tabLinks[tLink].className = 'selected'; } 
        else {tabLinks[tLink].className = '';}
    }

     // Show the selected content div, and hide all others.
    for (var cDiv in contentDivs) {
        if (cDiv == (selectedTab + suffix)) { contentDivs[cDiv].className = 'systemTabContent'; }
        else { contentDivs[cDiv].className = 'systemTabContent hide'; }
    }

    document.getElementById("currentPage").innerHTML = "Page: " + currentPages[selectedTab][suffix];
}

function getFirstChildWithTagName(element, tagName) {
    for (var i = 0; i < element.childNodes.length; i++) {
        if (element.childNodes[i].nodeName == tagName) return element.childNodes[i];
    }
}

function getHash(url) {
    var hashPos = url.lastIndexOf('#');
    return url.substring(hashPos + 1);
}

function refresh(pageDiff) {
    var pagetype;
    if (document.getElementById("rd_batch").checked) { pagetype = "batch"; }
    else { pagetype = "report"; }

    var pagesBack = currentPages[selectedTab][pagetype] + pageDiff;
    if (pagesBack < 1) { pagesBack = 1; }

    loadRpmTable(pagesBack);
}

function gotoPage() {
    var pagesBack = document.getElementById("tx_gotopage").value;

    if (pagesBack == parseInt(pagesBack) && pagesBack > 0) {
        pagesBack = parseInt(pagesBack);
        loadRpmTable(pagesBack)
    }
    else {
        alert("Invalid entry.  Please enter a number above 0");
    }
}

function search(form) {
    var searchString;
    var pagesBack=1;

    searchString = form.searchbox.value;
    pagesBack = form.startpage.value;

    if (searchString == "") { alert("Invalid Search String"); }
    else if (pagesBack != parseInt(pagesBack) || pagesBack <= 0) { alert("Invalid start page.  Must be an integer above 0"); }
    else {
        $("#searchDialog").dialog("close");
        var maxpages = form.maxpages.options[form.maxpages.selectedIndex].value;
        getTable(selectedTab, pagesBack, searchString, maxpages);
    }
    
    return false;
}

function loadRpmTable(pagesBack) {
    getTable(selectedTab, pagesBack, null, 1);

    return false;
}

function refreshAll() {
    getTable("bdsprod", 1, null, 1);
    getTable("smsprod", 1, null, 1);
    getTable("bds12", 1, null, 1);
    getTable("sms12", 1, null, 1);
    getTable("bds13", 1, null, 1);
    getTable("sms13", 1, null, 1);
}

function getTable(tabName, pagesBack, searchString, maxSearchPages) {
    var submitted = "false", running = "false", complete = "false", aborted = "false", deleted = "false";

    if (document.getElementById("ck_submitted").checked) { submitted = "true"; }
    if (document.getElementById("ck_running").checked) { running = "true"; }
    if (document.getElementById("ck_complete").checked) { complete = "true"; }
    if (document.getElementById("ck_aborted").checked) { aborted = "true"; }
    if (document.getElementById("ck_deleted").checked) { deleted = "true"; }

    var pagetype;
    if (document.getElementById("rd_batch").checked) { pagetype = "batch"; }
    else { pagetype = "report"; }

    var returnFunction = function (text) {
        if (searchString != null) {
            var textParts = text.split("</pageno>");
            pagesBack = parseInt(textParts[0].split("<pageno>")[1]);
            text = textParts[1];
        }
        contentDivs[tabName + pagetype].innerHTML = text;
        currentPages[tabName][pagetype] = pagesBack;
        
        document.getElementById("currentPage").innerHTML = "Page: " + pagesBack;
        document.getElementById("a_" + tabName).innerHTML = tabText[tabName];
    }
    var startString;
    if (searchString == null) { startString = "rpmtable.aspx?"; }
    else { startString = "rpmtable.aspx?maxpages=" + maxSearchPages + "&searchString=" + searchString + "&"; }
    var requestString = startString + "pagesback=" + pagesBack + "&system=" + tabName + "&pagetype=" + pagetype +
        "&submitted=" + submitted + "&running=" + running + "&complete=" + complete + "&aborted=" + aborted + "&deleted=" + deleted;
    ajaxRequest(requestString, returnFunction);

    document.getElementById("a_" + tabName).innerHTML = "<img style='border: none' src='/Content/Images/progress2.gif' />" + tabText[tabName];
}

function showJobStatusDialog(linkTag) {
    var returnFunction = function (text) {   
        document.getElementById('batchJobStatusDialog').innerHTML = text;
        $('#batchJobStatusDialog').dialog('open');
    }

    var requestString = "rpmjobstatus.aspx?system=" + selectedTab + "&batchId=" + getHash(linkTag.getAttribute("href"));
    ajaxRequest(requestString, returnFunction);

    return false;
}

function ajaxRequest(requestString, returnFunction) {
    var xmlhttp;
    if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari
        xmlhttp = new XMLHttpRequest();
    }
    else {// code for IE6, IE5
        xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
    }
    xmlhttp.onreadystatechange = xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            returnFunction(xmlhttp.responseText);
        }
        //else { alert(xmlhttp.responseText + " " + xmlhttp.status); }
    }

    xmlhttp.open("GET", encodeURI(requestString), true);
    xmlhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xmlhttp.send();

    return false;
}

function updateSearchType(sType) {
    if (sType == "text") {
        document.getElementById('tx_searchbox').disabled = false;
        document.getElementById('sl_joblist').disabled = true;
    }
    else if (sType == "list") {
        document.getElementById('tx_searchbox').disabled = true;
        document.getElementById('sl_joblist').disabled = false;
    }
}





