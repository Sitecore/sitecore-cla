// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CampaignViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   View Model which represen a LandingPage in clientSide
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Application.ViewModels
{
  using System;

  /// <summary>
  ///   View Model which represen a LandingPage in clientSide
  /// </summary>
  public class CampaignViewModel
  {
    #region Public Properties

    /// <summary>
    /// Gets or sets the alias.
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the item id.
    /// </summary>
    public string itemId { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    public string StartDate { get; set; }

    public string EndDate { get; set; }

    public bool isValid { get; set; }

    public int TotalValue { get; set; }

    public int AvgValue { get; set; }

    public bool isRetired { get; set; }

    public string State { get; set; }

    public int Cost { get; set; }

    #endregion
  }
}