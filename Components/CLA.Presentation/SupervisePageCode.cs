namespace Sitecore.Cla.Presentation
{
  using System;
  using Sitecore.Data;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The Supervise page code.
  /// </summary>
  public class SupervisePageCode : PageCodeBase
  {
    #region Fields

    public Rendering SuperviseEngagementFrame { get; set; }

    public Rendering BtnFullScreenSupervise { get; set; }    

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
        if (!string.IsNullOrEmpty(engagementPalnId) && this.SuperviseEngagementFrame != null)
        {
          string url = "/sitecore/shell/Applications/MarketingAutomation/Supervisor/MarketingAutomationSupervisor.aspx?Id=";
          url += engagementPalnId;
          this.SuperviseEngagementFrame.Parameters["SourceUrl"] = url;

          BtnFullScreenSupervise.Parameters["Click"] = "javascript:app.openEngagementDialogEvent('" + url + "')";
        }
      }
      catch
      { }
    }

    #endregion

  }
}