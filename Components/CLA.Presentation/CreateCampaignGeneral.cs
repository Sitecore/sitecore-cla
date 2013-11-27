// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCampaignGeneral.cs" company="">
//   
// </copyright>
// <summary>
//   The create campaign general.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Presentation
{
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The create campaign general.
  /// </summary>
  public class CreateCampaignGeneral : PageCodeBase
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the general alias text box.
    /// </summary>
    public Rendering GeneralAliasTextBox { get; set; }

    /// <summary>
    /// Gets or sets the general description text box.
    /// </summary>
    public Rendering GeneralDescriptionTextBox { get; set; }

    /// <summary>
    /// Gets or sets the general name text box.
    /// </summary>
    public Rendering GeneralNameTextBox { get; set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    public override void Initialize()
    {
      var mastredp = Sitecore.Configuration.Factory.GetDatabase("master");
      var referer = WebUtil.GetQueryString("id");

      var item = mastredp.GetItem(new ID(referer));

      var alias = item.Fields["Alias"].ToString();
      var description = item.Fields["Description"].ToString();

      this.GeneralNameTextBox.Parameters["Text"] = item.Fields["Name"].ToString();

      if (!string.IsNullOrEmpty(alias))
      {
        this.GeneralAliasTextBox.Parameters["Text"] = alias;
      }

      if (!string.IsNullOrEmpty(description))
      {
        this.GeneralDescriptionTextBox.Parameters["Text"] = description;
      }
    }

    #endregion
  }
}