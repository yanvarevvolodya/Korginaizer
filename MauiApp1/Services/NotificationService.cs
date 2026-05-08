using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
#endif

namespace MauiApp1.Services
{
    public class NotificationService
    {
        public void ScheduleNotification(int taskId, string title, DateTime scheduledTime)
        {
            Debug.WriteLine($"=== УВЕДОМЛЕНИЕ: попытка для задачи {taskId} на {scheduledTime}");

#if ANDROID
            var request = new NotificationRequest
            {
                NotificationId = taskId,
                Title = "Напоминание о задаче",
                Description = title,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = scheduledTime,
                    RepeatType = NotificationRepeat.No
                },
                Android = new AndroidOptions
                {
                    ChannelId = "default_channel_id",
                    IconSmallName = new AndroidIcon(AppInfo.Current.PackageName)
                }
            };
            LocalNotificationCenter.Current.Show(request);
            Debug.WriteLine("Android уведомление запланировано");
#endif

#if WINDOWS
            ShowWindowsNotification(title, scheduledTime);
#endif
        }

#if WINDOWS
        private void ShowWindowsNotification(string title, DateTime scheduledTime)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Напоминание о задаче")
                    .AddText($"Задача \"{title}\" запланирована на {scheduledTime:HH:mm}")
                    .Show();
                Debug.WriteLine("Windows Toast показан");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка показа Toast: {ex.Message}");
            }
        }
#endif

        public void CancelNotification(int taskId)
        {
#if ANDROID
            LocalNotificationCenter.Current.Cancel(taskId);
#endif
        }
    }
}
