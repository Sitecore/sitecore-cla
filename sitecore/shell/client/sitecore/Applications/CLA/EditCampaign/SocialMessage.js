/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {
  var SocialMessage = _sc.Definitions.App.extend({
    initialized: function () {
    },

    deleteMessage: function () {
      _sc.trigger("removeSubApp", this);
    }

  });

  return SocialMessage;
});