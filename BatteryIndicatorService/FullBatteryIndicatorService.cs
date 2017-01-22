using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FullBatteryIndicatorService
{
    public partial class FullBatteryIndicatorService : ServiceBase
    {
        private const String APP_ID = "Orga.Full.Battery.Indicator.Service";

        // Necessary to prevent timer creation twice due to the speculated bug 
        // introduced by OnPowerModeChanged function.
        bool mIsTimerRunning = false;

        // When the current battery hits this threshold, then the toast 
        // notification is shown. 
        double mBatteryThreshold = 0.9;

        // Checks the battery percentage of the device at a specified interval.
        System.Timers.Timer timer;

        PowerModeChangedEventHandler mPowerModeChangedEventHandler;

        ToastNotifier mToast;

        public FullBatteryIndicatorService()
        {
            InitializeComponent();
            CanPauseAndContinue = false;
            mIsTimerRunning = false;

            mPowerModeChangedEventHandler = new PowerModeChangedEventHandler(OnPowerModeChanged);

           
            mToast = ToastNotificationManager.CreateToastNotifier(APP_ID);
        }

        // OnStart function is not called during debugging. 
        // Allow OnStart function manual calling.
        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            SystemEvents.PowerModeChanged += mPowerModeChangedEventHandler;
            OnPowerModeChanged(null, null);
        }

        protected override void OnStop()
        {
            SystemEvents.PowerModeChanged -= mPowerModeChangedEventHandler;
            StopTimer();
        }


        // Bug Speculation: This function is called twice when power mode changes.
        // Check if the device is currently charging and take necessary actions.
        public void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;

            // Check if device is charging.
            //     If device is charging, then check if the current battery level is at the given threshold. 
            //         If current battery level is at the given threshold, then do not start timer and notify user.
            //         else, start timer for checking battery life.
            //     If the device is not charging, then stop timer if it's running.
            if (powerStatus.PowerLineStatus == PowerLineStatus.Online)
            {
                if (powerStatus.BatteryLifePercent >= mBatteryThreshold)
                {
                    ShowToastNotification();
                }
                else
                {
                    StartTimer();
                }
            }
            else
            {
                StopTimer();
            }
        }

        
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;

            // If the current battery level reaches the specified threshold, then stop the timer and notify the user.
            if (powerStatus.BatteryLifePercent >= mBatteryThreshold)
            {
                ShowToastNotification();
                StopTimer();
            }
        }

        private void ShowToastNotification()
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            ToastNotification toast = new ToastNotification(toastXml);

            mToast.Show(toast);
       }

        private void StartTimer()
        {
            if (!mIsTimerRunning)
            {
                timer = new System.Timers.Timer();
                timer.Interval = 300000; // 5 minutes.
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
                timer.Start(); // start timer.

                mIsTimerRunning = true;
            }
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop(); // stop the timer.
                mIsTimerRunning = false;
            }
        }
    }
}
