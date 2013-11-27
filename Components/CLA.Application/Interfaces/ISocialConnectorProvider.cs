namespace Sitecore.Cla.Application.Interfaces
{
  using System.Collections.Generic;

  public interface ISocialConnectorProvider
  {
    List<dynamic> GetSocialMessages(string name, string itemUri);

    void CreateSocialMessage(
      string campaignId,
      string networkName,
      string messageText,
      string linkTitle,
      string linkDescription,
      string publishWithItem,
      List<string> accountList
      );
    
    void EditSocialMessage(
      string campaignId,
      string messageId,
      string networkName,
      string messageText,
      string linkTitle,
      string linkDescription,
      string publishWithItem,
      List<string> accountsList);

    void PublishSocialMessage(string messageId);
  }
}
