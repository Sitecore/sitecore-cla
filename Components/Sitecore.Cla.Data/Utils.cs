// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="Sitecore">
//   Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.
// </copyright>
// <summary>
//   Defines the Utils type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Data
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Web.SessionState;
  using System.Linq;

  using Convert = Sitecore.Convert;

  /// <summary>
  /// Utils for landing page wizard application.
  /// </summary>
  public sealed class Utils
  {

    /// <summary>
    /// Prevents a default instance of the <see cref="Utils"/> class from being created.
    /// </summary>
    private Utils()
    {
    }

    protected static string LandingPageRottId
    {
      get
      {
        return Sitecore.Configuration.Settings.GetSetting("Cla.LandingPageRoot");
      }
    }

    public static List<NameValueCollection> GetCampaignIdList()
    {
      var list = new List<NameValueCollection>();
      var campaignsRoot = Sitecore.Context.ContentDatabase.GetItem(LandingPageRottId);

      if (campaignsRoot != null)
      {
        list = campaignsRoot.Children.Select(x => new NameValueCollection() { { "Name", x["Name"] }, { "Id", x.ID.ToString() } }
          ).ToList();
      }

      return list;
    }

    /// <summary>
    /// Get the letter for the variant by the index.
    /// </summary>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <returns>
    /// The letter of the variant.
    /// </returns>
    public static string GetLetter(int index)
    {
      int t = index / 27;
      if ((index % 27) < 27 - t)
      {
        index += t;
      }
      else
      {
        index += t + 1;
      }

      string letter = string.Empty;
      while (index > 0)
      {
        int bitLetter = index % 27;
        index = index / 27;
        letter = letter.Insert(0, new string(new char[] { (char)(bitLetter + 64) }));
      }

      return letter;
    }

    public static string NumberToName(int number)
    {
      int dividend = number;
      string name = String.Empty;
      int modulo;

      while (dividend > 0)
      {
        modulo = (dividend - 1) % 26;
        name = System.Convert.ToChar(65 + modulo).ToString() + name;
        dividend = (int)((dividend - modulo) / 26);
      }

      return name;
    }

    public static int NameToNumber(string name)
    {
      var characters = name.ToUpperInvariant().ToCharArray();
      var sum = 0;
      for (int i = 0; i < characters.Length; i++)
      {
        sum *= 26;
        sum += characters[i] - 'A' + 1;
      }

      return sum;
    }

    public static string NextName(string name)
    {
      var number = NameToNumber(name);
      return NumberToName(number + 1);
    }

    /// <summary>
    /// Gets current version index from the session.
    /// </summary>
    /// <param name="session">
    /// The session.
    /// </param>
    /// <returns>
    /// Current version index.
    /// </returns>
    public static int GetVersionIndex(HttpSessionState session)
    {
      if (session["LandingPageVersionIndex"] != null)
      {
        return (int)session["LandingPageVersionIndex"];
      }
      else
      {
        return -1;
      }
    }

    /// <summary>
    /// Save current version index to session.
    /// </summary>
    /// <param name="session">
    /// The session.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    public static void SetVersionIndex(HttpSessionState session, int value)
    {
      if (session["LandingPageVersionIndex"] == null)
      {
        session.Add("LandingPageVersionIndex", value);
      }
      else
      {
        session["LandingPageVersionIndex"] = value;
      }
    }
  }
}
