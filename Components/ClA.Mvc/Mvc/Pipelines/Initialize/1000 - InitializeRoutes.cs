using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Mvc.Pipelines.Initialize
{
  using System.Web.Mvc;
  using System.Web.Routing;
  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;

  /// <summary>
  /// <see cref="InitializeCommandRoute"/> class.
  /// </summary>
  public class InitializeCommandRoute
  {
    #region Public Methods and Operators

    /// <summary>
    /// Runs the processor.
    /// </summary>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public virtual void Process([NotNull] PipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      this.RegisterRoutes(RouteTable.Routes, args);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Registers the routes.
    /// </summary>
    /// <param name="routes">
    /// The routes.
    /// </param>
    /// <param name="args">
    /// The args.
    /// </param>
    protected virtual void RegisterRoutes([NotNull] RouteCollection routes, [NotNull] PipelineArgs args)
    {
      Debug.ArgumentNotNull(routes, "routes");
      Debug.ArgumentNotNull(args, "args");

      var prefix = ClaSettings.Mvc.CommandRoutePrefix;

      routes.MapRoute("Sitecore.Cla", prefix + "{controller}/{action}");
    }

    #endregion
  }
}
