import React, { useEffect, useRef } from 'react';
import '../../CSS/VideoPlayerView.css'; // Assuming a CSS file for the video player view

export default function VideoPlayerView({ isOpen, onClose, track, audioRef, isPlaying }) {
  if (!isOpen || !track || !track.id) return null;

  const videoRef = useRef(null);

  // Determine the video source.
  const videoSource = track.videoUrl || (track.mediaType === 'video' ? track.audioUrl : null);

  useEffect(() => {
    const video = videoRef.current;
    const audio = audioRef?.current;
    if (!isOpen || !video || !audio) return;

    // Sync initial state
    video.currentTime = audio.currentTime;
    if (isPlaying) {
      video.play().catch(() => { });
    } else {
      video.pause();
    }

    const syncPlay = () => {
      video.play().catch(() => { });
    };
    const syncPause = () => {
      video.pause();
    };
    const syncTime = () => {
      const diff = Math.abs(video.currentTime - audio.currentTime);
      if (diff > 0.3) {
        video.currentTime = audio.currentTime;
      }
    };

    audio.addEventListener('play', syncPlay);
    audio.addEventListener('pause', syncPause);
    audio.addEventListener('timeupdate', syncTime);

    return () => {
      audio.removeEventListener('play', syncPlay);
      audio.removeEventListener('pause', syncPause);
      audio.removeEventListener('timeupdate', syncTime);
    };
  }, [isOpen, isPlaying, audioRef]);

  const handleVideoPlay = () => {
    if (audioRef?.current && audioRef.current.paused) {
      audioRef.current.play().catch(() => { });
    }
  };

  const handleVideoPause = () => {
    if (audioRef?.current && !audioRef.current.paused) {
      audioRef.current.pause();
    }
  };

  const handleVideoSeeking = () => {
    if (audioRef?.current && videoRef.current) {
      const diff = Math.abs(audioRef.current.currentTime - videoRef.current.currentTime);
      if (diff > 0.3) {
        audioRef.current.currentTime = videoRef.current.currentTime;
      }
    }
  };

  return (
    <div className="video-player-overlay" onClick={onClose}>
      <div className="video-player-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
        {videoSource ? (
          <video
            ref={videoRef}
            src={videoSource}
            controls
            muted
            className="video-player"
            onPlay={handleVideoPlay}
            onPause={handleVideoPause}
            onSeeking={handleVideoSeeking}
            onSeeked={handleVideoSeeking}
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
        </div>
      </div>
    </div>
  );
}