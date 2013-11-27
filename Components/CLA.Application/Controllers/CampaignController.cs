// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CampaignController.cs" company="">
//   
// </copyright>
// <summary>
//   LandingPage Controller is used with ItemWebApi
//   in order to manage LandingPage from the Client Side
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Cla.Application.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Globalization;
  using System.Net;
  using System.Text;
  using System.Web.Mvc;
  using System.Xml.Linq;

  using Sitecore.Analytics.Data.Items;
  using Sitecore.Cla.Application.Interfaces;
  using Sitecore.Cla.Application.ViewModels;
  using Sitecore.Cla.Data;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Globalization;
  using Sitecore.Modules.EmailCampaign;
  using Sitecore.Modules.EmailCampaign.Messages;
  using Sitecore.Reflection;
  using Sitecore.Sites;
  using System.Linq;
  using Sitecore.Cla.Analytics;
  using Sitecore.SecurityModel;
  using Sitecore.Web;

  /// <summary>
  /// LandingPage Controller is used with ItemWebApi 
  /// in order to manage LandingPage from the Client Side
  /// </summary>
  public class CampaignController : Controller
  {
    #region Public Methods and Operators

    public ActionResult GetEngagementPlanStates(string engagementSourceId)
    {
      List<dynamic> list = new List<dynamic>();

      try
      {
        using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
        {
          var sourcePlan = Sitecore.Context.ContentDatabase.GetItem(engagementSourceId);
          if (sourcePlan != null)
          {
            for(int i = 0;i < sourcePlan.Children.Count(); i++) 
              list.Add(
                new
                {
                  Name = ((Item)sourcePlan.Children[i]).DisplayName,
                  itemId = ((Item)sourcePlan.Children[i]).ID.ToString()
                }
              );
          }
        }
      }
      catch 
      { }
      
      return this.Json(list, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// Item's renaming
    /// </summary>
    [HttpGet]
    public ActionResult RenameCampaignCategory(string itemID, string newName)
    {
      bool res = false;
      string errorMessage = "";
      try
      {
        using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
        {
          var campaign = new LandingPage(itemID);
          if (campaign.IsExist)
          {
            res = campaign.RenameCampaignCategory(newName, ref errorMessage);
          }
        }
      }
      catch (Exception ex)
      {
        errorMessage = ex.Message;
      }

      return this.Json(new { isOk = res, errorMessage = errorMessage }, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// Attaching an Enga
    /// gement Plan to an existing Landing Page
    /// </summary>
    /// <param name="landingPageId">The landing page id.</param>
    /// <param name="engagementId">The engagement id.</param>
    /// <returns>
    /// Landing Page
    /// </returns>
    [HttpPost]
    public ActionResult AttachEngagementPlan(string campaignId, string engagementId, string stateName)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        if (campaign.IsExist)
        {
          var engagementPlan = EngagementAutomationHelper.CreatePlan(campaign.Name, engagementId);
          if (engagementPlan != null)
          {
            // finding State item by stateName
            Item stateItem = null;
            for (int i = 0; i < engagementPlan.Children.Count(); i++)
            {
              if(stateName == engagementPlan.Children[i].DisplayName)
              {
                stateItem = engagementPlan.Children[i];
                break;
              }
            }

            if(stateItem != null)
            {
              campaign.Campaign.Editing.BeginEdit();
              campaign.Campaign["Enroll in Engagement Plan"] = stateItem.ID.ToString();
              campaign.Campaign.Editing.EndEdit();

              campaign.LandingPageItem.Editing.BeginEdit();
              campaign.LandingPageItem["Engagement Plan"] = engagementPlan.ID.ToString();
              campaign.LandingPageItem.Editing.EndEdit();
            }

            return this.Json(new { engagementId = engagementPlan.ID.ToString() });
          }
        }
      }

      return new HttpStatusCodeResult(200);
    }

    /// <summary>
    /// Attaching Social Message to an existing LandingPage
    /// </summary>
    /// <param name="landingPageId">The landing page id.</param>
    /// <param name="social">The social.</param>
    /// <returns>
    /// Landing Page
    /// </returns>
    [HttpPost]
    public ActionResult AttachSocial(string landingPageId, object social)
    {
      return new JsonResult();
    }

    /// <summary>
    /// In order to create a LandingPage from the Client Side
    /// </summary>
    /// <param name="campaign">The campaign.</param>
    /// <returns>
    /// Landing Page
    /// </returns>
    [HttpPost]
    public ActionResult Campaign(CampaignViewModel campaign)
    {
      var result = new CampaignViewModel();
      var location = "/sitecore/content/Home/CLA/Content";



      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var id = campaign.itemId != null ? campaign.itemId : "empty";
        var lp = new LandingPage(campaign.itemId);
        if (lp.IsExist)
        {
          lp.Campaign.Editing.BeginEdit();
          lp.Campaign["StartDate"] = DateUtil.ToIsoDate(DateTime.Parse(campaign.StartDate));
          lp.Campaign["EndDate"] = DateUtil.ToIsoDate(DateTime.Parse(campaign.EndDate));
          lp.Campaign["Cost"] = campaign.Cost.ToString();
          lp.Campaign.Editing.EndEdit();

          lp.UpdateChannelCampaigns();

          result.Alias = lp.Alias;
          result.Name = lp.Name;
          result.itemId = lp.LandingPageItem.ID.ToString();
          result.isValid = lp.isValid();
          result.TotalValue = lp.CampaignTotalValue;
          result.AvgValue = lp.CampaignAvgValue;
          result.isRetired = lp.IsRetired();
          result.State = lp.GetState();
        }
        else
        {
          var campaignRoot = Context.ContentDatabase.GetItem(location);

          if (campaignRoot.Children.Any(x => x.Name.Equals(campaign.Name)))
          {
            return new HttpStatusCodeResult(400, "Campaign with the same name already exists");
          }

          if (campaignRoot != null)
          {
            var branchItem = Context.ContentDatabase.GetItem(campaign.itemId);

            if (branchItem != null)
            {
              var name = ItemUtil.ProposeValidItemName(campaign.Name);

              var landingPage = new Data.LandingPage
              {
                Location = location,
                Name = name,
                DefaultVariantTemplate = branchItem,
                Alias = name
              };

              landingPage.Create();
              landingPage.SaveChanges();

              result.Name = landingPage.Name;
              result.itemId = landingPage.LandingPageItem.ID.ToString();
              result.Alias = landingPage.Alias;
              result.Description = landingPage.Description;
              result.isValid = landingPage.isValid();
              result.TotalValue = landingPage.CampaignTotalValue;
              result.AvgValue = landingPage.CampaignAvgValue;
              result.isRetired = landingPage.IsRetired();
              result.State = lp.GetState();
            }
          }
          else
          {
            return new HttpStatusCodeResult(500);
          }
        }
      }


      return this.Json(result);
    }

    [HttpGet]
    public ActionResult Campaign(string id)
    {
      var result = new CampaignViewModel();

      var campaign = new LandingPage(id);

      var cmp = new CampaignItem(campaign.Campaign);

      if (campaign.IsExist)
      {
        result.Name = campaign.Name;
        result.itemId = campaign.LandingPageItem.ID.ToString();
        result.Description = campaign.Description;
        result.Alias = campaign.Alias;
        result.StartDate = cmp.StartDate.ToShortDateString();
        result.EndDate = cmp.EndDate.ToShortDateString();
        int iCost = 0;
        int.TryParse(cmp.Cost, out iCost);
        result.Cost = iCost;
        result.isValid = campaign.isValid();
        result.TotalValue = campaign.CampaignTotalValue;
        result.AvgValue = campaign.CampaignAvgValue;
        result.isRetired = campaign.IsRetired();
        result.State = campaign.GetState();
        
      }


      return this.Json(result, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// TopLandingPages should return a List of LandingPage
    ///   with their associated Values
    /// </summary>
    /// <param name="campaignId">
    /// The campaign id.
    /// </param>
    /// <returns>
    /// List of { Landing Page, Value }
    /// </returns>
    [HttpGet]
    public ActionResult CampaignDevices(string campaignId)
    {
      var chartData = new StringBuilder();

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var lp = new LandingPage(campaignId);

        if (lp.IsExist)
        {
          var campaignItem = lp.CampaignCategory;
          if (campaignItem != null)
          {
            var reportData = ReportDataManager.GetTopDeviceData(campaignItem.ID);

            chartData.Append(@"<?xml version=""1.0""?>");
            chartData.Append(string.Format(@"<chart caption=""Devices"" subCaption=""For the {0} campaign"">", campaignItem.Name));

            foreach (var i in reportData)
            {
              chartData.Append(string.Format(@"<set label=""{0}"" value=""{1}""/>", i[0], i[1]));
            }

            chartData.Append("</chart>");
          }

        }
      }

      return this.Json(chartData.ToString(), JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// Save a LandingPage from the Client Side
    /// </summary>
    /// <param name="LandingPage">The landing page.</param>
    /// <returns>
    /// Landing Page
    /// </returns>
    [HttpPut]
    public ActionResult Save(CampaignViewModel LandingPage)
    {
      return new JsonResult();
    }

    /// <summary>
    ///   TopLandingPages should return a List of LandingPage
    ///   with their associated Values
    /// </summary>
    /// <returns>List of { Landing Page, Value }</returns>
    [HttpGet]
    public ActionResult TopLandingPages()
    {
      var result = new List<CampaignValueViewModel>();

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        result = Data.Utils.GetCampaignIdList()
             .Select(
               x =>
               {
                 var value = 0;
                 var lp = new LandingPage(x["Id"]);
                 if (lp.IsExist)
                 {
                   value = lp.CampaignTotalValue;
                 }
                 return new CampaignValueViewModel
                   {
                     Campaign =
                       new CampaignViewModel { Description = "Description", Name = x["Name"], itemId = x["Id"] },
                     Value = value
                   };
               }
               ).ToList();
      }

      return this.Json(result, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult Versions(string id, string langName)
    {
      var result = new List<CampaignVersionViewModel>();
      var campaignId = string.Empty;
      var als = string.Empty;

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var landingPage = new LandingPage(id);

        if (landingPage.IsExist)
        {
          foreach (var version in landingPage.Versions)
          {
            result.Add(new CampaignVersionViewModel { Name = version.Number.ToString(), State = version.Status, itemId = version.Number.ToString() });
          }
        }

        if (landingPage.Campaign != null)
          campaignId = landingPage.Campaign.ID.ToString();
        als = landingPage.Alias;
      }
      return this.Json(new { versions = result, campaign = campaignId, alias = als }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult Variants(string campaignId, string currentVersion, string langName)
    {
      var result = new List<CampaignVariantViewModel>();
      string status = "";

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        langName = langName.Replace("\t", "").Replace("\n", "").Replace(" ", "");
        Language itemLanguage = LanguageManager.GetLanguage(langName);
        if (itemLanguage == null)
          itemLanguage = LanguageManager.GetLanguage("en");

        if (itemLanguage != null)
        {
          using (new LanguageSwitcher(itemLanguage))
          {
            var landingPage = new LandingPage(campaignId);
            if (landingPage.IsExist)
            {
              try
              {
                if (landingPage.LandingPageItem.Versions.Count == 0)
                {
                  landingPage.LandingPageItem.Versions.AddVersion();
                }

                while (landingPage.LandingPageItem.Versions.GetLatestVersion().Version.Number < int.Parse(currentVersion))
                {
                  landingPage.LandingPageItem.Versions.AddVersion();
                }

                var version = landingPage.GetVersionByNumber(int.Parse(currentVersion));
                status = version.Status;

                foreach (var variant in landingPage.Versions.SingleOrDefault(x => x.Number.ToString() == currentVersion).Variants.OrderBy(x => x.LandingPageVariantName))
                {
                  if (variant.LandingPageVariantItem.Versions.Count > 0)
                  {
                    result.Add(
                      new CampaignVariantViewModel
                      {
                        Name = variant.VariantName,
                        itemId = variant.LandingPageVariantItem.ID.ToString()
                      });
                  }
                }
              }
              catch (Exception)
              {
                //
              }
            }
          }
        }
      }

      return this.Json(new { result = result, status = status }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult GetLanguages()
    {
      var result = new List<dynamic>();
      try
      {
        using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
        {
          result.Add(
              new
              {
                itemName = Sitecore.Context.Language.Name
              }
            );

          var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
          Language[] listLang = Sitecore.Globalization.Language.GetLanguages(masterdp);
          for (int i = 0; i < listLang.Count(); i++)
          {
            if (listLang[i].Name != Sitecore.Context.Language.Name)
            {
              result.Add(
                  new
                  {
                    itemName = listLang[i].Name
                  }
                );
            }

          }
        }
      }
      catch
      { }

      return this.Json(result, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult RemoveVersion(string campaignId, string versionId)
    {
      var result = new CampaignVersionViewModel();

      var campaign = new LandingPage(campaignId);

      if (campaign.IsExist)
      {
        if (campaign.Versions.Count > 1)
        {
          campaign.RemoveVersion(int.Parse(versionId));
        }

        result.itemId = campaign.LandingPageItem.ID.ToString();
        result.currentVersion = campaign.Versions.Last().Number.ToString();
      }

      return this.Json(result);
    }

    [HttpPost]
    public ActionResult CopyVersion(string campaignId, string versionId)
    {
      var result = new CampaignVersionViewModel();

      var campaign = new LandingPage(campaignId);

      if (campaign.IsExist)
      {
        campaign.CopyVersion(int.Parse(versionId));

        result.itemId = campaign.LandingPageItem.ID.ToString();
        result.currentVersion = campaign.Versions.Last().Number.ToString();
      }

      return this.Json(result);
    }

    [HttpDelete]
    public ActionResult RemoveVariant(string campaignId, string variantId, string currentVersion)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        var variantItem = Context.ContentDatabase.GetItem(new ID(variantId));

        var landingPageVariant = new LandingPageVariant(variantItem, campaign);

        if (campaign.Versions.Where(x => x.Number.ToString() == currentVersion).SingleOrDefault().Variants.Count > 1)
        {
          campaign.Versions.Where(x => x.Number.ToString() == currentVersion).SingleOrDefault().RemoveVariant(landingPageVariant);
        }
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult CopyVariant(string campaignId, string variantId, string currentVersion)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        var variantItem = Context.ContentDatabase.GetItem(new ID(variantId));

        var landingPageVariant = new LandingPageVariant(variantItem, campaign);

        campaign.Versions.Where(x => x.Number.ToString() == currentVersion).SingleOrDefault().CopyVariant(landingPageVariant);

        return new HttpStatusCodeResult(200);
      }
    }

    [HttpGet]
    public ActionResult SocialMessages(string campaignId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {

        var campaign = new LandingPage(campaignId);

        var version = campaign.Versions.OrderBy(x => x.Number).First();

        var uri = version.VersionItem.Uri.ToString();

        try
        {
          var socialConnectorProvider =
            ReflectionUtil.CreateObject(
              "Sitecore.Cla.SocialConnectorProvider",
              "Sitecore.Cla.SocialConnectorProvider.SocialConnectorProvider",
              new object[] { }) as ISocialConnectorProvider;

          if (socialConnectorProvider == null)
          {
            return new HttpStatusCodeResult(500, "Can't find to Social Connected module.");
          }

          var messages = socialConnectorProvider.GetSocialMessages("Publish", uri);

          var result = new List<dynamic>();
          foreach (var message in messages)
          {
            Item messageItem = Context.ContentDatabase.GetItem(message.MessageItem.ID);

            var url = string.Format("http://{0}/{1}.aspx", Request.Url.Host, campaign.Alias);

            var accounts = messageItem.Children.First().Fields["Accounts"].Value;

            List<string> accountList = new List<string>();

            if (!string.IsNullOrEmpty(accounts))
            {
              accountList =
                accounts.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Context.ContentDatabase.GetItem(x).Name)
                        .ToList();
            }

            if (message.NetworkName.ToString() == "Facebook")
            {


              result.Add(
                new
                {
                  MessageItemID = message.MessageItem.ID.ToString(),
                  NetworkName = "Facebook",
                  MessageText = messageItem.Fields["Message"].ToString(),
                  LinkTitle = messageItem.Fields["Link Title"].ToString(),
                  LinkDescription = messageItem.Fields["Link Description"].ToString(),
                  Url = url,
                  PublishWithItem = messageItem.Children[0].Fields["Publish with item"].Value,
                  Accounts = accountList
                });
            }
            else if (message.NetworkName.ToString() == "Twitter")
            {

              result.Add(
                new
                {
                  MessageItemID = message.MessageItem.ID.ToString(),
                  NetworkName = "Twitter",
                  MessageText = messageItem.Fields["Message"].ToString(),
                  Url = url,
                  PublishWithItem = messageItem.Children[0].Fields["Publish with item"].Value,
                  Accounts = accountList
                });
            }
          }

          return this.Json(result, JsonRequestBehavior.AllowGet);
        }
        catch (Exception exception)
        {
          return new HttpStatusCodeResult(500, exception.Message);
        }
      }
    }

    [HttpDelete]
    public ActionResult DeleteSocialMessage(string id)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var item = Context.ContentDatabase.GetItem(new ID(id));
        if (item != null)
        {
          item.Delete();
        }
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult CreateSocialMessage(string campaignId, string networkName, string messageText, string linkTitle, string linkDescription, string publishWithItem, string accounts)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {

        var accountList = accounts != null ? accounts.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
        var campaign = new LandingPage(campaignId);
        var channelCampaign = campaign.GetChannelCampaign(networkName);

        var channelCampaignId = channelCampaign == null ? campaignId : channelCampaign.ID.ToString();

        var version = campaign.Versions.OrderBy(x => x.Number).First();

        var uri = version.VersionItem.Uri.ToString();

        try
        {
          var socialConnectorProvider =
            ReflectionUtil.CreateObject(
              "Sitecore.Cla.SocialConnectorProvider",
              "Sitecore.Cla.SocialConnectorProvider.SocialConnectorProvider",
              new object[] { }) as ISocialConnectorProvider;

          if (socialConnectorProvider == null)
          {
            return new HttpStatusCodeResult(500, "Can't find to Social Connected module.");
          }

          socialConnectorProvider.CreateSocialMessage(campaignId, networkName, messageText, linkTitle, linkDescription, publishWithItem, accountList);

          return new HttpStatusCodeResult(200);
        }
        catch (Exception exception)
        {
          return new HttpStatusCodeResult(500, exception.Message);
        }
      }
    }

    [HttpPost]
    public ActionResult PublishSocialMessage(string messageId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        try
        {
          var socialConnectorProvider =
            ReflectionUtil.CreateObject(
              "Sitecore.Cla.SocialConnectorProvider",
              "Sitecore.Cla.SocialConnectorProvider.SocialConnectorProvider",
              new object[] { }) as ISocialConnectorProvider;

          if (socialConnectorProvider == null)
          {
            return new HttpStatusCodeResult(500, "Can't find to Social Connected module.");
          }

          socialConnectorProvider.PublishSocialMessage(messageId);

          return new HttpStatusCodeResult(200);
        }
        catch (Exception exception)
        {
          return new HttpStatusCodeResult(500, exception.Message);
        }
      }
    }

    [HttpPut]
    public ActionResult EditSocialMessage(string campaignId, string messageId, string networkName, string messageText, string linkTitle, string linkDescription, string publishWithItem, string accounts)
    {
      var accountsList = accounts != null
                           ? accounts.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                           : new List<string>();

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        try
        {
          var socialConnectorProvider =
            ReflectionUtil.CreateObject(
              "Sitecore.Cla.SocialConnectorProvider",
              "Sitecore.Cla.SocialConnectorProvider.SocialConnectorProvider",
              new object[] { }) as ISocialConnectorProvider;

          if (socialConnectorProvider == null)
          {
            return new HttpStatusCodeResult(500, "Can't find to Social Connected module.");
          }

          socialConnectorProvider.EditSocialMessage(campaignId, messageId, networkName, messageText, linkTitle, linkDescription, publishWithItem, accountsList);

          return new HttpStatusCodeResult(200);
        }
        catch (Exception exception)
        {
          return new HttpStatusCodeResult(500, exception.Message);
        }
      }
    }

    [HttpGet]
    public ActionResult WorkflowHistory(string campaignId, string versionId)
    {
      var eventList = new List<string>();
      var commandList = new List<object>();
      var sts = string.Empty;

      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        if (campaign.IsExist)
        {
          var version = campaign.GetVersionByNumber(int.Parse(versionId));
          sts = version.Status;

          var workflow = version.VersionItem.State.GetWorkflow();
          var workflowEvent = workflow.GetHistory(version.VersionItem);
          foreach (var @event in workflowEvent)
          {
            var oldState = @event.OldState;
            if (!string.IsNullOrEmpty(oldState))
            {
              oldState = workflow.GetState(oldState).DisplayName;
            }

            var newState = @event.NewState;
            if (!string.IsNullOrEmpty(newState))
            {
              newState = workflow.GetState(newState).DisplayName;
            }

            eventList.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1} - From {2} to {3}<br/>{4}", @event.User, @event.Date, oldState, newState, @event.Text));
          }

          var commandCount = 0;

          var skipCommandsList = new List<string>(new string[] { "Publish", "Retire", "Activate" }.ToList());

          var state = version.VersionItem.State.GetWorkflowState();
          foreach (var command in workflow.GetCommands(state.StateID))
          {
            if (skipCommandsList.Contains(command.DisplayName))
            {
              continue;
            }


            var commandItem = Sitecore.Context.ContentDatabase.GetItem(command.CommandID);
            var cName = string.Empty;
            if (commandItem != null)
            {
              cName = commandItem.Name;
            }

            if (string.IsNullOrEmpty(cName))
            {
              cName = command.CommandID;
            }

            commandList.Add(new { commandName = cName, commandId = command.CommandID });

            commandCount++;
          }
        }
      }


      return this.Json(new { status = sts, historyItems = eventList, commands = commandList }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult WorkflowCommands(string campaignId, string versionId)
    {
      var commandList = new List<string>();
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);
        var commandCount = 0;


        var version = campaign.GetVersionByNumber(int.Parse(versionId));

        var skipCommandsList = new List<string>(new string[] { "Publish", "Retire", "Activate" }.ToList());

        if (campaign.IsExist && version != null)
        {
          var workflow = version.VersionItem.State.GetWorkflow();
          var state = version.VersionItem.State.GetWorkflowState();
          foreach (var command in workflow.GetCommands(state.StateID))
          {
            if (skipCommandsList.Contains(command.DisplayName))
            {
              continue;
            }

            commandList.Add(command.CommandID);
            commandCount++;
          }
        }
      }

      return this.Json(new { commandId = commandList });
    }

    [HttpPost]
    public ActionResult ExecuteWorflowCommand(string campaignId, string versionId, string commandId, string comment)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);
        var version = campaign.GetVersionByNumber(int.Parse(versionId));
        if (campaign.IsExist && version != null)
        {
          var workflow = version.VersionItem.State.GetWorkflow();
          var result = workflow.Execute(commandId, version.VersionItem, comment ?? string.Empty, false, new object[0]);
        }
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult ExecuteWorflowCommandCommon(string itemId, string commandName, string comment)
    {
      bool isOK = false;
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        try
        {
          Database masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
          Item item = masterdp.GetItem(new ID(itemId));

          string commandId = "";
          if (commandName.ToLower() == "deploy")
            commandId = masterdp.GetItem("/sitecore/system/Workflows/Analytics Workflow/Draft/Deploy").ID.ToString();
          if (item != null && !string.IsNullOrEmpty(commandId))
          {
            var workflow = item.State.GetWorkflow();
            var result = workflow.Execute(commandId, item, comment ?? string.Empty, false, new object[0]);

            // publishing 
            if (commandName.ToLower() == "deploy")
            {
              var target = Sitecore.Configuration.Factory.GetDatabase("web");
              Sitecore.Publishing.PublishManager.PublishItem(item, new[] { target }, target.Languages, true, true);
            }

            isOK = true;
          }
        }
        catch
        { }
      }

      return new HttpStatusCodeResult(isOK ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
    }


    [HttpPost]
    public ActionResult AttachVariant(string campaignId, string versionId, string variantTemplateId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);
        if (campaign.IsExist)
        {
          var version = campaign.GetVersionByNumber(int.Parse(versionId));
          if (version != null)
          {
            var variantTemplateItem = Context.ContentDatabase.GetItem(variantTemplateId);
            if (variantTemplateItem != null)
            {
              version.AddVariant(variantTemplateItem);
            }
          }
        }
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult CreateEmail(string campaignId, string emailTemplateId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        string trickleTypeId = "{D7BFD8B4-CB7C-4157-91B1-E66C643DA2C9}";
        string subscriptionTypeId = "{5B774361-27C6-4795-AB8B-70D8331822CC}";

        var campaign = new LandingPage(campaignId);
        if (campaign.IsExist)
        {



          var message = MessageItemSource.Create(emailTemplateId, subscriptionTypeId);
          ABTestMessage msg = (ABTestMessage)Factory.GetMessage(message.InnerItem);
          var emailCampaign = campaign.GetChannelCampaign("Email");
          if (emailCampaign != null)
          {
            msg.Source.CampaignId = emailCampaign.ID;
          }

          campaign.AddEmail(new ID(msg.ID));
        }
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpPost]
    public ActionResult Publish(string campaignId, string versionId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        var version = campaign.GetVersionByNumber(int.Parse(versionId));

        campaign.CurrentVersion = version;

        campaign.Publish();
      }

      return new HttpStatusCodeResult(200);
    }

    [HttpGet]
    public ActionResult Emails(string campaignId, string currentVersion)
    {
      var emails = new List<Email>();
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        var version = campaign.GetVersionByNumber(int.Parse(currentVersion));

        campaign.CurrentVersion = version;

        var db = Sitecore.Context.ContentDatabase;

        foreach (var email in campaign.Emails)
        {
          var emailItem = db.GetItem(email);
          if (emailItem != null)
          {
            var source = emailItem.Children.FirstOrDefault();
            if (source != null)
            {
              emails.Add(new Email() { emailId = email, sourceId = source.ID.ToString() });
            }
          }
        }
      }

      return this.Json(new { emails = emails }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult Activate(string campaignId)
    {
      var retired = false;
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var landingPage = new LandingPage(campaignId);
        if (landingPage.IsExist)
        {
          landingPage.Activate();

          //foreach (var email in landingPage.Emails)
          //{
          //  var emailItem = Sitecore.Context.ContentDatabase.GetItem(email);
          //  if (emailItem != null)
          //  {
          //    ABTestMessage msg = (ABTestMessage)Factory.GetMessage(emailItem);
          //    if (msg != null)
          //    {
          //      msg.Source.State = MessageState.
          //    }
          //  }
          //}

          retired = landingPage.IsRetired();
        }
      }

      return this.Json(new { isRetired = retired });
    }

    [HttpPost]
    public ActionResult Retire(string campaignId)
    {
      bool retired = false;
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var landingPage = new LandingPage(campaignId);
        if (landingPage.IsExist)
        {
          landingPage.Retire();
          try
          {
            foreach (var email in landingPage.Emails)
            {
              var emailItem = Sitecore.Context.ContentDatabase.GetItem(email);
              if (emailItem != null)
              {
                ABTestMessage msg = (ABTestMessage)Factory.GetMessage(emailItem);
                if (msg != null)
                {
                  msg.Source.State = MessageState.Drafts;
                }
              }
            }
          }
          catch (Exception e)
          {

            Sitecore.Diagnostics.Log.Error("Email deactivating error", e);
          }



          retired = landingPage.IsRetired();
        }
      }
      return this.Json(new { isRetired = retired });
    }

    [HttpDelete]
    public ActionResult RemoveCampaign(string campaignId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var campaign = new LandingPage(campaignId);

        if (campaign.IsExist)
        {
          campaign.Delete();
        }

      }
      return new HttpStatusCodeResult(200);
    }

    [HttpGet]
    public ActionResult SocialAccounts(string networkName, string messageId)
    {
      var accounts = new List<string>();
      var selected = new List<string>();

      var accountsRottId = "{BD726323-E745-47D0-99F7-D25A870FE211}";
      var masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
      if (masterDb != null)
      {
        var accountsRoot = masterDb.GetItem(accountsRottId);

        foreach (Item account in accountsRoot.Children.OrderBy(x => x.Name))
        {
          var application = account["Application"];
          var applicationItem = masterDb.GetItem(application);
          if (applicationItem != null)
          {
            var network = applicationItem["Network"];
            var networkItem = masterDb.GetItem(network);
            if (networkItem != null && networkItem["Name"].Equals(networkName))
            {
              accounts.Add(account.ID.ToString());
            }
          }
        }
      }

      if (!string.IsNullOrEmpty(messageId))
      {
        var message = masterDb.GetItem(messageId);
        if (message != null)
        {
          var source = message.Children.Where(x => x.Name.Equals("Source")).FirstOrDefault();
          if (source != null)
          {
            var selectedaccounts = source["Accounts"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var selectedaccount in selectedaccounts)
            {
              selected.Add(selectedaccount);
            }
          }
        }
      }

      return this.Json(new { accounts = accounts, selectedaccounts = selected }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public ActionResult GetAssignedGoals(string itemid)
    {
      var masterdb = Sitecore.Configuration.Factory.GetDatabase("master");
      var item = masterdb.GetItem(itemid);
      var list = new List<dynamic>();
      if (item != null)
      {

        var tracking =
          new Sitecore.Analytics.Data.TrackingField(item.Fields[Sitecore.Analytics.AnalyticsIds.TrackingField]);

        var assignedgoals = tracking.Events.Where(x => x.DefinitionItem.IsGoal);
        foreach (var goal in Sitecore.Analytics.Tracker.DefinitionItems.Goals)
        {
          var selected = assignedgoals.Any(x => x.DefinitionItem.ID.ToString().Equals(goal.ID.ToString()));
          list.Add(new {goalname = goal.Name, goalid = goal.ID.ToString(), isselected = selected}); 
        }
      }

      return this.Json(list, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult AssignGoals(string itemid, string selected)
    {
      var masterdb = Sitecore.Configuration.Factory.GetDatabase("master");
      var item = masterdb.GetItem(itemid);
      var splitselected = selected.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
      if (item != null)
      {
        var list = new List<XElement>();
        var tracking = new Sitecore.Analytics.Data.TrackingField(item.Fields[Sitecore.Analytics.AnalyticsIds.TrackingField]);
        var xdoc = XDocument.Parse(tracking.GetFieldValue());
        var selectedevents = xdoc.Root.Elements().Where(x => x.Name.LocalName.Equals("event"));
        var goals = Sitecore.Analytics.Tracker.DefinitionItems.Goals;
        foreach (var goal in goals)
        {
          var sevent = selectedevents.Where(x => x.Attribute("id").Value.Equals(goal.ID.ToString())).FirstOrDefault();
          if (splitselected.Contains(goal.ID.ToString()))
          {
            if (sevent == null)
            {
              var element = new XElement("event");
              element.SetAttributeValue("id", goal.ID.ToString());
              element.SetAttributeValue("name", goal.Name);
              list.Add(element);
            }
          }
          else
          {
            if (sevent!= null)
            {
              sevent.Remove();
            }
          }
        }

        foreach (var xElement in list)
        {
          xdoc.Root.Add(xElement);
        }

        item.Editing.BeginEdit();
        item[Sitecore.Analytics.AnalyticsIds.TrackingField] = xdoc.ToString();
        item.Editing.EndEdit();

      }
      return new HttpStatusCodeResult(200);
    }

    private struct Email
    {
      public string emailId;

      public string sourceId;
    }

    #endregion
  }
}