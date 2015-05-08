﻿namespace Sitecore.Pathfinder.Emitters
{
  using Sitecore.Pathfinder.Projects;

  public interface IEmitter
  {
    double Sortorder { get; }

    bool CanEmit([NotNull] IEmitContext context, [NotNull] ProjectItem projectItem);

    void Emit([NotNull] IEmitContext context, [NotNull] ProjectItem projectItem);
  }
}