﻿using System;
using System.Linq;

namespace Chino
{
    /// <summary>
    /// Summary information about recent exposures.
    ///
    /// This class is deprecated.
    /// no longer used with Exposure Window API.
    ///
    /// The client can get this information via ExposureNotificationClient.getExposureSummary(String).
    /// </summary>
    /// https://developers.google.com/android/reference/com/google/android/gms/nearby/exposurenotification/ExposureSummary
    public class ExposureSummary
    {
        /// <summary>
        /// Array of durations in minutes at certain radio signal attenuations.
        /// </summary>
        public int[] AttenuationDurationsInMinutes { get; set; }

        /// <summary>
        /// Days since last match to a diagnosis key from the server.
        /// </summary>
        public int DaysSinceLastExposure { get; set; }

        /// <summary>
        /// Number of matched diagnosis keys.
        /// </summary>
        public ulong MatchedKeyCount { get; set; }

        /// <summary>
        /// The highest risk score of all exposure incidents, it will be a value 0-4096.
        /// </summary>
        public int MaximumRiskScore { get; set; }

        /// <summary>
        /// The summation of risk scores of all exposure incidents.
        /// </summary>
        public int SummationRiskScore { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is ExposureSummary summary))
            {
                return false;
            }

            bool attenuationDurationsInMinutesEqual;
            if (AttenuationDurationsInMinutes == summary.AttenuationDurationsInMinutes)
            {
                attenuationDurationsInMinutesEqual = true;
            }
            else if (AttenuationDurationsInMinutes == null || summary.AttenuationDurationsInMinutes == null)
            {
                attenuationDurationsInMinutesEqual = false;
            }
            else
            {
                attenuationDurationsInMinutesEqual = AttenuationDurationsInMinutes.SequenceEqual(summary.AttenuationDurationsInMinutes);
            }

            return
                   attenuationDurationsInMinutesEqual &&
                   DaysSinceLastExposure == summary.DaysSinceLastExposure &&
                   MatchedKeyCount == summary.MatchedKeyCount &&
                   MaximumRiskScore == summary.MaximumRiskScore &&
                   SummationRiskScore == summary.SummationRiskScore;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AttenuationDurationsInMinutes, DaysSinceLastExposure, MatchedKeyCount, MaximumRiskScore, SummationRiskScore);
        }
    }
}
