import { useEffect, useState } from 'react';
import {
  getRecentHistory,
  mediaPosterUrl,
  normalizeAssetUrl,
} from '../../../Services/MediaService.tsx';
import '../../CSS/ListeningHistoryActivity.css';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png')
  || 'https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=600&q=80';

function formatTime(seconds = 0) {
  if (!Number.isFinite(seconds) || seconds < 0) return '0:00';
  const totalSeconds = Math.floor(seconds);
  const minutes = Math.floor(totalSeconds / 60);
  const remainingSeconds = String(totalSeconds % 60).padStart(2, '0');
  return `${minutes}:${remainingSeconds}`;
}

function normalizeType(value) {
  const numericMap = ['audio', 'video', '', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';
  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;
  return normalized;
}

export default function ListeningHistoryActivity({ onClose, onPlayMedia }) {
  const [historyItems, setHistoryItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const loadHistory = async () => {
    setIsLoading(true);
    setError('');
    try {
      const recentHistory = await getRecentHistory();
      const history = Array.isArray(recentHistory) ? recentHistory : [];

      if (history.length === 0) {
        setHistoryItems([]);
        setIsLoading(false);
        return;
      }

      const details = history
        .map((track, index) => {
          const id = track.id || track.Id;
          if (!id) return null;

          const mediaType = normalizeType(track.type || track.Type || track.mediaType || track.MediaType);
          const coverImageUrl = track.coverImageUrl || track.CoverImageUrl || track.coverImgUrl || track.CoverImgUrl || null;
          const uploadedAt = track.uploadedAt || track.UploadedAt || track.releaseDate || track.ReleaseDate || null;

          return {
            ...track,
            id,
            kind: 'media',
            type: 'media',
            mediaType,
            ownerId: track.ownerId || track.OwnerId,
            ownerName: track.ownerName || track.OwnerName,
            title: track.title || track.Title || 'Không có tiêu đề',
            subtitle: track.genre || track.Genre || track.description || track.Description || 'Media',
            image: mediaPosterUrl(id) || normalizeAssetUrl(coverImageUrl) || defaultCoverUrl,
            coverImageUrl,
            audioUrl: track.audioUrl || track.AudioUrl || null,
            videoUrl: track.videoUrl || track.VideoUrl || null,
            durationSeconds: Number(track.durationSeconds ?? track.DurationSeconds ?? 0),
            reactionCount: Number(track.favoriteCount ?? track.FavoriteCount ?? 0),
            listenedAt: uploadedAt ? Date.parse(uploadedAt) : null,
            historyOrder: index + 1,
          };
        })
        .filter(Boolean);

      setHistoryItems(details);
    } catch (err) {
      console.error('Error loading history details:', err);
      setError('Không tải được lịch sử nghe.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadHistory();
  }, []);

  const formatListenedTime = (timestamp) => {
    if (!timestamp) return 'Gần đây';

    const date = new Date(timestamp);
    const now = new Date();
    
    // Kiểm tra ngày hôm nay.
    if (date.toDateString() === now.toDateString()) {
      return `Hôm nay, lúc ${date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    }
    
    // Kiểm tra ngày hôm qua.
    const yesterday = new Date(now);
    yesterday.setDate(now.getDate() - 1);
    if (date.toDateString() === yesterday.toDateString()) {
      return `Hôm qua, lúc ${date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    }

    return date.toLocaleString('vi-VN', { 
      day: '2-digit', 
      month: '2-digit', 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  };

  return (
    <aside className="listening-history-activity">
      <div className="history-activity-header">
        <div className="history-activity-title">
          <h2>Lịch sử nghe</h2>
          <span className="history-count-badge">{historyItems.length}</span>
        </div>
        <button type="button" aria-label="Đóng lịch sử" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      {isLoading ? (
        <div className="history-empty">Đang tải lịch sử nghe...</div>
      ) : error ? (
        <div className="history-empty error">{error}</div>
      ) : null}

      <div className="history-list">
        {!isLoading && !error && historyItems.length === 0 && (
          <div className="history-empty">Chưa có bài hát/video nào trong lịch sử nghe gần đây.</div>
        )}

        {historyItems.map((item, index) => {
          const artistLabel = item.ownerName || item.OwnerName || item.ownerId || item.OwnerId || 'TuneVault';

          return (
            <article 
              className="history-item clickable" 
              key={`${item.id}-${item.listenedAt}-${index}`}
              onClick={() => onPlayMedia?.(item)}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onPlayMedia?.(item);
                }
              }}
            >
              <img 
                src={item.image} 
                alt={item.title} 
                onError={(e) => {
                  e.currentTarget.onerror = null;
                  e.currentTarget.src = defaultCoverUrl;
                }}
              />
              <div className="history-item-info">
                <p className="history-item-title" title={item.title}>{item.title}</p>
                <p className="history-item-artist" title={artistLabel}>{artistLabel}</p>
                <div className="history-item-meta">
                  <span className="history-item-time">{formatListenedTime(item.listenedAt)}</span>
                  <span className="history-item-duration">• {formatTime(item.durationSeconds)}</span>
                </div>
              </div>
              <div className="history-play-icon">
                <span className="material-symbols-outlined fill-icon">play_arrow</span>
              </div>
            </article>
          );
        })}
      </div>
    </aside>
  );
}
