namespace Sitecore.Cla.SocialConnectorProvider
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  using Sitecore.Cla.Application.Interfaces;
  using Sitecore.Cla.Data;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Reflection;
  using Sitecore.Sites;
  using Sitecore.Social.Core.Configuration;
  using Sitecore.Social.Core.Networks;
  using Sitecore.Social.Core.Publishing;
  using Sitecore.Social.Core.Publishing.Items;
  using Sitecore.Social.Core.Publishing.Messages;
  using Sitecore.Social.Core.Publishing.SourceContext;

  public class SocialConnectorProvider : ISocialConnectorProvider
  {
    public List<dynamic> GetSocialMessages(string name, string itemUri)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var context = new Source(name, itemUri);
        var messages = new Social.Core.Publishing.Managers.DataManager().ReadMessages(context).ToList<dynamic>();
        return messages;
      }
    }


    public void CreateSocialMessage(
      string campaignId, string networkName, string messageText, string linkTitle, string linkDescription, string publishWithItem, List<string> accountList)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var item = Context.ContentDatabase.GetItem(new ID(campaignId));

        var message = new Social.Core.Publishing.Messages.Message(new NetworkFactory().GetNetwork(networkName));
        var dataManager = new Social.Core.Publishing.Managers.DataManager();
        var context = new Source("Publish", item.Uri.ToString());
        dataManager.CreateMessage(context, message);

        var messageItem = Context.ContentDatabase.GetItem(message.Id);
        if (messageItem == null)
        {
          return;
        }

        using (new EditContext(messageItem))
        {
          messageItem.Fields["Message"].Value = messageText;

          var lp = new LandingPage(campaignId);
          var db = Configuration.Factory.GetDatabase("master");
          var campaigncategory = db.GetItem("/sitecore/system/Marketing Center/Campaigns/Landing Pages");
          if (campaigncategory != null)
          {
            var campaign =
              campaigncategory.Children.Where(x => x.Name.Equals(lp.LandingPageItem.Name)).FirstOrDefault();
            if (campaign != null)
            {
              messageItem.Fields["Campaign"].Value = lp.GetChannelCampaign(networkName).ID.ToString();
            }
          }

          var url = string.Format("http://{0}/{1}.aspx", ConfigurationManager.Domain, lp.Alias); // TODO:
          LinkField linkField = messageItem.Fields["Link"];

          linkField.LinkType = "external";
          linkField.Url = url;

          if (networkName.ToUpper() == "Facebook".ToUpper())
          {
            messageItem.Fields["Link Title"].Value = linkTitle;
            messageItem.Fields["Link Description"].Value = linkDescription;
          }
        }

        var sourceTemplateId = Social.Core.Configuration.ConfigurationManager.GetSourceAttribute("Publish", "TemplateId");
        var sourceItem = messageItem.Axes.GetDescendants().FirstOrDefault(x => x.TemplateID == new ID(sourceTemplateId));
        if (sourceItem == null)
        {
          return;
        }

        using (new EditContext(sourceItem))
        {
          Item accountItem = null;

          if (accountList.Count() == 0)
          {
            if (networkName.ToUpperInvariant() == "Facebook".ToUpperInvariant())
            {
              // /sitecore/system/Social/Accounts/Jetstream Facebook Account
              accountItem = Context.ContentDatabase.GetItem("C82C1835-843B-4CE5-BF6E-535343EFC169");
            }
            else if (networkName.ToUpperInvariant() == "Twitter".ToUpperInvariant())
            {
              // /sitecore/system/Social/Accounts/Jetstream Twitter Account
              accountItem = Context.ContentDatabase.GetItem("C5A9E243-A289-4A74-A4F7-D202CF5D136D");
            }

            if (accountItem != null)
            {
              sourceItem.Fields["Accounts"].Value = accountItem.ID.ToString();
            }   
          }
          else
          {
            var accountsb = new StringBuilder();
            foreach (var accountId in accountList)
            {
              if (accountsb.Length > 0)
              {
                accountsb.Append("|");
              }

              accountsb.Append(accountId);
            }
            sourceItem.Fields["Accounts"].Value = accountsb.ToString();
          }

         

          sourceItem.Fields["Publish with item"].Value = publishWithItem == "true" ? "1" : string.Empty;
        }
      }
    }

    public void EditSocialMessage(
      string campaignId, string messageId, string networkName, string messageText, string linkTitle, string linkDescription, string publishWithItem, List<string> accountList)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var messageItem = Context.ContentDatabase.GetItem(new ID(messageId));

        using (new EditContext(messageItem))
        {
          messageItem.Fields["Message"].Value = messageText;

          var lp = new LandingPage(campaignId);
          var db = Configuration.Factory.GetDatabase("master");
          var campaigncategory = db.GetItem("/sitecore/system/Marketing Center/Campaigns/Landing Pages");
          if (campaigncategory != null)
          {
            var campaign =
              campaigncategory.Children.FirstOrDefault(x => x.Name.Equals(lp.LandingPageItem.Name));
            if (campaign != null)
            {
              messageItem.Fields["Campaign"].Value = campaign.ID.ToString();
            }
          }

          var url = string.Format("http://{0}/{1}.aspx", ConfigurationManager.Domain, lp.Alias);
          LinkField linkField = messageItem.Fields["Link"];

          linkField.LinkType = "external";
          linkField.Url = url;

          if (networkName.ToUpper() == "Facebook".ToUpper())
          {
            messageItem.Fields["Link Title"].Value = linkTitle;
            messageItem.Fields["Link Description"].Value = linkDescription;
          }
        }

        var sourceItem = messageItem.Children[0];

        using (new EditContext(sourceItem))
        {
          sourceItem.Fields["Publish with item"].Value = publishWithItem == "true" ? "1" : string.Empty;

          if (accountList.Count() == 0)
          {
            sourceItem.Fields["Accounts"].Value = "";
          }
          else
          {
            var accountsb = new StringBuilder();
            foreach (var accountId in accountList)
            {
              if (accountsb.Length > 0)
              {
                accountsb.Append("|");
              }

              accountsb.Append(accountId);
            }

            sourceItem.Fields["Accounts"].Value = accountsb.ToString(); 
          }
        }
      }
    }

    public void PublishSocialMessage(string messageId)
    {
      using (new SiteContextSwitcher(SiteContext.GetSite("shell")))
      {
        var messageItem = Context.ContentDatabase.GetItem(new ID(messageId));
        var messageAttribute = ConfigurationManager.GetMessageAttribute(ConfigurationManager.GetMessageNetworkName(messageItem), "type");
        object @object = ReflectionUtil.CreateObject(messageAttribute, new object[]
        {
          messageItem
        });
        PublishManager.PublishMessage(MessageBuilder.BuildMessage(new Source("Publish", messageItem.Uri.ToString()), @object as SocialMessageBase));
      }
    }
  }
}
