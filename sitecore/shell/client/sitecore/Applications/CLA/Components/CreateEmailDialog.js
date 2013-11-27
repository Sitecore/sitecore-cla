/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
    return _sc.Definitions.App.extend({
        initialized: function () {
            _sc.on("createEmail", this.openDialog, this);
            this.isSaving = false;
          
            this.CreateButton.set("isEnabled", false);
          
            this.EmailTemplateSource.refresh();
            this.EmailTemplates.on("change:selectedItemId", function () {
                this.enableCreateButton();

            }, this);
        },
        openDialog: function () {
            this.CreateEmail.show();
        },
        enableCreateButton: function () {
            this.CreateButton.set("isEnabled", this.isNameAndTemplateEntered());
        },
        closePopup: function () {
            this.EmailTemplateSource.refresh();
        },
        
        createEmail: function () {
            var self = this,
             selectedIdImported = this.EmailTemplates.get("selectedItemId"),
             
             selected = selectedIdImported;
            if (this.isSaving) {
                return;
            }

            var camp = this.get("camp");
            camp.once("emailCreated", this.emailCreatedListener, this);

            if (selected) {
                this.ProgressIndicator.set("isBusy", true);
                this.isSaving = true;
                

                camp.createEmail(selected);

                this.isSaving = false;
                this.CreateEmail.hide();

            } else {
                if (!selected) {
                    this.CreateCampaignMessageBar.set("isVisible", true);
                }
            }

        },
        
        emailCreatedListener: function () {
          this.ProgressIndicator.set("isBusy", false);
      },
        
      isNameAndTemplateEntered: function () {
            var selectedIdImported = this.EmailTemplates.get("selectedItemId");

            return (selectedIdImported);
        }
    });
});
