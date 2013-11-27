using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Configuration
{
  public class ClaSettings
  {
    public static class Mvc
    {
      #region Public Properties

      /// <summary>
      /// Gets the command route prefix.
      /// </summary>
      [NotNull]
      public static string CommandRoutePrefix
      {
        get
        {
          return Settings.GetSetting("ClaSettings.Mvc.CommandRoutePrefix", "api/sitecore/cla/");
        }
      }

      #endregion
    }
  }
}
