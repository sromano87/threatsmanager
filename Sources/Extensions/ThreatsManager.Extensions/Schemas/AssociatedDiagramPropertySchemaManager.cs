﻿using PostSharp.Patterns.Contracts;
using ThreatsManager.Extensions.Properties;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Properties;

namespace ThreatsManager.Extensions.Schemas
{
    public class AssociatedDiagramPropertySchemaManager
    {
        private const string SchemaName = "Associated Diagram for Entities";

        private readonly IThreatModel _model;

        public AssociatedDiagramPropertySchemaManager([NotNull] IThreatModel model)
        {
            _model = model;
        }

        public IPropertySchema GetSchema()
        {
            var result = _model.GetSchema(SchemaName, Properties.Resources.DefaultNamespace);
            if (result == null)
            {
                result = _model.AddSchema(SchemaName, Properties.Resources.DefaultNamespace);
                result.AppliesTo = Scope.Entity;
                result.AutoApply = true;
                result.Priority = 10;
                result.Visible = true;
                result.System = true;
                result.Description = Resources.AssociatedDiagramPropertySchemaDescription;
            }

            return result;
        }

        public IPropertyType GetAssociatedDiagramIdPropertyType()
        {
            IPropertyType result = null;

            var schema = GetSchema();
            if (schema != null)
            {
                result = schema.GetPropertyType("Associated Diagram");
                if (result == null)
                {
                    result =
                        schema.AddPropertyType("Associated Diagram", PropertyValueType.IdentityReference);
                    result.Visible = true;
                    result.Description = Resources.AssociatedDiagram;
                }
            }

            return result;
        }
    }
}