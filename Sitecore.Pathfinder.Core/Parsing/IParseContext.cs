﻿namespace Sitecore.Pathfinder.Parsing
{
  using Microsoft.Framework.ConfigurationModel;
  using Sitecore.Pathfinder.Configuration;
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.Documents;
  using Sitecore.Pathfinder.Projects;

  public interface IParseContext
  {
    [NotNull]
    IConfiguration Configuration { get; }

    [NotNull]
    string DatabaseName { get; }

    [NotNull]
    IFactoryService Factory { get; }

    [NotNull]
    string FilePath { get; }

    [NotNull]
    string ItemName { get; }

    [NotNull]
    string ItemPath { get; }

    [NotNull]
    IProject Project { get; }

    [NotNull]
    ISnapshot Snapshot { get; }

    [NotNull]
    ITraceService Trace { get; }

    [NotNull]
    IParseContext With([NotNull] IProject project, [NotNull] ISnapshot snapshot);
  }
}
