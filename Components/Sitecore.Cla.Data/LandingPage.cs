// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LandingPage.cs" company="Sitecore">
//   Copyright (C) 1999-2009 by Sitecore A/S. All rights reserved.
// </copyright>
// <summary>
//   Defines the LandingPage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Sitecore.Cla.Data
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Xml.Linq;
  using Sitecore;
  //using Sitecore.Analytics.Data.Items;
  using Sitecore.Analytics.Data.Items;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Layouts;
  using Sitecore.Publishing;
  using Sitecore.SecurityModel;
  using Sitecore.Sites;
  using Sitecore.Web;

  /// <summary>
  /// Class represent LandingPage type.
  /// </summary>
  public class LandingPage
  {
    /// <summary>
    /// The new name of the landing page.
    /// </summary>
    private string newName;

    /// <summary>
    /// The new alias of the landing page.
    /// </summary>
    private string newAlias;

    /// <summary>
    /// The new location of the landing page.
    /// </summary>
    private string newLocation;

    /// <summary>
    /// The new description of the landing page.
    /// </summary>
    private string newDescription;

    /// <summary>
    /// The current location of the landing page.
    /// </summary>
    private string location = "/sitecore/content/Globals/LandingPages";

    /// <summary>
    /// The landing page item.
    /// </summary>
    private Item landingPageItem;

    /// <summary>
    /// The landing page item template.
    /// </summary>
    private TemplateItem landingPageTemplate;

    /// <summary>
    /// The default template of the landing page variant item.
    /// </summary>
    private BranchItem defaultVariantTemplate;

    /// <summary>
    /// Landing page version collection.
    /// </summary>
    private Collection<LandingPageVersion> versions;

    /// <summary>
    /// The current version of the landing page.
    /// </summary>
    private LandingPageVersion currentVersion;

    /// <summary>
    /// The default database.
    /// </summary>
    private Database db;

    private Item campaignItem;

    private string stickyTestStrategyId = "{1744A9FF-6E22-43F0-83E8-CCB30DDE35E5}";

    /// <summary>
    /// states 
    /// </summary>
    public const string RETIRED_STATE = "Retired";
    public const string PUBLISHED_STATE = "Published";
    public const string DRAFT_STATE = "Draft";


    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPage"/> class.
    /// </summary>
    /// <param name="id">The id of the landing page item.</param>
    public LandingPage(string id)
      : this(new ID(id))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPage"/> class.
    /// </summary>
    /// <param name="id">
    /// The id of the landing page item.
    /// </param>
    public LandingPage(ID id)
    {
      var item = this.Database.GetItem(id);

      if (item.TemplateID.ToString().Equals(LandingPageConsts.LandingPageTemplateId))
      {
        this.LandingPageItem = item;  
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPage"/> class.
    /// </summary>
    /// <param name="landingPageItem">
    /// The landin page item.
    /// </param>
    public LandingPage(Item landingPageItem)
    {
      this.LandingPageItem = landingPageItem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LandingPage"/> class.
    /// </summary>
    public LandingPage()
    {
    }

    public Item CampaignCategory
    {
      get
      {
        Item category = null;
        if (Sitecore.Analytics.Configuration.AnalyticsSettings.Enabled)
        {
          if(!string.IsNullOrEmpty(this.LandingPageItem["CategoryId"]))
          {
            Database masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
            category = masterdp.GetItem(new ID(this.LandingPageItem["CategoryId"]));
          }
          else
          {
            category = Sitecore.Analytics.Tracker.DefinitionItems.CampaignCategories.Where(x => x.Name.Equals(this.Name)).FirstOrDefault();            
          }

        }

        return category;
      }
    }

    public bool RenameCampaignCategory(string newName, ref string errorMessage)
    {
      bool res = false;

      try
      {
        if (Sitecore.Analytics.Configuration.AnalyticsSettings.Enabled)
        {
          if (this.LandingPageItem != null && !string.IsNullOrEmpty(this.LandingPageItem["CategoryId"]))
          {
            Item category = this.Database.GetItem(new ID(this.LandingPageItem["CategoryId"]));
            if (category != null)
            {
              using (new EditContext(category))
              {
                string oldName = category.Name;
                category.Name = newName;
                foreach (Item child in category.GetChildren())
                {
                  using (new EditContext(child))
                  {
                    child.Name = child.Name.Replace(oldName, newName);
                    res = true;
                  }
                }
              }
            }

          }
        }
      }
      catch(Exception ex)
      {
        errorMessage = ex.Message;
      }

      return res;
    }

    public Item GetChannelCampaign(string channelName)
    {
      var campaign = this.CampaignCategory.GetChildren().Where(x => x.Name.EndsWith(channelName)).FirstOrDefault();

      if (campaign == null)
      {
        campaign = this.AddChannelCampaign(channelName);
      }

      return campaign;
    }

    public Item Campaign
    {
      get
      {
        if (this.campaignItem == null)
        {
          this.campaignItem = this.CampaignCategory.Children.Where(x => x.Name.Equals(this.Name)).FirstOrDefault();
        }

        return this.campaignItem;
      }
    }

    public Item AddChannelCampaign(string channelName)
    {
      Item campaign = null;
      var campaignTemplate = Sitecore.Analytics.Data.Items.CampaignItem.TemplateID;
      var name = string.Format("{0}_{1}", this.Name, channelName);
      if (this.CampaignCategory != null)
      {
        campaign = this.CampaignCategory.Add(name, new TemplateID(campaignTemplate));
        campaign.Editing.BeginEdit();
        campaign["StartDate"] = this.Campaign["StartDate"];
        campaign["EndDate"] = this.Campaign["EndDate"];
        campaign.Editing.EndEdit();
      }

      return campaign;
    }

    public void UpdateChannelCampaigns()
    {
      var category = this.CampaignCategory;
      foreach (Item channelCampaign in category.Children)
      {
        if (!channelCampaign.Name.Equals(this.Name))
        {
          channelCampaign.Editing.BeginEdit();
          channelCampaign["StartDate"] = this.Campaign["StartDate"];
          channelCampaign["EndDate"] = this.Campaign["EndDate"];
          channelCampaign.Editing.EndEdit();
        }
      }
    }

    public bool isValid()
    {
      var campaignItem = new CampaignItem(this.Campaign);
      return !(DateTime.Now.Date < campaignItem.StartDate.Date || DateTime.Now > campaignItem.EndDate);
    }

    /// <summary>
    /// Gets Versions.
    /// </summary>
    public Collection<LandingPageVersion> Versions
    {
      get
      {
        if (this.versions == null || this.versions.Count < this.LandingPageItem.Versions.Count)
        {
          var versionList = new Collection<LandingPageVersion>();
          int versionCount = 0;
          foreach (var version in this.LandingPageItem.Versions.GetVersions())
          {
            var landingPageVersion = new LandingPageVersion(version, this);
            versionList.Add(landingPageVersion);
            if (landingPageVersion.Variants.Count == 0)
            {
              if (versionCount > 0)
              {
                landingPageVersion.CopyVariants(versionList[versionCount - 1]);
              }
              else
              {
                landingPageVersion.AddVariant();
              }
            }

            versionCount++;
          }

          this.versions = versionList;
        }

        return this.versions;
      }
    }

    /// <summary>
    /// Gets or sets the landing page location.
    /// </summary>
    public string Location
    {
      get
      {
        return this.location;
      }

      set
      {
        this.newLocation = value;
      }
    }

    /// <summary>
    /// Gets the new landing page location.
    /// </summary>
    public string NewLocation
    {
      get
      {
        return this.newLocation;
      }
    }

    /// <summary>
    /// Gets or sets the default template of the landing page variant.
    /// </summary>
    public BranchItem DefaultVariantTemplate
    {
      get
      {
        if (this.defaultVariantTemplate == null)
        {
          return this.GetMasters()[0];
        }

        return this.defaultVariantTemplate;
      }

      set
      {
        this.defaultVariantTemplate = value;
      }
    }

    public string EnagagementPlanId
    {
      get
      {
        if (this.LandingPageItem != null)
        {
          return this.LandingPageItem["Engagement Plan"];
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public string SearchKeywordId
    {
      get
      {
        if (this.LandingPageItem != null)
        {
          return this.LandingPageItem["SearchKeyword"];
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public List<string> Emails
    {
      get
      {
        var list = new List<string>();
        if (this.landingPageItem != null)
        {
          list = this.LandingPageItem["Emails"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        return list;
      }
    }

    public void RefreshEmailsList()
    {

      var db = Sitecore.Configuration.Factory.GetDatabase("master");
      var emails = this.Emails;
      var emailssb = new StringBuilder();


      foreach (var emailid in emails)
      {
        var email = db.GetItem(emailid);
        if (email != null)
        {
          if (emailid.Length > 0)
          {
            emailssb.Append("|");
          }

          emailssb.Append(emailid);
        }
      }

      this.LandingPageItem.Editing.BeginEdit();
      this.LandingPageItem["Emails"] = emailssb.ToString();
      this.LandingPageItem.Editing.EndEdit();
    }


    public void AddEmail(ID emailId)
    {
      if (this.LandingPageItem != null)
      {
        var emails = new StringBuilder(this.LandingPageItem["Emails"]);
        if (emails.Length > 0)
        {
          emails.Append("|");
        }

        emails.Append(emailId.ToString());

        this.LandingPageItem.Editing.BeginEdit();
        this.LandingPageItem["Emails"] = emails.ToString();
        this.LandingPageItem.Editing.EndEdit();
      }
    }

    public void AddEngagementPlan(ID id)
    {
      if (this.LandingPageItem != null)
      {
        this.LandingPageItem.Editing.BeginEdit();
        this.LandingPageItem["Engagement Plan"] = id.ToString();
        this.LandingPageItem.Editing.EndEdit();
      }
    }

    /// <summary>
    /// Gets or sets the alias of the landing page.
    /// </summary>
    public string Alias
    {
      get
      {
        if (this.LandingPageItem != null)
        {
          return this.LandingPageItem["Alias"];
        }
        else
        {
          return string.Empty;
        }
      }

      set
      {
        this.newAlias = value;
      }
    }

    /// <summary>
    /// Gets or sets the landing page item.
    /// </summary>
    public Item LandingPageItem
    {
      get
      {
        return this.landingPageItem;
      }

      set
      {
        this.landingPageItem = value;
      }
    }

    /// <summary>
    /// Gets or sets the name of the landing page.
    /// </summary>
    public string Name
    {
      get
      {
        if (this.LandingPageItem != null)
        {
          var name = this.LandingPageItem["Name"];
          if (string.IsNullOrEmpty(name))
          {
            name = this.landingPageItem.DisplayName;
          }

          if (string.IsNullOrEmpty(name))
          {
            name = this.landingPageItem.Name;
          }

          return name;
        }

        return string.Empty;
      }

      set
      {
        this.newName = value;
      }
    }

    /// <summary>
    /// Gets the new name of the landin page.
    /// </summary>
    public string NewName
    {
      get
      {
        return this.newName;
      }
    }

    /// <summary>
    /// Gets the new alias of the landing page. 
    /// </summary>
    public string NewAlias
    {
      get
      {
        return this.newAlias;
      }
    }

    /// <summary>
    /// Gets or sets Description.
    /// </summary>
    public string Description
    {
      get
      {
        if (this.LandingPageItem != null)
        {
          return this.LandingPageItem["Description"];
        }

        return string.Empty;
      }

      set
      {
        this.newDescription = value;
      }
    }

    /// <summary>
    /// Gets the template of the landing page item.
    /// </summary>
    public TemplateItem LandingPageTemplate
    {
      get
      {
        if (this.landingPageTemplate == null)
        {
          this.landingPageTemplate = this.Database.Templates[new ID(LandingPageConsts.LandingPageTemplateId)];
        }

        return this.landingPageTemplate;
      }
    }

    /// <summary>
    /// Gets the Database.
    /// </summary>
    public Database Database
    {
      get
      {
        if (this.db == null)
        {
          this.db = Sitecore.Configuration.Factory.GetDatabase("master");
        }

        return this.db;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the landing page item is exist.
    /// </summary>
    public bool IsExist
    {
      get
      {
        return this.LandingPageItem != null;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the user create landing page.
    /// </summary>
    public bool CanCreate
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Location) && !string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(this.Alias))
        {
          return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Gets or sets the current version of the landing page.
    /// </summary>
    public LandingPageVersion CurrentVersion
    {
      get
      {
        if (this.currentVersion == null && this.Versions != null && this.Versions.Count() > 0)
          this.currentVersion = this.Versions.Last();

        return this.currentVersion;
      }

      set
      {
        this.currentVersion = value;
      }
    }

    /// <summary>
    /// Start the editing of the landing page.
    /// </summary>
    /// <returns>
    /// Return true if the current version vas in the final state.
    /// </returns>
    public bool Edit()
    {
      using (new Sitecore.Sites.SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        if (this.CurrentVersion.VersionItem.State.GetWorkflowState().FinalState)
        {
          this.CurrentVersion = this.AddVersion();
          return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Addd the new versiont of the landing page.
    /// </summary>
    /// <returns>
    /// Return the newly created version of the landing page.
    /// </returns>
    public LandingPageVersion AddVersion()
    {
      var versionItem = this.LandingPageItem.Versions.AddVersion();
      var version = new LandingPageVersion(versionItem, this);
      return version;
    }

    /// <summary>
    /// Create the landing page item, alias and campaign event.
    /// </summary>
    public void Create()
    {
      var parent = this.Database.GetItem(this.NewLocation);
      if (parent != null)
      {
        var shellsite = Sitecore.Configuration.Factory.GetSite("shell");
        using (new SiteContextSwitcher(shellsite))
        {
          this.LandingPageItem = parent.Add(this.newName, this.LandingPageTemplate);
        }
        this.CreateAlias();
        this.CurrentVersion = this.Versions[0];
        this.CreatePageLevelTest();
        this.CreateCampaignEvent();
      }
    }

    public void CreatePageLevelTest()
    {
      Item testVariable = null;
      var testRoot = this.Database.GetItem(LandingPageConsts.TestLabRootId);
      var equalNameTests = testRoot.Children.Where(x => x.Name.Equals(this.Name));
      if (testRoot != null)
      {
        var testTemplate = this.Database.Templates[LandingPageConsts.PageLevelTestDefinitionId];
        var test = (equalNameTests.Count() > 0) ? equalNameTests.First() : null;
        if (test == null)
        {
          test = testRoot.Add(this.LandingPageItem.Name, testTemplate);
          test.Editing.BeginEdit();
          test["Item"] = this.LandingPageItem.Paths.FullPath;
          test["Test Strategy"] = stickyTestStrategyId;
          test["__Is Running"] = "1";
          test["__Workflow state"] = "{1789B344-EA1D-4DF7-BA53-1BB2814F7A5A}";
          test.Editing.EndEdit();

          var variableTest = this.Database.Templates[LandingPageConsts.PageLevelTestVariableId];
          testVariable = test.Add(this.LandingPageItem.Name, variableTest);
        }
        else
        {
          testVariable = test.Children[0];
          testVariable.DeleteChildren();
        }

        foreach (var variant in this.CurrentVersion.Variants)
        {
          var testValueTemplate = this.Database.Templates[LandingPageConsts.PageLevelTestValueId];
          var testValue = testVariable.Add(variant.LandingPageVariantItem.Name, testValueTemplate);
          testValue.Editing.BeginEdit();
          testValue["Name"] = variant.LandingPageVariantItem.Name;
          testValue["Datasource"] = variant.LandingPageVariantItem.ID.ToString();
          testValue.Editing.EndEdit();
        }

        this.landingPageItem.BeginEdit();
        this.landingPageItem["__Page Level Test Set Definition"] = test.ID.ToString();
        this.landingPageItem.EndEdit();
      }
    }

    /// <summary>
    /// Create the campaign event for the landing page.
    /// </summary>
    public void CreateCampaignEvent()
    {
      var db = this.LandingPageItem.Database;
      var campaignsRoot = db.GetItem(Sitecore.ItemIDs.Analytics.MarketingCenter.Campaigns);
      var categoryPath = string.Format("{0}/{1}", campaignsRoot.Paths.FullPath, LandingPageConsts.LandingPagesCategoryName);
      var campaignCategoryRoot = db.GetItem(categoryPath);
      var campaignTempalte = db.Templates[Sitecore.Analytics.Data.Items.CampaignItem.TemplateID];
      var campaignCategoryTemplateId = new TemplateID(CampaignCategoryItem.TemplateID);

      if (campaignCategoryRoot == null)
      {
        campaignCategoryRoot = campaignsRoot.Add(LandingPageConsts.LandingPagesCategoryName, campaignCategoryTemplateId);
      }

      Item campaignEvent = null;

      var shellsite = Sitecore.Configuration.Factory.GetSite("shell");
      using (new SiteContextSwitcher(shellsite))
      {
        if (campaignCategoryRoot != null)
        {
          var category = campaignCategoryRoot.Add(this.Name, campaignCategoryTemplateId);
          using (new EditContext(this.LandingPageItem))
          {
            this.LandingPageItem["CategoryId"] = category.ID.ToString();
          }
          
          
          campaignEvent = category.Add(this.newName, campaignTempalte);
          campaignEvent.Editing.BeginEdit();
          campaignEvent["Name"] = campaignEvent.Name;
          campaignEvent["StartDate"] = Sitecore.DateUtil.ToIsoDate(DateTime.Now);
          campaignEvent["EndDate"] = Sitecore.DateUtil.ToIsoDate(DateTime.Now.AddDays(7));
          var workflow = campaignEvent.State.GetWorkflow();
          var commands = workflow.GetCommands(campaignEvent.State.GetWorkflowState().StateID);
          var deployCommand = commands.Where(x => x.DisplayName.Equals("Deploy")).FirstOrDefault();
          if (deployCommand != null)
          {
            var result = workflow.Execute(deployCommand.CommandID, campaignEvent, "Campaign has been deployed", false);
          }

          campaignEvent.Editing.EndEdit();
        }
      }

      var trackingField = this.LandingPageItem["__Tracking"];
      if (trackingField != null)
      {
        XDocument tracking;
        if (string.IsNullOrEmpty(trackingField))
        {
          tracking = new XDocument(new XElement("tracking"));
        }
        else
        {
          tracking = XDocument.Parse(trackingField);
        }

        var campaigns = tracking.Descendants().Where(x => x.Name.ToString().Equals("campaign"));
        if (campaigns.Count() <= 0)
        {
          var campaign = new XElement("campaign", new XAttribute("title", campaignCategoryRoot.Name), new XAttribute("id", campaignEvent.ID.ToString()));
          tracking.Elements().First().AddFirst(campaign);
        }

        this.LandingPageItem.Editing.BeginEdit();
        this.LandingPageItem["__Tracking"] = tracking.ToString();
        this.LandingPageItem.Editing.EndEdit();
      }
    }

    /// <summary>
    /// Create the alias for the landing page.
    /// </summary>
    public void CreateAlias()
    {
      Item root = Context.ContentDatabase.GetItem("/sitecore/system/Aliases");
      Error.AssertItemFound(root, "/sitecore/system/Aliases");
      TemplateItem template = root.Database.Templates["System/Alias"];
      Error.AssertTemplate(template, "Alias");
      var als = this.newAlias == null ? this.Alias : this.newAlias;

      if (root.Children[this.Alias] == null)
      {
        if (!string.IsNullOrEmpty(als))
        {
          Item alias = root.Add(als, template);
          alias.Editing.BeginEdit();
          alias["Linked Item"] = string.Concat(new object[] { "<link linktype=\"internal\" url=\"", this.LandingPageItem.Paths.ContentPath, "\" id=\"", this.LandingPageItem.ID, "\" />" });
          alias.Editing.EndEdit();
        }
      }
    }

    /// <summary>
    /// Return the branches assigned to the landing page folder.
    /// </summary>
    /// <returns>
    /// Array of the BranchItem
    /// </returns>
    public BranchItem[] GetMasters()
    {
      if (this.LandingPageItem != null)
      {
        return this.LandingPageItem.Parent.Branches;
      }

      if (!string.IsNullOrEmpty(this.NewLocation))
      {
        var parent = this.Database.GetItem(this.NewLocation);
        if (parent != null)
        {
          return parent.Branches;
        }
      }

      return null;
    }

    /// <summary>
    /// Copy the version by the version index.
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    public void CopyVersion(int version)
    {
      var shellsite = Sitecore.Configuration.Factory.GetSite("shell");
      using (new SiteContextSwitcher(shellsite))
      {
        this.CopyVersion(this.GetVersion(version));
      }
    }

    /// <summary>
    /// Copy the version by the LandingPageVersion.
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    public void CopyVersion(LandingPageVersion version)
    {
      var newVersion = this.AddVersion();
      newVersion.CopyVariants(version);
    }

    /// <summary>
    /// Remove the version by the version index.
    /// </summary>
    /// <param name="number">
    /// The number.
    /// </param>
    public void RemoveVersion(int number)
    {
      this.RemoveVersion(this.GetVersion(number));
    }

    /// <summary>
    /// Remove the version by the LandingPageVersion
    /// </summary>
    /// <param name="version">
    /// The version.
    /// </param>
    public void RemoveVersion(LandingPageVersion version)
    {
      var isFinal = version.VersionItem.State.GetWorkflowState().FinalState;
      this.Versions.Remove(version);
      version.Remove();

      if (isFinal)
      {
        this.Publish();
        var target = Sitecore.Configuration.Factory.GetDatabase("web");
        Sitecore.Publishing.PublishManager.PublishItem(this.LandingPageItem, new[] { target }, target.Languages, true, true);
      }
    }

    private void PublishRelatedItems(Item root)
    {
      try
      {
        var templateItem = root.Template;
        var sourceDb = Sitecore.Configuration.Factory.GetDatabase("master");
        var targetDb = Sitecore.Configuration.Factory.GetDatabase("web");
        var options = new PublishOptions(sourceDb, targetDb, PublishMode.Smart, Context.Language, DateTime.Now);
        options.Deep = true;
        var publisher = new Publisher(options);

        options.RootItem = templateItem;

        publisher.Publish();

        foreach (var device in Context.Database.Resources.Devices.GetAll())
        {
          var visualization = new ItemVisualization(root);
          var layout = visualization.GetLayout(device);
          if (layout != null)
            options.RootItem = layout.InnerItem;
          publisher.Publish();

          var renderings = visualization.GetRenderings(device, false);
          if(renderings != null)
          {
            foreach (var rendering in renderings)
            {
              if (rendering.RenderingItem != null)
                options.RootItem = rendering.RenderingItem.InnerItem;
              publisher.Publish();
            }
          }
        }
      }
      catch 
      { }
    }

    /// <summary>
    /// Gets the version by the version index.
    /// </summary>
    /// <param name="number">
    /// The number.
    /// </param>
    /// <returns>
    /// Version index.
    /// </returns>
    public LandingPageVersion GetVersion(int number)
    {
      return this.Versions.Where(x => x.Number.Equals(number)).FirstOrDefault();
    }

    /// <summary>
    /// Saves the changes to the landing page item.
    /// </summary>
    public void SaveChanges()
    {
      using (new SecurityDisabler())
      {
        var variantField = (Sitecore.Data.Fields.MultilistField)this.CurrentVersion.VersionItem.Fields["Variants"];
        this.CurrentVersion.VersionItem.Editing.BeginEdit();

        if (!string.IsNullOrEmpty(this.newName))
        {
          this.CurrentVersion.VersionItem["Name"] = this.newName;
        }

        if (!string.IsNullOrEmpty(this.newAlias))
        {
          this.CurrentVersion.VersionItem["Alias"] = this.newAlias;
        }

        this.CurrentVersion.VersionItem["Description"] = this.newDescription;

        variantField.Value = string.Empty;
        foreach (var variant in this.CurrentVersion.Variants)
        {
          variantField.Add(variant.LandingPageVariantItem.ID.ToString());
        }

        this.CurrentVersion.VersionItem.Editing.EndEdit();
      }
    }

    protected Item TestSet
    {
      get
      {
        var pageleveltestFieldId = "{8546D6E6-0749-4591-90F3-CEC033D6E8D8}";
        var testsetid = this.landingPageItem.Fields[pageleveltestFieldId].Value;
        return this.landingPageItem.Database.GetItem(testsetid);
      }
    }

    /// <summary>
    /// Publish the landing page item.
    /// </summary>
    public void Publish()
    {
      if (this.isValid() && !this.IsRetired())
      {
        
        Item testVariable = null;
        var testRoot = this.Database.GetItem(LandingPageConsts.TestLabRootId);
        var test = this.TestSet;
        if (testRoot != null)
        {
          var testTemplate = this.Database.Templates[LandingPageConsts.PageLevelTestDefinitionId];
          if (test == null)
          {
            test = testRoot.Add(this.Name, testTemplate);
            test.Editing.BeginEdit();
            test["Item"] = this.LandingPageItem.Paths.FullPath;
            test["__Workflow state"] = "{1789B344-EA1D-4DF7-BA53-1BB2814F7A5A}";
            test["__Is Running"] = "1";
            test.Editing.EndEdit();

            var variableTest = this.Database.Templates[LandingPageConsts.PageLevelTestVariableId];
            testVariable = test.Add(this.Name, variableTest);
          }
          else
          {
            testVariable = test.Children[0];
            testVariable.DeleteChildren();
          }

          foreach (var variant in this.CurrentVersion.Variants)
          {
            var testValueTemplate = this.Database.Templates[LandingPageConsts.PageLevelTestValueId];
            var testValue = testVariable.Add(variant.LandingPageVariantItem.Name, testValueTemplate);
            testValue.Editing.BeginEdit();
            testValue["Name"] = variant.LandingPageVariantItem.Name;
            testValue["Datasource"] = variant.LandingPageVariantItem.ID.ToString();
            testValue.Editing.EndEdit();
          }

          //var definition = LayoutDefinition.Parse(this.LandingPageItem[FieldIDs.LayoutField]);
          //var device = definition.GetDevice(LandingPageConsts.DefaultDeviceId);
          //var rendering = device.GetRendering(LandingPageConsts.VariantRedirectId);
          //rendering.MultiVariateTest = testVariable.ID.ToString();
          //using (new EditContext(this.LandingPageItem))
          //{
          //  this.LandingPageItem[FieldIDs.LayoutField] = definition.ToXml();
          //}

          var target = Sitecore.Configuration.Factory.GetDatabase("web");
          Sitecore.Publishing.PublishManager.PublishItem(test, new[] { target }, target.Languages, true, true);

          var aliasRoot = string.Format(CultureInfo.InvariantCulture, "/sitecore/system/aliases/");
          var aliasItem = this.Database.GetItem(string.Format(CultureInfo.InvariantCulture, "{0}{1}", aliasRoot, this.Alias));
          if (aliasItem == null)
          {
            this.CreateAlias();
          }

          aliasItem = this.Database.GetItem(string.Format(CultureInfo.InvariantCulture, "{0}{1}", aliasRoot, this.Alias));

          if (aliasItem != null)
          {
            Sitecore.Publishing.PublishManager.PublishItem(aliasItem, new[] { target }, target.Languages, true, true);
          }

          var campaignCategory = this.CampaignCategory;

          if (campaignCategory != null)
          {
            Sitecore.Publishing.PublishManager.PublishItem(campaignCategory, new[] { target }, target.Languages, true, true);
          }

          var workflow = this.CurrentVersion.VersionItem.State.GetWorkflow();
          var state = this.CurrentVersion.VersionItem.State.GetWorkflowState();

          foreach (var variant in this.CurrentVersion.Variants)
          {
            this.PublishRelatedItems(variant.LandingPageVariantItem);
          }

          var commands = workflow.GetCommands(state.StateID);
          if (commands.Length > 0)
          {
            var command = commands[0];
            if (command.DisplayName.Equals("Publish"))
            {
              workflow.Execute(command.CommandID, this.CurrentVersion.VersionItem, "Item has been published", true, new object[0]);
            }
          }
        } 
      }
    }

    public string GetState()
    {
      string state = DRAFT_STATE;
      try
      {
        if (this.IsRetired())
          state = RETIRED_STATE;
        else if (this.LandingPageItem != null && this.LandingPageItem.Versions != null)
        {
          var publishedVersion = this.Versions.Where(x => x.Status.Equals("Published")).LastOrDefault();
          if (publishedVersion != null)
            state = PUBLISHED_STATE;
        }
      }
      catch 
      { }

      return state;
    }

    public bool IsRetired()
    {
      if (this.IsExist)
      {
        var retiredFieldValue = new Sitecore.Data.Fields.CheckboxField(this.LandingPageItem.Fields["Retired"]);
        if (retiredFieldValue.Checked)
        {
          return true;
        }
      }

      return false;
    }

    public void Retire()
    {
      if (this.IsExist)
      {
        this.LandingPageItem.Editing.BeginEdit();
        var retireField = new Sitecore.Data.Fields.CheckboxField(this.LandingPageItem.Fields["Retired"]);
        retireField.Checked = true;
        this.LandingPageItem.Editing.EndEdit();

        Item root = Context.ContentDatabase.GetItem("/sitecore/system/Aliases");
        Error.AssertItemFound(root, "/sitecore/system/Aliases");
        TemplateItem template = root.Database.Templates["System/Alias"];
        Error.AssertTemplate(template, "Alias");

        if (root.Children[this.Alias] != null)
        {
          Item alias = root.Children[this.Alias];
          alias.Delete();
          var target = Sitecore.Configuration.Factory.GetDatabase("web");
          Sitecore.Publishing.PublishManager.PublishItem(root, new[] { target }, target.Languages, true, true);
        }
      }

      var campaignItem = new CampaignItem(this.Campaign);
      if (campaignItem != null)
      {
        campaignItem.InnerItem.Editing.BeginEdit();
        campaignItem.EndDate = DateTime.Now.Date;
        campaignItem.InnerItem.Editing.EndEdit();

        this.UpdateChannelCampaigns();
      }

      var publishedVersion = this.Versions.Where(x => x.Status.Equals("Published")).LastOrDefault();

      if (publishedVersion != null)
      {
        var workflow = publishedVersion.VersionItem.State.GetWorkflow();
        var state = publishedVersion.VersionItem.State.GetWorkflowState();

        var commands = workflow.GetCommands(state.StateID);
        if (commands.Length > 0)
        {
          var command = commands[0];
          if (command.DisplayName.Equals("Retire"))
          {
            workflow.Execute(command.CommandID, publishedVersion.VersionItem, "Item has been retired", true, new object[0]);
          }
        }  
      }
    }

    public void Activate()
    {
      if (this.IsExist)
      {
        this.LandingPageItem.Editing.BeginEdit();
        var retireField = new Sitecore.Data.Fields.CheckboxField(this.LandingPageItem.Fields["Retired"]);
        retireField.Checked = false;
        this.LandingPageItem.Editing.EndEdit();

        Item root = Context.ContentDatabase.GetItem("/sitecore/system/Aliases");
        Error.AssertItemFound(root, "/sitecore/system/Aliases");
        TemplateItem template = root.Database.Templates["System/Alias"];
        Error.AssertTemplate(template, "Alias");

        if (root.Children[this.Alias] == null)
        {
          Item alias = root.Add(this.Alias, template);
          alias.Editing.BeginEdit();
          alias["Linked Item"] = string.Concat(new object[] { "<link linktype=\"internal\" url=\"", this.LandingPageItem.Paths.ContentPath, "\" id=\"", this.LandingPageItem.ID, "\" />" });
          alias.Editing.EndEdit();
          var target = Sitecore.Configuration.Factory.GetDatabase("web");
          Sitecore.Publishing.PublishManager.PublishItem(alias, new[] { target }, target.Languages, true, true);
        }

        var retiredVersion = this.Versions.Where(x => x.Status.Equals("Retired")).LastOrDefault();

        if (retiredVersion != null)
        {
          var workflow = retiredVersion.VersionItem.State.GetWorkflow();
          var state = retiredVersion.VersionItem.State.GetWorkflowState();

          var commands = workflow.GetCommands(state.StateID);
          if (commands.Length > 0)
          {
            var command = commands[0];
            if (command.DisplayName.Equals("Activate"))
            {
              workflow.Execute(command.CommandID, retiredVersion.VersionItem, "Campaign has been activated.", true, new object[0]);
            }
          }
        }
      }
    }

    public LandingPageVersion GetVersionByNumber(int n)
    {
      var version = this.Versions.Where(x => x.Number.Equals(n));
      return version.FirstOrDefault();
    }

    public void Delete()
    {
      var webDb = Sitecore.Configuration.Factory.GetDatabase("web");

      this.DeleteRelatedItems(webDb);

      this.DeleteRelatedItems(Context.ContentDatabase);

    }

    public int CampaignTotalValue
    {
      get
      {
        return Sitecore.Cla.Analytics.ReportDataManager.GetCategoryTotalValue(this.CampaignCategory.ID.ToString());
      }
    }

    public int CampaignAvgValue
    {
      get
      {
        var campaignItem = new CampaignItem(this.Campaign);
        var days = campaignItem.EndDate.Subtract(campaignItem.StartDate).Days;
        

        if (DateTime.Now.Date >= campaignItem.StartDate && DateTime.Now.Date < campaignItem.EndDate)
        {
          days = DateTime.Now.Subtract(campaignItem.StartDate).Days;
        }

        days = days == 0 ? 1 : days;

        return CampaignTotalValue / days;
      }
    }

    private void DeleteRelatedItems(Database db)
    {
      try
      {
        var testRoot = db.GetItem(LandingPageConsts.TestLabRootId);
        var equalNameTests = testRoot.Children.Where(x => x.Name.Equals(this.Name));
        foreach (var equalNameTest in equalNameTests)
        {
          if (equalNameTest != null)
          {
            equalNameTest.Delete(); 
          }
        }

        var aliasRoot = string.Format(CultureInfo.InvariantCulture, "/sitecore/system/aliases/");
        var aliasItem = db.GetItem(string.Format(CultureInfo.InvariantCulture, "{0}{1}", aliasRoot, this.Alias));
        if (aliasItem != null)
        {
          aliasItem.Delete();  
        }

        var campaignsRoot = db.GetItem(Sitecore.ItemIDs.Analytics.MarketingCenter.Campaigns);
        var categoryPath = string.Format("{0}/{1}", campaignsRoot.Paths.FullPath, LandingPageConsts.LandingPagesCategoryName);
        var campaignCategoryRoot = db.GetItem(categoryPath);

        if (campaignCategoryRoot != null)
        {
          var campaignItem = campaignCategoryRoot.GetChildren().Where(x => x.Name.Equals(this.Name)).FirstOrDefault();

          if (campaignItem != null)
          {
            campaignItem.Delete();
          }
        }
        
        var engagementPlanItem = db.GetItem(this.EnagagementPlanId);

        if (engagementPlanItem != null)
        {
          engagementPlanItem.Delete(); 
        }

        var lp = db.GetItem(this.landingPageItem.ID);

        if (lp != null)
        {
          lp.Delete();

        }


      }
      catch (Exception)
      {
        throw new Exception(string.Format("Delete campaign error. Database name - {0}", db.Name));
      }
    }
  }
}
