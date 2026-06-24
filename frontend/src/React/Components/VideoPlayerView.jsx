import React from 'react';
import '../../CSS/VideoPlayerView.css'; // Assuming a CSS file for the video player view

export default function VideoPlayerView({ isOpen, onClose, track }) {
  if (!isOpen || !track || !track.id) return null;

  // Determine the video source. This might need adjustment based on how video URLs are stored.
  // Assuming track.videoUrl or a similar property exists, or we can derive it from track.id.
  // For now, using a placeholder or assuming track.audioUrl might also serve as a video source if it's a video file.
  const videoSource = track.videoUrl || (track.mediaType === 'video' ? track.audioUrl : null);

  return (
    <div className="video-player-overlay" onClick={onClose}>
      <div className="video-player-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
        {videoSource ? (
          <video
            src={videoSource}
            controls
            autoPlay
            className="video-player"
          // You might need to handle audio continuation here if the video player
          // doesn't automatically allow background audio.
          // For now, assuming the main audio element continues playing.
          />
        ) : (
          <div className="no-video-message">
            <p>Không tìm thấy nguồn video cho bài hát này.</p>
            <p>Thông tin bài hát: {track.title} - {track.artist}</p>
          </div>
        )}
        <div className="video-info-section">
          <h3>{track.title}</h3>
          <p>{track.artist}</p>
          {/* Add more track details here if available */}
        </div>
      </div>
    </div>
  );
}