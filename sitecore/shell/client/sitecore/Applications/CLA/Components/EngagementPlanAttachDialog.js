/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
    return _sc.Definitions.App.extend({
        initialized: function () {
            _sc.on("attachEngagementPlan", this.openDialog, this);
            this.isSaving = false;

            self = this;

            this.CreateButton.set("isEnabled", false);
            this.updateStateControls();


            this.EngagemenPlanSource.refresh();
            this.EPTemplates.on("change:selectedItemId", function () {

              this.enableCreateButton();
              this.updateStateControls();

              var selectedIdImported = this.EPTemplates.get("selectedItemId");


              // GetStates
              $.get("/api/sitecore/cla/campaign/GetEngagementPlanStates?ts=" + new Date().getTime()
                   + "&engagementSourceId=" + selectedIdImported)
                  .done(function (data) {
                    
                    self.StatesList.set("items", data);
                  });

            }, this);
          
            this.StatesList.on("change:selectedItemId", function () {
              this.enableCreateButton();

            }, this);

        },
        
        updateStateControls: function () {
          var selectedIdImported = this.EPTemplates.get("selectedItemId");
          var isVisible = (selectedIdImported != null && selectedIdImported != "");
          if (isVisible) {
            this.StatesLabel.viewModel.$el.show();
            this.StatesList.viewModel.$el.show();            
          } else {
            this.StatesLabel.viewModel.$el.hide();
            this.StatesList.viewModel.$el.hide();
          }
        },
        
        openDialog: function () {
            this.AttachEngagementPlan.show();
        },
        enableCreateButton: function () {
          this.CreateButton.set("isEnabled", this.isTemplateEntered());
        },
        closePopup: function () {
            this.EngagemenPlanSource.refresh();
        },
        
        attachLandingPage: function () {
            var self = this,
            selectedIdImported = this.EPTemplates.get("selectedItemId"),

            stateName = "";
            if(this.StatesList.get("selectedItemId"))
              stateName = this.StatesList.get("selectedItem").attributes.Name;
             
            selected = selectedIdImported;
            if (this.isSaving) {
                return;
            }

            var camp = this.get("camp");
            camp.once("engagementPlanCreated", this.engagementPlanCreatedListener, this);

            if (selected && stateName) {
                this.ProgressIndicator.set("isBusy", true);
                this.isSaving = true;
                

                camp.attachEngagementPlan(selected, stateName);

                this.isSaving = false;                
                this.AttachEngagementPlan.hide();

            } else {
                if (!selected) {
                    this.CreateCampaignMessageBar.set("isVisible", true);
                }
            }

        },
        
        engagementPlanCreatedListener: function () {
          this.ProgressIndicator.set("isBusy", false);
        },
        
        isTemplateEntered: function () {
            var selectedIdImported = this.EPTemplates.get("selectedItemId");
            var selectedIdState = this.StatesList.get("selectedItemId");

            return (selectedIdImported && selectedIdState);
        }
    });
});
