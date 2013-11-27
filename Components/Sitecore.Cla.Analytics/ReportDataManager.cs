namespace Sitecore.Cla.Analytics
{
  using System;
using System.Collections.Generic;
  using System.Data.SqlClient;
  using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using Sitecore.Analytics.Data.DataAccess.DataAdapters;
  using Sitecore.Configuration;
  using Sitecore.Data;

  public static class ReportDataManager
  {
    public static IEnumerable<string[]> GetTopDeviceData(ID campaignId)
    {
      var result = new List<string[]>();
      if (!Settings.Analytics.Enabled)
      {
        return result;
      }

      var query = string.Format(@" SELECT TOP 10 OS.MajorName, SUM(Visits.Value) FROM Visits JOIN OS ON Visits.OsId = OS.OsId JOIN Campaigns ON visits.campaignid= campaigns.campaignid WHERE Category2Id = '{0}' GROUP BY OS.MajorName", campaignId.ToString().TrimStart('{').TrimEnd('}'));

      result = DataAdapterManager.Sql.ReadMany(
        query,
        reader =>
        {
          var array = new object[2]; reader.InnerReader.GetValues(array);
          return new[]
          {
            array[0].ToString(),
            array[1].ToString()
          };
        },
          new object[] { });

      return result;
    }

    public static int GetCategoryTotalValue(string categoryId)
    {
      var result = 0;
      if (!Settings.Analytics.Enabled)
      {
        return result;
      }
      try
      {
        var query = string.Format(@" SELECT SUM(Visits.Value) FROM Visits JOIN Campaigns ON Visits.CampaignId = Campaigns.CampaignId WHERE Category2id = '{0}'", categoryId.ToString().TrimStart('{').TrimEnd('}'));
        var connection = new SqlConnection(Settings.GetConnectionString("analytics"));
        connection.Open();
        var sqlcommand = new SqlCommand(query, connection);
        result = (int)sqlcommand.ExecuteScalar();
        connection.Close();
      }
      catch (Exception)
      {
      }
      
      return result;
    }
  }
}
