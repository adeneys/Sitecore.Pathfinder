﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel.Composition;
using System.Linq;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Projects.Files;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Text;

namespace Sitecore.Pathfinder.Compiling.Compilers
{
    [Export(typeof(ICompiler))]
    public class MediaFileCompiler : CompilerBase
    {
        public override bool CanCompile(ICompileContext context, IProjectItem projectItem)
        {
            return projectItem is MediaFile;
        }

        public override void Compile(ICompileContext context, IProjectItem projectItem)
        {
            var mediaFile = projectItem as MediaFile;
            if (mediaFile == null)
            {
                return;
            }

            var project = mediaFile.Project;
            var snapshot = mediaFile.Snapshots.First();

            var guid = StringHelper.GetGuid(project, mediaFile.ItemPath);
            var item = context.Factory.Item(project, guid, new SnapshotTextNode(snapshot), mediaFile.DatabaseName, mediaFile.ItemName, mediaFile.ItemPath, string.Empty);
            item.ItemNameProperty.AddSourceTextNode(new FileNameTextNode(mediaFile.ItemName, snapshot));
            item.TemplateIdOrPathProperty.SetValue("/sitecore/templates/System/Media/Unversioned/File");
            item.IsEmittable = false;
            item.OverwriteWhenMerging = true;
            item.MergingMatch = MergingMatch.MatchUsingSourceFile;

            var addedItem = project.AddOrMerge(item);
            mediaFile.MediaItemUri = addedItem.Uri;
        }
    }
}
