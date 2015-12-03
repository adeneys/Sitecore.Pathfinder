﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Rules.Contexts;

namespace Sitecore.Pathfinder.Rules.Conditions
{
    public class FieldValueCondition : StringConditionBase
    {
        public FieldValueCondition() : base("field-value")
        {
        }

        protected override string GetValue(IRuleContext ruleContext, IDictionary<string, object> parameters)
        {
            var item = ruleContext.Object as Item;
            if (item == null)
            {
                return string.Empty;
            }

            var fieldName = GetParameterValue(parameters, "name", ruleContext.Object);
            if (fieldName == null)
            {
                throw new ConfigurationException("Field name expected");
            }

            return item[fieldName];
        }
    }
}
