﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Properties;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;
using ThreatsManager.Utilities;

namespace ThreatsManager.Extensions.Reporting
{
    [Extension("61FB2189-8F8C-4FF9-AE91-658706D3D39F", "Threat Type List Placeholder", 44, ExecutionMode.Business)]
    public class ListThreatTypesPlaceholderFactory : IPlaceholderFactory
    {
        public string Qualifier => "ListThreatTypes";

        public IPlaceholder Create(string parameters = null)
        {
            return new ListThreatTypesPlaceholder();
        }
    }

    public class ListThreatTypesPlaceholder : IListPlaceholder
    {
        public string Name => "Threat Types";
        public PlaceholderSection Section => PlaceholderSection.List;
        public Bitmap Image => Icons.Resources.threat_types_small;

        public bool Tabular => true;

        public IEnumerable<KeyValuePair<string, IPropertyType>> GetProperties([NotNull] IThreatModel model)
        {
            IEnumerable<KeyValuePair<string, IPropertyType>> result = null;

            var threatTypes = model.ThreatTypes?
                .OrderByDescending(x => x.Severity, new SeverityComparer())
                .ToArray();

            if (threatTypes?.Any() ?? false)
            {
                var dict = new Dictionary<string, IPropertyType>();

                foreach (var threatType in threatTypes)
                {
                    var properties = threatType.Properties?
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

                    var eventProperties = new ListThreatEventsPlaceholder().GetProperties(model)?.ToArray();
                    if (eventProperties?.Any() ?? false)
                    {
                        foreach (var ep in eventProperties)
                        {
                            dict.Add($"[From Events] {ep.Key}", ep.Value);
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

            var threatTypes = model.ThreatTypes?
                .OrderByDescending(x => x.Severity, new SeverityComparer())
                .ThenBy(x => x.Name)
                .ToArray();

            if (threatTypes?.Any() ?? false)
            {
                var list = new List<ListItem>();

                foreach (var threatType in threatTypes)
                {
                    var threatEvents = model.GetThreatEvents(threatType)?.ToArray();
                    if (threatEvents?.Any() ?? false)
                    {
                        var items = new List<ItemRow>();

                        items.Add(new TextRow("Severity", threatType.Severity.Name,
                            threatType.Severity.TextColor, threatType.Severity.BackColor));
                        items.Add(new TextRow("Description", threatType.Description));
                        items.Add(new ListRow("Affected Objects", 
                            threatEvents.Select(x => 
                                new Cell($"[{model.GetIdentityTypeInitial(x.Parent)}] {x.Parent.Name} ({x.Severity.Name})", new []
                                    {x.ParentId}))));
                        items.Add(new TableRow("Approved Mitigations", new[]
                        {
                            new TableColumn("Object", 150),
                            new TableColumn("Mitigation", 200),
                            new TableColumn("Severity", 75),
                            new TableColumn("Strength", 75)
                        }, GetCells(GetMitigations(threatEvents, MitigationStatus.Approved))));
                        items.Add(new TableRow("Existing Mitigations", new[]
                        {
                            new TableColumn("Object", 150),
                            new TableColumn("Mitigation", 200),
                            new TableColumn("Severity", 75),
                            new TableColumn("Strength", 75)
                        }, GetCells(GetMitigations(threatEvents, MitigationStatus.Existing))));
                        items.Add(new TableRow("Implemented Mitigations", new[]
                        {
                            new TableColumn("Object", 150),
                            new TableColumn("Mitigation", 200),
                            new TableColumn("Severity", 75),
                            new TableColumn("Strength", 75)
                        }, GetCells(GetMitigations(threatEvents, MitigationStatus.Implemented))));
                        items.Add(new TableRow("Planned Mitigations", new[]
                        {
                            new TableColumn("Object", 150),
                            new TableColumn("Mitigation", 200),
                            new TableColumn("Severity", 75),
                            new TableColumn("Strength", 75)
                        }, GetCells(GetMitigations(threatEvents, MitigationStatus.Planned))));
                        items.Add(new TableRow("Proposed Mitigations", new[]
                        {
                            new TableColumn("Object", 150),
                            new TableColumn("Mitigation", 200),
                            new TableColumn("Severity", 75),
                            new TableColumn("Strength", 75)
                        }, GetCells(GetMitigations(threatEvents, MitigationStatus.Proposed))));

                        var properties = threatType.Properties?
                            .Where(x => x.PropertyType != null && x.PropertyType.Visible &&
                                        !x.PropertyType.DoNotPrint &&
                                        (model.GetSchema(x.PropertyType.SchemaId)?.Visible ?? false))
                            .OrderBy(x => model.GetSchema(x.PropertyType.SchemaId).Priority)
                            .ThenBy(x => x.PropertyType.Priority)
                            .Select(x => ItemRow.Create(threatType, x))
                            .ToArray();
                        if (properties?.Any() ?? false)
                            items.AddRange(properties);

                        var eventProperties = new ListThreatEventsPlaceholder().GetProperties(model)?.ToArray();
                        if (eventProperties?.Any() ?? false)
                        {
                            foreach (var ep in eventProperties)
                            {
                                if (threatEvents.Any(x => x.HasProperty(ep.Value)))
                                    items.Add(new TableRow($"[From Events] {ep.Key}", new []
                                    {
                                        new TableColumn("Object", 150),
                                        new TableColumn("Value", 350)
                                    }, GetCells(threatEvents.Where(x => x.HasProperty(ep.Value)), ep.Value)));
                            }
                        }

                        list.Add(new ListItem(threatType.Name, threatType.Id, items));
                    }
                }

                result = list;
            }

            return result;
        }

        private IEnumerable<IThreatEventMitigation> GetMitigations(IEnumerable<IThreatEvent> threatEvents, MitigationStatus status)
        {
            IEnumerable<IThreatEventMitigation> result = null;

            var list = threatEvents?.ToArray();
            if (list?.Any() ?? false)
            {
                var mitigations = new List<IThreatEventMitigation>();

                foreach (var item in list)
                {
                    var ms = item.Mitigations?
                        .Where(x => x.Status == status)
                        .ToArray();
                    if (ms?.Any() ?? false)
                        mitigations.AddRange(ms);
                }

                result = mitigations
                    .OrderBy(x => x.ThreatEvent.Parent.Name)
                    .ThenBy(x => x.Mitigation.Name);
            }

            return result;
        }
        
        private IEnumerable<Cell> GetCells(IEnumerable<IThreatEventMitigation> mitigations)
        {
            IEnumerable<Cell> result = null;

            var list = mitigations?.ToArray();
            if (list?.Any() ?? false)
            {
                var cells = new List<Cell>();

                foreach (var item in list)
                {
                    cells.Add(new Cell($"[{item.Model.GetIdentityTypeInitial(item.ThreatEvent.Parent)}] {item.ThreatEvent.Parent.Name}", new []
                        {item.ThreatEvent.ParentId}));
                    cells.Add(new Cell(item.Mitigation.Name, new [] {item.MitigationId}));
                    cells.Add(new Cell(item.ThreatEvent.Severity.Name));
                    cells.Add(new Cell(item.Strength.Name));
                }

                result = cells;
            }

            return result;
        }

        private IEnumerable<Cell> GetCells(IEnumerable<IThreatEvent> threatEvents, [NotNull] IPropertyType propertyType)
        {
            IEnumerable<Cell> result = null;

            var list = threatEvents?.ToArray();
            if (list?.Any() ?? false)
            {
                var cells = new List<Cell>();

                foreach (var item in list)
                {
                    cells.Add(new Cell($"[{item.Model.GetIdentityTypeInitial(item.Parent)}] {item.Parent.Name}", new []
                        {item.ParentId}));
                    cells.Add(new Cell(item.GetProperty(propertyType)?.StringValue));
                }

                result = cells;
            }

            return result;
        }
    }
}
