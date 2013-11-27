<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Sitecore.Shell.Applications.Reports.Dashboard" %>
<!DOCTYPE html>
<html>
<head id="Head1" runat="server">
    <title>Executive Insight Dashboard</title>	
    <link id="Link1" rel="stylesheet" href="/sitecore/shell/Applications/Reports/Dashboard/Styles/Style.css" type="text/css" runat="server" />
    <script type="text/javascript" src="/sitecore/shell/Applications/Reports/Dashboard/Scripts/Silverlight.js"></script>
    <script type="text/javascript" src="/sitecore/shell/Applications/Reports/Dashboard/Scripts/Reports.js"></script>  
</head>
<body>
    <form id="form1" runat="server" style="height:100%;">
      <div id="silverlightControlHost">
        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2" id="scSilverlightExecutiveDashboard" width="100%" height="100%">                                     
          <param name="source" value="/sitecore/shell/ClientBin/Sitecore.Shell.Dashboard.xap"/>                        
          <param name="onError" value="onSilverlightError"/>                    
          <asp:placeholder runat="server" id="Parameters"/>          
          <param name="minRuntimeVersion" value="5.0.61118.0"/>
          <param name="autoUpgrade" value="true"/>
          
          <%--CLA, yvy - set to "true", because with "false" value "object" covered all elements --%>
          <param name="windowless" value ="true"/>
          <param name="onLoad" value="PluginLoaded"/>             
          <div style="width: 100%; height: 100%;">
          <table cellpadding="0" cellspacing="0" border="0" align="center">
            <tr><td><h2>Executive Insight Dashboard</h2></td></tr>
            <tr>
              <td align="left">
                <p>
                  Executive Insight Dashboard requires Microsoft Silverlight 5 to run.<br />
                  Silverlight is a small, safe, cross-platform browser plugin created and supported by Microsoft.</p>
                <br />
                <div align="center">
                  <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration: none">
                    <img src="/sitecore/shell/ClientBin/SilverlightMedaglion.png" alt="Get Microsoft Silverlight" style="border-style: none" /></a>
                </div>
              </td>
            </tr>
          </table>
        </div></object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div></form></body>
</html>
