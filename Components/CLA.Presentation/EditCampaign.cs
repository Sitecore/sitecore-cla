// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditCampaign.cs" company="">
//   
// </copyright>
// <summary>
//   The edit campaign.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Presentation
{
  using System;
  using Sitecore.Data;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The edit campaign.
  /// </summary>
  public class EditCampaign : PageCodeBase
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the general alias text box.
    /// </summary>
    public Rendering CampaignInfoName { get; set; }

    public Rendering BackButton { get; set; }

    public readonly string RootUrl = "/sitecore/client/sitecore/applications/cla/";

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      var itemId = WebUtil.GetQueryString("id");
      var referer = WebUtil.GetQueryString("ref");

      var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");

      var item = masterdp.GetItem(new ID(itemId));

      this.CampaignInfoName.Parameters["Text"] = item.Fields["Name"].ToString();
      if (referer.ToLowerInvariant() == "AllCampaigns".ToLowerInvariant())
      {
        this.BackButton.Parameters["NavigateUrl"] = this.RootUrl + "AllCampaigns";
      }
    }
    #endregion
  }
}