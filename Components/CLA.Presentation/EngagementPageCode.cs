// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngagementPageCode.cs" company="">
//   
// </copyright>
// <summary>
//   The engagement page code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Presentation
{
  using Sitecore.Data;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The engagement page code.
  /// </summary>
  public class EngagementPageCode : PageCodeBase
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the engagement accordion.
    /// </summary>
    public Rendering EngagementAccordion { get; set; }
    public Rendering EPAccordionBorder { get; set; }


    public Rendering EngagmentAttachBorder { get; set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
      var referer = WebUtil.GetQueryString("id");
      var item = masterdp.GetItem(new ID(referer));

      var engagementPalnId = item["Engagement Plan"];
      if (string.IsNullOrEmpty(engagementPalnId))
      {
        this.EPAccordionBorder.Parameters["IsVisible"] = false.ToString();
        this.EngagmentAttachBorder.Parameters["IsVisible"] = true.ToString();  
      }
      else
      {
        this.EPAccordionBorder.Parameters["IsVisible"] = true.ToString();
        this.EngagmentAttachBorder.Parameters["IsVisible"] = false.ToString();
      }
    }

    #endregion
  }
}