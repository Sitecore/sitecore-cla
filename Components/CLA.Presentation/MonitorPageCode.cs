namespace Sitecore.Cla.Presentation
{
  using System;
  using Sitecore.Data;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The Monitor page code.
  /// </summary>
  public class MonitorPageCode : PageCodeBase
  {
    #region Fields

    public Rendering MonitorEngagementFrame { get; set; }

    public Rendering BtnFullScreenMonitor { get; set; }    

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      try
      {
        var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
        var referer = WebUtil.GetQueryString("id");
        var item = masterdp.GetItem(new ID(referer));

        var engagementPalnId = item["Engagement Plan"];
        if (!string.IsNullOrEmpty(engagementPalnId) && this.MonitorEngagementFrame != null)
        {
          string url = "/sitecore/shell/Applications/MarketingAutomation/Monitor/MarketingAutomationMonitor.aspx?Id=";
          url += engagementPalnId;
          this.MonitorEngagementFrame.Parameters["SourceUrl"] = url;

          BtnFullScreenMonitor.Parameters["Click"] = "javascript:app.openEngagementDialogEvent('" + url + "')";
        }
      }
      catch
      { }
    }

    #endregion

  }
}