// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingPageVersion.cs" company="Sitecore">
//   Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.
// </copyright>
// <summary>
//   Defines the LandingPageVersion type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Data
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Sitecore.Data.Items;
  using Sitecore.Globalization;

  /// <summary>
  /// Represent the landing page version.
  /// </summary>
  public class LandingPageVersion
  {
    /// <summary>
    /// The collection of the landing page variants.
    /// </summary>
    private Collection<LandingPageVariant> variants;

    /// <summary>
    /// The item of the current version.
    /// </summary>
    private Item versionItem;

    /// <summary>
    /// The landing page.
    /// </summary>
    private LandingPage landingPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPageVersion"/> class.
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    /// <param name="landinPage">
    /// The landin page.
    /// </param>
    public LandingPageVersion(Item version, LandingPage landinPage)
    {
      this.landingPage = landinPage;
      this.versionItem = version;
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
    /// Gets Number.
    /// </summary>
    public int Number
    {
      get
      {
        return this.versionItem.Version.Number;
      }
    }

    /// <summary>
    /// Gets Variants.
    /// </summary>
    public Collection<LandingPageVariant> Variants
    {
      get
      {
        if (this.variants == null)
        {
          var variantsList = new Collection<LandingPageVariant>();
          foreach (Item child in this.versionItem.GetChildren())
          {
            var regex = new Regex(@"\d+");
            foreach (var match in regex.Matches(child.Name))
            {
              int childversionNumber;
              if (int.TryParse(match.ToString(), out childversionNumber))
              {
                if (childversionNumber.Equals(this.Number))
                {
                  var language = this.versionItem.Language;

                  using (new LanguageSwitcher(language))
                  {
                    if (child.Versions.Count == 0)
                    {
                      child.Versions.AddVersion();
                    }
                  }
                  variantsList.Add(new LandingPageVariant(child, this.LandingPage));
                }
              }
            }
          }

          this.variants = variantsList;
        }

        if (this.variants.Count <= 0)
        {
        }

        return this.variants;
      }
    }

    /// <summary>
    /// Gets Status.
    /// </summary>
    public string Status
    {
      get
      {
        if (this.versionItem.State.GetWorkflowState().FinalState)
        {
          var validVersion = this.versionItem.Publishing.GetValidVersion(DateTime.Now, true).Version;
          if (this.versionItem.Version != validVersion)
          {
            return "Obsolete";
          }
        }

        // checking on "publish" status of latest versions, if such version exist - it means that this version is "obsolete"
        if(landingPage != null && landingPage.Versions != null)
        {
          List<LandingPageVersion> list = landingPage.Versions.Where(x => x.Number > this.Number && x.Status.ToLower().Contains("publish")).ToList();
          if (list.Count > 0)
            return "Obsolete";
        }

        return this.versionItem.State.GetWorkflowState().DisplayName;
      }
    }

    /// <summary>
    /// Gets VersionItem.
    /// </summary>
    public Item VersionItem
    {
      get
      {
        return this.versionItem;
      }
    }

    /// <summary>
    /// Create the new variant by the default variant teplate of the landing page.
    /// </summary>
    /// <returns>
    /// Returns the newly created variant.
    /// </returns>
    public LandingPageVariant AddVariant()
    {
      return this.AddVariant(this.LandingPage.DefaultVariantTemplate);
    }

    /// <summary>
    /// Create the landing page by the varint template.
    /// </summary>
    /// <param name="variantTemplate">
    /// The variant template.
    /// </param>
    /// <returns>
    /// Return the newly created variant.
    /// </returns>
    public LandingPageVariant AddVariant(Item variantTemplate)
    {
      string landingPageVariantName = LandingPageVersion.GetVariantName(this);
      var landingPageVariant = new LandingPageVariant(landingPageVariantName, variantTemplate, this.LandingPage);
      this.variants.Add(landingPageVariant);
      landingPageVariant.Create();
      return landingPageVariant;
    }

    /// <summary>
    /// Copy variants from the version.
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    public void CopyVariants(LandingPageVersion version)
    {
      foreach (var variant in version.Variants.ToList())
      {
        // var variantItem = variant.LandingPageVariantItem.CopyTo(this.LandingPage.LandingPageItem, this.GetVariantName(this));
        // this.Variants.Add(new LandingPageVariant(variantItem, this.LandingPage));
        this.CopyVariant(variant);
      }
    }

    /// <summary>
    /// Remove the current version.
    /// </summary>
    public void Remove()
    {
      foreach (var variant in this.Variants)
      {
        variant.Remove();
      }

      this.Variants.Clear();
      this.versionItem.Versions.RemoveVersion();
    }

    /// <summary>
    /// Remove the varint by the LandingPageVariant.
    /// </summary>
    /// <param name="variant">
    /// The variant.
    /// </param>
    public void RemoveVariant(LandingPageVariant variant)
    {
      this.Variants.Remove(variant);
      variant.Remove();
    }

    /// <summary>
    /// Copy varian by the Landing page variant.
    /// </summary>
    /// <param name="variant">
    /// The variant.
    /// </param>
    public void CopyVariant(LandingPageVariant variant)
    {
      var variantItem = variant.LandingPageVariantItem.CopyTo(this.LandingPage.LandingPageItem, LandingPageVersion.GetVariantName(this));
      this.Variants.Add(new LandingPageVariant(variantItem, this.LandingPage));
    }

    /// <summary>
    /// Gets the name for current variant
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    /// <returns>
    /// Name for current variant.
    /// </returns>
    protected static string GetVariantName(LandingPageVersion version)
    {
      var newName = "A";
      if (version.Variants.Count > 0)
      {
        var lastVariantName = version.Variants.OrderBy(x => x.LandingPageVariantName).Last().VariantName;
        var name = lastVariantName.Replace("Page " + version.Number, string.Empty);
        newName = Utils.NextName(name);
      }
      return "Page " + version.Number + newName;
    }
  }
}
