import { useEffect, useState } from 'react';
import {
  getAlbumById,
  getArtistMedia,
  getFeaturedAlbums,
  getFeaturedPlaylists,
  getPlaylistById,
  mediaPosterUrl,
  normalizeAssetUrl,
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

export default function LibraryDetailView({ item }) {
  const [collectionCards, setCollectionCards] = useState(fallbackCollectionCards);
  const [detail, setDetail] = useState(item);
  const heading = detail?.type === 'artist' ? `${detail.title} Collection` : detail?.title || 'Your Playlists & Albums';
  const caption = detail?.description || 'Manage your collection and discovered sounds.';

  useEffect(() => {
    let isMounted = true;

    async function loadDetail() {
      try {
        const normalizedType = item?.type?.toLowerCase?.();

        if (normalizedType === 'playlist') {
          const playlist = await getPlaylistById(item.id);
          if (isMounted) {
            setDetail({ ...item, ...playlist, image: normalizeAssetUrl(playlist.coverImgUrl) || defaultCoverUrl });
          }
        } else if (normalizedType === 'album') {
          const album = await getAlbumById(item.id);
          if (isMounted) {
            setDetail({ ...item, ...album, image: normalizeAssetUrl(album.coverImageUrl) || defaultCoverUrl });
          }
        } else if (normalizedType === 'artist') {
          const mediaItems = await getArtistMedia(item.id).catch(() => []);
          if (isMounted && mediaItems.length > 0) {
            setCollectionCards(mediaItems.map((media) => ({
              title: media.title,
              subtitle: `${media.type || 'Media'} • ${media.genre || 'TuneVault'}`,
              image: mediaPosterUrl(media.id) || normalizeAssetUrl(media.coverImageUrl) || defaultCoverUrl,
            })));
          }
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

        if (mappedCollections.length > 0 && normalizedType !== 'artist') {
          setCollectionCards(mappedCollections.slice(0, 8));
        }
      } catch {
        if (!isMounted) return;
        setCollectionCards(fallbackCollectionCards);
      }
    }
    loadDetail();

    return () => {
      isMounted = false;
    };
  }, [item]);

  return (
    <div className="library-detail-view">
      <div className="library-detail-subhead">
        <div className="library-detail-chips">
          <button className="active" type="button">Music</button>
          <button type="button">Podcasts</button>
          <button type="button">Audiobooks</button>
        </div>
        <button className="library-upgrade" type="button">Upgrade</button>
      </div>

      <section className="library-detail-content">
        <div className="library-detail-heading">
          <div>
            <h1>{heading}</h1>
            <p>{caption}</p>
          </div>
          <button className="library-create" type="button">
            <span className="material-symbols-outlined">add</span>
            <span>Create New</span>
          </button>
        </div>

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
      </section>
    </div>
  );
}
