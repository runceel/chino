﻿using System;
using ExposureNotifications;

namespace Chino
{
    public class TemporaryExposureKey : ITemporaryExposureKey
    {
        public readonly ENTemporaryExposureKey Source;

        public TemporaryExposureKey(ENTemporaryExposureKey source)
        {
            Source = source;
        }

        public int DaysSinceOnsetOfSymptoms => throw new NotImplementedException();

        public byte[] KeyData => GetKeyData(Source.KeyData);

        public ReportType ReportType => throw new NotImplementedException();

        public int RollingPeriod => (int)Source.RollingPeriod;

        public int RollingStartIntervalNumber => (int)Source.RollingStartNumber;

        public RiskLevel RiskLevel => (RiskLevel)Enum.ToObject(typeof(RiskLevel), Source.TransmissionRiskLevel);

        private static byte[] GetKeyData(Foundation.NSData keyData)
        {
            byte[] dataBytes = new byte[keyData.Length];
            System.Runtime.InteropServices.Marshal.Copy(keyData.Bytes, dataBytes, 0, Convert.ToInt32(keyData.Length));
            return dataBytes;
        }
    }
}
