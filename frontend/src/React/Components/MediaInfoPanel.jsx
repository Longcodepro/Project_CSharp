import React from 'react';
import '../../CSS/MediaInfoPanel.css';

export default function MediaInfoPanel({ isOpen, onClose, track }) {
  if (!isOpen || !track || !track.id) return null;

  const formatDuration = (duration) => {
    if (duration === null || duration === undefined || duration === '') return 'N/A';
    
    let secondsVal = 0;
    if (typeof duration === 'number') {
      secondsVal = duration;
    } else {
      const str = String(duration).trim();
      if (str.includes(':')) {
        const parts = str.split(':');
        if (parts.length === 2) {
          const mins = parseFloat(parts[0]);
          const secs = parseFloat(parts[1]);
          if (!isNaN(mins) && !isNaN(secs)) {
            secondsVal = mins * 60 + secs;
          } else {
            return str;
          }
        } else {
          return str;
        }
      } else {
        const parsed = parseFloat(str);
        if (!isNaN(parsed)) {
          secondsVal = parsed;
        } else {
          return str;
        }
      }
    }
    
    const roundedSeconds = Math.round(secondsVal);
    const minutes = Math.floor(roundedSeconds / 60);
    const seconds = String(roundedSeconds % 60).padStart(2, '0');
    return `${minutes}:${seconds}`;
  };

  return (
    <div className="media-info-overlay" onClick={onClose}>
      <div className="media-info-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
        <div className="info-header">
          <img src={track.image || '/uploads/default-cover/Default.png'} alt={track.title} className="track-cover" />
          <div className="track-details">
            <h3>{track.title}</h3>
            <p>{track.artist}</p>
          </div>
        </div>
        <div className="track-metadata">
          <h4>Thông tin chi tiết</h4>
          <div className="metadata-item">
            <strong>Album/Playlist:</strong>
            <span>{track.collectionId ? `${track.collectionType} - ${track.collectionId}` : 'Độc lập'}</span>
          </div>
          <div className="metadata-item">
            <strong>Thời lượng:</strong>
            <span>{formatDuration(track.durationSeconds || track.duration)}</span>
          </div>
          <div className="metadata-item">
            <strong>Thể loại:</strong>
            <span>{track.genre || 'N/A'}</span>
          </div>
          <div className="metadata-item">
            <strong>Ngày phát hành:</strong>
            <span>{track.releaseDate || 'N/A'}</span>
          </div>
          <div className="metadata-item">
            <strong>Lượt nghe:</strong>
            <span>{track.playCount || 'N/A'}</span>
          </div>
          {/* Add more metadata fields as available in the track object */}
        </div>
      </div>
    </div>
  );
}
