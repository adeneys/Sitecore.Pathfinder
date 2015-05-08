﻿namespace Sitecore.Pathfinder.Parsing.Items.ElementParsers
{
  using System;
  using System.ComponentModel.Composition;
  using System.Linq;
  using System.Xml.Linq;
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.Extensions.StringExtensions;
  using Sitecore.Pathfinder.Extensions.XElementExtensions;
  using Sitecore.Pathfinder.IO;
  using Sitecore.Pathfinder.Projects.Items;
  using Sitecore.Pathfinder.Projects.Templates;

  [Export(typeof(IElementParser))]
  public class ItemParser : ElementParserBase
  {
    public override bool CanParse(ItemParseContext context, XElement element)
    {
      return element.Name.LocalName == "Item";
    }

    public override void Parse(ItemParseContext context, XElement element)
    {
      var item = new Item(context.ParseContext.SourceFile);
      context.ParseContext.Project.Items.Add(item);

      item.ItemName = element.GetAttributeValue("Name");
      if (string.IsNullOrEmpty(item.ItemName))
      {
        item.ItemName = context.ParseContext.ItemName;
      }

      item.DatabaseName = context.ParseContext.DatabaseName;
      item.ItemIdOrPath = context.ParentItemPath + "/" + item.ItemName;
      item.TemplateIdOrPath = this.GetTemplateIdOrPath(context, element);
      item.SourceElement = element;

      if (!string.IsNullOrEmpty(element.GetAttributeValue("Template.Create")))
      {
        var templateModel = this.ParseTemplate(context, element);
        item.TemplateIdOrPath = templateModel.ItemIdOrPath;
      }

      this.ParseChildElements(context, item, element);
    }

    [NotNull]
    protected virtual string GetTemplateIdOrPath([NotNull] ItemParseContext context, [NotNull] XElement element)
    {
      var templateIdOrPath = element.GetAttributeValue("Template") ?? string.Empty;
      if (string.IsNullOrEmpty(templateIdOrPath))
      {
        templateIdOrPath = element.GetAttributeValue("Template.Create") ?? string.Empty;
      }

      if (string.IsNullOrEmpty(templateIdOrPath))
      {
        return string.Empty;
      }

      // return if absolute path or guid
      templateIdOrPath = templateIdOrPath.Trim();
      if (templateIdOrPath.StartsWith("/") || templateIdOrPath.StartsWith("{"))
      {
        return templateIdOrPath;
      }

      // resolve relative paths
      templateIdOrPath = PathHelper.NormalizeItemPath(PathHelper.Combine(context.ParseContext.ItemPath, templateIdOrPath));

      return templateIdOrPath;
    }

    protected virtual void ParseChildElements([NotNull] ItemParseContext context, [NotNull] Item item, [NotNull] XElement element)
    {
      foreach (var child in element.Elements())
      {
        if (child.Name.LocalName == "Field")
        {
          this.ParseFieldElement(context, item, child);
        }
        else
        {
          var newContext = new ItemParseContext(context.ParseContext, context.Parser, context.ParentItemPath + "/" + child.Name.LocalName);
          context.Parser.ParseElement(newContext, child);
        }
      }
    }

    protected virtual void ParseFieldElement([NotNull] ItemParseContext context, [NotNull] Item item, [NotNull] XElement fieldElement)
    {
      var fieldName = fieldElement.GetAttributeValue("Name");
      if (string.IsNullOrEmpty(fieldName))
      {
        throw new BuildException(Texts.Text2011, context.ParseContext.SourceFile.SourceFileName, fieldElement);
      }

      var field = item.Fields.FirstOrDefault(f => string.Compare(f.Name, fieldName, StringComparison.OrdinalIgnoreCase) == 0);
      if (field != null)
      {
        throw new BuildException(Texts.Text2012, context.ParseContext.SourceFile.SourceFileName, fieldElement, fieldName);
      }

      var value = fieldElement.GetAttributeValue("Value");
      if (string.IsNullOrEmpty(value))
      {
        value = fieldElement.Value;
      }

      field = new Field(item.SourceFile);
      item.Fields.Add(field);

      field.Name = fieldName;
      field.Value = value;
      field.SourceElement = fieldElement;
    }

    [NotNull]
    protected virtual Template ParseTemplate([NotNull] ItemParseContext context, [NotNull] XElement element)
    {
      var templateBuilder = new Template(context.ParseContext.SourceFile);
      context.ParseContext.Project.Items.Add(templateBuilder);

      templateBuilder.ItemIdOrPath = this.GetTemplateIdOrPath(context, element);
      if (string.IsNullOrEmpty(templateBuilder.ItemIdOrPath))
      {
        throw new BuildException(Texts.Text2010, context.ParseContext.SourceFile.SourceFileName, element);
      }

      templateBuilder.DatabaseName = context.ParseContext.DatabaseName;
      templateBuilder.Icon = element.GetAttributeValue("Template.Icon");
      templateBuilder.BaseTemplates = element.GetAttributeValue("Template.BaseTemplates");
      if (string.IsNullOrEmpty(templateBuilder.BaseTemplates))
      {
        templateBuilder.BaseTemplates = "{1930BBEB-7805-471A-A3BE-4858AC7CF696}";
      }

      // get template name
      var n = templateBuilder.ItemIdOrPath.LastIndexOf('/');
      templateBuilder.ItemName = templateBuilder.ItemIdOrPath.Mid(n + 1);

      var sectionBuilder = new TemplateSection();
      templateBuilder.Sections.Add(sectionBuilder);
      sectionBuilder.Name = "Fields";

      foreach (var child in element.Elements())
      {
        if (child.Name.LocalName != "Field")
        {
          throw new BuildException(Texts.Text2015, context.ParseContext.SourceFile.SourceFileName, child);
        }

        var name = child.GetAttributeValue("Name");

        var fieldModel = new TemplateField();
        sectionBuilder.Fields.Add(fieldModel);
        fieldModel.Name = name;
        fieldModel.Type = child.GetAttributeValue("Field.Type");
        if (string.IsNullOrEmpty(fieldModel.Type))
        {
          fieldModel.Type = "Single-Line Text";
        }

        fieldModel.Shared = child.GetAttributeValue("Field.Sharing") == "Shared";
        fieldModel.Unversioned = child.GetAttributeValue("Field.Sharing") == "Unversioned";
        fieldModel.Source = child.GetAttributeValue("Field.Source") ?? string.Empty;
      }

      return templateBuilder;
    }
  }
}