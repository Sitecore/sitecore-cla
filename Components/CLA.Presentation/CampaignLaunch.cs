// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CampaignLaunch.cs" company="">
//   
// </copyright>
// <summary>
//   The campaign launch.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Presentation
{
  using Sitecore.Cla.Application.Helpers;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The campaign launch.
  /// </summary>
  public class CampaignLaunch : PageCodeBase
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the frame.
    /// </summary>
    public Rendering Frame { get; set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      this.Frame.Parameters["SourceUrl"] = DashboardHelper.GetUrl(DashboardSettings.DefaultSettings);

    }

    #endregion
  }
}