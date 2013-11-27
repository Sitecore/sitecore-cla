namespace Sitecore.Cla.Presentation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Web;
  using System.Web.UI;
  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.Data;
  //using Sitecore.Data.Items;
  using Sitecore.Data.Items;
  using Sitecore.Data.Fields;
  using Sitecore.Diagnostics;
  //using Sitecore.Links;
  using Sitecore.Links;
  using Sitecore.Mvc.Presentation;
  using Sitecore.Resources;
  using Sitecore.Web;
  using Sitecore.Web.PageCodes;
  using Sitecore.Web.UI.Controls;
  //using Sitecore.Web.UI.WebControls.Presentation;
  using Sitecore.Web.UI.Controls.Navigation.BreadCrumbs;
  using Sitecore.Data.Managers;


  public class BreadCrumbCustom : BlockControlBase
  {
    /// <summary>
    /// current "ContextItem"
    /// </summary>
    private Item currentContextItem = null;

    public ID RootItemId { get; set; }

    public BreadCrumbCustom()
    {
        this.InitializeProperties();
    }

    public BreadCrumbCustom(RenderingParametersResolver parametersResolver)
      : base(parametersResolver)
    {
        Assert.ArgumentNotNull(parametersResolver, "parametersResolver");
        this.InitializeProperties();
        this.RootItemId = parametersResolver.GetId("RootItemId", "rootItemId", null);
    }

    public static HtmlString GetHtmlString(Sitecore.Mvc.Presentation.Rendering rendering)
    {
      BreadCrumbCustom breadCrumb = new BreadCrumbCustom(new DatasourceBasedParametersResolver(rendering));
      return new HtmlString(breadCrumb.Render());
    }

    private Item FindItem(Item rootItem, Item currentItem)
    {
        Item item = null;

        if (((LinkField)rootItem.Fields["Page"]).TargetID.Equals(currentItem.ID))
        {
            return rootItem;
        }
        ChildList children = rootItem.GetChildren();
        if (children.Any<Item>())
        {
            foreach (Item item2 in children)
            {
                item = this.FindItem(item2, currentItem);
                if (item != null)
                {
                    return item;
                }
            }
        }
        return item;
    }

    public static string GetImageURL(Item currentItem)
    {
        string themedImageSource = Images.GetThemedImageSource(currentItem.Appearance.Icon);
        Providers.LinkProvider provider = new Providers.LinkProvider();
        return provider.ResolveDbInImageUrl(themedImageSource, false);
    }

    private void InitializeProperties()
    {
      base.Class = "sc-breadcrumb";
      base.DataBind = "visible: isVisible";
      base.Requires.Script("controls", "breadcrumb.js");
      base.HasNestedComponents = false;
      currentContextItem = Context.Item;
    }

    private static bool IsBreadcrumbPage(Item currentItem)
    {
      return Sitecore.Data.Managers.TemplateManager.GetTemplate(currentItem).ID.Equals(Sitecore.Names.TemplateIds.Breadcrumb);
    }

    private static bool IsSpeakBasePage(Item currentItem)
    {
      return Sitecore.Data.Managers.TemplateManager.GetTemplate(currentItem).DescendsFrom(Sitecore.Names.TemplateIds.SpeakBasePage);
    }

    protected override void Render(HtmlTextWriter output)
    {
      this.AddAttributes(output);
      output.RenderBeginTag(HtmlTextWriterTag.Div);
      output.RenderBeginTag(HtmlTextWriterTag.Ul);
      Item currentItem = null;
      Item item = Context.Item;
      if (!ID.IsNullOrEmpty(this.RootItemId))
      {
          currentItem = ClientHost.Items.GetItem(this.RootItemId);
      }
      if ((currentItem != null) && IsBreadcrumbPage(currentItem))
      {
          this.RenderForBreadcrumbItem(output, currentItem, item);
      }
      else
      {
          this.RenderForPage(output, currentItem, item);
      }
      output.RenderEndTag();
      output.RenderEndTag();
    }

    private void RenderForBreadcrumbItem(HtmlTextWriter output, Item rootItem, Item currentItem)
    {
      Item item = this.FindItem(rootItem, currentItem);
      List<Item> list = new List<Item>();
      if (item != null)
      {
          list.Add(item);
          Item parent = item.Parent;
          while ((parent != null) && (parent.ID != rootItem.ID))
          {
              currentItem = parent;
              parent = currentItem.Parent;
              if (IsBreadcrumbPage(currentItem))
              {
                  list.Add(currentItem);
              }
          }
          if (((parent != null) && (parent.ID == rootItem.ID)) && IsBreadcrumbPage(parent))
          {
              list.Add(rootItem);
          }
      }
      list.Reverse();
      for (int i = 0; i < list.Count; i++)
      {
          Item item3 = list[i];
          ID targetID = ((LinkField)item3.Fields["Page"]).TargetID;
          if (!targetID.IsNull)
          {
              Item item4 = ClientHost.Items.GetItem(targetID);
              if (item4 != null)
              {
                  this.RenderPath(output, item4, i == 0);
              }
          }
      }
    }

    private void RenderForPage(HtmlTextWriter output, Item rootItem, Item currentItem)
    {
      List<Item> list = new List<Item>();
      if (rootItem != null)
      {
          list.Add(currentItem);
          Item parent = currentItem.Parent;
          while ((parent != null) && (parent.ID != rootItem.ID))
          {
              currentItem = parent;
              parent = currentItem.Parent;
              if (IsSpeakBasePage(currentItem))
              {
                  list.Add(currentItem);
              }
          }
          if (((parent != null) && (parent.ID == rootItem.ID)) && IsSpeakBasePage(parent))
          {
              list.Add(rootItem);
          }
      }
      list.Reverse();
      for (int i = 0; i < list.Count; i++)
      {
          Item item = list[i];
          this.RenderPath(output, item, i == 0);
      }
    }

    private void RenderPath(HtmlTextWriter output, Item item, bool isPathRoot)
    {
        output.RenderBeginTag(HtmlTextWriterTag.Li);
        using (new ContextItemSwitcher(item))
        {
          // if item != currentContextItem - then it will be a "link" control
          if (currentContextItem.ID.ToString() != item.ID.ToString())
          {
            string itemUrl = LinkManager.GetItemUrl(item);
            output.AddAttribute(HtmlTextWriterAttribute.Href, itemUrl);
            output.RenderBeginTag(HtmlTextWriterTag.A);
          }

          if (isPathRoot)
          {
            output.AddAttribute(HtmlTextWriterAttribute.Src, GetImageURL(item));
            string displayName = item.DisplayName;
            if (!string.IsNullOrEmpty(item["Name"]))
              displayName = item["Name"];
            output.AddAttribute(HtmlTextWriterAttribute.Title, item.DisplayName);
            output.RenderBeginTag(HtmlTextWriterTag.Img);
            output.RenderEndTag();
          }
          else
          {
            string displayName = item.DisplayName;
            if (!string.IsNullOrEmpty(item["Name"]))
              displayName = item["Name"];

            // if item == currentContextItem and "id" param is in QueryString - then to give display name from item-param
            if (currentContextItem.ID.ToString() == item.ID.ToString())
            {
              string itemIdParam = WebUtil.GetQueryString("id");
              if (!string.IsNullOrEmpty(itemIdParam))
              {
                var masterdp = Sitecore.Configuration.Factory.GetDatabase("master");
                var itemParam = masterdp.GetItem(new ID(itemIdParam));
                if (itemParam != null)
                {
                  displayName = itemParam.DisplayName;
                  if (!string.IsNullOrEmpty(itemParam["Name"]))
                    displayName = itemParam["Name"];
                }                
              }
            }
            output.Write(displayName);
          }

          // if item != currentContextItem - then it will be a "link" control
          if (currentContextItem.ID.ToString() != item.ID.ToString())
            output.RenderEndTag();
        }
        output.RenderEndTag();
    }

  }

}