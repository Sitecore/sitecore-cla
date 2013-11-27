using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Cla.Analytics
{
  using Sitecore.Analytics.Data.Items;
  using Sitecore.Data;
  using Sitecore.Data.Items;

  public static class EngagementAutomationHelper
  {
    public static Item CreatePlan(string campaignName, string engagementSourceId)
    {
      EngagementPlanItem engagementPlan = null;

      var plansRoot = Sitecore.Context.ContentDatabase.GetItem(Consts.EAPlansRootId);
      var sourcePlan = Sitecore.Context.ContentDatabase.GetItem(engagementSourceId);
      if (sourcePlan != null && plansRoot != null)
      {
        engagementPlan = new EngagementPlanItem(sourcePlan.CopyTo(plansRoot, campaignName));
      }

      return engagementPlan;
    }
  }
}
