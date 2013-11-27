define(["sitecore"], function (_sc) {
  _sc.Factories.createBaseComponent({
    name: "SocialMessagesRepeater",
    selector: ".sc-social-repeater",

    initialize: function () {
      this.templates =
      {
        twitter: $("#sc-social-repeater-twitter-message-template").html(),
        facebook: $("#sc-social-repeater-facebook-message-template").html()
      };

      $("#sc-social-repeater-NewFacebook")
        .on("click", { sender: this }, this.showFacebookMessageDialog);
      $("#sc-social-repeater-NewTwitter")
        .on("click", { sender: this }, this.showTwitterMessageDialog);

      $("#sc-social-repeater-fb-btn-create")
        .on("click", { sender: this }, this.createFacebookMessage);
      $("#sc-social-repeater-twitter-btn-create")
        .on("click", {sender: this }, this.createTwitterMessage);

      $("#sc-social-repeater-fb-btn-edit")
        .on("click", { sender: this }, this.editFacebookMessage);
      $("#sc-social-repeater-twitter-btn-edit")
        .on("click", {sender: this }, this.editTwitterMessage);
		
		var databaseUri = new _sc.Definitions.Data.DatabaseUri("master");
		this.database = new _sc.Definitions.Data.Database(databaseUri);

      this.render();
    },

    publishSocialMessage: function(event) {
      var self = event.data.sender;

      var data =
      {
        id: $(this).attr("data-id")
      };

      $.ajax({
          url: "/api/sitecore/cla/campaign/PublishSocialMessage",
          data:
            {
              messageId: data.id
            },
          type: "POST",
          complete: function (jqXHR, textStatus) {
            alert("Message posted");
          }
      });
    },

    showFacebookMessageDialog: function(event) {
      var self = event.data.sender;
      self.resetDialogs();
	  self.getSocialAccounts("Facebook", "", $("#sc-social-repeater-fb-dialog"));
    },
	
	getSocialAccounts: function(ntName, mId, dialog)
	{
		var self = this;
		$.ajax({
          url: "/api/sitecore/cla/campaign/SocialAccounts",
          data:
            {
              networkName : ntName,
			  messageId: mId
			},
		  type: "GET",
          complete: function (jqXHR, textStatus) {
			dialog.find(".account-selector").each(function(index, el)
			{
				self.renderAccountsList(el, jqXHR.responseJSON.accounts, jqXHR.responseJSON.selectedaccounts);
			});
            dialog.show();
          }
		});
	},

    showTwitterMessageDialog: function(event) {
      var self = event.data.sender;
      self.resetDialogs();
	  self.getSocialAccounts("Twitter", "", $("#sc-social-repeater-twitter-dialog"));
    },

    showEditFacebookMessageDialog: function(event) {
      var self = event.data.self;
	  var messageid = $(this).attr("data-id");
      $("#facebook-edit-dialog-message-id").val($(this).attr("data-id"));

      $("#facebook-edit-dialog-message").val($(this).attr("data-message"));
      $("#facebook-edit-dialog-link-title").val($(this).attr("data-link-title"));
      $("#facebook-edit-dialog-link-description").val($(this).attr("data-link-description"));
      if ($(this).attr("data-publish") == "1") {
        $("#facebook-edit-dialog-publish").attr("checked", "true");
      };
	  self.getSocialAccounts("Facebook", messageid, $("#sc-social-repeater-fb-edit-dialog"));
    },
	
	

    showEditTwitterMessageDialog: function(event) {
      var self = event.data.self;
	  var messageid = $(this).attr("data-id");
      $("#twitter-edit-dialog-message-id").val($(this).attr("data-id"));

      $("#twitter-edit-dialog-message").val($(this).attr("data-message"));
      if ($(this).attr("data-publish") == "1") {
        $("#twitter-edit-dialog-publish").attr("checked", "true");
      };
	  self.getSocialAccounts("Twitter", messageid, $("#sc-social-repeater-twitter-edit-dialog"));
    },

    createFacebookMessage: function(event) {
      var self = event.data.sender;
	  
	  var acc = "";
	  
	  $("#sc-social-repeater-fb-dialog .account-selector input:checked").each(function(index, el){
		acc=acc+($(el).attr("value"))+"|";
	  })
	  
	  

      var data =
      {
        itemId: self.getItemId(),
        message: $("#facebook-dialog-message").val(),
        linkTitle: $("#facebook-dialog-link-title").val(),
        linkDescription: $("#facebook-dialog-link-description").val(),
        publishWithItem: $("#facebook-dialog-publish").is(":checked"),
		accounts: acc
      };

      $.ajax({
          url: "/api/sitecore/cla/campaign/CreateSocialMessage",
          data:
            {
              campaignId: data.itemId,
              networkName: "Facebook",
              messageText: data.message,
              publishWithItem: data.publishWithItem,
              linkTitle: data.linkTitle,
              linkDescription: data.linkDescription,
			  accounts: data.accounts
            },
          type: "POST",
          complete: function (jqXHR, textStatus) {
            self.render();
          }
      });

      $("#sc-social-repeater-fb-dialog").hide();
      self.resetDialogs();
    },

    createTwitterMessage: function(event) {
      var self = event.data.sender;
	  
	  var acc = "";
	  
	  $("#sc-social-repeater-twitter-dialog .account-selector input:checked").each(function(index, el){
		acc=acc+($(el).attr("value"))+"|";
	  })
	  
      var data =
      {
        itemId: event.data.sender.getItemId(),
        messageText: $("#twitter-dialog-message").val(),
        publishWithItem: $("#twitter-dialog-publish").is(":checked"),
		accounts:acc
		
      };

      $.ajax({
          url: "/api/sitecore/cla/campaign/CreateSocialMessage",
          data:
            {
              campaignId: data.itemId,
              networkName: "Twitter",
              messageText: data.messageText,
              publishWithItem: data.publishWithItem,
			  accounts: data.accounts
            },
          type: "POST",
          complete: function (jqXHR, textStatus) {
            self.render();
          }
      });

      $("#sc-social-repeater-twitter-dialog").hide();
      self.resetDialogs();
    },

    editFacebookMessage: function(event) {
      var self = event.data.sender;
	  
	  var acc = "";
	  
	  $("#sc-social-repeater-fb-edit-dialog .account-selector input:checked").each(function(index, el){
		acc=acc+($(el).attr("value"))+"|";
	  })

      var data =
      {
        itemId: self.getItemId(),
        messageId: $("#facebook-edit-dialog-message-id").val(),
        message: $("#facebook-edit-dialog-message").val(),
        linkTitle: $("#facebook-edit-dialog-link-title").val(),
        linkDescription: $("#facebook-edit-dialog-link-description").val(),
        publishWithItem: $("#facebook-edit-dialog-publish").is(":checked"),
		accounts: acc
      };

      $.ajax({
          url: "/api/sitecore/cla/campaign/EditSocialMessage",
          data:
            {
              campaignId: data.itemId,
              messageId: data.messageId,
              networkName: "Facebook",
              messageText: data.message,
              publishWithItem: data.publishWithItem,
              linkTitle: data.linkTitle,
              linkDescription: data.linkDescription,
			  accounts: data.accounts
            },
          type: "PUT",
          complete: function (jqXHR, textStatus) {
            self.render();
          }
      });

      $("#sc-social-repeater-fb-edit-dialog").hide();
      self.resetDialogs();
    },

    editTwitterMessage: function(event) {
      var self = event.data.sender;
	  
	  var acc = "";
	  
	  $("#sc-social-repeater-twitter-edit-dialog .account-selector input:checked").each(function(index, el){
		acc=acc+($(el).attr("value"))+"|";
	  })

      var data =
      {
        itemId: self.getItemId(),
        messageId: $("#twitter-edit-dialog-message-id").val(),
        message: $("#twitter-edit-dialog-message").val(),
        publishWithItem: $("#twitter-edit-dialog-publish").is(":checked"),
		accounts: acc
      };

      $.ajax({
          url: "/api/sitecore/cla/campaign/EditSocialMessage",
          data:
            {
              campaignId: data.itemId,
              messageId: data.messageId,
              networkName: "Twitter",
              messageText: data.message,
              publishWithItem: data.publishWithItem,
			  accounts: data.accounts
            },
          type: "PUT",
          complete: function (jqXHR, textStatus) {
            self.render();
          }
      });

      $("#sc-social-repeater-twitter-edit-dialog").hide();
      self.resetDialogs();
    },

    deleteMessage: function(event) {
      if (!confirm("Are you sure you want to delete this social message?")) {
        return;
      };

      var self = event.data.self;

      $.ajax({
          url: "/api/sitecore/cla/campaign/DeleteSocialMessage",
          data:
            {
              "id": $(this).attr("data-id")
            },
          type: "DELETE",
          complete: function (jqXHR, textStatus) {
            self.render();
          }
      });
    },

    resetDialogs: function() {
	  var self = this;
      $("#twitter-dialog-message").val("");
      $("#facebook-dialog-message").val("");
      $("#facebook-dialog-link-title").val("");
      $("#facebook-dialog-link-description").val("");

      $("#twitter-edit-dialog-message").val("");
      $("#facebook-edit-dialog-message").val("");
      $("#facebook-edit-dialog-link-title").val("");
      $("#facebook-edit-dialog-link-description").val("");

      $("#facebook-edit-dialog-message-id").val("");
      $("#twitter-edit-dialog-message-id").val("");
    },
	
	renderAccountsList: function(el, accounts, selected)
	{
		var sortedaccounts = new Object();
		var count =0;
		var length = accounts.length;
		$(el).find("div").remove("div");
		
		for (i in accounts)
		{
			this.database.getItem(accounts[i], function(data, test, test){
				var item = data.toModel();
				var accountname = item.get("itemName");
				var accountid = item.get("itemId");
				sortedaccounts[accountid] = accountname;
				count++;
				if (count == length)
				{
					for (k in accounts)
					{
						var accountinputdiv = $("<div></div>");
				
						var accountinput = $("<input type='checkbox'>"+sortedaccounts[accounts[k]]+"</input>");
						accountinput.attr("id","acc_"+accounts[k]);
						accountinput.attr("value", accounts[k]);
						accountinputdiv.append(accountinput);
						for (j in selected)
						{
							if (accounts[k] == selected[j])
							{
								accountinput.attr("checked", true);
							}
						}
						$(el).append(accountinputdiv);
					}
				}
				
				
			});
		}
	},

    render: function() {
      var self = this;

      $.ajax({
        url: "/api/sitecore/cla/campaign/SocialMessages?ts=" + new Date().getTime(),
        data:
          {
            "campaignId": self.getItemId()
          },
        type: "GET",
        complete: function(jqXHR, textStatus ) {
          self.messages = jqXHR.responseJSON;
          self.$el.empty();
          for (var i = 0; i < self.messages.length; i++) {
			var accounts = ""
			for (j in self.messages[i].Accounts)
			{
				accounts=accounts+"<li>"+self.messages[i].Accounts[j]+"</li>";
			}
			self.messages[i].AccountsList = accounts;
            if (self.messages[i].PublishWithItem === "1") {
              self.messages[i].PublishWithItemText
                = "The message will be posted automatically."
            }
            else {
              self.messages[i].PublishWithItemText = "";
            }
            if (self.messages[i].NetworkName === "Facebook") {
              self.$el.append(_.template(self.templates.facebook, self.messages[i]));
            }
            else if (self.messages[i].NetworkName === "Twitter") {
              self.$el.append(_.template(self.templates.twitter, self.messages[i]));
            };
          };

          $(".delete-twitter-message").on("click", { self: self }, self.deleteMessage);
          $(".delete-fb-message").on("click", { self: self }, self.deleteMessage);
          $(".edit-twitter-message").on("click", { self: self }, self.showEditTwitterMessageDialog);
          $(".edit-fb-message").on("click", { self: self }, self.showEditFacebookMessageDialog);

          $(".sc-social-repeater-fb-btn-publish")
            .on("click", { self: self }, self.publishSocialMessage);
          $(".sc-social-repeater-twitter-btn-publish")
            .on("click", { self: self }, self.publishSocialMessage);
        }
      });
    },
	
	renderSimpleAccountsList: function(network)
	{
		
	},

    getItemId: function() {
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
      return id;
    }
  });
});
