﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using NUnit.Framework;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.Parsing;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Snapshots.Xml;

namespace Sitecore.Pathfinder.Projects
{
    [TestFixture]
    public partial class ProjectTests : Tests
    {
        [NotNull]
        public IProject Project { get; set; }

        [TestFixtureSetUp]
        public void Startup()
        {
            Start();
            Project = Services.ProjectService.LoadProjectFromConfiguration();
        }

        [Test]
        public void AddRemoveTests()
        {
            var project = Resolve<IProject>().Load(new ProjectOptions(ProjectDirectory, "master"), Enumerable.Empty<string>());

            var fileName = Path.Combine(ProjectDirectory, "content\\Home\\HelloWorld.item.xml");

            project.Add(fileName);
            Assert.AreEqual(1, project.SourceFiles.Count);
            Assert.AreEqual(fileName, project.SourceFiles.First().FileName);

            project.Remove(fileName);
            Assert.AreEqual(0, project.SourceFiles.Count);
        }

        [Test]
        public void ExternalReferencesTests()
        {
            Assert.AreEqual(10, Project.Options.ExternalReferences.Count);
            Assert.AreEqual("/sitecore/layout/Devices/Default", Project.Options.ExternalReferences.ElementAt(0).Item2);
        }

        [Test]
        public void FindUsagesTest()
        {
            var references = Services.QueryService.FindUsages(Project, "/sitecore/media library/mushrooms");
            Assert.AreEqual(2, references.Count());
        }

        [Test]
        public void JsonContentItemTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/JsonContentItem");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("JsonContentItem", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/JsonContentItem", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("JsonContentItem", item.ItemName);
            Assert.AreEqual("/sitecore/content/Home/JsonContentItem", item.ItemIdOrPath);
            Assert.AreEqual("Sample Item", item.TemplateIdOrPath);
            Assert.IsNotNull(item.ItemNameProperty.SourceTextNodes);
            Assert.IsInstanceOf<FileNameTextNode>(item.ItemNameProperty.SourceTextNode);
            Assert.IsNotNull(item.TemplateIdOrPathProperty.SourceTextNodes);
            Assert.IsInstanceOf<AttributeNameTextNode>(item.TemplateIdOrPathProperty.SourceTextNode);
            Assert.AreEqual("Sample Item", TraceHelper.GetTextNode(item.TemplateIdOrPathProperty).Value);

            var field = item.Fields.FirstOrDefault(f => f.FieldName == "Text");
            Assert.IsNotNull(field);
            Assert.AreEqual("Hello World", field.Value);

            var textDocument = projectItem.Snapshots.First() as ITextSnapshot;
            Assert.IsNotNull(textDocument);
        }

        [Test]
        public void JsonItemTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/JsonItem");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("JsonItem", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/JsonItem", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("JsonItem", item.ItemName);
            Assert.AreEqual("/sitecore/content/Home/JsonItem", item.ItemIdOrPath);
            Assert.AreEqual("/sitecore/templates/Sample/HelloWorld", item.TemplateIdOrPath);
            Assert.AreEqual(1, item.ItemNameProperty.SourceTextNodes.Count());
            Assert.IsInstanceOf<FileNameTextNode>(item.ItemNameProperty.SourceTextNode);

            var textDocument = projectItem.Snapshots.First() as ITextSnapshot;
            Assert.IsNotNull(textDocument);

            var treeNode = textDocument.Root;
            Assert.AreEqual("Item", treeNode.Name);
            Assert.AreEqual(2, treeNode.Attributes.Count());

            var attr = treeNode.Attributes.First();
            Assert.AreEqual("Template", attr.Name);
            Assert.AreEqual("/sitecore/templates/Sample/HelloWorld", attr.Value);
            attr = treeNode.Attributes.ElementAt(1);
            Assert.AreEqual("Template.CreateFromFields", attr.Name);

            var field = item.Fields.FirstOrDefault(f => f.FieldName == "Title");
            Assert.IsNotNull(field);
            Assert.AreEqual("Hello", field.Value);

            var layout = item.Fields.FirstOrDefault(f => f.FieldName == "__Renderings");
            Assert.IsNotNull(layout);
            layout.Resolve();
            Assert.AreEqual(@"<r>
  <d id=""{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}"" l=""{5E9D5374-E00A-4053-9127-EBC96A02C721}"">
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par=""Text=123"" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
  </d>
</r>", layout.ResolvedValue);
        }

        [Test]
        public void LoadProjectTests()
        {
            Assert.IsTrue(Project.Items.Any());
            Assert.IsTrue(Project.SourceFiles.Any());
        }

        [Test]
        public void MergeByProjectUniqueIdTest()
        {
            var project = Resolve<IProject>();
            var context = Services.CompositionService.Resolve<IParseContext>().With(project, Snapshot.Empty);

            var projectItem1 = new Item(project, "SameId", TextNode.Empty, string.Empty, "SameId", string.Empty, string.Empty);
            var projectItem2 = new Item(project, "SameId", TextNode.Empty, string.Empty, "SameId", string.Empty, string.Empty);

            project.AddOrMerge(context, projectItem1);
            project.AddOrMerge(context, projectItem2);

            Assert.AreEqual(1, project.Items.Count());
        }

        [Test]
        public void MergeTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/media library/Mushrooms");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("Mushrooms", projectItem.ShortName);
            Assert.AreEqual("/sitecore/media library/Mushrooms", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("Mushrooms", item.ItemName);
            Assert.AreEqual("/sitecore/media library/Mushrooms", item.ItemIdOrPath);

            var field = item.Fields.FirstOrDefault(f => f.FieldName == "Description");
            Assert.IsNotNull(field);
            Assert.AreEqual("Mushrooms", field.Value);
        }

        [Test]
        public void SerializationItemTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/SerializedItem");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("SerializedItem", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/SerializedItem", projectItem.QualifiedName);
            Assert.AreEqual("{CEABE4B1-E915-4904-B396-BBC0C081F111}", projectItem.Guid.Format());
            Assert.AreEqual("{CEABE4B1-E915-4904-B396-BBC0C081F111}", projectItem.ProjectUniqueId);
            Assert.AreEqual(1, projectItem.Snapshots.Count);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("SerializedItem", item.ItemName);
            Assert.AreEqual("/sitecore/content/Home/SerializedItem", item.ItemIdOrPath);
            Assert.AreEqual("{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", item.TemplateIdOrPath);

            var field = item.Fields.FirstOrDefault();
            Assert.IsNotNull(field);
            Assert.AreEqual("__Workflow", field.FieldName);
            Assert.AreEqual("{A5BC37E7-ED96-4C1E-8590-A26E64DB55EA}", field.Value);

            field = item.Fields.ElementAt(1);
            Assert.IsNotNull(field);
            Assert.AreEqual("Title", field.FieldName);
            Assert.AreEqual("Pip 1", field.Value);
            field.Resolve();
            Assert.AreEqual("Pip 1", field.ResolvedValue);
            Assert.AreEqual("en", field.Language);
            Assert.AreEqual(1, field.Version);
            Assert.AreEqual(0, field.Diagnostics.Count);
        }

        [Test]
        public void XmlContentItemTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/XmlContentItem");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("XmlContentItem", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/XmlContentItem", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("XmlContentItem", item.ItemName);
            Assert.AreEqual("/sitecore/content/XmlContentItem", item.ItemIdOrPath);
            Assert.AreEqual("Sample-Item", item.TemplateIdOrPath);
            Assert.IsNotNull(item.ItemNameProperty.SourceTextNodes);
            Assert.IsInstanceOf<FileNameTextNode>(item.ItemNameProperty.SourceTextNode);
            Assert.IsInstanceOf<AttributeNameTextNode>(item.TemplateIdOrPathProperty.SourceTextNode);
            Assert.AreEqual("Sample-Item", TraceHelper.GetTextNode(item.TemplateIdOrPathProperty).Value);

            var field = item.Fields.FirstOrDefault(f => f.FieldName == "Text");
            Assert.IsNotNull(field);
            Assert.AreEqual("Hello World", field.Value);
            Assert.IsInstanceOf<XmlTextNode>(field.ValueProperty.SourceTextNode);
            Assert.AreEqual("Hello World", field.ValueProperty.SourceTextNode?.Value);
            Assert.AreEqual("Text", field.ValueProperty.SourceTextNode?.Name);

            var textDocument = projectItem.Snapshots.First() as ITextSnapshot;
            Assert.IsNotNull(textDocument);

            var treeNode = textDocument.Root;
            Assert.AreEqual("Sample-Item", treeNode.Name);
            Assert.AreEqual(2, treeNode.Attributes.Count());
        }

        [Test]
        public void XmlItemTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/HelloWorld");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("HelloWorld", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/HelloWorld", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("HelloWorld", item.ItemName);
            Assert.AreEqual("/sitecore/content/Home/HelloWorld", item.ItemIdOrPath);
            Assert.AreEqual("/sitecore/templates/Sample/HelloWorld", item.TemplateIdOrPath);
            Assert.IsNotNull(item.ItemNameProperty.SourceTextNodes);
            Assert.IsInstanceOf<FileNameTextNode>(item.ItemNameProperty.SourceTextNode);
            Assert.IsInstanceOf<XmlTextNode>(item.TemplateIdOrPathProperty.SourceTextNode);
            Assert.AreEqual("/sitecore/templates/Sample/HelloWorld", item.TemplateIdOrPathProperty.SourceTextNode?.Value);
            Assert.AreEqual("Template", item.TemplateIdOrPathProperty.SourceTextNode?.Name);

            var field = item.Fields.FirstOrDefault(f => f.FieldName == "Title");
            Assert.IsNotNull(field);
            Assert.AreEqual("Hello", field.Value);

            var textDocument = projectItem.Snapshots.First() as ITextSnapshot;
            Assert.IsNotNull(textDocument);

            var treeNode = textDocument.Root;
            Assert.AreEqual("Item", treeNode.Name);
            Assert.AreEqual(2, treeNode.Attributes.Count());

            var attr = treeNode.Attributes.First();
            Assert.AreEqual("Template", attr.Name);
            Assert.AreEqual("/sitecore/templates/Sample/HelloWorld", attr.Value);

            var layout = item.Fields.FirstOrDefault(f => f.FieldName == "__Renderings");
            Assert.IsNotNull(layout);
            layout.Resolve();
            Assert.AreEqual(@"<r>
  <d id=""{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}"" l=""{5E9D5374-E00A-4053-9127-EBC96A02C721}"">
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par=""Text=123"" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
  </d>
</r>", layout.ResolvedValue);
        }

        [Test]
        public void XmlLayoutTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/XmlLayout");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("XmlLayout", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/XmlLayout", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);

            var layout = item.Fields.FirstOrDefault(f => f.FieldName == "__Renderings");
            Assert.IsNotNull(layout);
            layout.Resolve();
            Assert.AreEqual(@"<r>
  <d id=""{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}"" l=""{5E9D5374-E00A-4053-9127-EBC96A02C721}"">
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par=""Text=123"" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
  </d>
</r>", layout.ResolvedValue);
        }

        [Test]
        public void JsonLayoutTest()
        {
            var projectItem = Project.Items.FirstOrDefault(i => i.QualifiedName == "/sitecore/content/Home/JsonLayout");
            Assert.IsNotNull(projectItem);
            Assert.AreEqual("JsonLayout", projectItem.ShortName);
            Assert.AreEqual("/sitecore/content/Home/JsonLayout", projectItem.QualifiedName);

            var item = projectItem as Item;
            Assert.IsNotNull(item);

            var layout = item.Fields.FirstOrDefault(f => f.FieldName == "__Renderings");
            Assert.IsNotNull(layout);
            layout.Resolve();
            Assert.AreEqual(@"<r>
  <d id=""{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}"" l=""{5E9D5374-E00A-4053-9127-EBC96A02C721}"">
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par=""Text=123"" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
    <r id=""{4E924ED2-1534-483A-49FD-85A6D99331EE}"" par="""" />
  </d>
</r>", layout.ResolvedValue);
        }
    }
}
