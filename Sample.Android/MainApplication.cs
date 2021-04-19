﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Runtime;
using Chino;

namespace Sample.Android
{

#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    public class MainApplication : Application, IExposureNotificationHandler
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        private ExposureNotificationClient EnClient = null;

        public override void OnCreate()
        {
            AbsExposureNotificationClient.Handler = this;
        }

        public AbsExposureNotificationClient GetEnClient()
        {
            if (EnClient == null)
            {
                EnClient = new ExposureNotificationClient();
                EnClient.Init(this);
            }

            return EnClient;
        }

        public void ExposureDetected(IList<IDailySummary> dailySummaries, IList<IExposureWindow> exposureWindows)
        {
            Logger.D("ExposureDetected ExposureWindows");
        }

        public void ExposureDetected(IExposureSummary exposureSummary, IList<IExposureInformation> exposureInformations)
        {
            Logger.D("ExposureDetected ExposureInformations");
        }

        public void ExposureNotDetected()
        {
            Logger.D("ExposureNotDetected");
        }

    }
}
