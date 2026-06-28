import { useEffect, useLayoutEffect, useRef, useState } from 'react';
import {
  checkFollowStatus,
  followUser,
  getFeaturedAlbums,
  getFeaturedPlaylists,
  getMyAlbums,
  getMyMedia,
  getMyPlaylists,
  getMyProfile,
  getUserById,
  normalizeAssetUrl,
  searchUsers,
  unfollowUser,
  updateProfile,
  getArtistMedia,
} from '../../../Services/MediaService.tsx';
import '../../CSS/Home.css';
import ReactionSummary from './ReactionSummary';

const defaultAvatarUrl = normalizeAssetUrl('/uploads/avatars/Default.png')
  || 'https://i.pravatar.cc/220?u=current-user';

function toDateLabel(value) {
  if (!value) return 'Không xác định';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Không xác định';
  return date.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

function normalizeUserProfile(profile, fallback = {}) {
  return {
    id: profile?.id || fallback.id || '',
    idDisplay: profile?.idDisplay || fallback.idDisplay || '',
    displayName: profile?.displayName || fallback.displayName || 'Tên hiển thị',
    email: profile?.email || fallback.email || '',
    avatarUrl: normalizeAssetUrl(profile?.avatarUrl || fallback.avatarUrl) || '',
    bio: profile?.bio || fallback.bio || '',
    role: profile?.role || fallback.role || '',
    totalFollowers: Number(profile?.totalFollowers ?? fallback.totalFollowers ?? 0),
    followingCount: Number(profile?.followingCount ?? fallback.followingCount ?? 0),
    createdAt: profile?.createdAt || fallback.createdAt || null,
    isActive: profile?.isActive ?? fallback.isActive ?? true,
  };
}

function buildSnapshot(profile) {
  return {
    displayName: profile?.displayName || '',
    idDisplay: profile?.idDisplay || '',
    bio: profile?.bio || '',
    avatarUrl: normalizeAssetUrl(profile?.avatarUrl) || '',
  };
}

function pickArtistMatch(items, target) {
  if (!Array.isArray(items) || items.length === 0) return null;

  const candidates = [
    target?.id,
    target?.idDisplay,
    target?.handle,
    target?.displayName,
    target?.name,
  ]
    .filter(Boolean)
    .map((value) => String(value).trim().toLowerCase());

  return items.find((item) => {
    const values = [
      item?.id,
      item?.userId,
      item?.artistId,
      item?.idDisplay,
      item?.userName,
      item?.displayName,
      item?.name,
    ]
      .filter(Boolean)
      .map((value) => String(value).trim().toLowerCase());

    return values.some((value) => candidates.includes(value));
  }) || items[0];
}

function buildMediaCard(item, fallbackTitle) {
  const kind = String(item?.type || item?.mediaType || '').trim().toLowerCase();
  const isVideo = kind === 'video';
  return {
    id: item?.id || item?.mediaItemId || `${fallbackTitle}-${Math.random().toString(36).slice(2, 8)}`,
    title: item?.title || fallbackTitle,
    subtitle: isVideo ? 'Video' : 'Bài hát',
    image: normalizeAssetUrl(item?.posterUrl || item?.thumbnailUrl || item?.coverImageUrl || item?.CoverImageUrl || item?.image) || defaultAvatarUrl,
    kind: 'media',
    mediaType: isVideo ? 'video' : 'audio',
    reactionCount: Number(item?.favoriteCount ?? item?.FavoriteCount ?? item?.reactionCount ?? 0),
    raw: item,
  };
}

function buildCollectionCard(item, typeLabel) {
  return {
    id: item?.id || `${typeLabel}-${Math.random().toString(36).slice(2, 8)}`,
    title: item?.title || typeLabel,
    subtitle: `${typeLabel} • ${(item?.tracks?.length ?? 0)} mục`,
    image: normalizeAssetUrl(item?.coverImgUrl || item?.coverImageUrl) || defaultAvatarUrl,
    kind: typeLabel.toLowerCase().includes('playlist') ? 'playlist' : 'album',
    raw: item,
  };
}

export default function ProfileView({
  onBackToHome,
  currentUserAvatarUrl,
  profileTarget,
  isAuthenticated = false,
  onPlayMedia,
  onOpenCollection,
  onDirtyChange,
  onProfileSaved,
}) {
  const isOwnProfile = !profileTarget;
  const [profile, setProfile] = useState(null);
  const [displayNameDraft, setDisplayNameDraft] = useState('');
  const [idDisplayDraft, setIdDisplayDraft] = useState('');
  const [bioDraft, setBioDraft] = useState('');
  const [avatarFile, setAvatarFile] = useState(null);
  const [avatarPreview, setAvatarPreview] = useState(normalizeAssetUrl(currentUserAvatarUrl) || defaultAvatarUrl);
  const [avatarSelectionMode, setAvatarSelectionMode] = useState(null);
  const [avatarMenuOpen, setAvatarMenuOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [followLoading, setFollowLoading] = useState(false);
  const [isFollowing, setIsFollowing] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [showConfirm, setShowConfirm] = useState(false);
  const [profileShelves, setProfileShelves] = useState({
    songs: [],
    videos: [],
    albums: [],
    playlists: [],
  });
  const [initialSnapshot, setInitialSnapshot] = useState({
    displayName: '',
    idDisplay: '',
    bio: '',
    avatarUrl: '',
  });
  const objectUrlRef = useRef(null);
  const avatarInputRef = useRef(null);
  const displayNameInputRef = useRef(null);
  const idDisplayInputRef = useRef(null);
  const avatarRemovalRequested = avatarSelectionMode === 'default'
    && (initialSnapshot.avatarUrl || '') !== defaultAvatarUrl;
  const dirty = isOwnProfile
    && !loading
    && (
      displayNameDraft.trim() !== initialSnapshot.displayName.trim()
      || idDisplayDraft.trim().toLowerCase() !== initialSnapshot.idDisplay.trim().toLowerCase()
      || bioDraft !== initialSnapshot.bio
      || Boolean(avatarFile)
      || avatarRemovalRequested
    );

  useEffect(() => {
    onDirtyChange?.(isOwnProfile ? dirty : false);
  }, [dirty, isOwnProfile, onDirtyChange]);

  useLayoutEffect(() => {
    const scrollRoot = document.querySelector('.home-panel');
    if (scrollRoot && typeof scrollRoot.scrollTo === 'function') {
      scrollRoot.scrollTo({ top: 0, left: 0, behavior: 'auto' });
    }

    window.scrollTo(0, 0);
  }, [isOwnProfile, profileTarget?.id, profileTarget?.idDisplay, profileTarget?.handle, profileTarget?.artistId, profileTarget?.userId]);

  useEffect(() => {
    let isMounted = true;

    async function loadProfile() {
      setLoading(true);
      setError('');
      setMessage('');
      setShowConfirm(false);
      setFollowLoading(false);
      setIsFollowing(false);

      try {
        if (isOwnProfile) {
          if (isMounted) {
            setProfileShelves({ songs: [], videos: [], albums: [], playlists: [] });
          }

          const [result, myMedia, myAlbums, myPlaylists] = await Promise.all([
            getMyProfile(),
            getMyMedia().catch(() => []),
            getMyAlbums().catch(() => []),
            getMyPlaylists().catch(() => []),
          ]);
          if (!isMounted) return;

          const nextProfile = normalizeUserProfile(result, {
            avatarUrl: currentUserAvatarUrl,
          });

          setProfile(nextProfile);
          setDisplayNameDraft(nextProfile.displayName || '');
          setIdDisplayDraft(nextProfile.idDisplay || '');
          setBioDraft(nextProfile.bio || '');
          setAvatarFile(null);
          setAvatarSelectionMode(null);
          setAvatarMenuOpen(false);
          setAvatarPreview(nextProfile.avatarUrl || normalizeAssetUrl(currentUserAvatarUrl) || defaultAvatarUrl);
          setInitialSnapshot(buildSnapshot(nextProfile));
          const ownMedia = Array.isArray(myMedia) ? myMedia : [];
          const audioCards = ownMedia.filter((item) => String(item?.type || item?.mediaType || '').toLowerCase() !== 'video')
            .map((item) => buildMediaCard(item, 'Bài hát đã tạo'));
          const videoCards = ownMedia.filter((item) => String(item?.type || item?.mediaType || '').toLowerCase() === 'video')
            .map((item) => buildMediaCard(item, 'Video đã tạo'));
          setProfileShelves({
            songs: audioCards,
            videos: videoCards,
            albums: Array.isArray(myAlbums) ? myAlbums.map((album) => buildCollectionCard(album, 'Album đã tạo')) : [],
            playlists: Array.isArray(myPlaylists) ? myPlaylists.map((playlist) => buildCollectionCard(playlist, 'Playlist đã tạo')) : [],
          });
          return;
        }

        let resolvedId = profileTarget?.id || profileTarget?.userId || profileTarget?.artistId || '';
        let matchedArtist = null;
        const keyword = String(profileTarget?.idDisplay || profileTarget?.handle || profileTarget?.displayName || profileTarget?.name || '').trim();

        if (!resolvedId && keyword) {
          const searchResults = await searchUsers(keyword, 1, 10).catch(() => []);
          matchedArtist = pickArtistMatch(searchResults, profileTarget);
          resolvedId = matchedArtist?.id || '';
        }

        const publicDetail = resolvedId ? await getUserById(resolvedId).catch(() => null) : null;
        const [createdMedia, allAlbums, allPlaylists] = await Promise.all([
          resolvedId ? getArtistMedia(resolvedId).catch(() => []) : Promise.resolve([]),
          getFeaturedAlbums(50).catch(() => []),
          getFeaturedPlaylists(50).catch(() => []),
        ]);
        const followersCount = publicDetail?.totalFollowers ?? matchedArtist?.totalFollowers ?? 0;
        const normalizedProfile = normalizeUserProfile(publicDetail, {
          id: resolvedId,
          idDisplay: publicDetail?.idDisplay || matchedArtist?.idDisplay || keyword,
          displayName: publicDetail?.displayName || matchedArtist?.displayName || matchedArtist?.name || keyword,
          avatarUrl: publicDetail?.avatarUrl || matchedArtist?.avatarUrl || profileTarget?.image,
          bio: publicDetail?.bio || '',
          role: publicDetail?.role || matchedArtist?.role || 'Artist',
          totalFollowers: followersCount,
          followingCount: publicDetail?.followingCount ?? matchedArtist?.followingCount ?? 0,
          createdAt: publicDetail?.createdAt || null,
          isActive: publicDetail?.isActive ?? matchedArtist?.isActive ?? true,
        });

        if (!isMounted) return;

        setProfile(normalizedProfile);
        setDisplayNameDraft(normalizedProfile.displayName || '');
        setIdDisplayDraft(normalizedProfile.idDisplay || '');
        setBioDraft(normalizedProfile.bio || '');
        setAvatarFile(null);
        setAvatarSelectionMode(null);
        setAvatarMenuOpen(false);
        setAvatarPreview(normalizedProfile.avatarUrl || normalizeAssetUrl(profileTarget?.image) || defaultAvatarUrl);
        setInitialSnapshot(buildSnapshot(normalizedProfile));
        const publicMedia = Array.isArray(createdMedia) ? createdMedia : [];
        const publicAlbums = Array.isArray(allAlbums)
          ? allAlbums.filter((album) => String(album?.artistId || album?.ownerId || '').toLowerCase() === String(resolvedId || '').toLowerCase())
          : [];
        const publicPlaylists = Array.isArray(allPlaylists)
          ? allPlaylists.filter((playlist) => String(playlist?.ownerId || playlist?.artistId || '').toLowerCase() === String(resolvedId || '').toLowerCase())
          : [];
        setProfileShelves({
          songs: publicMedia.filter((item) => String(item?.type || item?.mediaType || '').toLowerCase() !== 'video').map((item) => buildMediaCard(item, 'Bài hát')),
          videos: publicMedia.filter((item) => String(item?.type || item?.mediaType || '').toLowerCase() === 'video').map((item) => buildMediaCard(item, 'Video')),
          albums: publicAlbums.map((album) => buildCollectionCard(album, 'Album')),
          playlists: publicPlaylists.map((playlist) => buildCollectionCard(playlist, 'Playlist')),
        });

        if (resolvedId && (isAuthenticated || localStorage.getItem('auth_session'))) {
          const followStatus = await checkFollowStatus(resolvedId).catch(() => false);
          if (isMounted) setIsFollowing(Boolean(followStatus));
        }
      } catch (loadError) {
        if (!isMounted) return;
        setProfile(null);
        setError(loadError instanceof Error ? loadError.message : 'Không tải được hồ sơ.');
      } finally {
        if (isMounted) setLoading(false);
      }
    }

    void loadProfile();

    return () => {
      isMounted = false;
    };
  }, [currentUserAvatarUrl, isOwnProfile, profileTarget, isAuthenticated]);

  useEffect(() => {
    return () => {
      if (objectUrlRef.current) {
        URL.revokeObjectURL(objectUrlRef.current);
        objectUrlRef.current = null;
      }
    };
  }, []);

  useEffect(() => {
    const handleBeforeUnload = (event) => {
      if (!isOwnProfile || !dirty) return;
      event.preventDefault();
      event.returnValue = '';
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [dirty, isOwnProfile]);

  const triggerAvatarPicker = () => {
    if (!isOwnProfile) return;
    setAvatarMenuOpen(false);
    avatarInputRef.current?.click();
  };

  const selectDefaultAvatar = () => {
    if (!isOwnProfile) return;

    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }

    setAvatarMenuOpen(false);
    setAvatarSelectionMode('default');
    setAvatarFile(null);
    if (avatarInputRef.current) {
      avatarInputRef.current.value = '';
    }
    setAvatarPreview(defaultAvatarUrl);
    setError('');
    setMessage('');
  };

  const handleAvatarChange = (file) => {
    if (!isOwnProfile) return;

    if (objectUrlRef.current) {
      URL.revokeObjectURL(objectUrlRef.current);
      objectUrlRef.current = null;
    }

    if (!file) {
      if (avatarSelectionMode === 'default') {
        if (avatarInputRef.current) {
          avatarInputRef.current.value = '';
        }
        return;
      }

      setAvatarFile(null);
      setAvatarSelectionMode(null);
      setAvatarPreview(normalizeAssetUrl(profile?.avatarUrl) || normalizeAssetUrl(currentUserAvatarUrl) || defaultAvatarUrl);
      return;
    }

    if (!file.type?.startsWith('image/')) {
      setError('Avatar chỉ được chọn file ảnh.');
      setMessage('');
      if (avatarInputRef.current) {
        avatarInputRef.current.value = '';
      }
      if (avatarSelectionMode !== 'default') {
        setAvatarFile(null);
        setAvatarSelectionMode(null);
        setAvatarPreview(normalizeAssetUrl(profile?.avatarUrl) || normalizeAssetUrl(currentUserAvatarUrl) || defaultAvatarUrl);
      }
      return;
    }

    const previewUrl = URL.createObjectURL(file);
    objectUrlRef.current = previewUrl;
    setAvatarFile(file);
    setAvatarSelectionMode('upload');
    setAvatarMenuOpen(false);
    setAvatarPreview(previewUrl);
    setError('');
  };

  const closeConfirm = () => {
    if (!saving) {
      setShowConfirm(false);
    }
  };

  const handleOpenConfirm = () => {
    if (!isOwnProfile || !dirty || saving) return;
    setAvatarMenuOpen(false);
    setMessage('');
    setError('');
    setShowConfirm(true);
  };

  const handleSave = async () => {
    if (!isOwnProfile) return;

    if (!displayNameDraft.trim()) {
      setError('Không tìm thấy tên hiển thị để cập nhật.');
      setShowConfirm(false);
      return;
    }

    if (!idDisplayDraft.trim()) {
      setError('Không tìm thấy idname để cập nhật.');
      setShowConfirm(false);
      return;
    }

    try {
      setSaving(true);
      setError('');
      setMessage('');

      const result = await updateProfile(
        displayNameDraft.trim(),
        idDisplayDraft.trim().toLowerCase(),
        avatarFile,
        bioDraft.trim(),
        avatarSelectionMode === 'default'
      );

      const nextAvatarUrl = normalizeAssetUrl(result?.avatarUrl) || '';
      if (objectUrlRef.current) {
        URL.revokeObjectURL(objectUrlRef.current);
        objectUrlRef.current = null;
      }

      const normalizedProfile = normalizeUserProfile(result, {
        avatarUrl: nextAvatarUrl || defaultAvatarUrl,
      });

      setProfile(normalizedProfile);
      setInitialSnapshot(buildSnapshot(normalizedProfile));
      setDisplayNameDraft(normalizedProfile.displayName || '');
      setIdDisplayDraft(normalizedProfile.idDisplay || '');
      setBioDraft(normalizedProfile.bio || '');
      setAvatarFile(null);
      setAvatarSelectionMode(null);
      setAvatarMenuOpen(false);
      setAvatarPreview(nextAvatarUrl || defaultAvatarUrl);
      if (avatarInputRef.current) {
        avatarInputRef.current.value = '';
      }
      setShowConfirm(false);
      setMessage('Đã cập nhập thông tin cá nhân.');
      onProfileSaved?.(result);
    } catch (saveError) {
      setError(saveError instanceof Error ? saveError.message : 'Không thể lưu hồ sơ.');
    } finally {
      setSaving(false);
    }
  };

  const handleFollowToggle = async () => {
    if (isOwnProfile || !profile?.id || followLoading) return;
    if (!hasAuthToken) {
      setError('Bạn cần đăng nhập để theo dõi nghệ sĩ.');
      return;
    }

    try {
      setFollowLoading(true);
      setError('');

      if (isFollowing) {
        await unfollowUser(profile.id);
        setIsFollowing(false);
      } else {
        await followUser(profile.id);
        setIsFollowing(true);
      }

      const refreshedProfile = await getUserById(profile.id).catch(() => null);
      if (refreshedProfile) {
        setProfile((current) => normalizeUserProfile(refreshedProfile, current || {}));
      }
    } catch (followError) {
      setError(followError instanceof Error ? followError.message : 'Không thể cập nhật trạng thái theo dõi.');
    } finally {
      setFollowLoading(false);
    }
  };

  const displayName = profile?.displayName || 'Tên hiển thị';
  const idDisplay = profile?.idDisplay || profileTarget?.idDisplay || profileTarget?.handle || 'idname';
  const email = profile?.email || (isOwnProfile ? localStorage.getItem('user_email') : '') || 'Chưa cập nhật';
  const bioValue = isOwnProfile ? bioDraft : (profile?.bio || 'Chưa có tiểu sử.');
  const hasAuthToken = Boolean(isAuthenticated || localStorage.getItem('auth_session'));
  const publicProfileLabel = String(profile?.role || profileTarget?.role || '').trim().toLowerCase() === 'artist'
    ? 'Hồ sơ nghệ sĩ'
    : 'Hồ sơ người dùng';
  const pageTitle = isOwnProfile ? 'Chỉnh sửa thông tin tài khoản' : publicProfileLabel;

  const handlePlayMedia = (media) => {
    if (!media) return;
    onPlayMedia?.(media);
  };

  const renderShelf = (title, items, emptyLabel) => (
    <section className="content-section profile-content-section">
      <div className="content-section-header">
        <h2>{title}</h2>
        <span>{items.length} mục</span>
      </div>
      {items.length > 0 ? (
        <div className="content-row-scroll profile-content-row">
          {items.map((item) => (
            <button
              key={item.id}
              type="button"
              className={`content-card profile-content-card ${item.kind === 'playlist' ? 'playlist-card' : ''} ${item.kind === 'media' ? 'clickable' : ''}`}
              onClick={() => {
                if (item.kind === 'media') {
                  handlePlayMedia(item.raw);
                  return;
                }
                onOpenCollection?.(item.raw);
              }}
            >
              <div className="content-cover">
                <img
                  src={item.image}
                  alt={item.title}
                  onError={(event) => {
                    event.currentTarget.onerror = null;
                    event.currentTarget.src = defaultAvatarUrl;
                  }}
                />
                <div className="content-play">
                  <span className="material-symbols-outlined fill-icon">play_arrow</span>
                </div>
              </div>
              <div className="content-copy">
                <h3 title={item.title}>{item.title}</h3>
                <p title={item.subtitle}>{item.subtitle}</p>
                {item.kind === 'media' ? (
                  <ReactionSummary
                    totalCount={item.reactionCount || 0}
                  />
                ) : (
                  <span>{item.mediaType === 'video' ? 'Video' : item.subtitle}</span>
                )}
              </div>
            </button>
          ))}
        </div>
      ) : (
        <div className="section-empty-state">{emptyLabel}</div>
      )}
    </section>
  );

  return (
    <div className="create-studio profile-studio">
      <div className="background-glow create-glow">
        <div></div>
        <div></div>
      </div>

      <div className="create-studio-inner profile-studio-inner">
        <div className="profile-page-header">
          <button
            className="create-back-button profile-back-button"
            type="button"
            onClick={() => onBackToHome?.()}
          >
            <span className="material-symbols-outlined">arrow_back</span>
            <span>Quay lại</span>
          </button>

          <div className="profile-page-header-copy">
            <h1>{pageTitle}</h1>
          </div>
        </div>

        {isOwnProfile ? (
          <>
            <section className="profile-editor-shell">
              <div className="profile-editor-card profile-owner-card">
                <div className="profile-owner-avatar-block">
                  <div className="profile-section-title profile-section-title-center">
                    <span className="profile-section-kicker">Ảnh đại diện</span>
                  </div>

                  <button
                    type="button"
                    className="profile-avatar-editor profile-avatar-large"
                    onClick={triggerAvatarPicker}
                    aria-label="Đổi avatar"
                  >
                    <img
                      src={avatarPreview || defaultAvatarUrl}
                      alt="Avatar"
                      onError={(event) => {
                        event.currentTarget.onerror = null;
                        event.currentTarget.src = defaultAvatarUrl;
                      }}
                    />
                    <span className="profile-avatar-badge">
                      <span className="material-symbols-outlined fill-icon">photo_camera</span>
                    </span>
                  </button>

                  <input
                    ref={avatarInputRef}
                    id="profile-avatar-input"
                    type="file"
                    accept="image/*"
                    hidden
                    onChange={(event) => handleAvatarChange(event.target.files?.[0] || null)}
                  />

                  <div className="profile-avatar-action-group">
                    <button
                      type="button"
                      className="profile-avatar-action profile-avatar-action-trigger"
                      onClick={() => setAvatarMenuOpen((open) => !open)}
                      aria-haspopup="menu"
                      aria-expanded={avatarMenuOpen}
                    >
                      <span>Thay ảnh</span>
                      <span className="material-symbols-outlined fill-icon">expand_more</span>
                    </button>

                    {avatarMenuOpen ? (
                      <div className="profile-avatar-menu" role="menu" aria-label="Tùy chọn ảnh đại diện">
                        <button
                          type="button"
                          className="profile-avatar-menu-item"
                          onClick={selectDefaultAvatar}
                        >
                          Ảnh mặc định
                        </button>
                        <button
                          type="button"
                          className="profile-avatar-menu-item"
                          onClick={triggerAvatarPicker}
                        >
                          Chọn ảnh từ máy
                        </button>
                      </div>
                    ) : null}
                  </div>

                  {avatarSelectionMode === 'default' ? (
                    <p className="profile-file-chip profile-default-avatar-chip">Sẽ dùng ảnh mặc định khi lưu.</p>
                  ) : avatarFile ? (
                    <p className="profile-file-chip">Đã chọn file: {avatarFile.name}</p>
                  ) : null}
                </div>

                <div className="profile-owner-form-block">
                  <div className="profile-section-title">
                    <span className="profile-section-kicker">Thông tin cơ bản</span>
                    <h2>Chỉnh sửa hồ sơ</h2>
                  </div>

                  <label className="profile-form-field">
                    <span className="profile-field-label">Tên hiển thị</span>
                    <input
                      ref={displayNameInputRef}
                      type="text"
                      value={displayNameDraft}
                      placeholder="Tên hiển thị"
                      onChange={(event) => setDisplayNameDraft(event.target.value)}
                    />
                  </label>

                  <label className="profile-form-field">
                    <span className="profile-field-label">ID name / username</span>
                    <input
                      ref={idDisplayInputRef}
                      type="text"
                      value={idDisplayDraft}
                      placeholder="idname"
                      onChange={(event) => setIdDisplayDraft(event.target.value.replace(/\s+/g, '').toLowerCase())}
                    />
                  </label>

                  <label className="profile-form-field">
                    <span className="profile-field-label">Email</span>
                    <input
                      type="text"
                      value={email}
                      placeholder="Chưa có email"
                      readOnly
                      aria-readonly="true"
                    />
                  </label>
                </div>

                <div className="profile-owner-stats-block">
                  <div className="profile-section-title">
                    <span className="profile-section-kicker">Tổng quan</span>
                    <h2>Thống kê tài khoản</h2>
                  </div>

                  <div className="profile-owner-stats-row">
                    <article className="profile-stat-card">
                      <span>Followers</span>
                      <strong>{Number(profile?.totalFollowers || 0)}</strong>
                    </article>
                    <article className="profile-stat-card">
                      <span>Following</span>
                      <strong>{Number(profile?.followingCount || 0)}</strong>
                    </article>
                    <article className="profile-stat-card">
                      <span>Join Date</span>
                      <strong>{toDateLabel(profile?.createdAt)}</strong>
                    </article>
                  </div>
                </div>
              </div>

              <div className="profile-editor-card profile-bio-card">
                <div className="profile-section-title">
                  <span className="profile-section-kicker">Tiểu sử</span>
                  <h2>Giới thiệu bản thân</h2>
                </div>
                <p className="profile-section-caption">
                  Hãy viết 300-500 ký tự để mọi người hiểu hơn về bạn, phong cách âm nhạc và câu chuyện phía sau profile.
                </p>
                <div className="profile-bio-meta">
                  <span>Tối đa 500 ký tự</span>
                  <strong>{bioDraft.length}/500</strong>
                </div>
                <textarea
                  rows="8"
                  maxLength={500}
                  value={bioDraft}
                  placeholder="Viết vài dòng giới thiệu về bạn..."
                  onChange={(event) => setBioDraft(event.target.value)}
                />
                <div className="profile-actions-row">
                  <div className="profile-inline-copy">
                    <span className="profile-inline-copy-title">Lưu thay đổi</span>
                    <p>Mọi chỉnh sửa sẽ được cập nhật cùng lúc.</p>
                  </div>
                  <button
                    type="button"
                    className="create-primary profile-save-button"
                    onClick={handleOpenConfirm}
                    disabled={saving || !dirty}
                  >
                    {saving ? 'Đang lưu...' : 'Cập nhập'}
                  </button>
                </div>
                {message ? <div className="profile-inline-message profile-success">{message}</div> : null}
              </div>
            </section>

            <div className="profile-public-shelves" style={{ marginTop: '2rem' }}>
              {renderShelf('Bài hát của tôi', profileShelves.songs, 'Bạn chưa tải lên bài hát nào.')}
              {renderShelf('Video của tôi', profileShelves.videos, 'Bạn chưa tải lên video nào.')}
              {renderShelf('Album của tôi', profileShelves.albums, 'Bạn chưa tạo album nào.')}
              {renderShelf('Playlist của tôi', profileShelves.playlists, 'Bạn chưa tạo playlist nào.')}
            </div>
          </>
        ) : (
          <section className="profile-public-shell">
            <div className="profile-public-card">
              <div className="profile-public-hero profile-public-top">
                <div className="profile-avatar-editor profile-avatar-static profile-avatar-public">
                  <img
                    src={avatarPreview || defaultAvatarUrl}
                    alt="Avatar"
                    onError={(event) => {
                      event.currentTarget.onerror = null;
                      event.currentTarget.src = defaultAvatarUrl;
                    }}
                  />
                </div>

                <div className="profile-public-heading">
                  <h1>{displayName}</h1>
                  <p className="profile-public-handle">{`_${idDisplay}_`}</p>
                  <p className="profile-public-email">{email}</p>
                </div>

                <div className="profile-public-actions">
                  <button
                    type="button"
                    className={`create-primary profile-follow-button profile-follow-button-public ${isFollowing ? 'profile-following' : ''}`}
                    onClick={handleFollowToggle}
                    disabled={followLoading || !profile?.id || !hasAuthToken}
                  >
                    {followLoading
                      ? 'Đang xử lý...'
                      : !hasAuthToken
                        ? 'Đăng nhập để follow'
                        : isFollowing
                          ? 'Hủy follow'
                          : 'Follow'}
                  </button>
                </div>
              </div>

              <div className="profile-public-stats">
                <article className="profile-stat-card">
                  <span>Followers</span>
                  <strong>{Number(profile?.totalFollowers || 0)}</strong>
                </article>
                <article className="profile-stat-card">
                  <span>Following</span>
                  <strong>{Number(profile?.followingCount || 0)}</strong>
                </article>
                <article className="profile-stat-card">
                  <span>Join Date</span>
                  <strong>{toDateLabel(profile?.createdAt)}</strong>
                </article>
              </div>

              <div className="profile-public-bio">
                <div className="profile-section-title">
                  <span className="profile-section-kicker">Tiểu sử</span>
                  <h2>Giới thiệu</h2>
                </div>
                <p className="profile-bio-content">{bioValue}</p>
              </div>

              <div className="profile-public-shelves">
                {renderShelf('Bài hát', profileShelves.songs, 'Chưa có bài hát công khai.')}
                {renderShelf('Video', profileShelves.videos, 'Chưa có video công khai.')}
                {renderShelf('Album', profileShelves.albums, 'Chưa có album công khai.')}
                {renderShelf('Playlist', profileShelves.playlists, 'Chưa có playlist công khai.')}
              </div>
            </div>
          </section>
        )}

        {loading ? (
          <div className="content-state">Đang tải hồ sơ...</div>
        ) : error ? (
          <div className="content-state content-state-error">{error}</div>
        ) : null}
      </div>

      {isOwnProfile && showConfirm ? (
        <div className="profile-confirm-backdrop" role="presentation" onClick={closeConfirm}>
          <div className="profile-confirm-modal" role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
            <h3>Xác nhận cập nhập</h3>
            <p>Bạn có chắc muốn lưu những thay đổi này không?</p>
            <div className="profile-confirm-actions">
              <button type="button" className="create-secondary" onClick={closeConfirm} disabled={saving}>
                Hủy
              </button>
              <button type="button" className="create-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Đang lưu...' : 'Xác nhận'}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
