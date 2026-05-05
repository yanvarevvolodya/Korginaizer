using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiApp1.Services
{
    public class NotificationService
    {
        public void ScheduleNotification(int taskId, string title, DateTime scheduledTime)
        {
            Debug.WriteLine($"=== УВЕДОМЛЕНИЕ: попытка запланировать для задачи {taskId} на {scheduledTime}");



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
            return;
        }

        public void CancelNotification(int taskId)
        {
            LocalNotificationCenter.Current.Cancel(taskId);
        }
    }
}
