﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Entities;
using ThreatsManager.Interfaces.ObjectModel.Properties;

namespace ThreatsManager.Extensions.Reporting
{
    [Extension("763A3C06-79FF-4191-8228-4E021AD3A6FB", "Process List Placeholder", 40, ExecutionMode.Business)]
    public class ListProcessesPlaceholderFactory : IPlaceholderFactory
    {
        public string Qualifier => "ListProcesses";

        public IPlaceholder Create(string parameters = null)
        {
            return new ListProcessesPlaceholder();
        }
    }

    public class ListProcessesPlaceholder : IListPlaceholder
    {
        public string Name => "Processes";
        public PlaceholderSection Section => PlaceholderSection.List;
        public Bitmap Image => Icons.Resources.process_small;

        public bool Tabular => true;

        public IEnumerable<KeyValuePair<string, IPropertyType>> GetProperties([NotNull] IThreatModel model)
        {
            IEnumerable<KeyValuePair<string, IPropertyType>> result = null;

            var processes = model.Entities?.OfType<IProcess>().OrderBy(x => x.Name).ToArray();

            if (processes?.Any() ?? false)
            {
                var dict = new Dictionary<string, IPropertyType>();

                foreach (var process in processes)
                {
                    var properties = process.Properties?
                        .Where(x => x.PropertyType != null && x.PropertyType.Visible && !x.PropertyType.DoNotPrint &&
                                    (model.GetSchema(x.PropertyType.SchemaId)?.Visible ?? false))
                        .OrderBy(x => model.GetSchema(x.PropertyType.SchemaId).Priority)
                        .ThenBy(x => x.PropertyType.Priority)
                        .Select(x => x.PropertyType)
                        .ToArray();

                    if (properties?.Any() ?? false)
                    {
                        foreach (var property in properties)
                        {
                            if (!dict.ContainsKey(property.Name))
                                dict.Add(property.Name, property);
                        }
                    }
                }

                result = dict.ToArray();
            }

            return result;
        }

        public IEnumerable<ListItem> GetList(IThreatModel model)
        {
            IEnumerable<ListItem> result = null;

            var entities = model.Entities?.OfType<IProcess>().OrderBy(x => x.Name).ToArray();

            if (entities?.Any() ?? false)
            {
                var list = new List<ListItem>();

                foreach (var entity in entities)
                {
                    List<ItemRow> items = new List<ItemRow>();
                    items.Add(new TextRow("Description", entity.Description));
                    var properties = entity.Properties?
                        .Where(x => x.PropertyType != null && x.PropertyType.Visible && !x.PropertyType.DoNotPrint &&
                                    (model.GetSchema(x.PropertyType.SchemaId)?.Visible ?? false))
                        .OrderBy(x => model.GetSchema(x.PropertyType.SchemaId).Priority)
                        .ThenBy(x => x.PropertyType.Priority)
                        .Select(x => ItemRow.Create(entity, x))
                        .ToArray();
                    if (properties?.Any() ?? false)
                        items.AddRange(properties);

                    list.Add(new ListItem(entity.Name, entity.Id, items));
                }

                result = list;
            }

            return result;
        }
    }
}
