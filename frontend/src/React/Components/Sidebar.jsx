import { useEffect, useState } from 'react';
import {
  getFeaturedPlaylists,
  getLikedSongs,
  getMyAlbums,
  getMyMedia,
  getMyPlaylists,
  getRecentCollectionLikes,
  mediaPosterUrl,
  normalizeAssetUrl,
} from '../../../Services/MediaService.tsx';
import '../../CSS/Sidebar.css';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png')
  || 'https://images.unsplash.com/photo-1511379938547-c1f69419868d?auto=format&fit=crop&w=600&q=80';

function normalizeType(value) {
  const numericMap = ['audio', 'video', 'podcast', 'song'];
  if (typeof value === 'number') return numericMap[value] || 'media';

  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;

  return normalized || 'media';
}

function normalizeMediaItem(item, fallbackSubtitle) {
  const id = item.id || item.mediaItemId || item.title;
  const coverUrl = normalizeAssetUrl(item.coverImageUrl || item.coverImgUrl || item.imageUrl || item.avatarUrl);
  return {
    id,
    title: item.title || item.name || 'Nội dung',
    subtitle: item.subtitle || fallbackSubtitle,
    image: (id ? mediaPosterUrl(id) : undefined) || coverUrl || item.image || defaultCoverUrl,
    type: normalizeType(item.type),
    description: item.description,
  };
}

function getStoredRoles() {
  try {
    const roles = JSON.parse(localStorage.getItem('user_roles') || '[]');
    return Array.isArray(roles) ? roles.map((role) => String(role).toLowerCase()) : [];
  } catch {
    return [];
  }
}

export default function Sidebar({ activeItemId, onSelectItem, onAddCreate }) {
  const [likedItems, setLikedItems] = useState([]);
  const [savedCollections, setSavedCollections] = useState([]);
  const [createdMedia, setCreatedMedia] = useState([]);
  const [createdCollections, setCreatedCollections] = useState([]);

  useEffect(() => {
    let isMounted = true;

    async function loadLibrary() {
      try {
        const hasToken = Boolean(localStorage.getItem('auth_session'));
        const roles = getStoredRoles();
        const canLoadAlbums = roles.includes('artist') || roles.includes('admin');
        const [favoriteMedia, recentLikes, myMedia, featuredPlaylists, myPlaylists, myAlbums] = await Promise.all([
          hasToken ? getLikedSongs().catch(() => []) : Promise.resolve([]),
          hasToken ? getRecentCollectionLikes(4).catch(() => []) : Promise.resolve([]),
          hasToken ? getMyMedia().catch(() => []) : Promise.resolve([]),
          getFeaturedPlaylists(4).catch(() => []),
          hasToken ? getMyPlaylists().catch(() => []) : Promise.resolve([]),
          hasToken && canLoadAlbums ? getMyAlbums().catch(() => []) : Promise.resolve([]),
        ]);

        if (!isMounted) return;

        setLikedItems(favoriteMedia.slice(0, 4).map((item) => normalizeMediaItem(item, 'Đã thích')));

        const mappedRecentLikes = recentLikes.slice(0, 4).map((item) => ({
            id: item.targetId || item.id,
            title: item.title || item.name || 'Nội dung đã lưu',
            subtitle: String(item.targetType || item.type || 'collection').toLowerCase() === 'album'
              ? 'Album đã lưu'
              : 'Playlist đã lưu',
            image: normalizeAssetUrl(item.coverImageUrl || item.coverImgUrl || item.imageUrl) || defaultCoverUrl,
            type: normalizeType(item.targetType || item.type || 'collection'),
            description: item.description,
          }));
        const mappedFeaturedPlaylists = featuredPlaylists.slice(0, 4).map((playlist) => ({
            id: playlist.id,
            title: playlist.title,
            subtitle: `Playlist đã lưu • ${playlist.tracks?.length ?? 0} bài hát`,
            image: normalizeAssetUrl(playlist.coverImgUrl) || defaultCoverUrl,
            type: 'playlist',
            description: playlist.description,
          }));
        setSavedCollections(mappedRecentLikes.length > 0 ? mappedRecentLikes : mappedFeaturedPlaylists);

        const mediaCards = myMedia.slice(0, 4).map((item) => ({
          id: item.id,
          title: item.title,
          subtitle: normalizeType(item.type) === 'video' ? 'Video đã tạo' : 'Bài hát đã tạo',
          image: mediaPosterUrl(item.id) || normalizeAssetUrl(item.coverImageUrl || item.coverImgUrl) || defaultCoverUrl,
          type: 'media',
          description: item.description,
        }));

        const collectionCards = [
          ...myPlaylists.map((playlist) => ({
            id: playlist.id,
            title: playlist.title,
            subtitle: `Playlist đã tạo • ${playlist.tracks?.length ?? 0} bài hát`,
            image: normalizeAssetUrl(playlist.coverImgUrl) || defaultCoverUrl,
            type: 'playlist',
            description: playlist.description,
          })),
          ...myAlbums.map((album) => ({
            id: album.id,
            title: album.title,
            subtitle: `Album đã tạo • ${album.tracks?.length ?? 0} bài hát`,
            image: normalizeAssetUrl(album.coverImageUrl) || defaultCoverUrl,
            type: 'album',
            description: album.description,
          })),
        ].slice(0, 4);

        setCreatedMedia(mediaCards);
        setCreatedCollections(collectionCards);
      } catch {
        if (!isMounted) return;
        setLikedItems([]);
        setSavedCollections([]);
        setCreatedMedia([]);
        setCreatedCollections([]);
      }
    }

    loadLibrary();

    return () => {
      isMounted = false;
    };
  }, []);

  const renderItemList = (items, iconName, emptyMessage) => (
    <div className="sidebar-mini-group-body">
      {items.length > 0 ? (
        <div className="sidebar-item-list sidebar-item-list-scroll">
          {items.map((item) => (
            <button
              className={`sidebar-library-item ${activeItemId === item.id ? 'active' : ''}`}
              key={item.id}
              type="button"
              onClick={() => onSelectItem?.(item)}
            >
              <div className="sidebar-thumb">
                {item.image ? (
                  <img
                    src={item.image}
                    alt={item.title}
                    onError={(event) => {
                      event.currentTarget.onerror = null;
                      event.currentTarget.src = defaultCoverUrl;
                    }}
                  />
                ) : (
                  <span className="material-symbols-outlined">{iconName}</span>
                )}
              </div>
              <div>
                <p>{item.title}</p>
                <span>{item.subtitle}</span>
              </div>
            </button>
          ))}
        </div>
      ) : (
        <div className="sidebar-empty-state">{emptyMessage}</div>
      )}
    </div>
  );

  return (
    <aside className="sidebar">
      <div className="sidebar-heading">
        <div>
          <span className="material-symbols-outlined">library_music</span>
          <h1>Nội dung của tôi</h1>
        </div>
      </div>

      <div className="sidebar-stack">
        <section className="sidebar-section">
          <h2>Nội dung đã lưu</h2>
          <div className="sidebar-mini-group">
            <div className="sidebar-mini-group-header">
              <span>Video đã thích</span>
              <small>{likedItems.length}</small>
            </div>
            {renderItemList(likedItems, 'video_library', 'Chưa có video hoặc bài hát đã thích.')}
          </div>

          <div className="sidebar-mini-group">
            <div className="sidebar-mini-group-header">
              <span>Album / playlist đã lưu</span>
              <small>{savedCollections.length}</small>
            </div>
            {renderItemList(savedCollections, 'queue_music', 'Chưa có album hoặc playlist đã lưu.')}
          </div>
        </section>

        <section className="sidebar-section">
          <h2>Nội dung của tôi</h2>
          <div className="sidebar-mini-group">
            <div className="sidebar-mini-group-header">
              <span>Bài hát / video đã tạo</span>
              <small>{createdMedia.length}</small>
            </div>
            {renderItemList(createdMedia, 'library_music', 'Chưa có bài hát hoặc video nào được tạo.')}
          </div>

          <div className="sidebar-mini-group">
            <div className="sidebar-mini-group-header">
              <span>Playlist / album đã tạo</span>
              <small>{createdCollections.length}</small>
            </div>
            {renderItemList(createdCollections, 'queue_music', 'Chưa có playlist hoặc album nào được tạo.')}
          </div>

          <button
            className="sidebar-create-button"
            type="button"
            onClick={() => onAddCreate?.()}
            title="Thêm album, playlist, video hoặc bài hát"
          >
            <span className="sidebar-create-icon">
              <span className="material-symbols-outlined">add</span>
            </span>
            <div>
              <p>Thêm nội dung</p>
              <span>Album, playlist, video, bài hát</span>
            </div>
          </button>
        </section>
      </div>
    </aside>
  );
}
