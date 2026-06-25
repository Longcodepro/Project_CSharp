import React from 'react';

export default function PlaylistModal({
  isOpen,
  onClose,
  playlists,
  onAddTrackToPlaylist,
  currentTrackId,
}) {
  if (!isOpen) return null;

  const handlePlaylistClick = (playlistId) => {
    if (currentTrackId) {
      onAddTrackToPlaylist(playlistId);
    }
  };

  return (
    <div className="playlist-modal-overlay" onClick={onClose}>
      <div className="playlist-modal-content" onClick={(e) => e.stopPropagation()}>
        <h2>Thêm vào Playlist</h2>
        <button className="close-button" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
        {playlists.length === 0 ? (
          <p>Bạn chưa có playlist nào. Hãy tạo một playlist mới!</p>
        ) : (
          <ul className="playlist-list">
            {playlists.map((playlist) => (
              <li key={playlist.id} onClick={() => handlePlaylistClick(playlist.id)}>
                <div className="playlist-info">
                  <img src={playlist.coverImageUrl || '/uploads/default-cover/Default.png'} alt={playlist.name} className="playlist-cover" />
                  <span>{playlist.name}</span>
                </div>
                {/* Optionally show if track is already in playlist */}
                {/* <span className="material-symbols-outlined">check</span> */}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
