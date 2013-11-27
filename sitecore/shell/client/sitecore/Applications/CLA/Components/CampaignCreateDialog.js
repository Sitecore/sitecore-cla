/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
  return _sc.Definitions.App.extend({
    initialized: function () {

      self = this;
      _sc.on("createDialog", this.openDialog, this);
      this.NameTextBox.on("change:text", this.enableCreateButton, this);
      this.DataSource.refresh();
      this.isSaving = false;
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
        

      //checking selected values - for enabling CreateButton
      setInterval(function () {
          self.enableCreateButton();
      }, 500);

    },
    openDialog: function () {
        this.resetValues();
      this.CreateCampaignDialog.show();
    },
    resetValues: function () {
        this.NameTextBox.set("text", "");
        this.TemplatesListControl.set("selectedItemId", "");
        this.ImportedTemplatesListControl.set("selectedItemId", "");
        $("[data-sc-id=ImportedTemplatesListControl]").find(".active").removeClass("active");
        $("[data-sc-id=TemplatesListControl]").find(".active").removeClass("active");
    },
    enableCreateButton: function () {
      this.CreateButton.set("isEnabled", this.isNameAndTemplateEntered());
    },
    closePopup: function () {
      this.ImportedTemplatesDataSource.refresh();
    },
    createCampaign: function () {
        var self = this,
          name = this.NameTextBox.get("text"),
          selectedIdImported = this.ImportedTemplatesListControl.get("selectedItemId"),
          selectedIdDefault = this.TemplatesListControl.get("selectedItemId"),
          selected = selectedIdImported || selectedIdDefault;
      if (this.isSaving) {
          return;
      }

      $("[data-sc-id=NameTextBox]").removeAttr("style");
      this.CreateCampaignMessageBar.set("isVisible", false);
        
      if (name && selected) {
          var campaign = new _sc.Campaign({ name: name, itemid: selected });
          this.ProgressIndicator.set("isBusy", true);
          this.isSaving = true;
         campaign.save().done(function (data) {
            self.goToCampaign(data.itemId);
            this.isSaving = false;
            this.ProgressIndicator.set("isBusy", false);
         }).fail(function (jqXHR, textStatus, errorThrown) {
             self.isSaving = false;
             self.ProgressIndicator.set("isBusy", false);
             $("[data-sc-id=NameTextBox]").css("border-color", "red");
            alert(errorThrown);
        });
      } else {
        if (!selected) {
          this.CreateCampaignMessageBar.set("isVisible", true);
        }
        if (!name) {
          $("[data-sc-id=NameTextBox]").css("border-color", "red");
        }
      }
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
     var name = this.NameTextBox.viewModel.$el[0].value,
        selectedIdImported = this.ImportedTemplatesListControl.get("selectedItemId"),
        selectedIdDefault = this.TemplatesListControl.get("selectedItemId");

      var selVal = (name && (selectedIdImported || selectedIdDefault));
      return (selVal != null && selVal != "");
    }
  });
});
