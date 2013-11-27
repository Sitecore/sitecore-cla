define(["sitecore"], function (_sc) {

    _sc.Campaign = Backbone.Model.extend({
        url: function () {
            return "/api/sitecore/cla/campaign/Campaign";
        },
        attachEmail: function () {

        },
        attachEngagementPlan: function (id, stateName) {
            var self = this;

            $.post("/api/sitecore/cla/campaign/AttachEngagementPlan", {
                engagementId: id,
                campaignId: this.get("itemId"),
                stateName: stateName
            }).done(function (data) {
                self.set(data);
                self.trigger("engagementPlanCreated", "event");
            });
        },
        
        retire: function() {
            var self = this;
            var id = self.get("itemId");
            $.post("/api/sitecore/cla/campaign/Retire",
                {
                    "campaignId": id,
                }).done(function (data) {
                    self.trigger("campaignretired", "event");
                    self.set("isRetired", data.isRetired);
                });
        },
        
        activate: function() {
            var self = this;
            var id = self.get("itemId");
            $.post("/api/sitecore/cla/campaign/Activate",
                {
                    "campaignId": id,
                }).done(function (data) {
                    self.trigger("campaignactivated", "event");
                    self.set("isRetired", data.isRetired);
                });
        },
        
        createEmail: function(id) {
            var self = this;

            $.post("/api/sitecore/cla/campaign/CreateEmail", {
                emailTemplateId: id,
                campaignId: this.get("itemId")
            }).done(function (data) {
                self.set(data);
                self.trigger("emailCreated", "event");
                self.emails();
            });
        },

        removeCurrentVersion: function () {

          if (!confirm("Are you sure you want to delete this version?")) {
            return;
          };

            var self = this;

            $.post("/api/sitecore/cla/campaign/RemoveVersion",
              {
                  "campaignId": this.get("itemId"),
                  "versionId": this.get("currentVersion")
              }).done(function (data) {
                  self.set(data);
                  self.trigger("versionRemoved", "event");
              });
        },
        
        attachVariant: function(variantTemplateId) {
            var self = this;
            
            if (variantTemplateId != undefined) {
                $.post("/api/sitecore/cla/campaign/AttachVariant",
              {
                  "campaignId": this.get("itemId"),
                  "versionId": this.get("currentVersion"),
                  "variantTemplateId": variantTemplateId
              }).done(function (data) {
                  self.trigger("variantAdded", "event");
              });
            }
        },

        removeCurrentVariant: function () {
          if (!confirm("Are you sure you want to delete this variant?")) {
            return;
          };
            var self = this;

            $.ajax({
                url: "/api/sitecore/cla/campaign/RemoveVariant",
                data:
                  {
                      "campaignId": this.get("itemId"),
                      "variantId": this.get("variantId"),
                      "currentVersion": this.get("currentVersion")
                  },
                type: "DELETE",
                complete: function (jqXHR, textStatus) {
                    self.trigger("variantRemoved", "event");
                }
            });
        },

        copyCurrentVersion: function () {
            var self = this;
            $.post("/api/sitecore/cla/campaign/CopyVersion", { "campaignId": this.get("itemId"), "versionId": this.get("currentVersion") }).done(function (data) {
                self.set(data);
                self.trigger("versionAdded", "event");
            });
        },

        duplicateCurrentVariant: function () {
            var self = this;
            $.post("/api/sitecore/cla/campaign/CopyVariant",
              {
                  "campaignId": this.get("itemId"),
                  "variantId": this.get("variantId"),
                  "currentVersion": this.get("currentVersion")
              }).done(function (data) {
                  self.set(data);
                  self.trigger("variantRemoved", "event");
              });
        },

        versions: function () {
            var self = this;

            var campaignId = "";

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    campaignId = sParameterName[1];
                }
            }

            var langName = self.get("langName");
            if (typeof langName == "undefined" || langName == null)
                langName = "en";

            $.get("/api/sitecore/cla/campaign/Versions?ts=" + new Date().getTime(),
                {
                    "id": campaignId,
                    "langName": langName
                })
                  .done(function (data) {
                  
                      self.ver = data.versions;
                      self.set("campaignId", data.campaign);
                      self.set("Alias", data.alias);
                      self.trigger("versionsUpdated", "event");
                  });
        },

        variants: function () {
            var self = this;

            var campaignId = "";

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    campaignId = sParameterName[1];
                }
            }

            var currentVersion = self.get("currentVersion");
            var langName = self.get("langName");
            if (typeof langName == "undefined" || langName == null)
                langName = "en";

            $.get("/api/sitecore/cla/campaign/Variants?ts=" + new Date().getTime(),
              {
                  "campaignId": campaignId,
                  "currentVersion": currentVersion,
                  "langName": langName
              })
              .done(function (data) {
                  self.variantsUpdated(data);
              });
        },
        
        emails: function () {
            var self = this;

            var campaignId = "";

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    campaignId = sParameterName[1];
                }
            }

            var currentVersion = self.get("currentVersion");

            $.get("/api/sitecore/cla/campaign/Emails?ts=" + new Date().getTime(),
              {
                  "campaignId": campaignId,
                  "currentVersion": currentVersion
              })
              .done(function (data) {
                  self.set(data);
                  self.trigger("emailsLoaded", "event");
              });
        },

        workflow: function () {
            var self = this;

            var campaignId = "";

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    campaignId = sParameterName[1];
                }
            }

            var currentVersion = self.get("currentVersion");

            $.get("/api/sitecore/cla/campaign/WorkflowHistory?ts=" + new Date().getTime(),
                {
                    "campaignId": campaignId,
                    "versionId": currentVersion
                }).done(function (data) {
                    self.set(data);
                    self.trigger("workflowUpdated", "event");
                });
        },
        
        publish: function() {
            var self = this;
            var id = self.get("itemId");
            var currentVersion = self.get("currentVersion");
            $.post("/api/sitecore/cla/campaign/Publish",
                {
                    "campaignId": id,
                    "versionId": currentVersion
                }).done(function () {
                    self.trigger("versionPublished", "event");
                    self.versions();
                });

        },
        workflowCommand: function (commandid, comment) {
            var self = this;

            var campaignId = "";

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    campaignId = sParameterName[1];
                }
            }

            var currentVersion = self.get("currentVersion");

            var self = this;

            $.post("/api/sitecore/cla/campaign/ExecuteWorflowCommand",
                {
                    "campaignId": campaignId,
                    "versionId": currentVersion,
                    "commandId": commandid,
                    "comment": comment
                }).done(function (data) {
                    self.trigger("workflowCommandDone", "event");
                    self.versions();
                });
        },

        ver: null,
        
        _variants: null,

        variantsUpdated: function (data) {
          this._variants = data.result;
          if (data.status != null && data.status != "")
            this.set("status", data.status);
          this.trigger("variantsUpdated", "event");
        }
    });

    _sc.Campaigns = Backbone.Collection.extend({
        url: "/sitecore/client/campaign/all",
        model: _sc.Campaign,
        topLandingPages: function () {
            var self = this;
            $.get("/api/sitecore/cla/campaign/TopLandingPages").done(function (data) {
                self.addToCollection(data);
            });
        },

        addToCollection: function (data) {
            _.each(data, function (item) {
                var single = item.Campaign;
                single.Value = item.Value;
                this.add(single);
            }, this);
        }
    });

});
