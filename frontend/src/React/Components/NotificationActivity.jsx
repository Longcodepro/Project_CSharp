import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import {
  getNotifications,
  getUnreadNotifications,
  deleteNotification,
  getAuthAccessToken,
  markAllNotificationsAsRead,
  markNotificationAsRead,
  notificationHubUrl,
} from '../../../Services/MediaService.tsx';
import '../../CSS/NotificationActivity.css';

const filterOptions = [
  { value: 'all', label: 'Tất cả thông báo' },
  { value: 'unread', label: 'Chưa đọc' },
];

const notificationTypeLabels = {
  FriendRequest: 'Lời mời kết bạn',
  FriendAccepted: 'Lời mời kết bạn đã được chấp nhận',
  ShareSong: 'Bài hát được chia sẻ',
  ShareVideo: 'Video được chia sẻ',
  ShareAudio: 'Audio được chia sẻ',
};

function mapNotification(item) {
  const normalizedItem = {
    id: item.id || item.Id,
    type: item.type || item.Type,
    senderId: item.senderId || item.SenderId,
    senderIdDisplay: item.senderIdDisplay || item.SenderIdDisplay,
    senderDisplayName: item.senderDisplayName || item.SenderDisplayName,
    senderAvatarUrl: item.senderAvatarUrl || item.SenderAvatarUrl,
    title: item.title || item.Title,
    message: item.message || item.Message,
    payloadJson: item.payloadJson || item.PayloadJson,
    createdAt: item.createdAt || item.CreatedAt,
    isRead: item.isRead ?? item.IsRead ?? false,
  };

  let payload;
  try {
    payload = normalizedItem.payloadJson ? JSON.parse(normalizedItem.payloadJson) : {};
  } catch {
    payload = {};
  }

  const typeLabel = notificationTypeLabels[normalizedItem.type] || normalizedItem.type || 'Thông báo mới';
  const message = normalizedItem.message || payload.message || payload.title || payload.detail || `Có ${String(typeLabel).toLowerCase()} mới.`;
  const detail = payload.detail || payload.description || normalizedItem.payloadJson || message;
  const senderName = normalizedItem.senderDisplayName || payload.senderDisplayName || payload.senderName || 'Người gửi';
  const senderIdDisplay = normalizedItem.senderIdDisplay || payload.senderIdDisplay || payload.idDisplay || '';
  const senderAvatarUrl = normalizedItem.senderAvatarUrl || payload.senderAvatarUrl || payload.avatarUrl || '';

  return {
    id: normalizedItem.id,
    type: normalizedItem.type,
    source: senderName,
    senderIdDisplay,
    senderAvatarUrl,
    title: normalizedItem.title || payload.title || typeLabel,
    message,
    time: normalizedItem.createdAt ? new Date(normalizedItem.createdAt).toLocaleString('vi-VN') : 'Vừa xong',
    unread: !normalizedItem.isRead,
    detail,
  };
}

export default function NotificationActivity({ onClose, onOpenProfile }) {
  const [filter, setFilter] = useState('all');
  const [expandedId, setExpandedId] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [readIds, setReadIds] = useState(() => new Set());
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const openProfile = (notification) => {
    const target = notification?.senderIdDisplay;
    if (!target) return;
    onOpenProfile?.({
      idDisplay: target,
      displayName: notification?.source || target,
      avatarUrl: notification?.senderAvatarUrl || '',
    });
  };

  useEffect(() => {
    let isMounted = true;

    async function loadNotifications() {
      setIsLoading(true);
      setError('');

      try {
        const data = filter === 'unread'
          ? await getUnreadNotifications(50)
          : await getNotifications(50);

        if (!isMounted) return;

        const mapped = data.map(mapNotification);

        setNotifications(mapped);
        setReadIds(new Set(mapped.filter((item) => !item.unread).map((item) => item.id)));
      } catch {
        if (!isMounted) return;
        setNotifications([]);
        setReadIds(new Set());
        setError('Không tải được thông báo.');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadNotifications();

    return () => {
      isMounted = false;
    };
  }, [filter]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(notificationHubUrl(), {
        accessTokenFactory: () => getAuthAccessToken(),
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', (notification) => {
      const mapped = mapNotification(notification || {});
      if (!mapped.id) return;

      setNotifications((current) => {
        const withoutDuplicate = current.filter((item) => item.id !== mapped.id);
        return [mapped, ...withoutDuplicate];
      });
      setReadIds((previous) => {
        const next = new Set(previous);
        next.delete(mapped.id);
        return next;
      });
    });

    connection.start().catch((error) => {
      console.warn('[TuneVault] Không thể kết nối thông báo real-time.', error);
    });

    return () => {
      connection.stop().catch(() => null);
    };
  }, []);

  const visibleNotifications = notifications.filter((notification) => {
    if (filter === 'unread') {
      return !readIds.has(notification.id);
    }

    return true;
  });

  const unreadVisibleCount = notifications.filter((notification) => !readIds.has(notification.id)).length;

  return (
    <aside className="notification-activity">
      <div className="notification-activity-header">
        <div className="notification-activity-title">
          <h2>Thông báo</h2>
          <span className="notification-count-badge">{unreadVisibleCount}</span>
        </div>
        <button type="button" aria-label="Đóng thông báo" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      <div className="notification-tools">
        <label className="notification-select">
          <select value={filter} onChange={(event) => setFilter(event.target.value)} aria-label="Lọc thông báo">
            {filterOptions.map((option) => (
              <option value={option.value} key={option.value}>{option.label}</option>
            ))}
          </select>
          <span className="material-symbols-outlined">expand_more</span>
        </label>

        <button
          className="mark-read-button"
          type="button"
          onClick={async () => {
            setReadIds(new Set(notifications.map((notification) => notification.id)));
            await markAllNotificationsAsRead().catch(() => null);
          }}
        >
          Đánh dấu đã đọc tất cả
        </button>
      </div>

      {isLoading ? (
        <div className="notification-empty">Đang tải thông báo...</div>
      ) : error ? (
        <div className="notification-empty error">{error}</div>
      ) : null}

      <div className="notification-list">
        {!isLoading && !error && visibleNotifications.length === 0 && (
          <div className="notification-empty">Chưa có thông báo nào.</div>
        )}

        {visibleNotifications.map((notification) => {
          const isExpanded = expandedId === notification.id;
          const isUnread = !readIds.has(notification.id);

          return (
            <article className={`notification-item ${isUnread ? 'unread' : ''}`} key={notification.id}>
              <div
                className="notification-summary"
                role="button"
                tabIndex={0}
                onClick={() => {
                  setExpandedId(isExpanded ? null : notification.id);
                  setReadIds((previous) => new Set(previous).add(notification.id));
                  markNotificationAsRead(notification.id).catch(() => null);
                }}
                onKeyDown={(event) => {
                  if (event.key !== 'Enter' && event.key !== ' ') return;
                  event.preventDefault();
                  setExpandedId(isExpanded ? null : notification.id);
                  setReadIds((previous) => new Set(previous).add(notification.id));
                  markNotificationAsRead(notification.id).catch(() => null);
                }}
              >
                {notification.senderAvatarUrl ? (
                  <img src={notification.senderAvatarUrl} alt={notification.source} />
                ) : (
                  <div className="system-icon">
                    <span className="material-symbols-outlined">person</span>
                  </div>
                )}
                <div>
                  <p className="notification-title">
                    <strong>{notification.source}</strong>
                    {notification.senderIdDisplay ? (
                      <button
                        type="button"
                        className="profile-link"
                        onClick={(event) => {
                          event.stopPropagation();
                          openProfile(notification);
                        }}
                      >
                        @{notification.senderIdDisplay}
                      </button>
                    ) : null}
                  </p>
                  <p className="notification-message">{notification.title ? `${notification.title} · ${notification.message}` : notification.message}</p>
                  <small>{notification.time}</small>
                </div>
                <span className={`material-symbols-outlined expand-icon ${isExpanded ? 'open' : ''}`}>expand_more</span>
              </div>
              {isExpanded && (
                <div className="notification-detail-panel">
                  {notification.senderIdDisplay ? (
                    <button
                      type="button"
                      className="profile-link detail-link"
                      onClick={() => openProfile(notification)}
                    >
                      Xem profile @{notification.senderIdDisplay}
                    </button>
                  ) : null}
                  <p className="notification-detail">{notification.detail}</p>
                  <div className="notification-actions">
                    <button
                      type="button"
                      className="notification-action-button"
                      onClick={async () => {
                        await deleteNotification(notification.id).catch(() => null);
                        setNotifications((current) => current.filter((item) => item.id !== notification.id));
                        setReadIds((previous) => {
                          const next = new Set(previous);
                          next.delete(notification.id);
                          return next;
                        });
                        setExpandedId(null);
                      }}
                    >
                      Xóa
                    </button>
                  </div>
                </div>
              )}
            </article>
          );
        })}
      </div>
    </aside>
  );
}
