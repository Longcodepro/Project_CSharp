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

export default function NowPlayingView({ track }) {
  const progressStyle = { width: `${track.progress}%` };
  const knobStyle = { left: `${track.progress}%` };

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
              <div className="now-playing-progress">
                <div style={progressStyle}></div>
                <i style={knobStyle}></i>
              </div>
              <div className="now-playing-time">
                <span>{track.currentTime}</span>
                <span>{track.duration}</span>
              </div>
            </div>

            <div className="now-playing-control-row">
              <button type="button" aria-label="Bài trước">
                <span className="material-symbols-outlined large">skip_previous</span>
              </button>
              <button className="now-playing-pause" type="button" aria-label="Tạm dừng">
                <span className="material-symbols-outlined fill-icon">pause</span>
              </button>
              <button type="button" aria-label="Bài sau">
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
