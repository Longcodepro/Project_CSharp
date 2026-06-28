import { useEffect, useState } from 'react';
import {
  getShareInbox,
  getShareSent,
  getMediaById,
  mediaPosterUrl,
  normalizeAssetUrl,
} from '../../../Services/MediaService.tsx';
import '../../CSS/ShareActivity.css';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png');

function normalizeShareType(value) {
  return String(value || '').trim().toLowerCase();
}

function shareTypeLabel(value) {
  const type = normalizeShareType(value);
  if (type.includes('playlist')) return 'Playlist';
  if (type.includes('album')) return 'Album';
  return 'Media';
}

function formatSharedAt(value) {
  if (!value) return 'Vừa xong';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Vừa xong';
  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function ShareActivity({ onClose, onPlayMedia, onOpenProfile }) {
  const [activeTab, setActiveTab] = useState('inbox');
  const [items, setItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const openProfile = (item, activeKey) => {
    const senderIdDisplay = item.senderIdDisplay || item.SenderIdDisplay;
    const senderDisplayName = item.senderDisplayName || item.SenderDisplayName;
    const receiverIdDisplay = item.receiverIdDisplay || item.ReceiverIdDisplay;
    const receiverDisplayName = item.receiverDisplayName || item.ReceiverDisplayName;

    const target = activeKey === 'sent'
      ? {
          idDisplay: receiverIdDisplay,
          displayName: receiverDisplayName || receiverIdDisplay,
          avatarUrl: item.receiverAvatarUrl || item.ReceiverAvatarUrl || '',
        }
      : {
          idDisplay: senderIdDisplay,
          displayName: senderDisplayName || senderIdDisplay,
          avatarUrl: item.senderAvatarUrl || item.SenderAvatarUrl || '',
        };

    if (!target.idDisplay) return;
    onOpenProfile?.(target);
  };

  useEffect(() => {
    let cancelled = false;

    const loadShares = async () => {
      setIsLoading(true);
      setError('');

      try {
        const data = activeTab === 'sent'
          ? await getShareSent()
          : await getShareInbox();

        if (!cancelled) setItems(Array.isArray(data) ? data : []);
      } catch {
        if (!cancelled) {
          setItems([]);
          setError('Không tải được danh sách chia sẻ.');
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    };

    loadShares();

    return () => {
      cancelled = true;
    };
  }, [activeTab]);

  const playSharedMedia = async (item) => {
    const type = normalizeShareType(item.shareType || item.ShareType);
    const sharedItemId = item.sharedItemId || item.SharedItemId;
    if (!sharedItemId || type.includes('playlist') || type.includes('album')) return;

    const media = await getMediaById(sharedItemId).catch(() => null);
    if (media) onPlayMedia?.(media);
  };

  return (
    <aside className="share-activity">
      <div className="share-activity-header">
        <div className="share-activity-title">
          <h2>Chia sẻ</h2>
          <span className="share-count-badge">{items.length}</span>
        </div>
        <button type="button" aria-label="Đóng chia sẻ" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      <div className="share-tabs" role="tablist" aria-label="Danh sách chia sẻ">
        <button type="button" className={activeTab === 'inbox' ? 'active' : ''} onClick={() => setActiveTab('inbox')}>
          Được chia sẻ
        </button>
        <button type="button" className={activeTab === 'sent' ? 'active' : ''} onClick={() => setActiveTab('sent')}>
          Đã gửi
        </button>
      </div>

      {isLoading ? (
        <div className="share-empty">Đang tải danh sách chia sẻ...</div>
      ) : error ? (
        <div className="share-empty error">{error}</div>
      ) : null}

      <div className="share-list">
        {!isLoading && !error && items.length === 0 ? (
          <div className="share-empty">Chưa có nội dung chia sẻ.</div>
        ) : null}

        {items.map((item) => {
          const id = item.id || item.Id;
          const sharedItemId = item.sharedItemId || item.SharedItemId;
          const shareType = item.shareType || item.ShareType;
          const senderIdDisplay = item.senderIdDisplay || item.SenderIdDisplay;
          const senderDisplayName = item.senderDisplayName || item.SenderDisplayName;
          const receiverIdDisplay = item.receiverIdDisplay || item.ReceiverIdDisplay;
          const receiverDisplayName = item.receiverDisplayName || item.ReceiverDisplayName;
          const senderAvatarUrl = item.senderAvatarUrl || item.SenderAvatarUrl;
          const receiverAvatarUrl = item.receiverAvatarUrl || item.ReceiverAvatarUrl;
          const message = item.message || item.Message;
          const itemTitle = item.itemTitle || item.ItemTitle || sharedItemId;
          const partnerName = activeTab === 'sent' ? receiverDisplayName || receiverIdDisplay : senderDisplayName || senderIdDisplay;
          const partnerIdDisplay = activeTab === 'sent' ? receiverIdDisplay : senderIdDisplay;
          const partnerAvatarUrl = activeTab === 'sent' ? receiverAvatarUrl : senderAvatarUrl;
          const isPlayableMedia = !normalizeShareType(shareType).includes('playlist')
            && !normalizeShareType(shareType).includes('album');

          return (
            <article
              key={id || `${sharedItemId}-${partnerIdDisplay}`}
              className={`share-item ${isPlayableMedia ? 'clickable' : ''}`}
              role={isPlayableMedia ? 'button' : undefined}
              tabIndex={isPlayableMedia ? 0 : undefined}
              onClick={() => playSharedMedia(item)}
              onKeyDown={(event) => {
                if (!isPlayableMedia) return;
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  playSharedMedia(item);
                }
              }}
            >
              <img
                src={
                  isPlayableMedia
                    ? mediaPosterUrl(sharedItemId)
                    : (item.itemCoverImageUrl || item.ItemCoverImageUrl || defaultCoverUrl)
                }
                alt={itemTitle}
              />
              <div className="share-item-info">
                <p>{itemTitle}</p>
                <span>{shareTypeLabel(shareType)}</span>
                <div className="share-person-row">
                  <span>{activeTab === 'sent' ? 'Tới' : 'Từ'}</span>
                  {partnerAvatarUrl ? (
                    <img className="share-person-avatar" src={partnerAvatarUrl} alt={partnerName || partnerIdDisplay || 'Người dùng'} />
                  ) : null}
                  <strong>{partnerName || 'Người dùng'}</strong>
                  {partnerIdDisplay ? (
                    <button
                      type="button"
                      className="profile-link"
                      onClick={(event) => {
                        event.stopPropagation();
                        openProfile(item, activeTab);
                      }}
                    >
                      @{partnerIdDisplay}
                    </button>
                  ) : null}
                </div>
                {message ? <p className="share-message">{message}</p> : null}
                <small>{formatSharedAt(item.sharedAt || item.SharedAt)}</small>
              </div>
              {isPlayableMedia ? (
                <span className="material-symbols-outlined">play_arrow</span>
              ) : null}
            </article>
          );
        })}
      </div>
    </aside>
  );
}
