import { useEffect, useMemo, useRef, useState } from 'react';
import {
  getMediaById,
  normalizeAssetUrl,
} from '../../../Services/MediaService.tsx';
import '../../CSS/PlaylistModal.css';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png') || '';

function pick(...values) {
  return values.find((value) => value !== undefined && value !== null && String(value).trim() !== '') || '';
}

function normalizePlaylist(playlist) {
  return {
    id: pick(playlist?.id, playlist?.Id),
    title: pick(playlist?.title, playlist?.Title),
    coverImageUrl: pick(playlist?.coverImgUrl, playlist?.CoverImgUrl, playlist?.coverImageUrl, playlist?.CoverImageUrl),
    tracks: Array.isArray(playlist?.tracks || playlist?.Tracks) ? (playlist.tracks || playlist.Tracks) : [],
  };
}

function normalizeTrackMedia(media) {
  const id = pick(media?.id, media?.Id);
  return {
    id,
    title: pick(media?.title, media?.Title, id),
    artist: pick(media?.artist, media?.Artist, media?.ownerName, media?.OwnerName, media?.artistName, media?.ArtistName),
    coverImageUrl: pick(media?.coverImageUrl, media?.CoverImageUrl),
  };
}

function getTrackId(track) {
  return pick(track?.mediaItemId, track?.MediaItemId, track?.id, track?.Id);
}

export default function PlaylistModal({
  isOpen,
  onClose,
  playlists,
  onAddTrackToPlaylist,
  currentTrackId,
}) {
  const normalizedPlaylists = useMemo(() => (
    Array.isArray(playlists) ? playlists.map(normalizePlaylist).filter((playlist) => playlist.id) : []
  ), [playlists]);

  const [expandedPlaylistId, setExpandedPlaylistId] = useState('');
  const [expandedTracks, setExpandedTracks] = useState([]);
  const [isLoadingTracks, setIsLoadingTracks] = useState(false);
  const [addingPlaylistId, setAddingPlaylistId] = useState('');
  const [status, setStatus] = useState('');
  const loadRequestRef = useRef(0);

  useEffect(() => {
    if (!isOpen) {
      setExpandedPlaylistId('');
      setExpandedTracks([]);
      setIsLoadingTracks(false);
      setAddingPlaylistId('');
      setStatus('');
    }
  }, [isOpen]);

  const loadPlaylistTracks = async (playlist) => {
    const requestId = loadRequestRef.current + 1;
    loadRequestRef.current = requestId;
    const trackRefs = Array.isArray(playlist.tracks) ? playlist.tracks : [];
    if (trackRefs.length === 0) {
      if (loadRequestRef.current !== requestId) return;
      setExpandedTracks([]);
      setIsLoadingTracks(false);
      return;
    }

    setIsLoadingTracks(true);
    setStatus('');

    try {
      const details = await Promise.all(
        trackRefs.map(async (track) => {
          const trackId = getTrackId(track);
          if (!trackId) return null;

          const media = await getMediaById(trackId).catch(() => null);
          return media ? normalizeTrackMedia(media) : {
            id: trackId,
            title: trackId,
            artist: '',
            coverImageUrl: '',
          };
        }),
      );

      if (loadRequestRef.current !== requestId) return;
      setExpandedTracks(details.filter(Boolean));
    } catch {
      if (loadRequestRef.current !== requestId) return;
      setExpandedTracks([]);
      setStatus('Không tải được danh sách bài hát của playlist này.');
    } finally {
      if (loadRequestRef.current !== requestId) return;
      setIsLoadingTracks(false);
    }
  };

  const handleTogglePlaylist = async (playlist) => {
    const playlistId = playlist.id;
    if (!playlistId) return;

    if (expandedPlaylistId === playlistId) {
      setExpandedPlaylistId('');
      setExpandedTracks([]);
      setStatus('');
      return;
    }

    setExpandedPlaylistId(playlistId);
    setExpandedTracks([]);
    await loadPlaylistTracks(playlist);
  };

  const handleAddTrack = async (playlistId) => {
    if (!currentTrackId || !playlistId) return;

    setAddingPlaylistId(playlistId);
    setStatus('');

    try {
      await onAddTrackToPlaylist(playlistId);
      setStatus('Đã thêm bài hát vào playlist.');

      if (expandedPlaylistId === playlistId) {
        const playlist = normalizedPlaylists.find((item) => item.id === playlistId);
        if (playlist) {
          await loadPlaylistTracks(playlist);
        }
      }
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Không thể thêm bài hát vào playlist.');
    } finally {
      setAddingPlaylistId('');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="playlist-modal-overlay" role="presentation" onMouseDown={onClose}>
      <div className="playlist-modal-content" role="dialog" aria-modal="true" aria-label="Thêm bài hát vào playlist" onMouseDown={(event) => event.stopPropagation()}>
        <div className="playlist-modal-header">
          <div>
            <h2>Thêm vào playlist</h2>
            <p>Chọn playlist rồi mở danh sách để xem bài hát bên trong.</p>
          </div>
          <button type="button" className="close-button" onClick={onClose} aria-label="Đóng">
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        {status ? <div className="playlist-modal-status" role="status">{status}</div> : null}

        <div className="playlist-list">
          {normalizedPlaylists.length === 0 ? (
            <div className="playlist-empty">Bạn chưa có playlist nào. Hãy tạo playlist trước.</div>
          ) : (
            normalizedPlaylists.map((playlist) => {
              const isExpanded = expandedPlaylistId === playlist.id;
              const trackRefs = Array.isArray(playlist.tracks) ? playlist.tracks : [];
              const hasCurrentTrack = Boolean(currentTrackId) && trackRefs.some((track) => getTrackId(track) === currentTrackId);
              const totalTracks = trackRefs.length;

              return (
                <article className={`playlist-row ${isExpanded ? 'expanded' : ''}`} key={playlist.id}>
                  <div className="playlist-row-main">
                    <button
                      type="button"
                      className="playlist-row-toggle"
                      onClick={() => handleTogglePlaylist(playlist)}
                      aria-expanded={isExpanded}
                    >
                      <img
                        src={playlist.coverImageUrl ? normalizeAssetUrl(playlist.coverImageUrl) : defaultCoverUrl}
                        alt={playlist.title}
                        onError={(event) => {
                          event.currentTarget.onerror = null;
                          event.currentTarget.src = defaultCoverUrl;
                        }}
                      />
                      <div className="playlist-row-copy">
                        <strong>{playlist.title}</strong>
                        <span>{totalTracks} bài hát</span>
                      </div>
                      <span className="material-symbols-outlined playlist-row-chevron">
                        {isExpanded ? 'expand_less' : 'expand_more'}
                      </span>
                    </button>

                    <button
                      type="button"
                      className="playlist-row-action"
                      disabled={Boolean(addingPlaylistId) || !currentTrackId || hasCurrentTrack}
                      onClick={() => handleAddTrack(playlist.id)}
                    >
                      {hasCurrentTrack ? 'Đã có' : addingPlaylistId === playlist.id ? 'Đang thêm...' : 'Thêm'}
                    </button>
                  </div>

                  {isExpanded ? (
                    <div className="playlist-track-panel">
                      {isLoadingTracks ? (
                        <div className="playlist-track-empty">Đang tải bài hát trong playlist...</div>
                      ) : expandedTracks.length === 0 ? (
                        <div className="playlist-track-empty">Playlist này chưa có bài hát nào.</div>
                      ) : (
                        expandedTracks.map((track, index) => (
                          <div className="playlist-track-row" key={`${track.id}-${index}`}>
                            <img
                              src={track.coverImageUrl ? normalizeAssetUrl(track.coverImageUrl) : defaultCoverUrl}
                              alt={track.title}
                              onError={(event) => {
                                event.currentTarget.onerror = null;
                                event.currentTarget.src = defaultCoverUrl;
                              }}
                            />
                            <div className="playlist-track-copy">
                              <strong>{track.title}</strong>
                              <span>{track.artist || 'TuneVault'}</span>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  ) : null}
                </article>
              );
            })
          )}
        </div>
      </div>
    </div>
  );
}
