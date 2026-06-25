import { useEffect, useState } from 'react';
import {
  getMediaById,
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
  const numericMap = ['audio', 'video', 'podcast', 'song'];
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
      let rawHistory = [];
      try {
        rawHistory = JSON.parse(localStorage.getItem('listening_history') || '[]');
      } catch {
        rawHistory = [];
      }

      if (!Array.isArray(rawHistory)) {
        rawHistory = [];
      }

      // Filter for last 72 hours
      const now = Date.now();
      const cutoff = now - 259200000; // 72 hours
      const validHistory = rawHistory.filter(item => item && item.id && item.timestamp >= cutoff);

      // Save filtered back if changed
      if (validHistory.length !== rawHistory.length) {
        localStorage.setItem('listening_history', JSON.stringify(validHistory));
      }

      if (validHistory.length === 0) {
        setHistoryItems([]);
        setIsLoading(false);
        return;
      }

      // Fetch song details in parallel
      const details = await Promise.all(
        validHistory.map(async (item) => {
          try {
            const track = await getMediaById(item.id);
            if (!track) return null;

            const mediaType = normalizeType(track.type || track.mediaType);
            const coverImageUrl = track.coverImageUrl || track.CoverImageUrl || track.coverImgUrl || track.CoverImgUrl || null;

            // Map to standard playable item structure
            const playItem = {
              id: track.id,
              kind: 'media',
              type: 'media',
              mediaType,
              ownerId: track.ownerId || track.OwnerId,
              artists: track.artists || [],
              title: track.title || track.Title || 'Không có tiêu đề',
              subtitle: track.genre || track.Genre || track.description || track.Description || 'Media',
              image: mediaPosterUrl(track.id) || normalizeAssetUrl(coverImageUrl) || defaultCoverUrl,
              coverImageUrl,
              audioUrl: track.audioUrl || track.AudioUrl || null,
              videoUrl: track.videoUrl || track.VideoUrl || null,
              durationSeconds: Number(track.durationSeconds ?? track.DurationSeconds ?? 0),
              reactionCount: Number(track.favoriteCount ?? track.FavoriteCount ?? 0),
            };

            return {
              ...playItem,
              listenedAt: item.timestamp,
            };
          } catch (e) {
            console.error(`Failed to fetch media details for id: ${item.id}`, e);
            return null;
          }
        })
      );

      // Filter out failed loads
      setHistoryItems(details.filter(Boolean));
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

  const handleClearHistory = () => {
    if (window.confirm('Bạn có chắc chắn muốn xóa toàn bộ lịch sử nghe trong 72 giờ qua?')) {
      localStorage.removeItem('listening_history');
      setHistoryItems([]);
    }
  };

  const formatListenedTime = (timestamp) => {
    const date = new Date(timestamp);
    const now = new Date();
    
    // Check if today
    if (date.toDateString() === now.toDateString()) {
      return `Hôm nay, lúc ${date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    }
    
    // Check if yesterday
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

      {historyItems.length > 0 && (
        <div className="history-tools">
          <button
            className="clear-history-button"
            type="button"
            onClick={handleClearHistory}
          >
            <span className="material-symbols-outlined text-xs">delete</span>
            <span>Xóa lịch sử 72h</span>
          </button>
        </div>
      )}

      {isLoading ? (
        <div className="history-empty">Đang tải lịch sử nghe...</div>
      ) : error ? (
        <div className="history-empty error">{error}</div>
      ) : null}

      <div className="history-list">
        {!isLoading && !error && historyItems.length === 0 && (
          <div className="history-empty">Chưa có bài hát/video nào trong lịch sử 72h qua.</div>
        )}

        {historyItems.map((item, index) => {
          // Build artist name label
          let artistLabel = 'TuneVault';
          if (item.artists && item.artists.length > 0) {
            artistLabel = item.artists.map(a => a.artistName || a.ArtistName || a.artistId || a.ArtistId).join(', ');
          }

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
