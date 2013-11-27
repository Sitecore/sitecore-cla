namespace Sitecore.Cla.Presentation
{
  using System;

  using Sitecore.Cla.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web.PageCodes;

  public class ReportsPageCode : PageCodeBase
  {
    public Rendering VisitsFrame { get; set; }
    public Rendering ValuePerVisitFrame { get; set; }
    public Rendering CategoryFrame { get; set; }

    public override void Initialize()
    {
      try
      {
        var landingPage = new LandingPage(this.RenderingContext.PageContext.RequestContext.HttpContext.Request["id"]);

        var campaignId = landingPage.Campaign.ID;

        this.VisitsFrame.Parameters["SourceUrl"] = string.Format(
          "/sitecore/shell/applications/Reports/Dashboard/Dashboard.aspx?id={0}&defaultSettingsURL=/sitecore/shell/client/Sitecore/Applications/CLA/ExecutiveDashboard/Settings/ReportsSettingsVisits.config",
          System.Net.WebUtility.UrlEncode(campaignId.ToString()));
        this.ValuePerVisitFrame.Parameters["SourceUrl"] = string.Format(
          "/sitecore/shell/applications/Reports/Dashboard/Dashboard.aspx?id={0}&defaultSettingsURL=/sitecore/shell/client/Sitecore/Applications/CLA/ExecutiveDashboard/Settings/ReportsSettingsValuePerVisit.config",
          System.Net.WebUtility.UrlEncode(campaignId.ToString()));

        this.CategoryFrame.Parameters["SourceUrl"] = string.Format(
          "/sitecore/shell/applications/Reports/Dashboard/Dashboard.aspx?id={0}&defaultSettingsURL=/sitecore/shell/client/Sitecore/Applications/CLA/ExecutiveDashboard/Settings/LandingPageChannelPerformanceSettings.config",
          System.Net.WebUtility.UrlEncode(landingPage.LandingPageItem["CategoryId"]));

      }
      catch (Exception exception)
      {
        Log.Error(exception.Message, exception, this);
      }
    }
  }
}