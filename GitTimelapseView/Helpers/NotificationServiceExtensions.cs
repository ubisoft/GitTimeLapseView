using System.Threading.Tasks;
using AntDesign;

namespace GitTimelapseView.Helpers
{
    internal static class NotificationServiceExtensions
    {
        public static Task ShowSuccess(this NotificationService notificationService, string message)
        {
            return notificationService.Open(new NotificationConfig
            {
                Message = message,
                NotificationType = NotificationType.Success,
            });
        }

        public static Task ShowError(this NotificationService notificationService, string message)
        {
            return notificationService.Open(new NotificationConfig
            {
                Message = message,
                NotificationType = NotificationType.Error,
            });
        }

        public static Task ShowInfo(this NotificationService notificationService, string message)
        {
            return notificationService.Open(new NotificationConfig
            {
                Message = message,
                NotificationType = NotificationType.Info,
            });
        }

        public static Task ShowWarning(this NotificationService notificationService, string message)
        {
            return notificationService.Open(new NotificationConfig
            {
                Message = message,
                NotificationType = NotificationType.Warning,
            });
        }
    }
}
