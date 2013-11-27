/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
  var Emails = _sc.Definitions.App.extend({
    initialized: function () {
      
    },    
    openinEcm: function () {
        var emailId = this.get("emailId");

        //opening in other window
        var url = "/speak/EmailCampaign/TaskPages/TrickleTaskPage.aspx?id=" + emailId + "&sc_speakcontentlang=en";
        var editWindow = window.open(url, "", "height=" + $(document).height() + ",width=" + screen.width + ",top=1,left=1");
        editWindow.resizeTo(screen.availWidth, screen.availHeight);
    }

    });
  return Emails;
});
