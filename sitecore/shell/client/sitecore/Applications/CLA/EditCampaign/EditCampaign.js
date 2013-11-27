/// <reference path="../../../../../../assets/lib/dist/sitecore.js" />

define(["sitecore"], function (_sc) {

    var DesignEngagementFrame;
    var MonitorEngagementFrame;
    var SuperviseEngagementFrame;

    var EditCampaign = _sc.Definitions.App.extend({
        initialized: function () {
            $.ajaxSetup({ cache: false });

            var tcontrol = this.TabControl;

            DesignEngagementFrame = this.DesignEngagementFrame;
            MonitorEngagementFrame = this.MonitorEngagementFrame;
            SuperviseEngagementFrame = this.SuperviseEngagementFrame;

            this.TabControl.viewModel.$el.find("> ul li[data-tab-id]").each(function () {
                $(this).bind("click", function () {

                    document.cookie = "CLAActiveTab=" + $(this).attr("data-tab-id");

                });
            });

            this.restoreTab();


            _sc.on("openPageEditor", this.openPageEditor, this);
            var app = this;

            this.campaign = new _sc.Campaign();

            this.campaign.on("versionsUpdated", this.updateVersions, this);
            this.campaign.on("variantsUpdated", this.updateVariants, this);

            this.campaign.on("versionRemoved", this.versionUpdated, this);
            this.campaign.on("versionAdded", this.versionUpdated, this);

            this.campaign.on("workflowUpdated", this.renderWorkFlow, this);

            this.campaign.on("variantRemoved", this.variantUpdated, this);

            this.campaign.on("emailsLoaded", this.updateEmails, this);

            this.campaign.on("variantAdded", this.variantUpdated, this);

            this.campaign.on("change", this.campaignChanged, this);

            this.campaign.on("campaignretired", this.updateRestrictions, this);
            this.campaign.on("campaignactivated", this.updateRestrictions, this);


            // EngagementPlan created
            this.campaign.on("engagementPlanCreated", function () {
                this.EPAccordionBorder.set("isVisible", true);
                var eid = this.campaign.get("engagementId");
                var urlDesign = "/sitecore/shell/Applications/MarketingAutomation/Designer/MarketingAutomationDesigner.aspx?Id=" + eid;
                var urlMonitor = "/sitecore/shell/Applications/MarketingAutomation/Monitor/MarketingAutomationMonitor.aspx?Id=" + eid;
                var urlSupervise = "/sitecore/shell/Applications/MarketingAutomation/Supervisor/MarketingAutomationSupervisor.aspx?Id=" + eid;
                DesignEngagementFrame.set("sourceUrl", urlDesign);
                MonitorEngagementFrame.set("sourceUrl", urlMonitor);
                SuperviseEngagementFrame.set("sourceUrl", urlSupervise);

                $(this.BtnFullScreenDesign.viewModel.$el).bind("click", { url: urlDesign }, this.openEngagementDialogEvent);
                $(this.BtnFullScreenMonitor.viewModel.$el).bind("click", { url: urlMonitor }, this.openEngagementDialogEvent);
                $(this.BtnFullScreenSupervise.viewModel.$el).bind("click", { url: urlSupervise }, this.openEngagementDialogEvent);

                this.EngagmentAttachBorder.set("isVisible", false);

                //delpoyment-workflow
                $.post("/api/sitecore/cla/campaign/ExecuteWorflowCommandCommon",
                    {
                        "itemId": eid,
                        "commandName": "deploy",
                        "comment": "Engagement plan has been deployed"
                    }).done(function (data) {

                    });

            }, this);

            var databaseUri = new _sc.Definitions.Data.DatabaseUri("master"),
                database = new _sc.Definitions.Data.Database(databaseUri),
                id = "",
                self = this;

            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');

            for (var i = 0; i < sURLVariables.length; i++) {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == "id") {
                    id = sParameterName[1];
                }
            }

            this.campaign.set("itemId", id);
            this.updateCampaignInfo();

            // setting WIDTH for textboxes on General-tab
            var widthDescription = 620;
            this.GeneralNameTextBox.viewModel.$el.css("width", widthDescription);
            this.GeneralAliasTextBox.viewModel.$el.css("width", widthDescription);
            this.GeneralDescriptionTextBox.viewModel.$el.css("width", widthDescription);
            this.CostInfoTextBox.viewModel.$el.css("width", widthDescription);

            this.VersionsListControl.on("change:selectedItemId", this.setCurrentVersion, this);

            // GetLanguages
            $.get("/api/sitecore/cla/campaign/GetLanguages?ts=" + new Date().getTime())
              .done(function (data) {
                  if (typeof data != "undefined" && data != null && data.length > 0) {
                      self.ComboLanguage.set("items", data);

                      self.setCurrentLanguage();
                      self.ComboLanguage.on("change:selectedItem", self.setCurrentLanguageWithUpdate, self);

                  }
              });

            this.campaign.versions();

            _sc.on("removeSubApp", this.removeSocialMessage, this);

            // "CampaignAttachDialogLayout"(go to "_StandartValues") - Controls: AttachLandingPage, and etc.
            this.insertRendering("{AB5BE75E-5A68-4598-A133-86E47EC0A22C}", { $el: $("body") }, function (subApp) {
                var attachDialog = app["attachDialog"] = subApp;
                subApp.set("camp", self.campaign);
            });


            // "EngagementPlanAttachDialogLayout"(go to "_StandartValues") - Controls: AttachEngagementPlan, and etc.            
            this.insertRendering("{101C11AC-1D66-4170-BB34-A4CC649E95F2}", { $el: $("body") }, function (subApp) {
                var attachEPDialog = app["attachEPDialog"] = subApp;
                subApp.set("camp", self.campaign);
            });

            // "CreateEmailDialog" (go to "_StandartValues") 
            this.insertRendering("{AE0A9D4B-DF1A-4509-9826-07B0471F2F8D}", { $el: $("body") }, function (subApp) {
                var createEmailDialog = app["createEmailDialog"] = subApp;
                subApp.set("camp", self.campaign);
            });
            
            this.insertRendering("{01B5394E-1F2B-4BBA-A23D-268B62DBEF6D}", { $el: $("body") }, function (subApp) {
                var assignGoalsDialog = app["assignGoalsDialog"] = subApp;
                subApp.set("camp", self.campaign);
            });
            
            

            database.getItem(id, function (data) {
                self.item = data.toModel();
                self.current = new _sc.Campaign({ itemId: id });
                self.current.set("itemId", id);
                self.current.on("change", this.updateInfo, this);
                self.current.set(data);
            });
        },

        updateCampaignInfo: function () {
            var self = this;
            this.campaign.fetch({
                data: { id: this.campaign.get("itemId") }, success: function (model, response, options) {
                    var data = response;
                    self.campaign.set("StartDate", data.StartDate);
                    self.campaign.set("EndDate", data.EndDate);
                    self.DatePicker.set("date", data.StartDate);
                    self.EndDatePicker.set("date", data.EndDate);
                    self.StartDateTValue.set("text", data.StartDate);
                    self.EndDateTValue.set("text", data.EndDate);
                    self.TotalValue.set("text", data.TotalValue);
                    self.ValuePerDay.set("text", data.AvgValue);
                    self.StateValue.set("text", data.State);
                    self.campaign.set("Cost", data.Cost);
                    self.CostInfoTextBox.set("text", data.Cost);
                    self.CostOverallValue.set("text", data.Cost);
              
                    // Cost, Cost per Day
                    if (data.Cost > 0) {
                        self.CostOverallTitle.viewModel.$el.show();
                        self.CostOverallValue.viewModel.$el.show();
                
                        // if there is even over version published then calculate value of "Cost per Day"
                        var isPublished = self.isCampaignPublishVersion();
                        if (isPublished) {
                            try {
                                var today = new Date();
                                var startDate = new Date(data.StartDate);

                                var day = 1000 * 60 * 60 * 24;
                                var diff = Math.floor(today.getTime() - startDate.getTime());
                                var days = Math.floor(diff / day);
                                var costPerDay = 0;
                                if (days > 0)
                                    costPerDay = (data.Cost / days).toFixed(2);
                                self.CostPerDayValue.set("text", costPerDay);
                    
                                self.CostPerDayTitle.viewModel.$el.show();
                                self.CostPerDayValue.viewModel.$el.show();
                            } catch (e) { }                  
                        }
                        else {
                            self.CostPerDayTitle.viewModel.$el.hide();
                            self.CostPerDayValue.viewModel.$el.hide();
                        }
                
                    } else {
                        self.CostOverallTitle.viewModel.$el.hide();
                        self.CostOverallValue.viewModel.$el.hide();                

                        self.CostPerDayTitle.viewModel.$el.hide();
                        self.CostPerDayValue.viewModel.$el.hide();
                    }


                    self.renderStopControl();
                }
            });
        },

        campaignChanged: function () {
            this.updateRestrictions();
        },

        openEngagementDialogEvent: function (urlObj) {
            if (typeof urlObj == "undefined" || urlObj == null || urlObj == "") {
                return;
            }

            var url;
            if (urlObj != null) {
                if (urlObj.data != null && urlObj.data["url"] != null)
                    url = urlObj.data["url"];
                else {
                    url = urlObj;
                }
            }

            if (typeof url == "undefined") {
                return;
            }

            var editWindow = window.open(url, "", "height=" + $(document).height() + ",width=" + screen.width + ",top=1,left=1");
            editWindow.resizeTo(screen.availWidth, screen.availHeight);

            // updating after 
            var timer = setInterval(checkEdit, 500);
            function checkEdit() {
                if (editWindow.closed) {
                    clearInterval(timer);
                    if (url.indexOf("Designer/") != -1) {
                        DesignEngagementFrame.set("sourceUrl", url);
                        DesignEngagementFrame.set("sourceUrl", url);
                    }
                    else if (url.indexOf("Monitor/") != -1) {
                        MonitorEngagementFrame.set("sourceUrl", url);
                        MonitorEngagementFrame.set("sourceUrl", url);
                    }
                    else if (url.indexOf("Supervisor/") != -1) {
                        SuperviseEngagementFrame.set("sourceUrl", url);
                        SuperviseEngagementFrame.set("sourceUrl", url);
                    }
                }
            }
        },

        restoreTab: function () {
            var startIndex = document.cookie.indexOf("CLAActiveTab");

            if (startIndex > -1) {
                var id = document.cookie.substring(startIndex + 13, startIndex + 13 + 38);
                this.TabControl.set("selectedTab", id);
            }
        },

        setCurrentVersion: function () {
            this.resetLanguageContext();
            var vnumber = this.VersionsListControl.get("selectedItemId");
            if (vnumber != "") {
                this.campaign.set("currentVersion", vnumber);
                this.CampaignCurrentVersionInfo.set("text", "#" + vnumber);
                this.campaign.variants();
                this.campaign.emails();
                this.campaign.workflow();
            }
            else {
                var ic = 0;
            }
        },

        setCurrentLanguage: function () {
            var lang = this.ComboLanguage.get("selectedItem");
            if (typeof lang == "undefined" || lang == null) {
                var items = this.ComboLanguage.get("items");
                if (items != null && items.length > 0)
                    lang = items[0];
            }
            if (typeof lang != "undefined" && lang != null) {
                this.campaign.set("langName", lang.itemName);
            }
        },

        setCurrentLanguageWithUpdate: function () {
            this.resetLanguageContext();
            this.setCurrentLanguage();
            this.campaign.variants();
        },


        addLandingPage: function () {
            this.resetLanguageContext();
            _sc.trigger("addLandingPage");
        },

        attachEngagementPlan: function () {
            this.resetLanguageContext();
            _sc.trigger("attachEngagementPlan");
        },

        createEmail: function () {
            this.resetLanguageContext();
            _sc.trigger("createEmail");
        },
        
        assignGoals: function(variantid) {
            _sc.trigger("assignGoals", variantid);
        },

        removeCurrentVersion: function () {
            this.resetLanguageContext();
            this.campaign.removeCurrentVersion();
        },

        copyCurrentVersion: function () {
            this.resetLanguageContext();
            this.campaign.copyCurrentVersion();
        },

        createEngagement: function () {

        },

        updateVersions: function () {
            if (this.campaign.get("currentVersion") == undefined) {
                this.campaign.set("currentVersion", this.campaign.ver[0].itemId);
                this.campaign.variants();
                var vnumber = this.campaign.get("currentVersion");
                this.CampaignCurrentVersionInfo.set("text", "#" + vnumber);
            }
            this.VersionsListControl.set("items", this.campaign.ver);
            this.VersionsListControl.set("selectedItemId", "");
            this.campaign.emails();
            this.campaign.workflow();
        },

        updateVariants: function () {
            var variants = _.pluck(this.campaign._variants, "itemId");
            this.renderVariants.call(this, variants);
        },

        updateEmails: function () {
            var app = this;
            $("[data-sc-id=MessagesBorder]").empty();
            var emails = this.campaign.get("emails");

            _.each(emails, function (email) {
                // FIXME: insertRendering appends elements to DOM asunchronously
                // so I added a small delay to be sure what element is inserted
                // before insert another one. Acrually, we can't be sure anyway
                // but it solves a problem in WebKit browsers
                var start = new Date().getTime(), expire = start + 100;
                while (new Date().getTime() < expire) { }

                app.insertRendering(
                  "{C16EE6EE-056D-43E2-8BEB-0718CD6A5A20}&datasourceID=" + email.sourceId,
                  {
                      $el: $("[data-sc-id=MessagesBorder]")
                  },
                  function (subApp) {
                      app[email.emailId] = subApp;
                      app[email.emailId].set("emailId", email.emailId);
                      app[email.emailId].set("EditCampaign", app);
                      app[email.emailId].set("sourceId", email.sourceId);
                  });
            });


        },

        emailsLoaded: function () {
            var emails = this.campaign.emails;
            this.renderEmails(emails);
        },

        renderEmails: function (ids) {

        },

        versionUpdated: function () {
            this.resetLanguageContext();
            this.campaign.versions();
            this.campaign.variants();
            var vnumber = this.campaign.get("currentVersion");
            this.CampaignCurrentVersionInfo.set("text", "#" + vnumber);
        },

        variantUpdated: function () {
            this.resetLanguageContext();
            this.campaign.variants();
        },

        renderVariants: function (ids) {
            // clear view before render
            $("[data-sc-id=PageTabContent]").empty();

            var app = this;

            _.each(ids, function (variantId) {
                // FIXME: insertRendering appends elements to DOM asunchronously
                // so I added a small delay to be sure what element is inserted
                // before insert another one. Acrually, we can't be sure anyway
                // but it solves a problem in WebKit browsers
                var start = new Date().getTime(), expire = start + 100;
                var language = app.campaign.get("langName");
                var lang = "";

                if (language != undefined) {
                    lang = "&lang=" + language;
                }


                while (new Date().getTime() < expire) { }

                app.insertRendering(
                  "{FCDD082E-BC72-443B-92D2-4F8BD01D4728}&datasourceID=" + variantId + lang,
                  {
                      $el: $("[data-sc-id=PageTabContent]")
                  },
                  function (subApp) {
                      app[variantId] = subApp;
                      app[variantId].set("variantId", variantId);
                      app[variantId].set("status", app.campaign.get("status"));
                      app[variantId].set("EditCampaign", app);
                  });
            });

            // FIXME: this is a quick solution to check if variants are appended
            // in the right order and if not, sort them in DOM
            setTimeout(function () {
                var variants = $('div[data-sc-id=PageTabContent]').children();
                var sortedVariants = _.sortBy(
                    variants,
                    function (variant) {
                        return variant.id;
                    }
                );

                for (var i = 0; i < variants.length; i++) {
                    if (variants[i].id !== sortedVariants[i].id) {
                        variants.detach();
                        $('div[data-sc-id=PageTabContent]').append(sortedVariants);
                        break;
                    }
                }
            }, 2000);


            console.log(document.cookie);
        },
        updateInfo: function () {
            //update the info in the right panel
        },

        removeSocialMessage: function (app) {
            var self = this,
                item = this.Repeater.getItem(app);

            item.toModel().destroy({
                success: function () {
                    self.Repeater.remove(app);
                }
            });
        },

        saveCampaign: function () {
            this.resetLanguageContext();
            this.SaveValidationMessageBar.removeMessages();
            var name = this.GeneralNameTextBox.get("text"),
                alias = this.GeneralAliasTextBox.get("text"),
                description = this.GeneralDescriptionTextBox.get("text"),
                startdate = this.DatePicker.viewModel.$el.val(),
                enddate = this.EndDatePicker.viewModel.$el.val();

            // cost parsing
            var isCostParse = true;
            var cost = 0;
            var costText = this.CostInfoTextBox.get("text");
            if (costText != null && costText != "") {
                cost = parseInt(costText);
                if (isNaN(cost)) {
                    isCostParse = false;
                }
            }

            var startd = new Date(startdate);
            var endd = new Date(enddate);
            var now = new Date();
            var nowDate = new Date(now.getMonth() + 1 + "/" + now.getDate() + "/" + now.getFullYear());

            if (startd < nowDate) {
                this.SaveValidationMessageBar.addMessage('error', 'Start date should be future date');
            }
            else if (endd < startd) {
                this.SaveValidationMessageBar.addMessage('error', 'End date should be greated than start date');
            }
            else if (!isCostParse) {
                this.SaveValidationMessageBar.addMessage('error', "Type of 'Cost overall' field must be Number and value must be positive");
            } else {
                this.SaveValidationMessageBar.removeMessages();
                var self = this;
                this.campaign.set("StartDate", startdate);
                this.campaign.set("EndDate", enddate);
                this.campaign.set("Cost", cost);
                self.StartDateTValue.set("text", startdate);
                self.EndDateTValue.set("text", enddate);
                self.CostOverallValue.set("text", cost.toString());
                this.campaign.save();

                if (name) {
                    this.ProgressIndicator.set("isBusy", true);
                    self.item.set("Name", name);
                    self.item.set("Alias", alias);
                    self.item.set("Description", description);

                    self.CampaignInfoName.set("text", name);
                    self.item.save();

                    // RenameItem
                    $.get("/api/sitecore/cla/campaign/RenameCampaignCategory?ts=" + new Date().getTime()
                        + "&itemID=" + this.item.id + "&newName=" + name)
                      .done(function (data) {
                          self.ProgressIndicator.set("isBusy", false);
                      });
                }
            }
        },

        createaEngagementPlan: function () {
            /*
            this.Frame.set("sourceUrl", item.get("sourceUrl"));
            this.Frame.set("isVisible", true);
            */
        },

        addNewVariant: function () {
            //show dialog
        },
        createSocialMessage: function (socialNetworkType) {
            var self = this;

            _sc.Definitions.Data.createItem({
                templateId: "{7F5510AB-B599-49EF-A2C1-220A162D29C9}",
                name: socialNetworkType,
                parentId: "{4A966342-7357-4578-9D29-1E7FC74C3368}"
            }, {
                Type: socialNetworkType,
                Message: "...insert your message"
            }, function (item) {
                self.Repeater.add(item);
                if (socialNetworkType === "twitter") {
                    self.TwitterWindow.hide();
                }
                if (socialNetworkType === "facebook") {
                    self.FacebookWindow.hide();
                }
            });
        },

        openPageEditor: function (variantId) {
            var self = this;
            var lang = "sc_lang = en";
            var currentLanguage = this.campaign.get("langName");
            if (currentLanguage != undefined) {
                lang = "sc_lang = " + currentLanguage;
            }

            var url = "/?sc_itemid=" + variantId + "&sc_mode=edit&" + lang;
            /*open new popup windo*/
            // opening the urk in edit mode insdide DialogWindow breaks the page
            /*this.PageEditorFrame.set("sourceUrl", url);
            this.PageEditorDialogWindow.show();*/

            var pageeditorwindow = window.open(url, "variantPageEditor", "height=1200,width=1200");
            var timer = setInterval(checkChild, 500);

            function checkChild() {
                if (pageeditorwindow.closed) {
                    self.campaign.variants();
                    clearInterval(timer);
                }
            }
        },

        renderWorkFlow: function () {

            var status = this.campaign.get("status");
            var h = "";
            var history = this.campaign.get("historyItems");
            var commands = this.campaign.get("commands");
            for (i in history) {
                h = h + history[i] + "<br/>";
            }

            var d = $("<div></div>").html(h);

            this.WorkflowDescription.set("text", "Curent State: " + status);

            this.WorkflowHistoryEntry.viewModel.$el.empty();
            this.WorkflowHistoryEntry.viewModel.$el.append(d);

            var buttonBorder = $('[data-sc-id="ButtonsBorder"]');
            buttonBorder.empty();
            var self = this;


            for (i in commands) {
                var button = $("<button command-id='" + commands[i].commandId + "' style='margin-right:4px;' class='btn sc-button btn-inverse'><span class='sc-button-text'>" + commands[i].commandName + "</span></button>");
                button.click(function () {
                    var comment = self.WorkflowCommandComment.get("text");
                    self.WorkflowCommandComment.set("text", "");
                    self.campaign.workflowCommand($(this).attr("command-id"), comment);
                });
                buttonBorder.append(button);
            }

            if (commands.length == 0) {
                this.CommentBox.set("isVisible", false);
            } else {
                this.CommentBox.set("isVisible", true);
            }



            if (status == "Approved") {
                this.PublishNotReadyMessage.set("isVisible", false);
                this.PublishReadyBorder.set("isVisible", true);
                this.PublishedMessage.set("isVisible", false);
            } else if (status == "Published") {
                this.PublishNotReadyMessage.set("isVisible", false);
                this.PublishReadyBorder.set("isVisible", false);
                this.PublishedMessage.set("isVisible", true);
            }
            else {
                this.PublishNotReadyMessage.set("isVisible", true);
                this.PublishReadyBorder.set("isVisible", false);
                this.PublishedMessage.set("isVisible", false);
            }


            if (status === "Published" || status === "Obsolete") {
                this.CreateVariantButton.viewModel.$el.hide();
            }
            else {
                this.CreateVariantButton.viewModel.$el.show();
            }


            // checking whether one version is "published" - then editing of campaign is disabled
            var isDisabled = this.isCampaignPublishVersion();

            this.GeneralNameTextBox.viewModel.$el.attr("disabled", isDisabled);
            this.GeneralAliasTextBox.viewModel.$el.attr("disabled", isDisabled);


            var directLink = window.location.protocol + "//" + window.location.hostname + "/" + this.campaign.get("Alias") + ".aspx?sc_camp=" + this.campaign.get("campaignId");

            this.LandingPageDirectLink.set("text", directLink);

            this.updateCampaignInfo();
        },

        isCampaignPublishVersion: function () {
            var isPublished = false;
            if (this.campaign != null && this.campaign.ver != null) {
                for (var i = 0; i < this.campaign.ver.length; i++) {
                    var version = this.campaign.ver[i];
                    if (version.State === "Published") {
                        isPublished = true;
                        break;
                    }
                }
            }

            return isPublished;
        },

        updateRestrictions: function () {
            var isvalid = this.campaign.get("isValid");
            var isRetired = this.campaign.get("isRetired");
            this.OutOfDateMessage.removeMessages();
            
            if (!isvalid || isRetired) {
                this.PublishAccordionBorder.set("isVisible", false);
                if (!isvalid) {
                    this.OutOfDateMessage.addMessage('warning', 'Publish restricted. Campaign is out of date.');
                }
                
                if (isRetired) {
                    this.OutOfDateMessage.addMessage('warning', 'Publish restricted. Campaign must be activated.');
                }
            } else {
                this.OutOfDateMessage.removeMessages();
                this.PublishAccordionBorder.set("isVisible", true);
            }

            this.renderStopControl();

            this.updateCampaignInfo();
        },
        
        renderStopControl: function () {
            var isRetired = this.campaign.get("isRetired");
            
            if (!isRetired) {
                this.RetireAccordionBorder.set("isVisible", true);
                this.ActivateAccordionBorder.set("isVisible", false);
            } else {
                this.RetireAccordionBorder.set("isVisible", false);
                this.ActivateAccordionBorder.set("isVisible", true);
            }
        },
        
        retire: function() {
            this.campaign.retire();
        },
        
        activate: function() {
            this.campaign.activate();
        },

        publish: function () {
            this.resetLanguageContext();
            this.campaign.publish();
        },

        resetLanguageContext: function () {
            document.cookie = "website#lang=en;path=/";
        }


    });

    return EditCampaign;
});
