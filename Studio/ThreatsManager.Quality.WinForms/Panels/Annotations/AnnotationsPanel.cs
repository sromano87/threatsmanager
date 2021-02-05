﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Layout;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.Extensions;
using ThreatsManager.Interfaces.Extensions.Panels;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.Entities;
using ThreatsManager.Interfaces.ObjectModel.Properties;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;
using ThreatsManager.Quality.Annotations;
using ThreatsManager.Quality.Schemas;
using ThreatsManager.Utilities;
using ThreatsManager.Utilities.Aspects;
using ToolTip = DevComponents.DotNetBar.ToolTip;

namespace ThreatsManager.Quality.Panels.Annotations
{
    public partial class AnnotationsPanel : UserControl, IShowThreatModelPanel<Form>, 
        ICustomRibbonExtension, IInitializableObject
    {
        enum Show
        {
            All,
            OpenTopicsOnly,
            HighlightsOnly
        }

        private IThreatModel _model;
        private AnnotationsPropertySchemaManager _schemaManager;
        private IPropertyType _propertyType;
        private IPropertiesContainer _selected;
        private Show _show = Show.All;
        private Button _currentButton;

        public AnnotationsPanel()
        {
            InitializeComponent();
        }

        #region Implementation of interface IShowThreatModelPanel.
        public Form PanelContainer { get; set; }

        public void SetThreatModel([NotNull] IThreatModel model)
        {
            _model = model;
            _schemaManager = new AnnotationsPropertySchemaManager(_model);
            _propertyType = _schemaManager.GetAnnotationsPropertyType();

            LoadModel();
        }
        #endregion

        public bool IsInitialized => _model != null && _schemaManager != null && _propertyType != null;

        public IActionDefinition ActionDefinition => new ActionDefinition(Id, "Annotations", "Annotations",
            Properties.Resources.note_text_big, Properties.Resources.note_text);

        [InitializationRequired]
        private void _objectTypes_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_objectTypes.SelectedItem is string selected)
            {
                _objects.Items.Clear();
                _imageList.Images.Clear();
                _properties.Item = null;

                ChangeCustomActionStatus?.Invoke("AddNotes", false);
                ChangeCustomActionStatus?.Invoke("AddTopic", false);
                ChangeCustomActionStatus?.Invoke("AddHighlight", false);
                ChangeCustomActionStatus?.Invoke("RemoveNotes", false);
                ChangeCustomActionStatus?.Invoke("RemoveTopic", false);
                ChangeCustomActionStatus?.Invoke("RemoveHighlight", false);

                RemoveButtons();
                _annotation.Annotation = null;

                switch (selected)
                {
                    case "External Interactors":
                        AddObjects(_model.GetExternalInteractors(_schemaManager, _propertyType), true);
                       break;
                    case "Processes":
                        AddObjects(_model.GetProcesses(_schemaManager, _propertyType), true);
                        break;
                    case "Data Stores":
                        AddObjects(_model.GetDataStores(_schemaManager, _propertyType), true);
                        break;
                    case "Flows":
                        AddObjects(_model.GetFlows(_schemaManager, _propertyType));
                        break;
                    case "Trust Boundaries":
                        AddObjects(_model.GetTrustBoundaries(_schemaManager, _propertyType));
                        break;
                    case "Threat Events":
                        AddObjects(_model.GetThreatEvents(_schemaManager, _propertyType));
                        break;
                    case "Threat Event Mitigations":
                        var tems = _model.GetThreatEventMitigations(_schemaManager, _propertyType).ToArray();
                        if (tems?.Any() ?? false)
                        {
                            bool first = true;

                            foreach (var tem in tems)
                            {
                                if (first)
                                {
                                    _imageList.Images.Add(Icons.Resources.mitigations_small);
                                    first = false;
                                }

                                _objects.Items.Add(new ListViewItem(tem.Mitigation.Name, 0)
                                {
                                    ToolTipText = $"'{tem.Mitigation.Name}' for '{tem.ThreatEvent.Name}' on '{tem.ThreatEvent.Parent.Name}'",
                                    Tag = tem
                                });
                            }
                        }
                        break;
                    case "Threat Types":
                        AddObjects(_model.GetThreatTypes(_schemaManager, _propertyType));
                        break;
                    case "Known Mitigations":
                        AddObjects(_model.GetKnownMitigations(_schemaManager, _propertyType));
                        break;
                    case "Standard Mitigations":
                        var sms = _model.GetStandardMitigations(_schemaManager, _propertyType).ToArray();
                        if (sms?.Any() ?? false)
                        {
                            bool first = true;

                            foreach (var sm in sms)
                            {
                                if (first)
                                {
                                    _imageList.Images.Add(Icons.Resources.standard_mitigations_small);
                                    first = false;
                                }

                                _objects.Items.Add(new ListViewItem(sm.Mitigation.Name, 0)
                                {
                                    ToolTipText = $"'{sm.Mitigation.Name}' for '{sm.ThreatType.Name}'",
                                    Tag = sm
                                });
                            }
                        }
                        break;
                    case "Entity Templates":
                        AddObjects(_model.GetEntityTemplates(_schemaManager, _propertyType), true);
                        break;
                    case "Flow Templates":
                        AddObjects(_model.GetFlowTemplates(_schemaManager, _propertyType));
                        break;
                    case "Trust Boundary Templates":
                        AddObjects(_model.GetTrustBoundaryTemplates(_schemaManager, _propertyType));
                        break;
                }

                _objects.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        [InitializationRequired]
        private void _objects_SelectedIndexChanged(object sender, EventArgs e)
        {
            _annotationLayoutControlItem.Visible = false;
            ChangeCustomActionStatus?.Invoke("RemoveNotes", false);
            ChangeCustomActionStatus?.Invoke("RemoveTopic", false);
            ChangeCustomActionStatus?.Invoke("RemoveHighlight", false);
            RemoveButtons();

            if (_objects.SelectedItems.Count == 1 && _objects.SelectedItems[0].Tag is IPropertiesContainer container)
            {
                _selected = container;
                _properties.Item = _selected;
                ChangeCustomActionStatus?.Invoke("AddTopic", true);
                ChangeCustomActionStatus?.Invoke("AddHighlight", true);

                _right.SuspendLayout();
                var annotations = _schemaManager.GetAnnotations(container)?.ToArray();

                var notes = annotations?
                    .Where(x => !(x is TopicToBeClarified || x is Highlight || x is ReviewNote)).ToArray();
                if (notes?.Any() ?? false)
                {
                    foreach (var note in notes)
                    {
                        AddButton(note);
                    }
                    ChangeCustomActionStatus?.Invoke("AddNotes", false);
                }
                else
                {
                    ChangeCustomActionStatus?.Invoke("AddNotes", true);
                }

                var topicsToBeClarified = annotations?.Where(x => x is TopicToBeClarified).ToArray();
                if (topicsToBeClarified?.Any() ?? false)
                {
                    foreach (var topic in topicsToBeClarified)
                    {
                        AddButton(topic);
                    }
                }

                var highlights = annotations?.Where(x => x is Highlight).ToArray();
                if (highlights?.Any() ?? false)
                {
                    foreach (var highlight in highlights)
                    {
                        AddButton(highlight);
                    }
                }

                var reviewNotes = annotations?.Where(x => x is ReviewNote).ToArray();
                if (reviewNotes?.Any() ?? false)
                {
                    foreach (var reviewNote in reviewNotes)
                    {
                        AddButton(reviewNote);
                    }
                }

                _right.ResumeLayout();
            }
            else
            {
                _selected = null;
                _properties.Item = null;
                ChangeCustomActionStatus?.Invoke("AddNotes", false);
                ChangeCustomActionStatus?.Invoke("AddTopic", false);
                ChangeCustomActionStatus?.Invoke("AddHighlight", false);
            }
        }

        #region Auxiliary members.
        [InitializationRequired]
        private void LoadModel()
        {
            _objectTypes.Items.Clear();

            if (_model.Entities?.OfType<IExternalInteractor>()
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                    (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                    (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("External Interactors");
            if (_model.Entities?.OfType<IProcess>()
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Processes");
            if (_model.Entities?.OfType<IDataStore>()
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Data Stores");
            if (_model.DataFlows?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Flows");
            if (_model.Groups?.OfType<ITrustBoundary>()
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Trust Boundaries");
            if (_model.GetThreatEvents()?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Threat Events");
            if (_model.GetThreatEventMitigations()?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Threat Event Mitigations");
            if (_model.ThreatTypes?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Threat Types");
            if (_model.Mitigations?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Known Mitigations");
            if (_model.GetThreatTypeMitigations()?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Standard Mitigations");
            if (_model.EntityTemplates?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Entity Templates");
            if (_model.FlowTemplates?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Flow Templates");
            if (_model.TrustBoundaryTemplates?
                .Any(x => (_show == Show.All && (_schemaManager.HasNotes(x) || _schemaManager.HasTopics(x) || _schemaManager.HasHighlights(x))) ||
                          (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(x)) ||
                          (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(x))) ?? false)
                _objectTypes.Items.Add("Trust Boundary Templates");

            _objects.Items.Clear();
            _properties.Item = null;
            RemoveButtons();
            _annotation.Annotation = null;
        }

        private void AddObjects(IEnumerable<IIdentity> identities, bool useSpecificImage = false)
        {
            var items = identities?
                .Where(x => x is IPropertiesContainer container && 
                            ((_show == Show.All && (_schemaManager.HasNotes(container) || _schemaManager.HasTopics(container) || _schemaManager.HasHighlights(container))) ||
                            (_show == Show.OpenTopicsOnly && _schemaManager.HasOpenTopics(container)) ||
                            (_show == Show.HighlightsOnly && _schemaManager.HasHighlights(container))))
                .ToArray();
            if (items?.Any() ?? false)
            {
                int index = -1;

                foreach (var item in items)
                {
                    if (useSpecificImage)
                        AddObject(item);
                    else
                        index = AddObject(item, index);
                }
            }
        }

        private int AddObject(IIdentity item, int index = -1)
        {
            if (index < 0)
            {
                _imageList.Images.Add(item.GetImage(ImageSize.Small));
                index = _imageList.Images.Count - 1;
            }

            _objects.Items.Add(new ListViewItem(item.Name, index)
            {
                ToolTipText = item.Description,
                Tag = item
            });

            return index;
        }

        private void AddButton([NotNull] Annotation annotation)
        {
            Image image;
            string tooltip;
            int index;
            if (annotation is TopicToBeClarified topicToBeClarified)
            {
                index = _right.RootGroup.Items.OfType<LayoutControlItem>().Count(x => x.Control is Button);
                image = topicToBeClarified.Answered
                    ? Properties.Resources.speech_balloon_answer
                    : Properties.Resources.speech_balloon_question;
                tooltip = $"<b>{(topicToBeClarified.Answered ? "Question Answered" : "Needs Clarification")}</b><br/>{topicToBeClarified.Text?.Replace("\n", "<br/>")}";
            }
            else if (annotation is Highlight highlight)
            {
                index = _right.RootGroup.Items.OfType<LayoutControlItem>().Count(x => x.Control is Button);
                image = Properties.Resources.marker;
                tooltip = $"<b>Highlight</b><br/>{highlight.Text?.Replace("\n", "<br/>")}";
            }
            else if (annotation is ReviewNote reviewNote)
            {
                index = -1;
                image = null;
                tooltip = null;
            }
            else
            {
                index = 0;
                image = Properties.Resources.note_text;
                tooltip = null;
            }

            if (index >= 0)
            {
                var button = new Button()
                {
                    Image = image,
                    Enabled = true,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = {BorderSize = 0},
                    Tag = annotation
                };
                button.Click += AnnotationButtonClick;
                var item = new LayoutControlItem()
                {
                    Text = string.Empty,
                    TextVisible = false,
                    Control = button,
                    Height = 54,
                    HeightType = eLayoutSizeType.Absolute,
                    Width = 54,
                    WidthType = eLayoutSizeType.Absolute
                };
                if (tooltip != null)
                {
                    _superTooltip.SetSuperTooltip(button, new SuperTooltipInfo()
                    {
                        HeaderVisible = false,
                        BodyText = tooltip,
                        FooterVisible = false
                    });
                }

                _right.Controls.Add(button);
                _right.RootGroup.Items.Insert(index, item);
            }
        }

        private void RemoveButton([NotNull] Annotation annotation)
        {
            var item = _right.RootGroup.Items.OfType<LayoutControlItem>()
                .FirstOrDefault(x => x.Control.Tag is Annotation a && a == annotation);
            
            if (item?.Control is Button button)
            {
                button.Click -= AnnotationButtonClick;
                item.Control = null;
                _right.Controls.Remove(button);
                _superTooltip.SetSuperTooltip(button, null);
            }

            _right.RootGroup.Items.Remove(item);
        }

        private void RemoveButtons()
        {
            var items = _right.RootGroup.Items.OfType<LayoutControlItem>().Where(x => x.Control is Button).ToArray();
            if (items.Any())
            {
                foreach (var item in items)
                {
                    if (item.Control is Button button)
                    {
                        button.Click -= AnnotationButtonClick;
                        item.Control = null;
                        _right.Controls.Remove(button);
                        _superTooltip.SetSuperTooltip(button, null);
                    }

                    _right.RootGroup.Items.Remove(item);
                }
            }
        }

        private void AnnotationButtonClick(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is Annotation annotation)
            {
                if (_currentButton != null)
                    _currentButton.FlatAppearance.BorderColor = Color.White;
                _currentButton = button;
                button.FlatAppearance.BorderColor = ThreatsManager.Utilities.ThreatModelManager.StandardColor;
                button.FlatAppearance.BorderSize = 2;

                _annotation.Annotation = annotation;
                _annotationLayoutControlItem.Visible = true;

                if (annotation is TopicToBeClarified)
                {
                    ChangeCustomActionStatus?.Invoke("RemoveNotes", false);
                    ChangeCustomActionStatus?.Invoke("RemoveTopic", true);
                    ChangeCustomActionStatus?.Invoke("RemoveHighlight", false);
                }
                else if (annotation is Highlight)
                {
                    ChangeCustomActionStatus?.Invoke("RemoveNotes", false);
                    ChangeCustomActionStatus?.Invoke("RemoveTopic", false);
                    ChangeCustomActionStatus?.Invoke("RemoveHighlight", true);
                }
                else if (annotation is ReviewNote)
                {
                    ChangeCustomActionStatus?.Invoke("RemoveNotes", false);
                    ChangeCustomActionStatus?.Invoke("RemoveTopic", false);
                    ChangeCustomActionStatus?.Invoke("RemoveHighlight", false);
                }
                else
                {
                    ChangeCustomActionStatus?.Invoke("RemoveNotes", true);
                    ChangeCustomActionStatus?.Invoke("RemoveTopic", false);
                    ChangeCustomActionStatus?.Invoke("RemoveHighlight", false);
                }
            }
        }
        #endregion
    }
}
