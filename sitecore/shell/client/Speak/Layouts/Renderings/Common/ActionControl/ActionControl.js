/// <reference path="../../../../../../assets/vendors/lodash/lodash.min.js" />
/// <reference path="../../../../../../assets/vendors/Backbone/backbone.js" />
/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />
/// <reference path="../../../../../../assets/vendors/KnockOut/knockout-2.1.0.js" />

/*
* Local Storage
* Styling
* Close when clicking outside
* BDD tests
*/
define(["sitecore", "knockout", "bootstrap"], function (Sitecore, ko) {
  var ActionModel = function () {
    this.id = new ko.observable("");
    this.text = new ko.observable("");
    this.tooltip = new ko.observable("");
    this.isIcon = new ko.observable(false);
    this.iconSrc = new ko.observable("");
    this.iconBackgroundPosition = new ko.observable("");
    this.isFavorite = new ko.observable(false);
    this.isDefaultAction = new ko.observable(false);
    this.click = new ko.observable("");
  };

  ActionModel.prototype.invoke = function (app) {
    var click = this.click();
    if (click) {
      Sitecore.Helpers.invocation.execute(click, { control: this, app: app });
    }
  };

  var model = Sitecore.Definitions.Models.ControlModel.extend({
    initialize: function (options) {
      this._super();

      this.set("text", "");
      this.set("isOpen", false);
      this.set("actions", []);
      this.set("favorites", []);
    }
  });

  var view = Sitecore.Definitions.Views.ControlView.extend(
    {
      initialize: function (options) {
        this._super();
		//CLA #17777
		this.$el.show();
        var actions = this.$el.find("[data-sc-actionid]").map(function () {
          var action = new ActionModel(),
          url = $(this).find("div").css('background-image');
          backgroundPosition = $(this).find("div").css('background-position');

          action.id($(this).attr("data-sc-actionid"));
          action.text($(this).text());
          action.iconSrc(url);
          action.iconBackgroundPosition(backgroundPosition);
          action.isIcon(url ? true : false);

          action.isFavorite($(this).attr("data-sc-favorite") === "true");
          action.tooltip($(this).attr("data-sc-tooltip"));
          action.isDefaultAction($(this).attr("data-sc-favorite") === "true");
          action.click($(this).attr("data-sc-click"));

          return action;
        });
       
        this.model.set("actions", actions);
        this.updateFavorites();
      },

      toggleIsOpen: function () {
        this.model.set("isOpen", !this.model.get("isOpen"));
      },

      toggleFavorite: function (data, event) {
        
        $(event.target).toggleClass("selected");
        var action = this.getAction(event.target);

        if (action != null) {
          action.isFavorite(!action.isFavorite());
          this.updateFavorites();
        }
      },

      invokeAction: function (data, event) {
        this.model.set("isOpen", false);

        var action = this.getAction(event.target);
        if (action != null) {
          action.invoke(this.app);
        }
      },

      invokeFavorite: function (action) {
        action.invoke(this.app);
      },

      getAction: function (target) {
        var source = $(target);
        if (!source.attr("data-sc-actionid")) {
          source = source.parents("[data-sc-actionid]");
        }

        if (source == null) {
          return null;
        }

        var id = $(source).attr("data-sc-actionid");
        if (!id) {
          return null;
        }

        return _.find(this.model.get("actions"), function (e) {
          return e.id() == id;
        });
      },

      updateFavorites: function () {
        var favorites = _.select(this.model.get("actions"), function (e) {
          return e.isFavorite();
        });

        this.model.set("favorites", favorites);
      }
    });

  Sitecore.Factories.createComponent("ActionControl", model, view, ".sc-actioncontrol");
});