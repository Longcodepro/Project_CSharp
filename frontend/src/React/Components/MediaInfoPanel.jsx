import React from 'react';
import '../../CSS/MediaInfoPanel.css'; // Assuming a CSS file for the info panel

export default function MediaInfoPanel({ isOpen, onClose, track }) {
  if (!isOpen || !track || !track.id) return null;

  // Helper to format duration if it's in seconds
  const formatDuration = (duration) => {
    if (!duration) return 'N/A';
    if (typeof duration === 'number') {
      const minutes = Math.floor(duration / 60);
      const seconds = String(duration % 60).padStart(2, '0');
      return `${minutes}:${seconds}`;
    }
    return duration; // Assume it's already formatted
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