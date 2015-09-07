// � 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects.Templates;
using Sitecore.Pathfinder.Snapshots;

namespace Sitecore.Pathfinder.Projects.Items
{
    // todo: consider basing this on ProjectElement
    [DebuggerDisplay("{GetType().Name,nq}: {FieldName,nq} = {Value}")]
    public class Field : IHasSourceTextNodes
    {
        private bool _isValid;

        public Field([NotNull] Item item, [NotNull] ITextNode textNode)
        {
            Item = item;

            SourceTextNodes.Add(textNode);

            ValueProperty.PropertyChanged += HandlePropertyChanged;
        }

        [NotNull]
        public ICollection<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        [NotNull]
        public string FieldName
        {
            get { return FieldNameProperty.GetValue(); }
            set { FieldNameProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> FieldNameProperty { get; } = new SourceProperty<string>("Name", string.Empty);

        public bool IsResolved { get; set; }

        public bool IsTestable { get; set; } = true;

        public bool IsValid
        {
            get
            {
                if (!IsResolved)
                {
                    Resolve();
                }

                return _isValid;
            }

            protected set { _isValid = value; }
        }

        [NotNull]
        public Item Item { get; set; }

        [NotNull]
        public string Language
        {
            get { return LanguageProperty.GetValue(); }
            set { LanguageProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> LanguageProperty { get; } = new SourceProperty<string>("Language", string.Empty);

        [NotNull]
        public string ResolvedValue { get; private set; }

        public ICollection<ITextNode> SourceTextNodes { get; } = new List<ITextNode>();

        [NotNull]
        public TemplateField TemplateField => Item.Template.Sections.SelectMany(s => s.Fields).FirstOrDefault(f => string.Compare(f.FieldName, FieldName, StringComparison.OrdinalIgnoreCase) == 0) ?? TemplateField.Empty;

        [NotNull]
        public string Value
        {
            get { return ValueProperty.GetValue(); }
            set { ValueProperty.SetValue(value); }
        }

        [NotNull]
        public string ValueHint
        {
            get { return ValueHintProperty.GetValue(); }
            set { ValueHintProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> ValueHintProperty { get; } = new SourceProperty<string>("Value.Hint", string.Empty);

        [NotNull]
        public SourceProperty<string> ValueProperty { get; } = new SourceProperty<string>("Value", string.Empty);

        public int Version
        {
            get { return VersionProperty.GetValue(); }
            set { VersionProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<int> VersionProperty { get; } = new SourceProperty<int>("Version", 0);

        public void Invalidate()
        {
            IsResolved = false;
            IsValid = false;
            Diagnostics.Clear();
        }

        public void Resolve()
        {
            if (IsResolved)
            {
                return;
            }

            IsResolved = true;
            ResolvedValue = Value;
            Diagnostics.Clear();

            foreach (var fieldResolver in Item.Project.FieldResolvers.OrderBy(r => r.Priority))
            {
                if (fieldResolver.CanResolve(this))
                {
                    ResolvedValue = fieldResolver.Resolve(this);
                    break;
                }
            }

            IsValid = Diagnostics.All(d => d.Severity != Severity.Error);
        }

        public virtual void WriteDiagnostic(Severity severity, [NotNull] string text, [NotNull] string details = "")
        {
            var source = TraceHelper.GetTextNode(FieldNameProperty);
            WriteDiagnostic(severity, text, source, details.Trim());
        }

        public void WriteDiagnostic(Severity severity, [NotNull] string text, [NotNull] ITextNode textNode, [NotNull] string details = "")
        {
            details = details.Trim();

            if (!string.IsNullOrEmpty(details))
            {
                text += ": " + details;
            }

            Diagnostics.Add(new Diagnostic(string.Empty, textNode.Position, severity, text));
        }

        private void HandlePropertyChanged([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                Invalidate();
            }
        }
    }
}
