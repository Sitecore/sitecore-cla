using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Cla.Application.Helpers
{
  public static class DashboardHelper
  {

    public const string DashboardUrl = "/sitecore/shell/Applications/Reports/Dashboard/Dashboard.aspx?defaultSettingsURL=";
    /// <summary>
    /// Return the approriate URL based on parameter passed
    /// </summary>
    /// <returns>A string representing a URL</returns>
    public static string GetUrl(string settingUrl) {
      return DashboardUrl  + settingUrl;
    }
  }
}
