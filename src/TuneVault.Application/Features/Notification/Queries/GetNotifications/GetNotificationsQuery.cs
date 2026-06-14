using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TuneVault.Application.Features.Notification.Queries.GetNotifications
{
    public interface INotificationQueryRepository
    {
        Task<IEnumerable<dynamic>> GetNotificationsAsync(string userId, int limit = 50);

        Task<IEnumerable<dynamic>> GetUnreadNotificationsAsync(string userId, int limit = 50);

        Task<int> CountUnreadNotificationsAsync(string userId);
    }

    public sealed class GetNotificationsQuery
    {
        private readonly INotificationQueryRepository _notificationRepository;

        public GetNotificationsQuery(INotificationQueryRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<dynamic>> GetAllAsync(string userId, int limit = 50)
        {
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.GetNotificationsAsync(
                userId.Trim(),
                limit);
        }

        public async Task<IEnumerable<dynamic>> GetUnreadAsync(string userId, int limit = 50)
        {
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.GetUnreadNotificationsAsync(
                userId.Trim(),
                limit);
        }

        public async Task<int> CountUnreadAsync(string userId)
        {
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.CountUnreadNotificationsAsync(
                userId.Trim());
        }

        private static void ValidateRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
        }
    }
}