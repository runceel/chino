﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;

using Nearby = Android.Gms.Nearby.NearbyClass;
using Java.IO;
using Android.Gms.Nearby.ExposureNotification;
using System.Threading.Tasks;
using System.Linq;

using AndroidTemporaryExposureKey = Android.Gms.Nearby.ExposureNotification.TemporaryExposureKey;

using Logger = Chino.ChinoLogger;
using Android.Gms.Common.Apis;
using System.Threading;

[assembly: UsesFeature("android.hardware.bluetooth_le", Required = true)]
[assembly: UsesFeature("android.hardware.bluetooth")]
[assembly: UsesPermission(Android.Manifest.Permission.Bluetooth)]

namespace Chino.Android.Google
{
    // https://developers.google.com/android/reference/com/google/android/gms/nearby/exposurenotification/ExposureNotificationClient
    public class ExposureNotificationClient : AbsExposureNotificationClient
    {
        private const string ACTION_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED = "com.google.android.gms.exposurenotification.ACTION_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED";

        private const string PERMISSION_BIND_JOB_SERVICE = "android.permission.BIND_JOB_SERVICE";

        private const string EXTRA_TEMPORARY_EXPOSURE_KEY_LIST = "com.google.android.gms.exposurenotification.EXTRA_TEMPORARY_EXPOSURE_KEY_LIST";

        private static readonly IntentFilter INTENT_FILTER_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED
            = new IntentFilter(ACTION_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED);

        private const int API_TIMEOUT_MILLIS = 3 * 60 * 1000;

#nullable enable
        private Context? _appContext = null;
        internal IExposureNotificationClient? EnClient = null;

#nullable disable

        public JobSetting ExposureDetectedV1JobSetting { get; set; }
        public JobSetting ExposureDetectedV2JobSetting { get; set; }
        public JobSetting ExposureNotDetectedJobSetting { get; set; }

        public void Init(Context applicationContext)
        {
            _appContext = applicationContext;

            try
            {
                EnClient = Nearby.GetExposureNotificationClient(applicationContext);
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        private void CheckInitialized()
        {
            if (EnClient == null)
            {
                Logger.E("Init method must be called first.");
                throw new UnInitializedException("Init method must be called first.");
            }
        }

        public override async Task StartAsync()
        {
            CheckInitialized();

            try
            {
                await EnClient.StartAsync();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task StopAsync()
        {
            CheckInitialized();

            try
            {
                await EnClient.StopAsync();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task<bool> IsEnabledAsync()
        {
            CheckInitialized();

            try
            {
                return await EnClient.IsEnabledAsync();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task<long> GetVersionAsync()
        {
            CheckInitialized();

            try
            {
                return await EnClient.GetVersionAsync();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task<IList<ExposureNotificationStatus>> GetStatusesAsync()
        {
            CheckInitialized();

            try
            {
                var statuses = await EnClient.GetStatusAsync();
                return statuses.Select(status => status.ToExposureNotificationStatus()).ToList();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles)
        {
            await ProvideDiagnosisKeysAsync(keyFiles, new ExposureConfiguration()
            {
                GoogleExposureConfig = new ExposureConfiguration.GoogleExposureConfiguration(),
                GoogleDailySummariesConfig = new DailySummariesConfig()
            });
        }

        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration)
        {
            CheckInitialized();

            Logger.D($"DiagnosisKey {keyFiles.Count}");

            if (keyFiles.Count == 0)
            {
                Logger.D($"No DiagnosisKey found.");
                return;
            }

            ExposureConfiguration = configuration;
            DiagnosisKeysDataMapping diagnosisKeysDataMapping = configuration.GoogleDiagnosisKeysDataMappingConfig.ToDiagnosisKeysDataMapping();

            try
            {
                DiagnosisKeysDataMapping currentDiagnosisKeysDataMapping = await EnClient.GetDiagnosisKeysDataMappingAsync();

                // https://github.com/google/exposure-notifications-internals/blob/aaada6ce5cad0ea1493930591557f8053ef4f113/exposurenotification/src/main/java/com/google/samples/exposurenotification/nearby/DiagnosisKeysDataMapping.java#L113
                if (!diagnosisKeysDataMapping.Equals(currentDiagnosisKeysDataMapping))
                {
                    await EnClient.SetDiagnosisKeysDataMappingAsync(diagnosisKeysDataMapping);
                    Logger.I("DiagnosisKeysDataMapping have been updated.");
                }
                else
                {
                    Logger.D("DiagnosisKeysDataMapping is not updated.");
                }

                var files = keyFiles.Select(f => new File(f)).ToList();
                DiagnosisKeyFileProvider diagnosisKeyFileProvider = new DiagnosisKeyFileProvider(files);
                await EnClient.ProvideDiagnosisKeysAsync(diagnosisKeyFileProvider);
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        public override async Task<List<TemporaryExposureKey>> GetTemporaryExposureKeyHistoryAsync()
        {
            try
            {
                var teks = await EnClient.GetTemporaryExposureKeyHistoryAsync();
                return teks.Select(tek => (TemporaryExposureKey)new PlatformTemporaryExposureKey(tek)).ToList();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public override async Task ProvideDiagnosisKeysAsync(List<string> keyFiles, ExposureConfiguration configuration, string token)
        {
            CheckInitialized();

            Logger.D($"DiagnosisKey {keyFiles.Count}");

            if (keyFiles.Count == 0)
            {
                Logger.D($"No DiagnosisKey found.");
                return;
            }

            ExposureConfiguration = configuration;

            var files = keyFiles.Select(f => new File(f)).ToList();

            try
            {
                await EnClient.ProvideDiagnosisKeysAsync(files, configuration.ToAndroidExposureConfiguration(), token);
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public override async Task RequestPreAuthorizedTemporaryExposureKeyHistoryAsync()
        {
            CheckInitialized();

            try
            {
                await EnClient.RequestPreAuthorizedTemporaryExposureKeyHistoryAsync();
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
        }

        class PreAuthorizeReleasePhoneUnlockedBroadcastReceiver : BroadcastReceiver
        {
            private readonly TaskCompletionSource<Intent> _taskCompletionSource;

            public PreAuthorizeReleasePhoneUnlockedBroadcastReceiver(TaskCompletionSource<Intent> taskCompletionSource)
            {
                _taskCompletionSource = taskCompletionSource;
            }
            public override void OnReceive(Context context, Intent intent)
            {
                Logger.D($"ACTION_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED");
                context.UnregisterReceiver(this);
                _taskCompletionSource.SetResult(intent);
            }
        }

        public override async Task RequestPreAuthorizedTemporaryExposureKeyReleaseAsync()
        {
            Logger.D("RequestPreAuthorizedTemporaryExposureKeyReleaseAsync");

            CheckInitialized();

            IExposureNotificationHandler? handler = null;
            ExposureNotificationClient? enClient = null;
            if (_appContext is IExposureNotificationHandler exposureNotificationHandler)
            {
                handler = exposureNotificationHandler;
                enClient = (ExposureNotificationClient)exposureNotificationHandler.GetEnClient();
            }

            if (handler == null)
            {
                Logger.E("TemporaryExposureKeyReleasedJob: handler is null.");
                return;
            }
            if (enClient == null)
            {
                Logger.E("TemporaryExposureKeyReleasedJob: enClient is null.");
                return;
            }

            try
            {
                IList<TemporaryExposureKey> temporaryExposureKeys = await GetReleasedTemporaryExposureKeys(enClient);
                handler.TemporaryExposureKeyReleased(temporaryExposureKeys);
            }
            catch (ApiException exception)
            {
                if (exception.IsENException())
                {
                    throw exception.ToENException();
                }
                throw exception;
            }
            finally
            {
            }
        }

        private async Task<IList<TemporaryExposureKey>> GetReleasedTemporaryExposureKeys(
            ExposureNotificationClient enClient
            )
        {
            TaskCompletionSource<Intent> taskCompletionSource = new TaskCompletionSource<Intent>();
            BroadcastReceiver receiver = new PreAuthorizeReleasePhoneUnlockedBroadcastReceiver(taskCompletionSource);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(API_TIMEOUT_MILLIS);
            using (cancellationTokenSource.Token.Register(() =>
            {
                Logger.D("cancellationTokenSource canceled.");
                taskCompletionSource.TrySetCanceled();
                _appContext.UnregisterReceiver(receiver);
            }))
            {
                _appContext.RegisterReceiver(
                    receiver,
                    INTENT_FILTER_PRE_AUTHORIZE_RELEASE_PHONE_UNLOCKED
                    );

                await enClient.EnClient.RequestPreAuthorizedTemporaryExposureKeyReleaseAsync();

                Intent intent = await taskCompletionSource.Task;

                IList<TemporaryExposureKey> temporaryExposureKeys = intent.GetParcelableArrayListExtra(EXTRA_TEMPORARY_EXPOSURE_KEY_LIST)
                    .Cast<AndroidTemporaryExposureKey>()
                    .Select(tek => (TemporaryExposureKey)new PlatformTemporaryExposureKey(tek))
                    .ToList();

                return temporaryExposureKeys;
            }
        }
    }

    class UnInitializedException : Exception
    {
        public UnInitializedException(string message) : base(message)
        {
        }
    }
}
