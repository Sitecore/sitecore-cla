/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
  return _sc.Definitions.App.extend({
      initialized: function () {
	    $.ajaxSetup({ cache: false });
        var app = this;
        this.CampaignsListControl.on("change:selectedItemId", this.refreshCharts, this);
        this.insertRendering("{686B4619-CBBE-4C8E-A82C-7887E0325187}", { $el: $("body") }, function (subApp) {
          app["createDialog"] = subApp;
        });

        this.resetLanguageContext();
      },
      createCampaignDialog: function () {
        _sc.trigger("createDialog");
      },
      refreshCharts: function () {
        var self = this,
            campaignId = this.CampaignsListControl.get("selectedItemId");

        $.get("/api/sitecore/cla/campaign/CampaignDevices?campaignId=" +  campaignId).done(function (data) {
          self.CampaignDevicesChart.set("chartData", data);          
        });
      },
      editCampaign: function () {
        var campaignId = this.CampaignsListControl.get("selectedItemId");
        if (!campaignId) {
            alert("You need to choose campaign!");
            return;
        }          
          
        this.goToCampaign(campaignId);
      },
      deleteCampaign: function () {
          var campaignId = this.CampaignsListControl.get("selectedItemId");
          if (!campaignId) {
              alert("You need to choose campaign!");
              return;
          }

          if (!confirm("Are you sure you want to delete this campaign?")) {
              return;
          };
          
          var self = this;

          $.ajax({
              url: "/api/sitecore/cla/campaign/RemoveCampaign",
              data:
                {
                    "campaignId": campaignId
                },
              type: "DELETE",
              complete: function (jqXHR, textStatus) {
                  self.QueryDataSource.refresh();
              }
          });

      },
      goToCampaign: function (id) {
        window.location = "/sitecore/client/sitecore/applications/cla/EditCampaign?id=" + id + "&ref=CampaignLaunch";
      },
      
      resetLanguageContext: function () {
          document.cookie = "website#lang=en;path=/";
      }

  });
});
