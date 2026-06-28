import { useEffect, useMemo, useState } from 'react';
import {
  getAlbumById,
  getFeaturedAlbums,
  getFeaturedPlaylists,
  getMediaById,
  getPlaylistById,
  mediaPosterUrl,
  normalizeAssetUrl,
  reorderTrackInPlaylist,
} from '../../../Services/MediaService.tsx';
import '../../CSS/LibraryDetailView.css';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png')
  || 'https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=480&q=80';

const fallbackCollectionCards = [
  {
    title: 'Midnight Echoes',
    subtitle: 'Playlist • 48 Songs',
    image: 'https://images.unsplash.com/photo-1493246507139-91e8fad9978e?auto=format&fit=crop&w=480&q=80',
  },
  {
    title: 'Peak Performance',
    subtitle: 'Album • 12 Tracks',
    image: 'https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=480&q=80',
  },
  {
    title: 'Synthesized Reality',
    subtitle: 'Playlist • 156 Songs',
    image: 'https://images.unsplash.com/photo-1511379938547-c1f69419868d?auto=format&fit=crop&w=480&q=80',
  },
  {
    title: 'Deep Waters',
    subtitle: 'Playlist • 22 Songs',
    image: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=480&q=80',
  },
  {
    title: 'Urban Pulse',
    subtitle: 'Playlist • 89 Songs',
    image: 'https://images.unsplash.com/photo-1524650359799-842906ca1c06?auto=format&fit=crop&w=480&q=80',
  },
  {
    title: 'Structural Flow',
    subtitle: 'Album • 10 Tracks',
    image: 'https://images.unsplash.com/photo-1518609878373-06d740f60d8b?auto=format&fit=crop&w=480&q=80',
  },
];

function pick(...values) {
  return values.find((value) => value !== undefined && value !== null && String(value).trim() !== '') || '';
}

function normalizeCollection(item) {
  return {
    id: pick(item?.id, item?.Id),
    title: pick(item?.title, item?.Title),
    description: pick(item?.description, item?.Description),
    coverImageUrl: pick(item?.coverImgUrl, item?.CoverImgUrl, item?.coverImageUrl, item?.CoverImageUrl),
    type: pick(item?.type, item?.Type).toLowerCase(),
    tracks: Array.isArray(item?.tracks || item?.Tracks) ? (item.tracks || item.Tracks) : [],
  };
}

function normalizeTrackEntry(track, fallbackOrder) {
  const mediaItemId = pick(track?.mediaItemId, track?.MediaItemId, track?.id, track?.Id);
  return {
    mediaItemId,
    trackOrder: Number(pick(track?.trackOrder, track?.TrackOrder, fallbackOrder)),
    title: pick(track?.title, track?.Title, mediaItemId),
    artist: pick(track?.artist, track?.Artist, track?.ownerName, track?.OwnerName),
    coverImageUrl: pick(track?.coverImageUrl, track?.CoverImageUrl),
    mediaType: pick(track?.mediaType, track?.MediaType, track?.type, track?.Type),
  };
}

function sortByTrackOrder(left, right) {
  return Number(left.trackOrder || 0) - Number(right.trackOrder || 0);
}

export default function LibraryDetailView({ item }) {
  const [detail, setDetail] = useState(() => normalizeCollection(item));
  const [collectionCards, setCollectionCards] = useState(fallbackCollectionCards);
  const [trackRows, setTrackRows] = useState([]);
  const [status, setStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [updatingTrackId, setUpdatingTrackId] = useState('');

  const normalizedType = String(detail?.type || item?.type || '').toLowerCase();
  const isPlaylist = normalizedType === 'playlist';
  const isAlbum = normalizedType === 'album';
  const heading = detail?.title || item?.title || 'Your Playlists & Albums';
  const caption = detail?.description || item?.description || 'Manage your collection and discovered sounds.';

  const trackCount = useMemo(() => trackRows.length, [trackRows]);

  useEffect(() => {
    let isMounted = true;

    async function loadDetail() {
      setIsLoading(true);
      setStatus('');

      try {
        const nextBase = normalizeCollection(item);

        if (isPlaylist && nextBase.id) {
          const playlist = normalizeCollection(await getPlaylistById(nextBase.id));
          const tracks = Array.isArray(playlist.tracks) ? playlist.tracks : [];
          const trackDetails = await Promise.all(
            tracks.map(async (track, index) => {
              const normalizedTrack = normalizeTrackEntry(track, index + 1);
              if (!normalizedTrack.mediaItemId) return null;

              const media = await getMediaById(normalizedTrack.mediaItemId).catch(() => null);
              return {
                ...normalizedTrack,
                title: pick(media?.title, media?.Title, normalizedTrack.title, normalizedTrack.mediaItemId),
                artist: pick(media?.artist, media?.Artist, media?.ownerName, media?.OwnerName, normalizedTrack.artist, 'TuneVault'),
                coverImageUrl: pick(media?.coverImageUrl, media?.CoverImageUrl, media?.image, media?.Image, normalizedTrack.coverImageUrl),
                mediaType: pick(media?.mediaType, media?.MediaType, media?.type, media?.Type, normalizedTrack.mediaType),
              };
            }),
          );

          if (!isMounted) return;
          setDetail({
            ...nextBase,
            ...playlist,
            coverImageUrl: playlist.coverImageUrl || nextBase.coverImageUrl,
          });
          setTrackRows(trackDetails.filter(Boolean).sort(sortByTrackOrder));
        } else if (isAlbum && nextBase.id) {
          const album = normalizeCollection(await getAlbumById(nextBase.id));
          const tracks = Array.isArray(album.tracks) ? album.tracks : [];
          const trackDetails = await Promise.all(
            tracks.map(async (track, index) => {
              const normalizedTrack = normalizeTrackEntry(track, index + 1);
              if (!normalizedTrack.mediaItemId) return null;

              const media = await getMediaById(normalizedTrack.mediaItemId).catch(() => null);
              return {
                ...normalizedTrack,
                title: pick(media?.title, media?.Title, normalizedTrack.title, normalizedTrack.mediaItemId),
                artist: pick(media?.artist, media?.Artist, media?.ownerName, media?.OwnerName, normalizedTrack.artist, 'TuneVault'),
                coverImageUrl: pick(media?.coverImageUrl, media?.CoverImageUrl, media?.image, media?.Image, normalizedTrack.coverImageUrl),
                mediaType: pick(media?.mediaType, media?.MediaType, media?.type, media?.Type, normalizedTrack.mediaType),
              };
            }),
          );

          if (!isMounted) return;
          setDetail({
            ...nextBase,
            ...album,
            coverImageUrl: album.coverImageUrl || nextBase.coverImageUrl,
          });
          setTrackRows(trackDetails.filter(Boolean).sort(sortByTrackOrder));
        } else if (nextBase.id) {
          setDetail(nextBase);
          setTrackRows([]);
        }

        const [playlists, albums] = await Promise.all([
          getFeaturedPlaylists(6).catch(() => []),
          getFeaturedAlbums(6).catch(() => []),
        ]);

        if (!isMounted) return;

        const mappedCollections = [
          ...playlists.map((playlist) => ({
            title: playlist.title,
            subtitle: `Playlist • ${playlist.tracks?.length ?? 0} Songs`,
            image: normalizeAssetUrl(playlist.coverImgUrl) || defaultCoverUrl,
          })),
          ...albums.map((album) => ({
            title: album.title,
            subtitle: `Album • ${album.tracks?.length ?? 0} Tracks`,
            image: normalizeAssetUrl(album.coverImageUrl) || defaultCoverUrl,
          })),
        ].filter((card) => card.title);

        if (mappedCollections.length > 0) {
          setCollectionCards(mappedCollections.slice(0, 8));
        }
      } catch (error) {
        if (!isMounted) return;
        setStatus(error instanceof Error ? error.message : 'Không thể tải chi tiết thư viện.');
        setCollectionCards(fallbackCollectionCards);
        setTrackRows([]);
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadDetail();

    return () => {
      isMounted = false;
    };
  }, [item, isPlaylist, isAlbum]);

  const handleMoveTrack = async (row, nextOrder) => {
    if (!detail?.id || !row?.mediaItemId) return;

    const currentOrder = Number(row.trackOrder || 0);
    const targetOrder = Number(nextOrder);
    if (!Number.isInteger(targetOrder) || targetOrder < 1 || targetOrder > trackRows.length) return;
    if (targetOrder === currentOrder) return;

    try {
      setUpdatingTrackId(row.mediaItemId);
      setStatus('');
      await reorderTrackInPlaylist(detail.id, row.mediaItemId, targetOrder);
      const refreshed = await getPlaylistById(detail.id);
      const normalizedPlaylist = normalizeCollection(refreshed);
      const refreshedTracks = await Promise.all(
        (normalizedPlaylist.tracks || []).map(async (track, index) => {
          const normalizedTrack = normalizeTrackEntry(track, index + 1);
          if (!normalizedTrack.mediaItemId) return null;
          const media = await getMediaById(normalizedTrack.mediaItemId).catch(() => null);
          return {
            ...normalizedTrack,
            title: pick(media?.title, media?.Title, normalizedTrack.title, normalizedTrack.mediaItemId),
            artist: pick(media?.artist, media?.Artist, media?.ownerName, media?.OwnerName, normalizedTrack.artist, 'TuneVault'),
            coverImageUrl: pick(media?.coverImageUrl, media?.CoverImageUrl, media?.image, media?.Image, normalizedTrack.coverImageUrl),
            mediaType: pick(media?.mediaType, media?.MediaType, media?.type, media?.Type, normalizedTrack.mediaType),
          };
        }),
      );

      setDetail((current) => ({ ...current, ...normalizedPlaylist }));
      setTrackRows(refreshedTracks.filter(Boolean).sort(sortByTrackOrder));
      setStatus('Đã cập nhật thứ tự playlist.');
    } catch (error) {
      setStatus(error instanceof Error ? error.message : 'Không thể thay đổi thứ tự track.');
    } finally {
      setUpdatingTrackId('');
    }
  };

  const heroCoverUrl = normalizeAssetUrl(detail?.coverImageUrl) || defaultCoverUrl;

  return (
    <div className="library-detail-view">
      <div className="library-detail-subhead">
        <div className="library-detail-chips">
          <button className="active" type="button">Music</button>
          <button type="button">Audio</button>
          <button type="button">Video</button>
        </div>
        <button className="library-upgrade" type="button">Upgrade</button>
      </div>

      <section className="library-detail-content">
        <div className="library-detail-heading">
          <div>
            <h1>{heading}</h1>
            <p>{caption}</p>
          </div>
        </div>

        {status ? <div className="library-detail-status">{status}</div> : null}

        {isPlaylist || isAlbum ? (
          <section className="library-detail-collection-shell">
            <div className="library-detail-hero">
              <div className="library-detail-cover">
                <img
                  src={heroCoverUrl}
                  alt={heading}
                  onError={(event) => {
                    event.currentTarget.onerror = null;
                    event.currentTarget.src = defaultCoverUrl;
                  }}
                />
              </div>

              <div className="library-detail-copy">
                <span className="library-detail-eyebrow">{isPlaylist ? 'Playlist' : 'Album'}</span>
                <h2>{heading}</h2>
                <p>{caption}</p>
                <div className="library-detail-meta">
                  <div>
                    <strong>{trackCount}</strong>
                    <span>Bài hát</span>
                  </div>
                  <div>
                    <strong>{isLoading ? '...' : 'Tải xong'}</strong>
                    <span>Trạng thái</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="library-track-panel">
              <div className="library-track-panel-header">
                <h3>Danh sách bài hát</h3>
                <span>{trackCount} mục</span>
              </div>

              {isLoading ? (
                <div className="library-track-empty">Đang tải playlist...</div>
              ) : trackRows.length === 0 ? (
                <div className="library-track-empty">Playlist này chưa có bài hát nào.</div>
              ) : (
                <div className="library-track-list">
                  {trackRows.map((row, index) => (
                    <article className="library-track-row" key={`${row.mediaItemId}-${index}`}>
                      <div className="library-track-order">{row.trackOrder || index + 1}</div>
                      <img
                        src={normalizeAssetUrl(row.coverImageUrl) || heroCoverUrl}
                        alt={row.title}
                        onError={(event) => {
                          event.currentTarget.onerror = null;
                          event.currentTarget.src = defaultCoverUrl;
                        }}
                      />
                      <div className="library-track-copy">
                        <strong>{row.title}</strong>
                        <span>{row.artist || 'TuneVault'}</span>
                      </div>
                      <div className="library-track-actions">
                        <button
                          type="button"
                          className="library-track-move"
                          disabled={Boolean(updatingTrackId) || (row.trackOrder || index + 1) <= 1}
                          onClick={() => handleMoveTrack(row, Number(row.trackOrder || index + 1) - 1)}
                          aria-label="Đưa lên trên"
                        >
                          <span className="material-symbols-outlined">arrow_upward</span>
                        </button>
                        <button
                          type="button"
                          className="library-track-move"
                          disabled={Boolean(updatingTrackId) || (row.trackOrder || index + 1) >= trackRows.length}
                          onClick={() => handleMoveTrack(row, Number(row.trackOrder || index + 1) + 1)}
                          aria-label="Đưa xuống dưới"
                        >
                          <span className="material-symbols-outlined">arrow_downward</span>
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </div>
          </section>
        ) : (
          <div className="library-grid">
            {collectionCards.map((card) => (
              <article className="library-card" key={card.title}>
                <div className="library-card-cover">
                  <img
                    src={card.image || defaultCoverUrl}
                    alt={card.title}
                    onError={(event) => {
                      event.currentTarget.onerror = null;
                      event.currentTarget.src = defaultCoverUrl;
                    }}
                  />
                  <div className="library-play-button">
                    <span className="material-symbols-outlined fill-icon">play_arrow</span>
                  </div>
                </div>
                <div>
                  <h2>{card.title}</h2>
                  <p>{card.subtitle}</p>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
