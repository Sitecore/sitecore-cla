// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PageVariantPageCode.cs" company="">
//   
// </copyright>
// <summary>
//   The Page Variant Page Code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Presentation
{
  using System;

  using Sitecore.Mvc.Presentation;
  using Sitecore.Web.PageCodes;

  /// <summary>
  /// The edit campaign.
  /// </summary>
  public class PageVariantPageCode : PageCodeBase
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the engagement accordion.
    /// </summary>
    public Rendering Frame { get; set; }

    /// <summary>
    /// Gets or sets the accordion.
    /// </summary>
    /// <value>
    /// The accordion.
    /// </value>
    public Rendering PageVariantAccordion { get; set; }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// The initialize.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public override void Initialize()
    {
      var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
      var itemId = Web.WebUtil.GetQueryString("datasourceID");
      var languageName = Web.WebUtil.GetQueryString("lang");
      

      

      var language = languageName == null ? "en" : languageName;
      var item = masterdp.GetItem(itemId);
      if (item != null)
      {
        //this.Frame.Parameters["SourceUrl"] = string.Format("/default.aspx?sc_itemid={0}&amp;sc_temporary=0&amp;sc_webedit=0&amp;sc_mode=preview&amp;sc_database=master&amp", itemId);
        this.Frame.Parameters["SourceUrl"] = string.Format("/?sc_itemid={0}&amp;sc_database=master&amp;sc_mode=normal&amp;sc_lang={1}", itemId, language);
      

        
        // TODO: find out how to get the Variant name
        this.PageVariantAccordion.Parameters["Text"] = item.Appearance.DisplayName;
      }
    }

    #endregion
  }
}