// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingPageVariant.cs" company="Sitecore">
//   Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.
// </copyright>
// <summary>
//   Defines the LandingPageVariant type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Data
{
  using Sitecore.Data.Items;

  /// <summary>
  /// Represent LandingPageVariant type
  /// </summary>
  public class LandingPageVariant
  {
    /// <summary>
    /// The landing page.
    /// </summary>
    private LandingPage landingPage;

    /// <summary>
    /// The item of the landing page variant.
    /// </summary>
    private Item landingPageVariantItem;

    /// <summary>
    /// The name of the variant.
    /// </summary>
    private string landingPageVariantName;

    /// <summary>
    /// The template of the variant.
    /// </summary>
    private BranchItem landingPageVariantTemplate;

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPageVariant"/> class.
    /// </summary>
    /// <param name="landinPageVariantItem">
    /// The landin page variant item.
    /// </param>
    /// <param name="landingPage">
    /// The landing page.
    /// </param>
    public LandingPageVariant(Item landinPageVariantItem, LandingPage landingPage)
    {
      this.LandingPageVariantItem = landinPageVariantItem;
      this.landingPage = landingPage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPageVariant"/> class.
    /// </summary>
    /// <param name="variantName">
    /// The variant name.
    /// </param>
    /// <param name="variantTemplate">
    /// The variant template.
    /// </param>
    /// <param name="landingPage">
    /// The landing page.
    /// </param>
    public LandingPageVariant(string variantName, BranchItem variantTemplate, LandingPage landingPage)
    {
      this.landingPage = landingPage;
      this.landingPageVariantTemplate = variantTemplate;
      this.LandingPageVariantName = variantName;
    }

    /// <summary>
    /// Gets or sets the landing page variant item.
    /// </summary>
    public Item LandingPageVariantItem
    {
      get
      {
        return this.landingPageVariantItem;
      }

      set
      {
        this.landingPageVariantItem = value;
      }
    }

    /// <summary>
    /// Gets or sets the landing page variant name.
    /// </summary>
    public string LandingPageVariantName
    {
      get
      {
        return this.landingPageVariantName;
      }

      set
      {
        this.landingPageVariantName = value;
      }
    }

    /// <summary>
    /// Gets LandingPage.
    /// </summary>
    public LandingPage LandingPage
    {
      get
      {
        return this.landingPage;
      }
    }

    /// <summary>
    /// Gets the variant name.
    /// </summary>
    public string VariantName
    {
      get
      {
        return this.LandingPageVariantItem.Name;
      }
    }

    /// <summary>
    /// Remove item of the current variant.
    /// </summary>
    public void Remove()
    {
      this.LandingPageVariantItem.Delete();
    }

    /// <summary>
    /// Create the item for the current variant.
    /// </summary>
    public void Create()
    {
      if (Sitecore.Data.Managers.TemplateManager.IsTemplate(this.landingPageVariantTemplate))
      {
        this.LandingPageVariantItem = this.LandingPage.LandingPageItem.Add(this.LandingPageVariantName, this.landingPageVariantTemplate);
      }
      else
      {
        this.LandingPageVariantItem = this.landingPageVariantTemplate.InnerItem.CopyTo(
          this.LandingPage.LandingPageItem, this.landingPageVariantName);
      }

    }
  }
}
