﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;

using Nearby = Android.Gms.Nearby.NearbyClass;
using Java.IO;
using Android.Gms.Nearby.ExposureNotification;
using System.Threading.Tasks;
using System.Linq;

[assembly: UsesFeature("android.hardware.bluetooth_le", Required = true)]
[assembly: UsesFeature("android.hardware.bluetooth")]
[assembly: UsesPermission(Android.Manifest.Permission.Bluetooth)]

namespace Chino
{
    // https://developers.google.com/android/reference/com/google/android/gms/nearby/exposurenotification/ExposureNotificationClient
    public class ExposureNotificationClient : AbsExposureNotificationClient
    {
        public const string PERMISSION_EXPOSURE_CALLBACK = "com.google.android.gms.nearby.exposurenotification.EXPOSURE_CALLBACK";
        public const string ACTION_EXPOSURE_STATE_UPDATED = "com.google.android.gms.exposurenotification.ACTION_EXPOSURE_STATE_UPDATED";
        public const string ACTION_EXPOSURE_NOT_FOUND = "com.google.android.gms.exposurenotification.ACTION_EXPOSURE_NOT_FOUND";
        public const string SERVICE_STATE_UPDATED = "com.google.android.gms.exposurenotification.SERVICE_STATE_UPDATED";

        public const string EXTRA_TOKEN = "com.google.android.gms.exposurenotification.EXTRA_TOKEN";
        public const string EXTRA_EXPOSURE_SUMMARY = "com.google.android.gms.exposurenotification.EXTRA_EXPOSURE_SUMMARY";

        [BroadcastReceiver(
            Exported = true,
            Permission = PERMISSION_EXPOSURE_CALLBACK
            )]
        [IntentFilter(new[] { ACTION_EXPOSURE_STATE_UPDATED, ACTION_EXPOSURE_NOT_FOUND })]
        [Preserve]
        public class ExposureStateBroadcastReceiver : BroadcastReceiver
        {
            public override async void OnReceive(Context context, Intent intent)
            {
                if (Handler == null)
                {
                    Logger.E("ExposureStateBroadcastReceiver: Handler is not set.");
                    return;
                }

                ExposureNotificationClient? enClient = null;
                if (context.ApplicationContext is IExposureNotificationHandler exposureNotificationHandler)
                {
                    enClient = (ExposureNotificationClient)exposureNotificationHandler.GetEnClient();
                }

                if (enClient == null)
                {
                    Logger.E("ExposureStateBroadcastReceiver: enClient is null.");
                    return;
                }

                var action = intent.Action;
                Logger.D($"Intent Action {action}");

                switch (action)
                {
                    case ACTION_EXPOSURE_STATE_UPDATED:
                        Logger.D($"ACTION_EXPOSURE_STATE_UPDATED");
                        bool v1 = intent.HasExtra(EXTRA_EXPOSURE_SUMMARY);

                        string varsionStr = v1 ? "1" : "2";
                        Logger.D($"EN version {varsionStr}");

                        if (v1)
                        {
                            string token = intent.GetStringExtra(EXTRA_TOKEN);
                            await GetExposureV1Async(enClient, token);
                        }
                        else
                        {
                            await GetExposureV2Async(enClient);
                        }
                        break;
                    case ACTION_EXPOSURE_NOT_FOUND:
                        Logger.D($"ACTION_EXPOSURE_NOT_FOUND");
                        Handler.ExposureNotDetected();
                        break;
                }
            }

            private async Task GetExposureV1Async(ExposureNotificationClient enClient, string token)
            {
                Logger.D($"GetExposureV1Async");

                Android.Gms.Nearby.ExposureNotification.ExposureSummary exposureSummary = await enClient.EnClient.GetExposureSummaryAsync(token);

                IList<Android.Gms.Nearby.ExposureNotification.ExposureInformation> eis = await enClient.EnClient.GetExposureInformationAsync(token);
                List<IExposureInformation> exposureInformations = eis.Select(ei => (IExposureInformation)new ExposureInformation(ei)).ToList();

                Handler.ExposureDetected(new ExposureSummary(exposureSummary), exposureInformations);
            }

            private async Task GetExposureV2Async(ExposureNotificationClient enClient)
            {
                Logger.D($"GetExposureV2Async");

                Android.Gms.Nearby.ExposureNotification.DailySummariesConfig config = Convert(enClient.ExposureConfiguration.GoogleDailySummariesConfig);
                IList<Android.Gms.Nearby.ExposureNotification.DailySummary> dss = await enClient.EnClient.GetDailySummariesAsync(config);
                List<IDailySummary> dailySummaries = dss.Select(ds => (IDailySummary)new DailySummary(ds)).ToList();

                Print(dailySummaries);

                IList<Android.Gms.Nearby.ExposureNotification.ExposureWindow> ews = await enClient.EnClient.GetExposureWindowsAsync();
                List<IExposureWindow> exposureWindows = ews.Select(ew => (IExposureWindow)new ExposureWindow(ew)).ToList();

                Logger.D(exposureWindows);

                Handler.ExposureDetected(dailySummaries, exposureWindows);
            }

        }

        private static void Print(IList<IDailySummary> dailySummaries)
        {
            Logger.D($"dailySummaries - {dailySummaries.Count()}");

            foreach (var d in dailySummaries)
            {
                Logger.D($"MaximumScore: {d.DaySummary.MaximumScore}");
                Logger.D($"ScoreSum: {d.DaySummary.ScoreSum}");
                Logger.D($"WeightedDurationSum: {d.DaySummary.WeightedDurationSum}");
            }
        }

#nullable enable
        private IExposureNotificationClient? EnClient = null;
#nullable disable

        public void Init(Context applicationContext)
        {
            EnClient = Nearby.GetExposureNotificationClient(applicationContext);
        }

        public override async Task Start()
        {
            await EnClient.StartAsync();
        }

        public override async Task Stop()
        {
            await EnClient.StopAsync();
        }

        public override async Task<bool> IsEnabled()
        {
            return await EnClient.IsEnabledAsync();
        }

        public override async Task<long> GetVersion()
        {
            return await EnClient.GetVersionAsync();
        }

        public override async Task<IExposureNotificationStatus> GetStatus()
        {
            return new ExposureNotificationStatus(await EnClient.GetStatusAsync());
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles)
        {
            await ProvideDiagnosisKeys(keyFiles, new ExposureConfiguration()
            {
                GoogleDailySummariesConfig = new DailySummariesConfig()
            });
        }

        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration)
        {
            Logger.D($"DiagnosisKey {keyFiles.Count}");

            if (keyFiles.Count == 0)
            {
                Logger.D($"No DiagnosisKey found.");
                return;
            }

            ExposureConfiguration = configuration;

            var files = keyFiles.Select(f => new File(f)).ToList();
            DiagnosisKeyFileProvider diagnosisKeyFileProvider = new DiagnosisKeyFileProvider(files);
            await EnClient.ProvideDiagnosisKeysAsync(diagnosisKeyFileProvider);
        }

        public override async Task<List<ITemporaryExposureKey>> GetTemporaryExposureKeyHistory()
        {
            var teks = await EnClient.GetTemporaryExposureKeyHistoryAsync();

            return teks.Select(tek => (ITemporaryExposureKey)new TemporaryExposureKey(tek)).ToList();

        }

#pragma warning disable CS0618 // Type or member is obsolete
        public override async Task ProvideDiagnosisKeys(List<string> keyFiles, ExposureConfiguration configuration, string token)
        {
            var files = keyFiles.Select(f => new File(f)).ToList();
            await EnClient.ProvideDiagnosisKeysAsync(files, Convert(configuration), token);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        private static Android.Gms.Nearby.ExposureNotification.DailySummariesConfig Convert(DailySummariesConfig dailySummariesConfig)
        {
            Android.Gms.Nearby.ExposureNotification.DailySummariesConfig.DailySummariesConfigBuilder builder
                = new Android.Gms.Nearby.ExposureNotification.DailySummariesConfig.DailySummariesConfigBuilder()
                .SetAttenuationBuckets(
                (IList<Java.Lang.Integer>)dailySummariesConfig.AttenuationBucketThresholdDb,
                (IList<Java.Lang.Double>)dailySummariesConfig.AttenuationBucketWeights
                )
                .SetDaysSinceExposureThreshold(dailySummariesConfig.DaysSinceExposureThreshold)
                .SetMinimumWindowScore(dailySummariesConfig.MinimumWindowScore);

            dailySummariesConfig.InfectiousnessWeights.Keys.Zip(
                dailySummariesConfig.InfectiousnessWeights.Values,
                (key, value) => builder.SetInfectiousnessWeight((int)key, value)
                );

            dailySummariesConfig.ReportTypeWeights.Keys.Zip(
                dailySummariesConfig.ReportTypeWeights.Values,
                (key, value) => builder.SetReportTypeWeight((int)key, value)
                );

            return builder.Build();
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private Android.Gms.Nearby.ExposureNotification.ExposureConfiguration Convert(ExposureConfiguration exposureConfiguration)
        {
            ExposureConfiguration.GoogleExposureConfiguration googleExposureConfiguration = exposureConfiguration.GoogleExposureConfig;

            return new Android.Gms.Nearby.ExposureNotification.ExposureConfiguration.ExposureConfigurationBuilder()
                .SetAttenuationScores(googleExposureConfiguration.AttenuationScores)
                .SetAttenuationWeight(googleExposureConfiguration.AttenuationWeight)
                .SetDaysSinceLastExposureScores(googleExposureConfiguration.DaysSinceLastExposureScores)
                .SetDaysSinceLastExposureWeight(googleExposureConfiguration.DaysSinceLastExposureWeight)
                .SetDurationAtAttenuationThresholds(googleExposureConfiguration.DurationAtAttenuationThresholds)
                .SetDurationScores(googleExposureConfiguration.DurationScores)
                .SetDurationWeight(googleExposureConfiguration.DurationWeight)
                .SetMinimumRiskScore(googleExposureConfiguration.MinimumRiskScore)
                .SetTransmissionRiskScores(googleExposureConfiguration.TransmissionRiskScores)
                .SetTransmissionRiskWeight(googleExposureConfiguration.TransmissionRiskWeight)
                .Build();
        }
#pragma warning restore CS0618 // Type or member is obsolete

    }
}
