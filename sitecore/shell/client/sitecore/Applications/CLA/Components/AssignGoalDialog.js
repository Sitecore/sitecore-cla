/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
    return _sc.Definitions.App.extend({
        initialized: function () {
            _sc.on("assignGoals", this.openDialog, this);
            
        },
        
        openDialog: function (variantId) {
            var self = this;
            self.set("variantid", variantId);
            $.get("/api/sitecore/cla/campaign/GetAssignedGoals?ts=" + new Date().getTime(),
              {
                  "itemid": variantId
              })
              .done(function (data) {
                  self.renderGoalsList(data);
                  self.AssignGoal.show();
              });
            
        },
        
        renderGoalsList: function (data) {
            var goalBorder = this.AttachGoalDialogBorder.viewModel.$el;
            goalBorder.empty();
            for (var i in data) {
                var goal = data[i];
                var div = $("<div></div>");
                var checkbox = $("<input type='checkbox'>" + goal.goalname + "</input>");
                checkbox.attr("checked", goal.isselected);
                checkbox.attr("goalid", goal.goalid);
                div.append(checkbox);
                goalBorder.append(div);
            }
        },

        closePopup: function () {
            
        },
        

        assignGoals: function () {
            var self = this;
            var variantid = self.get("variantid");
            var selected = "";
            var goalsborder = this.AttachGoalDialogBorder.viewModel.$el;
            var goals = goalsborder.find("input:checked");

            goals.each(function() {
                var g = $(this);
                selected = selected + g.attr("goalid")+"|";
            });

            $.post("/api/sitecore/cla/campaign/AssignGoals?ts=" + new Date().getTime(),
              {
                  "itemid": variantid,
                  "selected":selected
              })
              .done(function (data) {
                  
              });
            this.AssignGoal.hide();
            
        }
    });
});
