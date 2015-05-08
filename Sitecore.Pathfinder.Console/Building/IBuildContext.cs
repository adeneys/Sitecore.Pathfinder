﻿namespace Sitecore.Pathfinder.Building
{
  using System.Collections.Generic;
  using System.ComponentModel.Composition;
  using Microsoft.Framework.ConfigurationModel;
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.IO;
  using Sitecore.Pathfinder.Projects;

  public interface IBuildContext
  {
    [NotNull]
    ICompositionService CompositionService { get; }

    [NotNull]
    IConfiguration Configuration { get; }

    [NotNull]
    IFileSystemService FileSystem { get; }

    bool IsAborted { get; set; }

    bool IsDeployable { get; set; }

    [NotNull]
    string OutputDirectory { get; set; }

    [NotNull]
    IList<string> OutputFiles { get; }

    [NotNull]
    IProject Project { get; }

    [NotNull]
    string ProjectDirectory { get; }

    [NotNull]
    string SolutionDirectory { get; }

    [NotNull]
    IList<ProjectItem> ModifiedProjectItems { get; }

    [NotNull]
    IDictionary<string, string> SourceMap { get; }

    [NotNull]
    ITraceService Trace { get; }
  }
}