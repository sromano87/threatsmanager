﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using PostSharp.Patterns.Contracts;
using ThreatsManager.Interfaces;
using ThreatsManager.Interfaces.Extensions;
using ThreatsManager.Interfaces.ObjectModel;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;
using ThreatsManager.Utilities;

namespace ThreatsManager.Extensions.StatusInfoProviders
{
    [Export(typeof(IStatusInfoProviderExtension))]
    [ExportMetadata("Id", "89B5883D-91F9-4C6B-8DBC-514C2A718079")]
    [ExportMetadata("Label", "Threat Type Counter Status Info Provider")]
    [ExportMetadata("Priority", 20)]
    [ExportMetadata("Parameters", null)]
    [ExportMetadata("Mode", ExecutionMode.Simplified)]
    public class ThreatTypeCounter : IStatusInfoProviderExtension
    {
        private IThreatModel _model;

        
        public event Action<string, string> UpdateInfo;

        public void Initialize([NotNull] IThreatModel model)
        {
            if (_model != null)
            {
                Dispose();
            }

            _model = model;
            _model.ChildCreated += Update;
            _model.ChildRemoved += Update;
        }

        public string CurrentStatus =>
            $"Threat Types: {_model?.ThreatTypes?.Count() ?? 0}";

        public string Description => "Counter of the Threat Types defined in the Threat Model.";

        private void Update(IIdentity obj)
        {
            if (obj is IThreatType)
                UpdateInfo?.Invoke(this.GetExtensionId(), CurrentStatus);
        }

        public override string ToString()
        {
            return "Threat Type Counter";
        }
 
        public void Dispose()
        {
            _model.ChildCreated -= Update;
            _model.ChildRemoved -= Update;
            _model = null;
        }
    }
}
