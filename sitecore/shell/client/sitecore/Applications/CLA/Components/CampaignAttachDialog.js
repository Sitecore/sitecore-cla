/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
    return _sc.Definitions.App.extend({
        initialized: function () {
            _sc.on("addLandingPage", this.openDialog, this);
            this.isSaving = false;
            this.DataSource.refresh();

            this.CreateButton.set("isEnabled", false);

            this.ImportedTemplatesDataSource.refresh();
            this.ImportedTemplatesListControl.on("change:selectedItemId", function () {
                this.enableCreateButton();

                if (this.ImportedTemplatesListControl.get("selectedItemId")) {
                    this.TemplatesListControl.set("selectedItemId", "");
                    $("[data-sc-id=TemplatesListControl]").find(".active").removeClass("active");
                }
            }, this);

            this.TemplatesListControl.on("change:selectedItemId", function () {
                this.enableCreateButton();
                if (this.TemplatesListControl.get("selectedItemId")) {
                    this.ImportedTemplatesListControl.set("selectedItemId", "");
                    $("[data-sc-id=ImportedTemplatesListControl]").find(".active").removeClass("active");
                }
            }, this);
        },

        openDialog: function () {
            this.AttachLandingPage.show();
        },
        enableCreateButton: function () {
            this.CreateButton.set("isEnabled", this.isNameAndTemplateEntered());
        },
        closePopup: function () {
            this.ImportedTemplatesDataSource.refresh();
        },
        attachLandingPage: function () {
            var self = this,
             selectedIdImported = this.ImportedTemplatesListControl.get("selectedItemId"),
             selectedIdDefault = this.TemplatesListControl.get("selectedItemId"),
             selected = selectedIdImported || selectedIdDefault;
            if (this.isSaving) {
                return;
            }

            var camp = this.get("camp");
            camp.once("variantAdded", this.variantAddedListener, this);

            if (selected) {
                this.ProgressIndicator.set("isBusy", true);
                this.isSaving = true;
                camp.attachVariant(selected);

                this.isSaving = false;
                this.AttachLandingPage.hide();

            } else {
                if (!selected) {
                    this.CreateCampaignMessageBar.set("isVisible", true);
                }
            }

        },
        
        variantAddedListener: function () {
          this.ProgressIndicator.set("isBusy", false);
        },

        openDesignImporter: function () {
            var self = this,
                designImporter = window.open("/sitecore%20modules/Shell/DesignImporter/DesignImporter.aspx?saveOptions=show&mo=popup&cp={CF59E54B-EE42-45E0-8DE9-9C52B7538F24}", "designImporter", "width=800,height=600");

            var closure = function () {
                self.closePopup();
            };
            var timer = setInterval(function () {
                if (designImporter.closed) {
                    clearInterval(timer);
                    closure();
                }
            }, 100);
        },
        goToCampaign: function (id) {
            window.location = "/sitecore/client/sitecore/applications/cla/EditCampaign?id=" + id + "&ref=CampaignLaunch";
        },
        isNameAndTemplateEntered: function () {
            var selectedIdImported = this.ImportedTemplatesListControl.get("selectedItemId"),
                selectedIdDefault = this.TemplatesListControl.get("selectedItemId");

            return (selectedIdImported || selectedIdDefault);
        }
    });
});
