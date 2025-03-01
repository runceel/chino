﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chino
{
    public static class ChinoLogger
    {
        public static void D(string message)
        {
#if DEBUG
            Debug.Print(message);
#endif
        }

        public static void I(string message)
        {
            Debug.Print(message);
        }

        public static void W(string message)
        {
            Debug.Print(message);
        }

        public static void E(string message)
        {
            Debug.Print(message);
        }

        public static void D(List<TemporaryExposureKey> teks, int transmissonRisk, ReportType reportType)
        {
            string content = "{\"temporaryExposureKeys\":[\n";

            foreach (TemporaryExposureKey tek in teks)
            {
                bool isLast = teks.IndexOf(tek) == teks.Count - 1;

                string keyString = Convert.ToBase64String(tek.KeyData);
                int rollingStartNumber = tek.RollingStartIntervalNumber;
                int rollingPeriod = tek.RollingPeriod;
                content += "{\n";
                content += $"    \"key\":\"{keyString}\",\n";
                content += $"    \"rollingStartNumber\":{rollingStartNumber},\n";
                content += $"    \"rollingPeriod\":{rollingPeriod},\n";
                content += $"    \"reportType\":{(int)reportType},\n";
                content += $"    \"transmissionRisk\":{transmissonRisk}\n";

                content += isLast ? "}\n" : "},\n";
            }

            content += "]}\n";
            D(content);
        }

        public static void D(IList<ExposureWindow> exposureWindows)
        {
            D($"exposureWindows - {exposureWindows.Count()}");

            foreach (var ew in exposureWindows)
            {
                D($"Infectiousness: {ew.Infectiousness}");
                D($"CalibrationConfidence: {ew.CalibrationConfidence}");
                D($"DateMillisSinceEpoch: {ew.DateMillisSinceEpoch}");
                D($"ReportType: {ew.ReportType}");

                D($"scanInstances - {ew.ScanInstances.Count()}");

                foreach (var si in ew.ScanInstances)
                {
                    D($"MinAttenuationDb: {si.MinAttenuationDb}");
                    D($"SecondsSinceLastScan: {si.SecondsSinceLastScan}");
                    D($"TypicalAttenuationDb: {si.TypicalAttenuationDb}");
                }
            }
        }

        public static void D(Exception exception)
        {
            D($"{exception.GetType().Name} occurred - {exception.Message}");
        }

        public static void E(Exception exception)
        {
            E($"{exception.GetType().Name} occurred - {exception.Message}");
        }
    }
}
