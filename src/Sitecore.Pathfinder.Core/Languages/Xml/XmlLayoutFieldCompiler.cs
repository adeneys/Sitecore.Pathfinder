﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel.Composition;
using Sitecore.Pathfinder.Compiling.FieldCompilers;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.IO;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Snapshots;

namespace Sitecore.Pathfinder.Languages.Xml
{
    public class XmlLayoutFieldCompiler : FieldCompilerBase
    {
        [ImportingConstructor]
        public XmlLayoutFieldCompiler([NotNull] IFileSystemService fileSystem) : base(Constants.FieldCompilers.Normal)
        {
            FileSystem = fileSystem;
        }

        [NotNull]
        protected IFileSystemService FileSystem { get; }

        public override bool CanCompile(IFieldCompileContext context, Field field)
        {
            // avoid being called by Json
            var textNode = TraceHelper.GetTextNode(field.ValueProperty);
            if (!(textNode is XmlTextNode))
            {
                return false;
            }

            return string.Equals(field.TemplateField.Type, "layout", StringComparison.OrdinalIgnoreCase) || field.ValueHint.Contains("Layout") || field.FieldName == "__Renderings" || field.FieldName == "Final __Renderings";
        }

        public override string Compile(IFieldCompileContext context, Field field)
        {
            var textNode = TraceHelper.GetTextNode(field.ValueProperty);
            if (textNode == TextNode.Empty)
            {
                return field.Value;
            }

            var textSnapshot = textNode.Snapshot as ITextSnapshot;
            if (textSnapshot == null)
            {
                return field.Value;
            }

            var layoutResolveContext = new LayoutCompileContext(context, FileSystem, field, textSnapshot);

            var resolver = new LayoutCompiler();

            return resolver.Compile(layoutResolveContext, textNode);
        }
    }
}
