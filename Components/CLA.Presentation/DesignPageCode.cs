namespace Sitecore.Cla.Presentation
{
  using System;
  using Sitecore.Data;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The Design page code.
  /// </summary>
  public class DesignPageCode : PageCodeBase
  {
    #region Fields

    public Rendering DesignEngagementFrame { get; set; }

    public Rendering BtnFullScreenDesign { get; set; }    

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
        if (!string.IsNullOrEmpty(engagementPalnId) && this.DesignEngagementFrame != null)
        {
          string url = "/sitecore/shell/Applications/MarketingAutomation/Designer/MarketingAutomationDesigner.aspx?Id=";
          url += engagementPalnId;
          this.DesignEngagementFrame.Parameters["SourceUrl"] = url;

          BtnFullScreenDesign.Parameters["Click"] = "javascript:app.openEngagementDialogEvent('" + url + "')";
        }
      }
      catch 
      { }
    }

    #endregion

  }
}