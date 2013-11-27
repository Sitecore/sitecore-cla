// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomerPageCode.cs" company="">
//
// </copyright>
// <summary>
//   The customer page code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Web.PageCodes;

namespace Sitecore.Cla.Presentation
{
  using Sitecore;

  /// <summary>
  /// The customer page code.
  /// </summary>
  public class SocialMessagePageCode : PageCodeBase
  {
    #region Public properties

    /// <summary>
    /// Gets or sets the first name text.
    /// </summary>
    public Sitecore.Mvc.Presentation.Rendering Image { get; set; }

    #endregion

    #region Public methods

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      var currentItem = Context.Item;
      var type = currentItem.Fields["Type"].ToString();

      var imageUrl = "/sitecore/";
      switch (type)
      {
        case "Facebook":
          imageUrl = "/sitecore/shell/client/sitecore/Applications/CLA/Images/facebook2.png";
          break;
        case "Twitter":
          imageUrl = "/sitecore/shell/client/sitecore/Applications/CLA/Images/twitter2.png";
          break;
      }

      this.Image.Parameters["imageUrl"] = imageUrl;
    }

    #endregion
  }
}