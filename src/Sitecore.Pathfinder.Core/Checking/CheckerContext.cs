﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel.Composition;
using Microsoft.Framework.ConfigurationModel;
using Sitecore.Pathfinder.Configuration;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Checking
{
    [Export(typeof(ICheckerContext))]
    public class CheckerContext : ICheckerContext
    {
        [ImportingConstructor]
        public CheckerContext([NotNull] IConfiguration configuration, [NotNull] ICompositionService compositionService, [NotNull] IConsoleService console, [NotNull] IFactoryService factory)
        {
            Configuration = configuration;
            CompositionService = compositionService;
            Console = console;
            Factory = factory;

            IsDeployable = true;
        }

        public ICompositionService CompositionService { get; }

        [NotNull]
        protected IConsoleService Console { get; }

        public bool IsDeployable { get; set; }

        public IProject Project { get; private set; }

        public ITraceService Trace { get; private set; }

        [NotNull]
        protected IConfiguration Configuration { get; }

        [NotNull]
        protected IFactoryService Factory { get; }

        public ICheckerContext With(IProject project)
        {
            Project = project;
            Trace = new ProjectDiagnosticTraceService(Configuration, Console, Factory).With(Project);

            return this;
        }
    }
}
