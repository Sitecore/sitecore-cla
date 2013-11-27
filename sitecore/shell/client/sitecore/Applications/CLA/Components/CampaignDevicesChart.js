require.config({
  paths: {
    fusionCharts: "/sitecore/shell/client/sitecore/Applications/CLA/FusionCharts/FusionCharts"
  }
});

define(["sitecore", "fusionCharts"], function (_sc) {
  _sc.Factories.createBaseComponent({
    name: "CampaignDevicersChart",
    selector: ".sc-campaignDevicesChart",

    attributes: [
      { name: "itemId", defaultValue:3 , value: "$el.data:sc-itemId" }
    ],
    initialize: function () {
      this.model.set("chartData", "");
      this.model.on("change:chartData", this.setChartData, this);
      this.chartControl = new FusionCharts("/sitecore/shell/client/sitecore/Applications/CLA/FusionCharts/Pie2D.swf", "_" + Math.floor(Math.random() * 1000000), "100%", "400", "0");
      FusionCharts.setCurrentRenderer('JavaScript');
    },

    afterRender: function () {
      var options = {
        itemId: this.model.get("itemId")
      };
    },

    setChartData: function () {
      this.chartControl.setXMLData(this.model.get("chartData"));
      this.chartControl.render("sc-chartContainer");
    }
  });
});
