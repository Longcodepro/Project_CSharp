import { useEffect, useRef, useState } from 'react';
import {
  getAlbumReactionCount,
  getArtists,
  getFeaturedAlbums,
  getFeaturedPlaylists,
  getMediaReactionCount,
  getMedia,
  getPlaylistReactionCount,
  getTrendingTracks,
  mediaPosterUrl,
  normalizeAssetUrl,
  searchAll,
} from '../../../Services/MediaService.tsx';
import ManageStudio from './ManageStudio';
import '../../CSS/Home.css';
import LibraryDetailView from './LibraryDetailView';
import ProfileView from './ProfileView';
import NowPlayingView from './NowPlayingView';
import ReactionSummary from './ReactionSummary';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png')
  || 'https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=600&q=80';

const fallbackAvatarSvg = `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(`
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256" role="img" aria-label="Avatar fallback">
    <defs>
      <linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" stop-color="#39444d"/>
        <stop offset="100%" stop-color="#1e242a"/>
      </linearGradient>
    </defs>
    <rect width="256" height="256" rx="128" fill="url(#bg)"/>
    <circle cx="128" cy="103" r="44" fill="#cbd5df"/>
    <path d="M44 214c18-39 49-59 84-59s66 20 84 59" fill="#cbd5df"/>
  </svg>
`)}`;

const defaultArtistUrl = normalizeAssetUrl('/uploads/avatars/Default.png')
  || fallbackAvatarSvg;

const contentTabs = [
  { key: 'all', label: 'Tất cả' },
  { key: 'album', label: 'Album' },
  { key: 'playlist', label: 'Playlist' },
  { key: 'audio', label: 'Audio' },
  { key: 'song', label: 'Song' },
  { key: 'video', label: 'Video' },
];

const sectionLabels = {
  album: 'Album nổi bật',
  playlist: 'Playlist nổi bật',
  audio: 'Audio được tương tác nhiều',
  song: 'Song được tương tác nhiều',
  video: 'Video được tương tác nhiều',
};

const mediaTypeAliases = {
  audio: ['audio'],
  song: ['song'],
  video: ['video'],
};

function normalizeType(value) {
  const numericMap = ['audio', 'video', '', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';

  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;

  return normalized;
}

function sortByReactionCount(items) {
  return [...items].sort((left, right) => {
    const byCount = (right.reactionCount || 0) - (left.reactionCount || 0);
    if (byCount !== 0) return byCount;
    return left.title.localeCompare(right.title, 'vi');
  });
}

async function withReactionCounts(items, getCount, fallbackCount) {
  const results = await Promise.all(items.map(async (item) => {
    try {
      const count = await getCount(item.id);
      return {
        ...item,
        reactionCount: Number(count?.totalCount ?? fallbackCount?.(item) ?? 0),
      };
    } catch {
      return {
        ...item,
        reactionCount: Number(fallbackCount?.(item) ?? 0),
      };
    }
  }));

  return sortByReactionCount(results);
}

function buildMediaItem(item) {
  const mediaType = normalizeType(item.type);
  const coverImageUrl = item.coverImageUrl || item.CoverImageUrl || item.coverImgUrl || item.CoverImgUrl || null;
  const audioUrl = item.audioUrl || item.AudioUrl || null;
  const videoUrl = item.videoUrl || item.VideoUrl || null;

  return {
    id: item.id,
    kind: 'media',
    type: 'media',
    mediaType,
    ownerId: item.ownerId || item.OwnerId,
    ownerName: item.ownerName || item.OwnerName,
    title: item.title || item.Title || 'Không có tiêu đề',
    subtitle: item.genre || item.Genre || item.description || item.Description || 'Media',
    image: mediaPosterUrl(item.id) || normalizeAssetUrl(coverImageUrl) || defaultCoverUrl,
    coverImageUrl,
    audioUrl,
    videoUrl,
    durationSeconds: Number(item.durationSeconds ?? item.DurationSeconds ?? 0),
    reactionCount: Number(item.favoriteCount ?? item.FavoriteCount ?? 0),
  };
}

function buildCollectionItem(item, kind) {
  const coverImageUrl = item.coverImageUrl || item.CoverImageUrl || item.coverImgUrl || item.CoverImgUrl || null;

  return {
    id: item.id,
    kind,
    type: kind,
    title: item.title || item.Title || 'Không có tiêu đề',
    subtitle: kind === 'album' ? 'Album' : 'Playlist',
    image: normalizeAssetUrl(coverImageUrl) || defaultCoverUrl,
    reactionCount: 0,
  };
}

function buildArtistItem(item, index) {
  const id = item.id || item.userId || item.artistId || item.idDisplay || `artist-${index}`;
  const avatarUrl = item.avatarUrl
    || item.AvatarUrl
    || item.avatar
    || item.Avatar
    || item.avatarPath
    || item.AvatarPath
    || item.imageUrl
    || item.ImageUrl
    || item.pictureUrl
    || item.PictureUrl
    || null;
  return {
    id,
    name: item.displayName || item.name || item.idDisplay || item.email || `Artist ${index + 1}`,
    handle: item.idDisplay || item.userName || item.userNameDisplay || item.userName || item.id || id,
    image: normalizeAssetUrl(avatarUrl) || defaultArtistUrl,
  };
}

function normalizeSearchResults(payload) {
  const raw = payload?.data || payload?.Data || payload || {};
  const artists = Array.isArray(raw.artists || raw.Artists) ? (raw.artists || raw.Artists) : [];
  const media = Array.isArray(raw.media || raw.Media) ? (raw.media || raw.Media) : [];
  const playlists = Array.isArray(raw.playlists || raw.Playlists) ? (raw.playlists || raw.Playlists) : [];
  const derivedCount = artists.length + media.length + playlists.length;
  const totalCountValue = raw.totalCount ?? raw.TotalCount ?? derivedCount;
  const totalCount = Number(totalCountValue || 0);

  return {
    artists,
    media,
    playlists,
    totalCount,
  };
}

function normalizeSearchText(value) {
  return String(value ?? '').trim().toLowerCase();
}

function includesQuery(value, query) {
  const normalizedValue = normalizeSearchText(value);
  const normalizedQuery = normalizeSearchText(query);
  if (!normalizedValue || !normalizedQuery) return false;
  return normalizedValue.includes(normalizedQuery);
}

function filterSearchArtists(items, query) {
  return items.filter((item) => (
    includesQuery(item.idDisplay || item.IdDisplay || item.userName || item.UserName || item.id || item.Id, query) ||
    includesQuery(item.displayName || item.DisplayName || item.name || item.Name, query)
  ));
}

function filterSearchMedia(items, query) {
  return items.filter((item) => (
    includesQuery(item.title || item.Title || item.id || item.Id, query) ||
    includesQuery(item.artistName || item.ArtistName || item.ownerName || item.OwnerName, query)
  ));
}

function ContentCard({ item, onPlayMedia, onOpenCollection }) {
  const handleClick = () => {
    if (item.kind === 'media') {
      onPlayMedia?.(item);
      return;
    }

    onOpenCollection?.(item);
  };

  return (
    <article
      className={`content-card ${item.kind === 'playlist' ? 'playlist-card' : ''} ${item.kind === 'media' ? 'clickable' : ''}`}
      onClick={handleClick}
      role="button"
      tabIndex={0}
      onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          handleClick();
        }
      }}
    >
      <div className="content-cover">
        <img
          src={item.image || defaultCoverUrl}
          alt={item.title}
          loading="lazy"
          onError={(event) => {
            event.currentTarget.onerror = null;
            event.currentTarget.src = defaultCoverUrl;
          }}
        />
        <div className="content-play">
          <span className="material-symbols-outlined fill-icon">play_arrow</span>
        </div>
      </div>
      <div className="content-copy">
        <h3 title={item.title}>{item.title}</h3>
        <p title={item.subtitle}>{item.subtitle}</p>
        <ReactionSummary
          totalCount={item.reactionCount || 0}
        />
      </div>
    </article>
  );
}

function ContentSection({ title, items, variant = 'row', onPlayMedia, onOpenCollection }) {
  return (
    <section className={`content-section ${variant === 'grid' ? 'content-section-detail' : ''}`}>
      <div className="content-section-header">
        <h2>{title}</h2>
        <span>{items.length} mục</span>
      </div>
      {items.length > 0 ? (
        <div className={variant === 'grid' ? 'content-grid-detail' : 'content-row-scroll'}>
          {items.map((item) => (
            <ContentCard
              key={`${item.kind}-${item.id}`}
              item={item}
              onPlayMedia={onPlayMedia}
              onOpenCollection={onOpenCollection}
            />
          ))}
        </div>
      ) : (
        <div className="section-empty-state">Mục này hiện chưa có nội dung để hiển thị.</div>
      )}
    </section>
  );
}

function ArtistStrip({ title = 'Nghệ sĩ nổi bật', countLabel = 'artist', artists, onOpenArtistProfile }) {
  const visibleArtists = artists.slice(0, 10);

  return (
    <section className="artist-strip-section">
      <div className="content-section-header">
        <h2>{title}</h2>
        <span>{visibleArtists.length} {countLabel}</span>
      </div>
      {visibleArtists.length > 0 ? (
        <div className="artist-row-scroll">
          {visibleArtists.map((artist) => (
            <button
              className="artist-pill-card"
              key={artist.id}
              type="button"
              onClick={() => onOpenArtistProfile?.(artist)}
            >
              <img
                src={artist.image || defaultArtistUrl}
                alt={artist.name}
                loading="lazy"
                onError={(event) => {
                  event.currentTarget.onerror = null;
                  event.currentTarget.src = fallbackAvatarSvg;
                }}
              />
              <p title={artist.name}>{artist.name}</p>
            </button>
          ))}
        </div>
      ) : (
        <div className="section-empty-state">Chưa có artist đang hoạt động trong database.</div>
      )}
    </section>
  );
}

export default function Home({
  audioRef = null,
  activePanel = null,
  onTogglePanel,
  isNowPlayingExpanded = false,
  selectedLibraryItem = null,
  onHomeClick,
  track,
  bodyMode = 'home',
  onBackToHome,
  isAuthenticated = false,
  onRequireAuth,
  onRequestSignup,
  onLogout,
  isRightPanelOpen = false,
  onPlayMedia,
  onOpenCollection,
  onOpenArtistProfile,
  onOpenProfile,
  onOpenChangePassword,
  profileTarget = null,
  currentUserAvatarUrl = null,
  onProfileDirtyChange,
  onProfileSaved,
  onManageDirtyChange,
  onNavBack,
  onNavForward,
  canNavBack = false,
  canNavForward = false,
  isPlaying = false,
  onTogglePlay,
  onPlayNext,
  onPlayPrevious,
  isNextDisabled = false,
  isPreviousDisabled = false,
  onSeek,
  manageInitialTab = null,
  manageInitialEntityId = null,
  onClearManageInit = null,
}) {
  const [activeContentTab, setActiveContentTab] = useState('all');
  const [contentSections, setContentSections] = useState({
    album: [],
    playlist: [],
    audio: [],
    song: [],
    video: [],
  });
  const [artists, setArtists] = useState([]);
  const [contentLoading, setContentLoading] = useState(true);
  const [contentError, setContentError] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState(null);
  const [searchLoading, setSearchLoading] = useState(false);
  const searchRequestIdRef = useRef(0);
  const searchTimerRef = useRef(null);

  const handleSearchChange = (event) => {
    const value = event.target.value;
    setSearchQuery(value);
  };

  const defaultAvatarUrl = normalizeAssetUrl(currentUserAvatarUrl)
    || normalizeAssetUrl('/uploads/avatars/Default.png')
    || fallbackAvatarSvg;

  useEffect(() => {
    let isMounted = true;

    async function loadHomeData() {
      setContentLoading(true);
      setContentError('');

      try {
        const [mediaItems, trendingItems, playlists, albums, artistItems] = await Promise.all([
          getMedia(1, 80).catch(() => []),
          getTrendingTracks(40).catch(() => []),
          getFeaturedPlaylists(50).catch(() => []),
          getFeaturedAlbums(50).catch(() => []),
          getArtists().catch(() => []),
        ]);

        if (!isMounted) return;

        const mergedMediaById = new Map();
        [...trendingItems, ...mediaItems].filter(Boolean).forEach((item) => {
          if (item?.id) mergedMediaById.set(item.id, item);
        });

        const mediaWithCounts = await withReactionCounts(
          Array.from(mergedMediaById.values()).map(buildMediaItem),
          getMediaReactionCount,
          (item) => item.reactionCount,
        );
        const albumWithCounts = await withReactionCounts(
          albums.filter(Boolean).map((item) => buildCollectionItem(item, 'album')),
          getAlbumReactionCount,
        );
        const playlistWithCounts = await withReactionCounts(
          playlists.filter(Boolean).map((item) => buildCollectionItem(item, 'playlist')),
          getPlaylistReactionCount,
        );

        const nextSections = {
          album: albumWithCounts,
          playlist: playlistWithCounts,
          audio: mediaWithCounts.filter((item) => mediaTypeAliases.audio.includes(item.mediaType)),
          song: mediaWithCounts.filter((item) => mediaTypeAliases.song.includes(item.mediaType)),
          video: mediaWithCounts.filter((item) => mediaTypeAliases.video.includes(item.mediaType)),
        };

        setContentSections(nextSections);
        setArtists(artistItems.filter(Boolean).map(buildArtistItem).slice(0, 10));
        setContentLoading(false);
      } catch {
        if (!isMounted) return;
        setContentSections({
          album: [],
          playlist: [],
          audio: [],
          song: [],
          video: [],
        });
        setArtists([]);
        setContentError('Không tải được nội dung. Vui lòng kiểm tra backend API.');
        setContentLoading(false);
      }
    }

    loadHomeData();

    return () => {
      isMounted = false;
    };
  }, []);

  useEffect(() => {
    const trimmedQuery = searchQuery.trim();

    if (searchTimerRef.current) {
      window.clearTimeout(searchTimerRef.current);
      searchTimerRef.current = null;
    }

    if (trimmedQuery.length < 2) {
      searchRequestIdRef.current += 1;
      setSearchResults(null);
      setSearchLoading(false);
      return undefined;
    }

    const requestId = ++searchRequestIdRef.current;
    setSearchLoading(true);
    searchTimerRef.current = window.setTimeout(async () => {
      try {
        const res = await searchAll(trimmedQuery, 1, 50);
        if (requestId !== searchRequestIdRef.current) return;
        setSearchResults(normalizeSearchResults(res));
      } catch (err) {
        if (requestId !== searchRequestIdRef.current) return;
        console.error('[TuneVault] Search error:', err);
        setSearchResults({ artists: [], media: [], playlists: [], totalCount: 0 });
      } finally {
        if (requestId === searchRequestIdRef.current) {
          setSearchLoading(false);
        }
      }
    }, 250);

    return () => {
      if (searchTimerRef.current) {
        window.clearTimeout(searchTimerRef.current);
        searchTimerRef.current = null;
      }
    };
  }, [searchQuery]);

  const detailItems = contentSections[activeContentTab] || [];

  const normalizedQuery = normalizeSearchText(searchQuery);
  const normalizedSearchResults = normalizeSearchResults(searchResults);
  const filteredSearchArtists = normalizedQuery ? filterSearchArtists(normalizedSearchResults.artists, normalizedQuery) : [];
  const filteredSearchMedia = normalizedQuery ? filterSearchMedia(normalizedSearchResults.media, normalizedQuery) : [];
  const searchArtistsMapped = filteredSearchArtists
    ? filteredSearchArtists.map((artist, idx) => buildArtistItem(artist, idx))
    : [];
  const searchMediaMapped = filteredSearchMedia
    ? filteredSearchMedia.map((item) => buildMediaItem(item))
    : [];
  const filteredTotalCount = searchArtistsMapped.length + searchMediaMapped.length;

  return (
    <main className="home-panel">
      <div className="background-glow">
        <div></div>
      </div>

      <header className={`home-header${isRightPanelOpen ? ' panel-open' : ''}`}>
        <div className="header-left">
          <div className="nav-controls">
            <button
              type="button"
              aria-label="Quay lại"
              onClick={onNavBack}
              className={!canNavBack ? 'muted' : ''}
              disabled={!canNavBack}
            >
              <span className="material-symbols-outlined">chevron_left</span>
            </button>
            <button
              type="button"
              aria-label="Đi tới"
              onClick={onNavForward}
              className={!canNavForward ? 'muted' : ''}
              disabled={!canNavForward}
            >
              <span className="material-symbols-outlined">chevron_right</span>
            </button>
          </div>

          <div className="header-center-controls">
            <button className="home-icon-btn" type="button" aria-label="Trang chủ" onClick={onHomeClick}>
              <span className="material-symbols-outlined fill-icon">home</span>
            </button>

            <label className="main-search">
              <span className="material-symbols-outlined">search</span>
              <input
                type="text"
                placeholder="Bạn muốn phát gì?"
                value={searchQuery}
                onChange={handleSearchChange}
              />
            </label>
          </div>
        </div>

        <div className="header-actions">
          <div className="header-action-icons">
            <button
              className={activePanel === 'notifications' ? 'active' : ''}
              type="button"
              aria-label="Thông báo"
              aria-pressed={activePanel === 'notifications'}
              onClick={() => onTogglePanel?.('notifications')}
            >
              <span className="material-symbols-outlined">notifications</span>
            </button>
            <button
              className={activePanel === 'history' ? 'active' : ''}
              type="button"
              aria-label="Lịch sử nghe"
              aria-pressed={activePanel === 'history'}
              onClick={() => onTogglePanel?.('history')}
            >
              <span className="material-symbols-outlined">history</span>
            </button>
            <button
              className={activePanel === 'shares' ? 'active' : ''}
              type="button"
              aria-label="Chia sẻ"
              aria-pressed={activePanel === 'shares'}
              onClick={() => onTogglePanel?.('shares')}
            >
              <span className="material-symbols-outlined">ios_share</span>
            </button>
            <button
              className={activePanel === 'friends' ? 'active' : ''}
              type="button"
              aria-label="Bạn bè"
              aria-pressed={activePanel === 'friends'}
              onClick={() => onTogglePanel?.('friends')}
            >
              <span className="material-symbols-outlined">group</span>
            </button>
          </div>

          <div className="profile-menu">
            {isAuthenticated ? (
              <button className="profile-avatar" type="button" aria-label="Tài khoản" onClick={() => onOpenProfile?.()}>
                <img
                  src={defaultAvatarUrl}
                  alt="Profile"
                  onError={(event) => {
                    event.currentTarget.onerror = null;
                    event.currentTarget.src = fallbackAvatarSvg;
                  }}
                />
              </button>
            ) : (
              <button
                className="profile-login-button"
                type="button"
                onClick={() => onRequireAuth?.('Đăng nhập để tiếp tục.')}
              >
                Đăng nhập
              </button>
            )}
            <div className="profile-dropdown">
              {isAuthenticated ? (
                <>
                  <button type="button" onClick={() => onOpenProfile?.()}>
                    <span className="material-symbols-outlined">person</span>
                    <span>Hồ sơ</span>
                  </button>
                  <button type="button" onClick={() => onOpenChangePassword?.()}>
                    <span className="material-symbols-outlined">settings</span>
                    <span>Đổi mật khẩu</span>
                  </button>
                  <div></div>
                  <button type="button" onClick={onLogout}>
                    <span className="material-symbols-outlined">logout</span>
                    <span>Đăng xuất</span>
                  </button>
                </>
              ) : (
                <>
                  <button type="button" onClick={() => onRequireAuth?.('Đăng nhập để tiếp tục.')}>
                    <span className="material-symbols-outlined">login</span>
                    <span>Đăng nhập</span>
                  </button>
                  <button type="button" onClick={() => onRequestSignup?.()}>
                    <span className="material-symbols-outlined">person_add</span>
                    <span>Đăng ký</span>
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      </header>

      {selectedLibraryItem ? (
        <LibraryDetailView key={selectedLibraryItem?.id || selectedLibraryItem?.type || 'library-detail'} item={selectedLibraryItem} />
      ) : bodyMode === 'profile' ? (
        <ProfileView
          onBackToHome={onBackToHome || onHomeClick}
          currentUserAvatarUrl={currentUserAvatarUrl}
          profileTarget={profileTarget}
          isAuthenticated={isAuthenticated}
          onPlayMedia={onPlayMedia}
          onOpenCollection={onOpenCollection}
          onDirtyChange={onProfileDirtyChange}
          onProfileSaved={onProfileSaved}
        />
      ) : bodyMode === 'manage' ? (
        <ManageStudio
          onBackToHome={onBackToHome || onHomeClick}
          isAuthenticated={isAuthenticated}
          onRequireAuth={onRequireAuth}
          onDirtyChange={onManageDirtyChange}
          initialTab={manageInitialTab}
          initialEntityId={manageInitialEntityId}
          onClearInit={onClearManageInit}
        />
      ) : isNowPlayingExpanded ? (
        <NowPlayingView
          track={track}
          isPlaying={isPlaying}
          onTogglePlay={onTogglePlay}
          onPlayNext={onPlayNext}
          onPlayPrevious={onPlayPrevious}
          isNextDisabled={isNextDisabled}
          isPreviousDisabled={isPreviousDisabled}
          onSeek={onSeek}
          audioRef={audioRef}
        />
      ) : (
        <>
          <div className="home-content">
            {searchQuery.trim() ? (
              <>
                <section className="content-hero">
                  <h1>Kết quả tìm kiếm cho "{searchQuery.trim()}"</h1>
                  {searchLoading ? (
                    <span>Đang tìm kiếm...</span>
                  ) : (
                    <span>Tìm thấy {filteredTotalCount || 0} kết quả.</span>
                  )}
                </section>

                {searchLoading ? (
                  <div className="content-state">Đang tải kết quả tìm kiếm...</div>
                ) : !searchResults || filteredTotalCount === 0 ? (
                  <div className="section-empty-state">Không tìm thấy kết quả phù hợp cho từ khóa này.</div>
                ) : (
                  <>
                    {searchArtistsMapped.length > 0 && (
                      <ArtistStrip
                        title="Người dùng"
                        countLabel="user"
                        artists={searchArtistsMapped}
                        onOpenArtistProfile={onOpenArtistProfile}
                      />
                    )}

                    {searchMediaMapped.length > 0 && (
                      <ContentSection
                        title="Bài hát"
                        items={searchMediaMapped}
                        variant="grid"
                        onPlayMedia={onPlayMedia}
                        onOpenCollection={onOpenCollection}
                      />
                    )}
                  </>
                )}
              </>
            ) : (
              <>
                <section className="content-hero">
                  <h1>Nội dung nổi bật</h1>
                  <span>Xếp theo lượt thích.</span>
                </section>

                <div className="filter-chips content-tabs" role="tablist" aria-label="Lọc nội dung">
                  {contentTabs.map((tab) => (
                    <button
                      className={activeContentTab === tab.key ? 'active' : ''}
                      type="button"
                      role="tab"
                      aria-selected={activeContentTab === tab.key}
                      key={tab.key}
                      onClick={() => setActiveContentTab(tab.key)}
                    >
                      {tab.label}
                    </button>
                  ))}
                </div>

                {contentLoading ? (
                  <div className="content-state">Đang tải nội dung...</div>
                ) : contentError ? (
                  <div className="content-state content-state-error">{contentError}</div>
                ) : activeContentTab === 'all' ? (
                  <>
                    {['album', 'playlist', 'audio', 'song', 'video'].map((sectionKey) => (
                      <ContentSection
                        key={sectionKey}
                        title={sectionLabels[sectionKey]}
                        items={(contentSections[sectionKey] || []).slice(0, 10)}
                        onPlayMedia={onPlayMedia}
                        onOpenCollection={onOpenCollection}
                      />
                    ))}
                    <ArtistStrip artists={artists} onOpenArtistProfile={onOpenArtistProfile} />
                  </>
                ) : (
                  <>
                    <ContentSection
                      title={sectionLabels[activeContentTab] || 'Nội dung'}
                      items={detailItems.slice(0, 15)}
                      variant="grid"
                      onPlayMedia={onPlayMedia}
                      onOpenCollection={onOpenCollection}
                    />
                    <ArtistStrip artists={artists} onOpenArtistProfile={onOpenArtistProfile} />
                  </>
                )}
              </>
            )}
          </div>

          <div className="home-bottom-space"></div>
        </>
      )}
    </main>
  );
}
