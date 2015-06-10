// � 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Documents;
using Sitecore.Pathfinder.Projects.Templates;

namespace Sitecore.Pathfinder.Projects.Items
{
    public enum MergingMatch
    {
        MatchUsingItemPath,

        MatchUsingSourceFile
    }

    public class Item : ItemBase
    {
        public static readonly Item Empty = new Item(Projects.Project.Empty, "{935B8D6C-D25A-48B8-8167-2C0443D77027}", TextNode.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public Item([NotNull] IProject project, [NotNull] string projectUniqueId, [NotNull] ITextNode textNode, [NotNull] string databaseName, [NotNull] string itemName, [NotNull] string itemIdOrPath, [NotNull] string templateIdOrPath) : base(project, projectUniqueId, textNode, databaseName, itemName, itemIdOrPath)
        {
            TemplateIdOrPath = new Attribute<string>("Template", templateIdOrPath);
            TemplateIdOrPath.SourceFlags = SourceFlags.IsQualified;
        }

        [NotNull]
        public IList<Field> Fields { get; } = new List<Field>();

        public MergingMatch MergingMatch { get; set; }

        public bool OverwriteWhenMerging { get; set; }

        [NotNull]
        public Template Template => Project.Items.OfType<Template>().FirstOrDefault(i => string.Compare(i.QualifiedName, TemplateIdOrPath.Value, StringComparison.OrdinalIgnoreCase) == 0) ?? Template.Empty;

        [NotNull]
        public Attribute<string> TemplateIdOrPath { get; }

        public void Merge([NotNull] Item newProjectItem)
        {
            Merge(newProjectItem, OverwriteWhenMerging);
        }

        protected override void Merge(IProjectItem newProjectItem, bool overwrite)
        {
            base.Merge(newProjectItem, overwrite);

            var newItem = newProjectItem as Item;
            if (newItem == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(newItem.TemplateIdOrPath.Value))
            {
                TemplateIdOrPath.SetValue(newItem.TemplateIdOrPath.Source);
            }

            OverwriteWhenMerging = OverwriteWhenMerging && newItem.OverwriteWhenMerging;
            MergingMatch = MergingMatch == MergingMatch.MatchUsingSourceFile && newItem.MergingMatch == MergingMatch.MatchUsingSourceFile ? MergingMatch.MatchUsingSourceFile : MergingMatch.MatchUsingItemPath;

            // todo: add SourceFile
            foreach (var newField in newItem.Fields)
            {
                var field = Fields.FirstOrDefault(f => string.Compare(f.FieldName.Value, newField.FieldName.Value, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(f.Language.Value, newField.Language.Value, StringComparison.OrdinalIgnoreCase) == 0 && f.Version.Value == newField.Version.Value);
                if (field == null)
                {
                    newField.Item = this;
                    Fields.Add(newField);
                    continue;
                }

                // todo: trace that field is being overwritten
                field.Value.SetValue(newField.Value.Source);
            }
        }
    }
}
