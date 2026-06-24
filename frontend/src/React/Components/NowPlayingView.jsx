import '../../CSS/NowPlayingView.css';

const lyrics = [
  { text: 'In the echo of the neon rain', active: false, muted: false },
  { text: 'The frequencies begin to fade', active: false, muted: false },
  { text: 'Midnight resonance in my veins', active: true, muted: false },
  { text: 'Waiting for the morning light to cascade', active: false, muted: true },
  { text: 'We are digital ghosts in a machine', active: false, muted: true },
  { text: 'Dancing in the static on the screen', active: false, muted: true },
  { text: 'Searching for a feeling so serene', active: false, muted: true },
  { text: "In a world that's only shades of green", active: false, muted: true },
];

export default function NowPlayingView({
  track,
  isPlaying = false,
  onTogglePlay,
  onPlayNext,
  onPlayPrevious,
  isNextDisabled = false,
  isPreviousDisabled = false,
  onSeek,
}) {
  const progressStyle = { width: `${track.progress}%` };
  const knobStyle = { left: `${track.progress}%` };

  const handleProgressClick = (e) => {
    if (!track.durationSeconds || !onSeek) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const width = rect.width;
    if (width <= 0) return;
    const percentage = Math.max(0, Math.min(1, clickX / width));
    const targetTime = percentage * track.durationSeconds;
    onSeek(targetTime);
  };

  return (
    <div className="now-playing-view">
      <div className="now-playing-glow"></div>

      <section className="now-playing-left">
        <div className="now-playing-cover">
          <img src={track.image} alt={track.title} />
          <div>
            <span className="material-symbols-outlined">expand_more</span>
          </div>
        </div>

        <div className="now-playing-info">
          <div className="now-playing-title-row">
            <div>
              <h2>{track.title}</h2>
              <p>{track.artist}</p>
            </div>
            <button type="button" aria-label="Thêm bài hát">
              <span className="material-symbols-outlined">add_circle</span>
            </button>
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
        </div>
      </section>

      <section className="now-playing-right">
        <div className="now-playing-tabs">
          <button className="active" type="button">Lyrics</button>
          <button type="button">Next Up</button>
        </div>

        <div className="lyrics-list">
          {lyrics.map((line) => (
            <p
              className={`${line.active ? 'active' : ''}${line.muted ? ' muted' : ''}`}
              key={line.text}
            >
              {line.text}
            </p>
          ))}
        </div>
      </section>
    </div>
  );
}
