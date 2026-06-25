import { useEffect, useState } from 'react';
import {
  getNotifications,
  getUnreadNotificationCount,
  getUnreadNotifications,
  deleteNotification,
  markAllNotificationsAsRead,
  markNotificationAsRead,
} from '../../../Services/MediaService.tsx';
import '../../CSS/NotificationActivity.css';

const filterOptions = [
  { value: 'all', label: 'Tất cả thông báo' },
  { value: 'unread', label: 'Chưa đọc' },
  { value: 'system', label: 'Thông báo hệ thống' },
];

const notificationTypeLabels = {
  NewFollower: 'Người theo dõi mới',
  FriendRequest: 'Lời mời kết bạn',
  MediaShared: 'Nội dung được chia sẻ',
  SystemAlert: 'Thông báo hệ thống',
  FriendAccepted: 'Lời mời kết bạn đã được chấp nhận',
  ArtistNewMedia: 'Nghệ sĩ đăng bài mới',
};

function mapNotification(item) {
  let payload;
  try {
    payload = item.payloadJson ? JSON.parse(item.payloadJson) : {};
  } catch {
    payload = {};
  }

  const typeLabel = notificationTypeLabels[item.type] || item.type || 'Thông báo mới';
  const message = payload.message || payload.title || payload.detail || `Có ${String(typeLabel).toLowerCase()} mới.`;
  const detail = payload.detail || payload.description || item.payloadJson || message;

  return {
    id: item.id,
    source: payload.source || typeLabel,
    message,
    time: item.createdAt ? new Date(item.createdAt).toLocaleString('vi-VN') : 'Vừa xong',
    unread: !item.isRead,
    detail,
    avatar: payload.avatarUrl,
    system: item.type?.toLowerCase?.().includes('system') || !payload.avatarUrl,
  };
}

export default function NotificationActivity({ onClose }) {
  const [filter, setFilter] = useState('all');
  const [expandedId, setExpandedId] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [readIds, setReadIds] = useState(() => new Set());
  const [unreadCount, setUnreadCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const refreshUnreadCount = async () => {
    try {
      const data = await getUnreadNotificationCount();
      setUnreadCount(Number(data?.unreadCount ?? data?.count ?? 0));
    } catch {
      setUnreadCount(0);
    }
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
        await refreshUnreadCount();
      } catch {
        if (!isMounted) return;
        setNotifications([]);
        setReadIds(new Set());
        setError('Không tải được thông báo.');
        setUnreadCount(0);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadNotifications();

    return () => {
      isMounted = false;
    };
  }, [filter]);

  const visibleNotifications = notifications.filter((notification) => {
    if (filter === 'unread') {
      return !readIds.has(notification.id);
    }

    if (filter === 'system') {
      return notification.system || !notification.avatar;
    }

    return true;
  });

  return (
    <aside className="notification-activity">
      <div className="notification-activity-header">
        <div className="notification-activity-title">
          <h2>Thông báo</h2>
          <span className="notification-count-badge">{unreadCount}</span>
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
            await refreshUnreadCount();
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
              <button
                className="notification-summary"
                type="button"
                onClick={() => {
                  setExpandedId(isExpanded ? null : notification.id);
                  setReadIds((previous) => new Set(previous).add(notification.id));
                  markNotificationAsRead(notification.id).catch(() => null);
                  refreshUnreadCount();
                }}
              >
                {notification.avatar ? (
                  <img src={notification.avatar} alt={notification.source} />
                ) : (
                  <div className="system-icon">
                    <span className="material-symbols-outlined">shield</span>
                  </div>
                )}
                <div>
                  <p><strong>{notification.source}</strong> {notification.message}</p>
                  <small>{notification.time}</small>
                </div>
                <span className={`material-symbols-outlined expand-icon ${isExpanded ? 'open' : ''}`}>expand_more</span>
              </button>
              {isExpanded && (
                <div className="notification-detail-panel">
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
                        await refreshUnreadCount();
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
