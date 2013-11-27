namespace SCExtensions.Commands
{
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Web.UI.Sheer;

  class OpenApplication : Command
  {
    public OpenApplication()
    {
      
    }

    public override void Execute(CommandContext context)
    {
      var claapplicationPath = "/sitecore/client/Sitecore/Applications/CLA/CampaignLaunch";
      SheerResponse.Eval(string.Format("window.open('{0}')", claapplicationPath));
    

    }
  }
}
