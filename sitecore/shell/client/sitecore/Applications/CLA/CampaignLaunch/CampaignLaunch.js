/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
return _sc.Definitions.App.extend({
    initialized: function () {
	  $.ajaxSetup({ cache: false });
      var app = this;
      this.on("closePopup", this.closePopup, this);
      this.campaigns = new _sc.Campaigns();
      this.campaigns.topLandingPages();
      this.campaigns.on("add", this.updateListControl, this);
      this.insertRendering("{686B4619-CBBE-4C8E-A82C-7887E0325187}", { $el: $("body") }, function (subApp) {
          app["createDialog"] = subApp;
      });  
      this.TopCampaignListControl.on("change:selectedItemId", this.refreshCharts, this);
        
      this.resetLanguageContext();
    },
    updateListControl: function () {
      this.TopCampaignListControl.set("items", this.campaigns.toJSON());
    },
    goToCampaign: function (id) {
      window.location = "/sitecore/client/sitecore/applications/cla/EditCampaign?id=" + id + "&ref=CampaignLaunch";
    },
    refreshCharts: function () {
        var self = this,
            campaignId = this.TopCampaignListControl.get("selectedItemId");

        if (campaignId) {
          self.CampaignChartSmartPanel.set("isOpen", true);
          $.get("/api/sitecore/cla/campaign/CampaignDevices?campaignId=" + campaignId).done(function(data) {
            self.CampaignDevicesChart.set("chartData", data);
          });
        } else {
          self.CampaignChartSmartPanel.set("isOpen", false);
        }
    },
    createCampaignDialog: function () {
      _sc.trigger("createDialog");
    },
    editCampaign: function () {
      var campaignId = this.TopCampaignListControl.get("selectedItemId");
      this.goToCampaign(campaignId);
    },
    
    resetLanguageContext: function () {
        document.cookie = "website#lang=en;path=/";
    }
  });
});
