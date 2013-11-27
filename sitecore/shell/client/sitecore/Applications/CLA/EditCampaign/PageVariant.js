/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
    var PageVariant = _sc.Definitions.App.extend({
        initialized: function () {
            this.on("change:variantId", this.getVariant, this);
            this.on("change:EditCampaign", this.editCampaignChanged, this);
          
            this.on("change:status", this.setStatus, this);
        },

        setStatus: function () {
            var state = this.get("status");
            this.updateEditingControls(state);
        },

        editCampaignChanged: function () {
            var versionsListControl = this.get("EditCampaign").VersionsListControl;
            var state;
            if (versionsListControl.get() != undefined) {


                if (versionsListControl.get("selectedItems").length === 0) {
                    state = versionsListControl.get("items")[0].State;
                }
                else {
                    state = versionsListControl.get("selectedItems")[0].State();
                }

                this.updateEditingControls(state);
            }
        },
        
        updateEditingControls: function (state) {
            var CreateVariantButton;
            if(this.get("EditCampaign") != null && this.get("EditCampaign").CreateVariantButton != null)
                CreateVariantButton = this.get("EditCampaign").CreateVariantButton;

            if (state === "Published" || state === "Obsolete") {
                this.ActionControl.viewModel.$el.hide();
                if (CreateVariantButton != null)
                    CreateVariantButton.viewModel.$el.hide();
            }
            else {
                this.ActionControl.viewModel.$el.show();
                if (CreateVariantButton != null)
                    CreateVariantButton.viewModel.$el.show();
            }

        },

        getVariant: function () {
            var databaseUri = new _sc.Definitions.Data.DatabaseUri("master"),
              database = new _sc.Definitions.Data.Database(databaseUri),
              app = this;

            database.getItem(this.get("variantId"), function (data) {
                app.currentItem = data.toModel();
            });
        },
        
        assignGoals: function () {
            var app = this;
            var parentapp = app.get("EditCampaign");
            parentapp.assignGoals(this.get("variantId"));
        },

        deleteVariant: function () {
            var app = this;
            var parentapp = app.get("EditCampaign");
            var campaign = parentapp.campaign;

            var currentVersion = campaign.get("currentVersion");

            var databaseUri = new _sc.Definitions.Data.DatabaseUri("master");
            var database = new _sc.Definitions.Data.Database(databaseUri);
            var id = "";
            var self = this;

            var sURLVariables = window.location.search
              .substring(1)
              .split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split("=");
                if (sParameterName[0] == "id") {
                    id = sParameterName[1];
                }
            }

            campaign.set("variantId", app.get("variantId"));
            parentapp.resetLanguageContext();
            campaign.removeCurrentVariant();
        },

        duplicateVariant: function () {
            var app = this;
            var parentapp = app.get("EditCampaign");
            var campaign = parentapp.campaign;

            var currentVersion = campaign.get("currentVersion");

            var databaseUri = new _sc.Definitions.Data.DatabaseUri("master");
            var database = new _sc.Definitions.Data.Database(databaseUri);
            var id = "";
            var self = this;

            var sURLVariables = window.location.search
              .substring(1)
              .split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split("=");
                if (sParameterName[0] == "id") {
                    id = sParameterName[1];
                }
            }

            campaign.set("variantId", app.get("variantId"));
            parentapp.resetLanguageContext();
            campaign.duplicateCurrentVariant();
        },

        editVariant: function () {
            // open PageEditor
            _sc.trigger("openPageEditor", this.get("variantId"));
        },

    });
    return PageVariant;
});
