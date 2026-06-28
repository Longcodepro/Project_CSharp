import { useEffect, useMemo, useRef } from 'react';
import '../../CSS/NowPlayingView.css';

function formatCount(value) {
  const count = Number(value);
  if (!Number.isFinite(count) || count < 0) return '0';
  return new Intl.NumberFormat('vi-VN').format(count);
}

function formatReleaseDate(value) {
  if (!value) return 'N/A';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return String(value);
  return date.toLocaleDateString('vi-VN');
}

function formatDetailValue(value) {
  if (value === null || value === undefined || value === '') return 'N/A';
  return String(value);
}

function isVideoTrack(track) {
  return String(track?.mediaType || '').toLowerCase() === 'video';
}

export default function NowPlayingView({
  track,
  isPlaying = false,
  onTogglePlay,
  onPlayNext,
  onPlayPrevious,
  isNextDisabled = false,
  isPreviousDisabled = false,
  onSeek,
  audioRef,
}) {
  const videoRef = useRef(null);
  const isVideo = isVideoTrack(track);
  const videoSource = isVideo ? (track.videoUrl || track.audioUrl || '') : '';
  const ownerName = track.ownerName || track.artist;
  const progressStyle = { width: `${track.progress}%` };
  const knobStyle = { left: `${track.progress}%` };

  const infoItems = useMemo(() => ([
    { label: 'Lượt xem', value: formatCount(track.viewCount) },
    { label: 'Lượt thích', value: formatCount(track.favoriteCount) },
    { label: 'Nghệ sĩ', value: formatDetailValue(ownerName) },
    { label: 'Ngày phát hành', value: formatReleaseDate(track.releaseDate) },
    { label: 'Thể loại', value: formatDetailValue(track.genre) },
    { label: 'Công khai', value: track.isPublic === false ? 'Không' : 'Có' },
  ]), [ownerName, track.favoriteCount, track.genre, track.isPublic, track.releaseDate, track.viewCount]);

  useEffect(() => {
    const video = videoRef.current;
    const audio = audioRef?.current;
    if (!isVideo || !video || !audio) return undefined;

    video.currentTime = audio.currentTime || 0;
    if (isPlaying) {
      video.play().catch(() => {});
    } else {
      video.pause();
    }

    const syncPlay = () => video.play().catch(() => {});
    const syncPause = () => video.pause();
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
  }, [audioRef, isPlaying, isVideo, track.id]);

  const handleProgressClick = (event) => {
    if (!track.durationSeconds || !onSeek) return;
    const rect = event.currentTarget.getBoundingClientRect();
    const clickX = event.clientX - rect.left;
    const width = rect.width;
    if (width <= 0) return;
    const percentage = Math.max(0, Math.min(1, clickX / width));
    onSeek(percentage * track.durationSeconds);
  };

  const handleVideoPlay = () => {
    if (audioRef?.current && audioRef.current.paused) {
      audioRef.current.play().catch(() => {});
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
    <div className="now-playing-view">
      <section className="now-playing-hero">
        {isVideo ? (
          <div className="now-playing-video-shell">
            <video
              ref={videoRef}
              src={videoSource || undefined}
              poster={track.image}
              controls
              muted
              className="now-playing-video"
              onPlay={handleVideoPlay}
              onPause={handleVideoPause}
              onSeeking={handleVideoSeeking}
              onSeeked={handleVideoSeeking}
            />
          </div>
        ) : (
          <div className="now-playing-cover-shell">
            <img src={track.image} alt={track.title} className="now-playing-cover-image" />
          </div>
        )}

        <div className="now-playing-title-block">
          <h2>{track.title}</h2>
          <p>{ownerName}</p>
        </div>

        <div className="now-playing-stats">
          {infoItems.map((item) => (
            <div className="now-playing-stat" key={item.label}>
              <span>{item.label}</span>
              <strong>{item.value}</strong>
            </div>
          ))}
        </div>

        <div className="now-playing-controls">
          <div className="now-playing-progress-wrap">
            <div className="now-playing-progress" onClick={handleProgressClick} style={{ cursor: 'pointer' }}>
              <div style={progressStyle}></div>
              <i style={knobStyle}></i>
            </div>
            <div className="now-playing-time">
              <span>{track.currentTime}</span>
              <span>{track.duration}</span>
            </div>
          </div>

          <div className="now-playing-control-row">
            <button
              type="button"
              aria-label="Bài trước"
              onClick={onPlayPrevious}
              disabled={isPreviousDisabled}
              style={{ opacity: isPreviousDisabled ? 0.5 : 1, cursor: isPreviousDisabled ? 'not-allowed' : 'pointer' }}
            >
              <span className="material-symbols-outlined large">skip_previous</span>
            </button>
            <button
              className="now-playing-pause"
              type="button"
              aria-label={isPlaying ? 'Tạm dừng' : 'Phát'}
              onClick={onTogglePlay}
              style={{ cursor: 'pointer' }}
            >
              <span className="material-symbols-outlined fill-icon">{isPlaying ? 'pause' : 'play_arrow'}</span>
            </button>
            <button
              type="button"
              aria-label="Bài sau"
              onClick={onPlayNext}
              disabled={isNextDisabled}
              style={{ opacity: isNextDisabled ? 0.5 : 1, cursor: isNextDisabled ? 'not-allowed' : 'pointer' }}
            >
              <span className="material-symbols-outlined large">skip_next</span>
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
