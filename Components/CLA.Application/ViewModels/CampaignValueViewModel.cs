using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Cla.Application.ViewModels
{
  /// <summary>
  /// Object which represent a Landing Page which is associated value
  /// </summary>
  public class CampaignValueViewModel
  {
    public CampaignViewModel Campaign { get; set; }
    public int Value { get; set; }
  }
}
