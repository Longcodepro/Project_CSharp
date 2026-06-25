import { useMemo, useState, useRef } from 'react';
import '../../CSS/PlayerBar.css';

const DEFAULT_REACTION_ICONS = {
  Love: 'favorite',
  Like: 'thumb_up',
  Dislike: 'thumb_down',
  Chill: 'ac_unit',
  Energetic: 'whatshot',
  Save: 'bookmark',
};

const FALLBACK_REACTIONS = [
  { name: 'Love' },
  { name: 'Like' },
  { name: 'Dislike' },
  { name: 'Chill' },
  { name: 'Energetic' },
  { name: 'Save' },
];

export default function PlayerBar({
  isExpanded = false,
  isPlaying = false,
  onTogglePlay,
  onRequireAuth,
  playerTrack,
  onPlayNext,
  onPlayPrevious,
  isNextDisabled = false,
  isPreviousDisabled = false,
  onVolumeChange,
  volume = 1,
  onToggleMute,
  isMuted = false,
  onAddPlaylist, // New prop for add to playlist
  onOpenInfo, // New prop for info panel
  onToggleFavorite, // New prop for toggling favorite (direct love)
  currentFavoriteReaction, // New prop for current favorite reaction
  onSelectFavoriteReaction, // New prop for selecting other reactions
  onToggleExpanded, // Added for fullscreen button
  availableReactions = [],
  isFavoritePickerOpen, // Added prop to control picker visibility
  onToggleFavoritePicker, // Added prop to toggle the picker
  onSeek, // Seek callback
}) {
  const [dragProgress, setDragProgress] = useState(null);
  const [dragTime, setDragTime] = useState('');
  const progressBarRef = useRef(null);

  const isDragging = dragProgress !== null;
  const displayProgress = isDragging ? dragProgress : playerTrack.progress;
  const displayCurrentTime = isDragging ? dragTime : playerTrack.currentTime;

  const progressStyle = { width: `${displayProgress}%` };
  const knobStyle = { left: `${displayProgress}%` };
  const requirePlayerAuth = () => onRequireAuth?.();

  // State for controlling the visibility of the favorite reaction picker
  const [isPickerVisible, setIsPickerVisible] = useState(false);
  const reactions = useMemo(() => {
    if (Array.isArray(availableReactions) && availableReactions.length > 0) {
      return availableReactions
        .filter((reaction) => reaction?.name && reaction.name !== 'Remove')
        .map((reaction) => ({
          name: reaction.name,
          icon: reaction.icon || DEFAULT_REACTION_ICONS[reaction.name] || 'favorite',
        }));
    }

    return FALLBACK_REACTIONS.map((reaction) => ({
      name: reaction.name,
      icon: DEFAULT_REACTION_ICONS[reaction.name] || 'favorite',
    }));
  }, [availableReactions]);

  // Handler for the main heart button click
  const handleFavoriteClick = () => {
    if (!onToggleFavorite) return;

    if (currentFavoriteReaction === 'Love') {
      // If already 'Love', unlike it
      onToggleFavorite(null);
    } else {
      // Otherwise, favorite with 'Love'
      onToggleFavorite('Love');
    }
  };

  // Handler for selecting a reaction from the picker
  const handleReactionSelect = (reactionName) => {
    if (onSelectFavoriteReaction) {
      onSelectFavoriteReaction(reactionName);
    }
    setIsPickerVisible(false); // Hide picker after selection
  };

  // Toggle picker visibility on hover
  const handleFavoriteMouseEnter = () => {
    if (reactions.length > 0) {
      setIsPickerVisible(true);
    }
  };

  const handleFavoriteMouseLeave = () => {
    setIsPickerVisible(false);
  };

  const formatDuration = (seconds) => {
    if (!Number.isFinite(seconds) || seconds < 0) return '0:00';
    const totalSeconds = Math.floor(seconds);
    const minutes = Math.floor(totalSeconds / 60);
    const remainingSeconds = String(totalSeconds % 60).padStart(2, '0');
    return `${minutes}:${remainingSeconds}`;
  };

  const getPercentageFromEvent = (clientX, rect) => {
    const clickX = clientX - rect.left;
    const width = rect.width;
    if (width <= 0) return 0;
    return Math.max(0, Math.min(100, (clickX / width) * 100));
  };

  const handleProgressMouseDown = (e) => {
    if (!playerTrack.durationSeconds || !onSeek) return;
    const rect = progressBarRef.current.getBoundingClientRect();
    const pct = getPercentageFromEvent(e.clientX, rect);
    setDragProgress(pct);
    setDragTime(formatDuration((pct / 100) * playerTrack.durationSeconds));

    const handleMouseMove = (moveEvent) => {
      if (!progressBarRef.current) return;
      const currentRect = progressBarRef.current.getBoundingClientRect();
      const currentPct = getPercentageFromEvent(moveEvent.clientX, currentRect);
      setDragProgress(currentPct);
      setDragTime(formatDuration((currentPct / 100) * playerTrack.durationSeconds));
    };

    const handleMouseUp = (upEvent) => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
      
      if (progressBarRef.current) {
        const currentRect = progressBarRef.current.getBoundingClientRect();
        const finalPct = getPercentageFromEvent(upEvent.clientX, currentRect);
        const targetTime = (finalPct / 100) * playerTrack.durationSeconds;
        onSeek(targetTime);
      }
      
      setDragProgress(null);
      setDragTime('');
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  };

  const handleProgressTouchStart = (e) => {
    if (!playerTrack.durationSeconds || !onSeek) return;
    const rect = progressBarRef.current.getBoundingClientRect();
    const touch = e.touches[0];
    const pct = getPercentageFromEvent(touch.clientX, rect);
    setDragProgress(pct);
    setDragTime(formatDuration((pct / 100) * playerTrack.durationSeconds));

    const handleTouchMove = (moveEvent) => {
      if (!progressBarRef.current) return;
      const currentRect = progressBarRef.current.getBoundingClientRect();
      const currentTouch = moveEvent.touches[0];
      const currentPct = getPercentageFromEvent(currentTouch.clientX, currentRect);
      setDragProgress(currentPct);
      setDragTime(formatDuration((currentPct / 100) * playerTrack.durationSeconds));
    };

    const handleTouchEnd = () => {
      document.removeEventListener('touchmove', handleTouchMove);
      document.removeEventListener('touchend', handleTouchEnd);
      
      setDragProgress((currentProgress) => {
        if (currentProgress !== null) {
          const targetTime = (currentProgress / 100) * playerTrack.durationSeconds;
          onSeek(targetTime);
        }
        return null;
      });
      setDragTime('');
    };

    document.addEventListener('touchmove', handleTouchMove);
    document.addEventListener('touchend', handleTouchEnd);
  };

  const handleVolumeChange = (e) => {
    const newVolume = parseFloat(e.target.value);
    onVolumeChange?.(newVolume);
  };

  const volumeIcon = isMuted || volume === 0
    ? 'volume_off'
    : volume < 0.5
      ? 'volume_down'
      : 'volume_up';

  const volumePercent = (isMuted ? 0 : volume) * 100;
  const volumeSliderStyle = {
    background: `linear-gradient(to right, var(--primary) ${volumePercent}%, var(--surface-container-highest) ${volumePercent}%)`
  };

  return (
    <footer className="player-bar">
      <div className="player-song">
        <div className="player-cover">
          <img src={playerTrack.image} alt={playerTrack.title} />
        </div>
        <div className="player-track-copy">
          <p>{playerTrack.title}</p>
          <span>{playerTrack.artist}</span>
        </div>
        <button type="button" aria-label="Thêm vào playlist" onClick={onAddPlaylist || requirePlayerAuth}>
          <span className="material-symbols-outlined">playlist_add</span>
        </button>
        <div className="player-favorite-container" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <button
            type="button"
            aria-label="Yêu thích"
            onClick={handleFavoriteClick}
            className={`favorite-button ${currentFavoriteReaction === 'Love' ? 'active' : ''}`}
            onMouseEnter={handleFavoriteMouseEnter}
            onMouseLeave={handleFavoriteMouseLeave}
          >
            <span className="material-symbols-outlined fill-icon">favorite</span>
          </button>
          {playerTrack?.favoriteCount > 0 && (
            <span className="favorite-count-badge" style={{ fontSize: '11px', color: 'rgba(255, 255, 255, 0.6)', fontWeight: '600' }}>
              {playerTrack.favoriteCount}
            </span>
          )}

          {/* Reaction picker */}
          {isPickerVisible && (
            <div className="favorite-reactions-picker" onMouseEnter={handleFavoriteMouseEnter} onMouseLeave={handleFavoriteMouseLeave}>
              {reactions.map((reaction) => (
                <button
                  key={reaction.name}
                  type="button"
                  aria-label={`Reaction: ${reaction.name}`}
                  onClick={() => handleReactionSelect(reaction.name)}
                  className={currentFavoriteReaction === reaction.name ? 'active' : ''}
                >
                  <span className="material-symbols-outlined">{reaction.icon}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="player-main-controls">
        <div className="player-controls-row">
          <button
            type="button"
            aria-label="Bài trước"
            onClick={onPlayPrevious || requirePlayerAuth}
            disabled={isPreviousDisabled}
          >
            <span className="material-symbols-outlined">skip_previous</span>
          </button>
          <button
            className="player-play-btn"
            type="button"
            aria-label={isPlaying ? 'Tạm dừng' : 'Phát'}
            onClick={onTogglePlay || requirePlayerAuth}
          >
            <span className="material-symbols-outlined fill-icon">{isPlaying ? 'pause' : 'play_arrow'}</span>
          </button>
          <button
            type="button"
            aria-label="Bài sau"
            onClick={onPlayNext || requirePlayerAuth}
            disabled={isNextDisabled}
          >
            <span className="material-symbols-outlined">skip_next</span>
          </button>
        </div>
        <div className="player-progress-row">
          <span>{displayCurrentTime}</span>
          <div 
            ref={progressBarRef}
            className="player-progress" 
            onMouseDown={handleProgressMouseDown}
            onTouchStart={handleProgressTouchStart}
            style={{ cursor: 'pointer' }}
          >
            <div style={progressStyle}></div>
            <i style={knobStyle}></i>
          </div>
          <span>{playerTrack.duration}</span>
        </div>
      </div>

      <div className="player-secondary">
        <button type="button" aria-label="Thông tin media" onClick={onOpenInfo || requirePlayerAuth}>
          <span className="material-symbols-outlined">info</span>
        </button>
        <div className="player-volume">
          <button type="button" aria-label={isMuted ? 'Bật âm thanh' : 'Tắt âm thanh'} onClick={onToggleMute || requirePlayerAuth}>
            <span className="material-symbols-outlined">{volumeIcon}</span>
          </button>
          <div className="player-volume-slider">
            <input
              type="range"
              min="0"
              max="1"
              step="0.01"
              value={isMuted ? 0 : volume}
              onChange={handleVolumeChange}
              style={volumeSliderStyle}
              aria-label="Âm lượng"
            />
          </div>
        </div>
        <button
          className={isExpanded ? 'active' : ''}
          type="button"
          aria-label={isExpanded ? 'Thu nhỏ trình phát' : 'Phóng to trình phát'}
          aria-pressed={isExpanded}
          onClick={onToggleExpanded}
        >
          <span className="material-symbols-outlined">
            {isExpanded ? 'fullscreen_exit' : 'fullscreen'}
          </span>
        </button>
      </div>
    </footer>
  );
}
