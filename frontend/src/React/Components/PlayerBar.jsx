import { useMemo, useState } from 'react';
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
  onOpenVideo, // New prop for video view
  onOpenInfo, // New prop for info panel
  onToggleFavorite, // New prop for toggling favorite (direct love)
  currentFavoriteReaction, // New prop for current favorite reaction
  onSelectFavoriteReaction, // New prop for selecting other reactions
  onToggleExpanded, // Added for fullscreen button
  availableReactions = [],
  isFavoritePickerOpen, // Added prop to control picker visibility
  onToggleFavoritePicker, // Added prop to toggle the picker
}) {
  const progressStyle = { width: `${playerTrack.progress}%` };
  const knobStyle = { left: `${playerTrack.progress}%` };
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

  const handleVolumeChange = (e) => {
    const newVolume = parseFloat(e.target.value);
    onVolumeChange?.(newVolume);
  };

  const volumeIcon = isMuted || volume === 0
    ? 'volume_off'
    : volume < 0.5
      ? 'volume_down'
      : 'volume_up';

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
        <div className="player-favorite-container">
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

        <div className="player-secondary">
          <button type="button" aria-label="Xem video" onClick={onOpenVideo || requirePlayerAuth}>
            <span className="material-symbols-outlined">slideshow</span>
          </button>
          <button type="button" aria-label="Thông tin media" onClick={onOpenInfo || requirePlayerAuth}>
            <span className="material-symbols-outlined">info</span>
          </button>
          <button type="button" aria-label="Micro" onClick={requirePlayerAuth}>
            <span className="material-symbols-outlined">mic</span>
          </button>
          <button type="button" aria-label="Thiết bị" onClick={requirePlayerAuth}>
            <span className="material-symbols-outlined">devices</span>
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
                aria-label="Âm lượng"
              />
            </div>
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
