﻿using System;

namespace ThreatsManager.Interfaces.Exceptions
{
    /// <summary>
    /// Exception raised when the Threat Model cannot be opened for some reason.
    /// </summary>
    /// <remarks>It is typically associated with serialization issues.</remarks>
    [Serializable]
    public class ThreatModelOpeningFailureException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreatModelOpeningFailureException()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message to be shown.</param>
        /// <param name="innerException">Inner exception.</param>
        public ThreatModelOpeningFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}