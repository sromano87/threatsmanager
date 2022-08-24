﻿using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Patterns.Model;
using PostSharp.Serialization;
using ThreatsManager.Interfaces.ObjectModel.ThreatsMitigations;
using ThreatsManager.Utilities;

namespace ThreatsManager.Engine.Aspects
{
    /// <summary>
    /// This attribute is assigned to a property of an object and allows to automatically assign a value to another property of the same object,
    /// when the value of the property is changed. For example, if it is applied to a property Model receiving a IThreatModel and if it has as "Id"
    /// sourcePropertyName, and "ParentId" as targetPropertyName, then when the property Model is changed, the aspect retrieves the value of
    /// property Id of the new IThreatModel and assigns it to property ParentId of the object containing property Model.
    /// </summary>
    [PSerializable]
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(NotifyPropertyChangedAttribute))]
    public class UpdateStrengthIdAttribute : LocationInterceptionAspect
    {
        /// <summary>
        /// Code executed when the property is set.
        /// </summary>
        /// <param name="args">Arguments describing the operation.</param>
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            base.OnSetValue(args);

            if (!UndoRedoManager.IsUndoing && !UndoRedoManager.IsRedoing &&
                args.Value is IStrength strength &&
                args.Instance is IStrengthIdChanger target)
            {
                var oldValue = target.GetStrengthId();
                var newValue = strength.Id;
                if (oldValue != newValue)
                    target.SetStrengthId(newValue);
            }
        }
    }
}
