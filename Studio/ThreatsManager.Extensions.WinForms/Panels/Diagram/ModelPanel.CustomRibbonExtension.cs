﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Northwoods.Go;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Extensions.Dialogs;
using ThreatsManager.Extensions.Panels.DiagramConfiguration;
using ThreatsManager.Icons;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.Extensions;
using ThreatsManager.Interfaces.Extensions.Actions;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Diagrams;
using ThreatsManager.Interfaces.ObjectModel.Entities;
using ThreatsManager.Interfaces.ObjectModel.Properties;
using ThreatsManager.Utilities;
using ThreatsManager.Utilities.Aspects;
using ThreatsManager.Utilities.WinForms.Dialogs;
using Shortcut = ThreatsManager.Interfaces.Extensions.Shortcut;

namespace ThreatsManager.Extensions.Panels.Diagram
{
    public partial class ModelPanel
    {
        private Dictionary<string, List<ICommandsBarDefinition>> _commandsBarContextAwareActions;

        public event Action<string, bool> ChangeCustomActionStatus;

        public string TabLabel => "Diagram";

        public IEnumerable<ICommandsBarDefinition> CommandBars
        {
            get
            {
                var result = new List<ICommandsBarDefinition>();

                if (_executionMode != ExecutionMode.Business && _executionMode != ExecutionMode.Management)
                {
                    #region Add/Remove.
                    var addRemoveList = new List<IActionDefinition>()
                    {
                        new ActionDefinition(Id, "CreateExtInteractor", "New External Interactor",
                            Resources.external_big_new,
                            Resources.external_new,
                            true, Shortcut.CtrlShiftE),
                        new ActionDefinition(Id, "CreateProcess", "New Process",
                            Resources.process_big_new,
                            Resources.process_new,
                            true, Shortcut.CtrlShiftP),
                        new ActionDefinition(Id, "CreateDataStore", "New Data Store",
                            Resources.storage_big_new,
                            Resources.storage_new,
                            true, Shortcut.CtrlShiftS),
                        new ActionDefinition(Id, "CreateTrustBoundary", "New Trust Boundary",
                            Resources.trust_boundary_big_new,
                            Resources.trust_boundary_new,
                            true, Shortcut.CtrlShiftB),
                        new ActionDefinition(Id, "CreateThreatType", "New Threat Type",
                            Resources.threat_type_big_new,
                            Resources.threat_type_new,
                            true, Shortcut.CtrlShiftT)
                    };
                    if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Add", x.Key) == 0) ?? false)
                    {
                        var definitions = _commandsBarContextAwareActions["Add"];
                        List<IActionDefinition> actions = new List<IActionDefinition>();
                        foreach (var definition in definitions)
                        {
                            foreach (var command in definition.Commands)
                            {
                                actions.Add(command);
                            }
                        }

                        addRemoveList.AddRange(actions);
                        _commandsBarContextAwareActions.Remove("Add");
                    }

                    addRemoveList.Add(new ActionDefinition(Id, "RemoveDiagram", "Remove Current Diagram",
                            Resources.model_big_delete,
                            Resources.model_delete));
                    if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Remove", x.Key) == 0) ?? false)
                    {
                        var definitions = _commandsBarContextAwareActions["Remove"];
                        List<IActionDefinition> actions = new List<IActionDefinition>();
                        foreach (var definition in definitions)
                        {
                            foreach (var command in definition.Commands)
                            {
                                actions.Add(command);
                            }
                        }

                        addRemoveList.AddRange(actions);
                        _commandsBarContextAwareActions.Remove("Remove");
                    }
                    result.Add(new CommandsBarDefinition("AddRemove", "Add/Remove", addRemoveList));
                    #endregion
                }

                #region View.
                var viewList = new List<IActionDefinition>()
                {
                    new ActionDefinition(Id, "MarkerToggle", "Toggle Markers",
                        Properties.Resources.cubes_big, Properties.Resources.cubes, true),
                    new ActionDefinition(Id, "AllPanelsToggle", "Toggle all Panels",
                        Properties.Resources.panel_big, Properties.Resources.panel, true)
                };
                if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("View", x.Key) == 0) ?? false)
                {
                    var definitions = _commandsBarContextAwareActions["View"];
                    List<IActionDefinition> actions = new List<IActionDefinition>();
                    foreach (var definition in definitions)
                    {
                        foreach (var command in definition.Commands)
                        {
                            actions.Add(command);
                        }
                    }

                    viewList.AddRange(actions);
                    _commandsBarContextAwareActions.Remove("View");
                }
                result.Add(new CommandsBarDefinition("View", "View", viewList, false));
                #endregion

                #region Zoom.
                var zoomList = new List<IActionDefinition>()
                {
                    new ActionDefinition(Id, "ZoomIn", "Zoom In", Properties.Resources.zoom_in_big,
                        Properties.Resources.zoom_in),
                    new ActionDefinition(Id, "ZoomOut", "Zoom Out", Properties.Resources.zoom_out_big,
                        Properties.Resources.zoom_out),
                    new ActionDefinition(Id, "ZoomNormal", "100%", Properties.Resources.view_1_1_big,
                        Properties.Resources.view_1_1),
                    new ActionDefinition(Id, "ZoomFit", "Zoom to Fit", Properties.Resources.window_size_big,
                        Properties.Resources.window_size),
                };
                if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Zoom", x.Key) == 0) ?? false)
                {
                    var definitions = _commandsBarContextAwareActions["Zoom"];
                    List<IActionDefinition> actions = new List<IActionDefinition>();
                    foreach (var definition in definitions)
                    {
                        foreach (var command in definition.Commands)
                        {
                            actions.Add(command);
                        }
                    }

                    zoomList.AddRange(actions);
                    _commandsBarContextAwareActions.Remove("Zoom");
                }
                result.Add(new CommandsBarDefinition("Zoom", "Zoom", zoomList));
                #endregion

                #region Snapshot.
                var snapshotList = new List<IActionDefinition>()
                {
                    new ActionDefinition(Id, "Clipboard", "Copy to Clipboard", Properties.Resources.clipboard_big,
                        Properties.Resources.clipboard),
                    new ActionDefinition(Id, "File", "Copy to File", Properties.Resources.floppy_disk_big,
                        Properties.Resources.floppy_disk),
                };
                if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Snapshot", x.Key) == 0) ?? false)
                {
                    var definitions = _commandsBarContextAwareActions["Snapshot"];
                    List<IActionDefinition> actions = new List<IActionDefinition>();
                    foreach (var definition in definitions)
                    {
                        foreach (var command in definition.Commands)
                        {
                            actions.Add(command);
                        }
                    }

                    snapshotList.AddRange(actions);
                    _commandsBarContextAwareActions.Remove("Snapshot");
                }
                result.Add(new CommandsBarDefinition("Snapshot", "Create Snapshot", snapshotList));
                #endregion

                if (_executionMode != ExecutionMode.Business && _executionMode != ExecutionMode.Management)
                {
                    #region Layout.
                    var layoutList = new List<IActionDefinition>()
                    {
                        new ActionDefinition(Id, "AlignH", "Align Horizontally",
                            Properties.Resources.layout_horizontal_big,
                            Properties.Resources.layout_horizontal, false),
                        new ActionDefinition(Id, "AlignV", "Align Vertically", Properties.Resources.layout_vertical_big,
                            Properties.Resources.layout_vertical, false),
                        new ActionDefinition(Id, "AlignT", "Align Top", Properties.Resources.layout_top,
                            Properties.Resources.layout_top, false),
                        new ActionDefinition(Id, "AlignB", "Align Bottom", Properties.Resources.layout_bottom_big,
                            Properties.Resources.layout_bottom, false),
                        new ActionDefinition(Id, "AlignL", "Align Left", Properties.Resources.layout_left_big,
                            Properties.Resources.layout_left, false),
                        new ActionDefinition(Id, "AlignR", "Align Right", Properties.Resources.layout_right_big,
                            Properties.Resources.layout_right, false),
                    };
                    if (_executionMode == ExecutionMode.Pioneer || _executionMode == ExecutionMode.Expert)
                    {
                        layoutList.Add(new ActionDefinition(Id, "Layout", "Automatic Layout",
                            Properties.Resources.graph_star_big,
                            Properties.Resources.graph_star));
                    }
                    if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Layout", x.Key) == 0) ?? false)
                    {
                        var definitions = _commandsBarContextAwareActions["Layout"];
                        List<IActionDefinition> actions = new List<IActionDefinition>();
                        foreach (var definition in definitions)
                        {
                            foreach (var command in definition.Commands)
                            {
                                actions.Add(command);
                            }
                        }

                        layoutList.AddRange(actions);
                        _commandsBarContextAwareActions.Remove("Layout");
                    }
                    result.Add(new CommandsBarDefinition("Layout", "Layout", layoutList, true, Properties.Resources.graph_star));
                    #endregion
                }

                #region Fix.
                var fixList = new List<IActionDefinition>()
                {
                    new ActionDefinition(Id, "FixDiagram", "Fix Current Diagram",
                        Properties.Resources.tools_big,
                        Properties.Resources.tools)
                };
                if (_commandsBarContextAwareActions?.Any(x => string.CompareOrdinal("Fix", x.Key) == 0) ?? false)
                {
                    var definitions = _commandsBarContextAwareActions["Fix"];
                    List<IActionDefinition> actions = new List<IActionDefinition>();
                    foreach (var definition in definitions)
                    {
                        foreach (var command in definition.Commands)
                        {
                            actions.Add(command);
                        }
                    }

                    fixList.AddRange(actions);
                    _commandsBarContextAwareActions.Remove("Fix");
                }
                result.Add(new CommandsBarDefinition("Fix", "Fix", fixList, true, Properties.Resources.tools));
                #endregion

                #region Other.
                if (_commandsBarContextAwareActions?.Any() ?? false)
                {
                    foreach (var definitions in _commandsBarContextAwareActions.Values)
                    {
                        List<IActionDefinition> actions = new List<IActionDefinition>();
                        foreach (var definition in definitions)
                        {
                            foreach (var command in definition.Commands)
                            {
                                actions.Add(command);
                            }
                        }

                        result.Add(new CommandsBarDefinition(definitions[0].Name, 
                            definitions[0].Label, 
                            actions, 
                            definitions[0].Collapsible,
                            definitions[0].CollapsedImage));
                    }
                }
                #endregion

                return result;
            }
        }

        [InitializationRequired]
        public void ExecuteCustomAction([NotNull] IActionDefinition action)
        {
            var configuration = new DiagramConfigurationManager(_diagram.Model);
            var hFloat = (float) configuration.DiagramHorizontalSpacing;
            var vFloat = (float) configuration.DiagramVerticalSpacing;

            switch (action.Name)
            {
                case "CreateExtInteractor":
                    var p1 = GetFreePoint(new PointF(50 + _iconSize, vFloat), new SizeF(250, _iconSize + 20), hFloat, vFloat);

                    using (var scope = UndoRedoManager.OpenScope("Create External Interactor"))
                    {
                        var interactor = _diagram.Model?.AddEntity<IExternalInteractor>();
                        AddShape(_diagram.AddShape(interactor, p1));
                        scope?.Complete();
                    }

                    CheckRefresh();
                    break;
                case "CreateProcess":
                    var p2 = GetFreePoint(new PointF(50 + _iconSize, vFloat), new SizeF(250, _iconSize + 20), hFloat, vFloat);

                    using (var scope = UndoRedoManager.OpenScope("Create Process"))
                    {
                        var process = _diagram.Model?.AddEntity<IProcess>();
                        AddShape(_diagram.AddShape(process, p2));
                        scope?.Complete();
                    }

                    CheckRefresh();
                    break;
                case "CreateDataStore":
                    var p3 = GetFreePoint(new PointF(50 + _iconSize, vFloat), new SizeF(250, _iconSize + 20), hFloat, vFloat);

                    using (var scope = UndoRedoManager.OpenScope("Create Data Store"))
                    {
                        var dataStore = _diagram.Model?.AddEntity<IDataStore>();
                        AddShape(_diagram.AddShape(dataStore, p3));
                        scope?.Complete();
                    }

                    CheckRefresh();
                    break;
                case "CreateTrustBoundary":
                    var p4 = GetFreePoint(new PointF(hFloat, vFloat), new SizeF(400, 200), hFloat, vFloat);

                    using (var scope = UndoRedoManager.OpenScope("Create Trust Boundary"))
                    {
                        var trustBoundary = _diagram.Model?.AddGroup<ITrustBoundary>();
                        AddShape(_diagram.AddShape(trustBoundary, p4, new SizeF(400, 200)));
                        scope?.Complete();
                    }

                    CheckRefresh();
                    break;
                case "CreateThreatType":
                    using (var dialog = new ThreatTypeCreationDialog(_diagram.Model))
                    {
                        dialog.ShowDialog(Form.ActiveForm);
                    }
                    break;
                case "RemoveDiagram":
                    if (MessageBox.Show(Form.ActiveForm,
                            "Are you sure you want to remove the current Diagram from the Model?",
                            "Delete Diagram", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        _properties.Item = null;
                        if (_factory.Delete(this))
                        {
                            ShowMessage("Diagram removed successfully.");
                        }
                        else
                        {
                            ShowWarning?.Invoke("Diagram removal has failed.");
                        }
                    }
                    break;
                case "FixDiagram":
                    var fixDiagram = new FixDiagram(_diagram);
                    fixDiagram.ShowDialog(Form.ActiveForm);
                    break;
                case "AlignH":
                    _graph.AlignHorizontally();
                    break;
                case "AlignV":
                    _graph.AlignVertically();
                    break;
                case "AlignT":
                    _graph.AlignTops();
                    break;
                case "AlignB":
                    _graph.AlignBottoms();
                    break;
                case "AlignL":
                    _graph.AlignLeftSides();
                    break;
                case "AlignR":
                    _graph.AlignRightSides();
                    break;
                case "Layout":
                    if (MessageBox.Show("Are you sure you want to automatically layout the Diagram?",
                        "Automatic Layout confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        _graph.DoLayout(configuration.DiagramHorizontalSpacing, configuration.DiagramVerticalSpacing);
                    break;
                case "MarkerToggle":
                    switch (MarkerStatusTrigger.CurrentStatus)
                    {
                        case MarkerStatus.Full:
                            MarkerStatusTrigger.RaiseMarkerStatusUpdated(MarkerStatus.Hidden);
                            break;
                        case MarkerStatus.Hidden:
                            MarkerStatusTrigger.RaiseMarkerStatusUpdated(MarkerStatus.Full);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case "AllPanelsToggle":
                    switch (PanelItemListFormTrigger.CurrentStatus)
                    {
                        case PanelsStatus.Normal:
                            PanelItemListFormTrigger.RaiseShowPanels(PanelsStatus.Visible, _graph);
                            break;
                        case PanelsStatus.Visible:
                            PanelItemListFormTrigger.RaiseShowPanels(PanelsStatus.Normal, _graph);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case "ZoomIn":
                    _graph.ZoomIn();
                    break;
                case "ZoomOut":
                    _graph.ZoomOut();
                    break;
                case "ZoomNormal":
                    _graph.ZoomNormal();
                    break;
                case "ZoomFit":
                    _graph.ZoomToFit();
                    break;
                case "Clipboard":
                    _loading = true;
                    _graph.ToClipboard();
                    _loading = false;
                    break;
                case "File":
                    var dialog2 = new SaveFileDialog();
                    dialog2.CreatePrompt = false;
                    dialog2.OverwritePrompt = true;
                    dialog2.AddExtension = true;
                    dialog2.AutoUpgradeEnabled = true;
                    dialog2.CheckPathExists = true;
                    dialog2.DefaultExt = "png";
                    dialog2.Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|GIF (*.gif)|Bitmap (*.bmp)";
                    dialog2.SupportMultiDottedExtensions = true;
                    dialog2.Title = "Export Diagram as Image";
                    dialog2.RestoreDirectory = true;
                    if (dialog2.ShowDialog(Form.ActiveForm) == DialogResult.OK)
                    {
                        _loading = true;
                        _graph.ToFile(dialog2.FileName);
                        _loading = false;
                    }
                    break;
                default:
                    if (action.Tag is IShapesContextAwareAction shapesAction)
                    {
                        if (GetShapesAndLinks(out var shapes, out var links))
                            shapesAction.Execute(shapes, links);
                    }
                    else if (action.Tag is IIdentityContextAwareAction identityAction)
                    {
                        if (identityAction.Scope.HasFlag(Scope.Diagram))
                        {
                            identityAction.Execute(_diagram);
                        }
                        else if (identityAction.Scope.HasFlag(Scope.ThreatModel))
                        {
                            identityAction.Execute(_diagram.Model);
                        }
                    }
                    else if (action.Tag is IIdentitiesContextAwareAction identitiesContextAwareAction)
                    {
                        if (GetShapesAndLinks(out var shapes, out var links) &&
                            ((identitiesContextAwareAction.Scope & Scope.Entity) != 0 ||
                            (identitiesContextAwareAction.Scope & Scope.TrustBoundary) != 0 ||
                            (identitiesContextAwareAction.Scope & Scope.DataFlow) != 0))
                        {
                            var identities = new List<IIdentity>();

                            if (shapes?.Any() ?? false)
                            {
                                if ((identitiesContextAwareAction.Scope & Scope.Entity) != 0)
                                {
                                    var entities = shapes.Where(x => x is IEntityShape).Select(x => x.Identity);
                                    if (entities.Any())
                                        identities.AddRange(entities);
                                }
                            }
                            if (links?.Any() ?? false)
                            {
                                identities.AddRange(links.Select(x => x.DataFlow));
                            }

                            if (identities.Any())
                            {
                                if (identitiesContextAwareAction.Execute(identities))
                                {
                                    ShowMessage?.Invoke($"{identitiesContextAwareAction.Label} succeeded.");
                                    var item = _properties.Item;
                                    _properties.Item = null;
                                    _properties.Item = item;
                                }
                                else
                                {
                                    ShowWarning?.Invoke($"{identitiesContextAwareAction.Label} failed.");
                                }
                            }

                        }
                    }
                    else if (action.Tag is IPropertiesContainersContextAwareAction containersContextAwareAction)
                    {
                        if (GetShapesAndLinks(out var shapes, out var links) &&
                            ((containersContextAwareAction.Scope & Scope.Entity) != 0 ||
                            (containersContextAwareAction.Scope & Scope.TrustBoundary) != 0 ||
                            (containersContextAwareAction.Scope & Scope.DataFlow) != 0))
                        {
                            var containers = new List<IPropertiesContainer>();
                            if (shapes?.Any() ?? false)
                            {
                                containers.AddRange(shapes.Select(x => x.Identity as IPropertiesContainer));
                            }
                            if (links?.Any() ?? false)
                            {
                                containers.AddRange(links.Select(x => x.DataFlow as IPropertiesContainer));
                            }

                            if (containers.Any())
                            {
                                if (containersContextAwareAction.Execute(containers))
                                {
                                    ShowMessage?.Invoke($"{containersContextAwareAction.Label} succeeded.");
                                    var item = _properties.Item;
                                    _properties.Item = null;
                                    _properties.Item = item;
                                }
                                else
                                {
                                    ShowWarning?.Invoke($"{containersContextAwareAction.Label} failed.");
                                }
                            }

                        }
                    }

                    break;
            }
        }

        private bool GetShapesAndLinks(out List<IShape> shapes, out List<ILink> links)
        {
            var selection = _graph.Selection.ToArray();
            shapes = new List<IShape>();
            links = new List<ILink>();
            foreach (var shape in selection)
            {
                RecursivelyAddShapes(shapes, links, shape);
            }

            return shapes.Any() || links.Any();
        }

        private void RecursivelyAddShapes([NotNull] List<IShape> shapes, [NotNull] List<ILink> links, 
            [NotNull] GoObject shape, IGroup root = null)
        {
            if (shape is GraphEntity node)
                shapes.Add(node.EntityShape);
            else if (shape is GraphGroup group)
            {
                if (root == null && group.GroupShape.Identity is IGroup rootGroup)
                    root = rootGroup;

                shapes.Add(group.GroupShape);
                var entities = new List<IEntity>();
                foreach (var child in group)
                {
                    RecursivelyAddShapes(shapes, links, child, root);
                    if (child is GraphEntity graphEntity && graphEntity.EntityShape.Identity is IEntity entity)
                        entities.Add(entity);
                }

                var internalLinks = _diagram.Links?
                    .Where(x => x.DataFlow?.Source is IEntity source && x.DataFlow?.Target is IEntity target && 
                                ((entities.Contains(source) && entities.Contains(target)) ||
                                (root != null && ReferToSameParent(root, source, target))) &&
                                !links.Contains(x))
                    .ToArray();
                if (internalLinks?.Any() ?? false)
                {
                    links.AddRange(internalLinks);
                }
            } else if (shape is GraphLink glink && glink.Link is ILink link && !links.Contains(link))
            {
                links.Add(link);
            }
        }

        private bool ReferToSameParent([NotNull] IGroup parent, [NotNull] IEntity first, [NotNull] IEntity second)
        {
            return (first.Parent == parent && second.Parent != parent && CheckParentRecursively(parent, second)) ||
                   (first.Parent != parent && second.Parent == parent && CheckParentRecursively(parent, first));
        }

        private bool CheckParentRecursively([NotNull] IGroup parent, [NotNull] IGroupElement child)
        {
            bool result = false;

            if (child.Parent == parent)
                result = true;
            else if (child.Parent is IGroupElement parentGroup)
            {
                result = CheckParentRecursively(parent, parentGroup);
            }

            return result;
        }

        private PointF GetFreePoint(PointF center, SizeF size, float xSpace, float ySpace)
        {
            PointF result = center;

            var upperLeft = new PointF(center.X - (size.Width / 2f), center.Y - (size.Height / 2f));

            if (!_graph.Doc.IsUnoccupied(new RectangleF(upperLeft, new SizeF(size.Width + 20f, size.Height + 20f)), null))
            {
                result = GetFreePoint(new PointF(center.X + xSpace, center.Y + ySpace), size, xSpace, ySpace);
            }
            else
            {
                var groups = _groups.Values;
                if (groups.Any(x => x.ContainsPoint(upperLeft) || x.ContainsPoint(new PointF(center.X + (size.Width / 2f), center.Y + (size.Height / 2f)))))
                    result = GetFreePoint(new PointF(center.X + xSpace, center.Y + ySpace), size, xSpace, ySpace);
            }

            return result;
        }
    }
}