import { useEffect, useMemo, useState } from 'react';
import {
  addTrackToAlbum,
  addTrackToPlaylist,
  createAlbum,
  createPlaylist,
  deleteAlbum,
  deleteMedia,
  deletePlaylist,
  getMyAlbums,
  getMyMedia,
  getMyPlaylists,
  getTrackById,
  mediaPosterUrl,
  normalizeAssetUrl,
  reorderTrackInAlbum,
  reorderTrackInPlaylist,
  removeTrackFromAlbum,
  removeTrackFromPlaylist,
  updateAlbum,
  updateMedia,
  updatePlaylist,
  uploadMedia,
} from '../../../Services/MediaService.tsx';
import '../../CSS/Home.css';

const manageTabs = [
  { key: 'song', label: 'Thêm nhạc', kind: 'Song', scope: 'media' },
  { key: 'video', label: 'Video', kind: 'Video', scope: 'media' },
  { key: 'audio', label: 'Audio', kind: 'Podcast', scope: 'media' },
  { key: 'playlist', label: 'Playlist', kind: 'Playlist', scope: 'collection' },
  { key: 'album', label: 'Album', kind: 'Album', scope: 'collection' },
];

const sessionSkipRemoveTrackKey = 'tunevault_skip_remove_track_confirm';

const defaultCoverUrl = normalizeAssetUrl('/uploads/default-cover/Default.png')
  || 'https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=800&q=80';

function normalizeType(value) {
  const numericMap = ['audio', 'video', 'podcast', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';

  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;

  return normalized;
}

function toDateTimeLocal(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function fromDateTimeLocal(value) {
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

function getMediaKindFromTab(tabKey) {
  if (tabKey === 'video') return 'Video';
  if (tabKey === 'audio') return 'Podcast';
  return 'Song';
}

function getMediaRouteKind(tabKey) {
  if (tabKey === 'video') return 'video';
  return 'auto';
}

function getMediaTypeLabel(tabKey) {
  if (tabKey === 'video') return 'Video';
  if (tabKey === 'audio') return 'Podcast';
  return 'Song';
}

function durationToLabel(seconds) {
  const total = Number(seconds || 0);
  if (!Number.isFinite(total) || total <= 0) return '0:00';
  const minutes = Math.floor(total / 60);
  const remain = String(Math.floor(total % 60)).padStart(2, '0');
  return `${minutes}:${remain}`;
}

function isMediaTabItem(tabKey, item) {
  const mediaType = normalizeType(item?.mediaType || item?.type);
  if (tabKey === 'video') return mediaType === 'video';
  if (tabKey === 'audio') return mediaType === 'audio' || mediaType === 'podcast';
  return mediaType === 'song';
}

async function readDurationFromFile(file, mediaType) {
  if (!file) return 0;

  const objectUrl = URL.createObjectURL(file);
  const element = mediaType === 'video' ? document.createElement('video') : document.createElement('audio');

  return new Promise((resolve) => {
    const cleanup = () => {
      URL.revokeObjectURL(objectUrl);
      element.src = '';
    };

    element.preload = 'metadata';
    element.onloadedmetadata = () => {
      const duration = Number.isFinite(element.duration) ? Math.round(element.duration) : 0;
      cleanup();
      resolve(duration);
    };
    element.onerror = () => {
      cleanup();
      resolve(0);
    };

    element.src = objectUrl;
  });
}

function createMediaDraft(tabKey, currentUserName) {
  return {
    tabKey,
    title: '',
    description: '',
    genre: '',
    releaseDate: toDateTimeLocal(new Date()),
    isPublic: true,
    accessLevel: 'Normal',
    coverFile: null,
    canvasFile: null,
    mediaFile: null,
    coverPreview: '',
    mediaPreview: '',
    durationSeconds: 0,
    ownerLabel: currentUserName || 'Tự động theo tài khoản đang đăng nhập',
    typeLabel: getMediaTypeLabel(tabKey),
  };
}

function createCollectionDraft(kind) {
  return {
    kind,
    title: '',
    description: '',
    releaseDate: toDateTimeLocal(new Date()),
    isPublic: true,
    contentType: 'Song',
    coverFile: null,
    coverPreview: '',
    ownerLabel: 'Tự động theo tài khoản đang đăng nhập',
  };
}

function buildMediaDraft(item, tabKey, currentUserName) {
  return {
    ...createMediaDraft(tabKey, currentUserName),
    title: item?.title || item?.Title || '',
    description: item?.description || item?.Description || '',
    genre: item?.genre || item?.Genre || '',
    releaseDate: toDateTimeLocal(item?.releaseDate || item?.ReleaseDate || new Date()),
    isPublic: item?.isPublic ?? item?.IsPublic ?? true,
    accessLevel: item?.accessLevel || item?.AccessLevel || 'Normal',
    coverPreview: mediaPosterUrl(item?.id || item?.Id || '') || normalizeAssetUrl(item?.coverImageUrl || item?.CoverImageUrl) || defaultCoverUrl,
    mediaPreview: normalizeAssetUrl(item?.audioUrl || item?.AudioUrl || item?.videoUrl || item?.VideoUrl) || '',
    durationSeconds: Number(item?.durationSeconds ?? item?.DurationSeconds ?? 0),
    typeLabel: getMediaTypeLabel(tabKey),
  };
}

function buildCollectionDraft(item, kind) {
  return {
    ...createCollectionDraft(kind),
    title: item?.title || item?.Title || '',
    description: item?.description || item?.Description || '',
    releaseDate: toDateTimeLocal(item?.releaseDate || item?.ReleaseDate || new Date()),
    isPublic: item?.isPublic ?? item?.IsPublic ?? true,
    contentType: item?.contentType || item?.ContentType || 'Song',
    coverPreview: normalizeAssetUrl(item?.coverImgUrl || item?.coverImageUrl || item?.CoverImgUrl || item?.CoverImageUrl) || defaultCoverUrl,
  };
}

function entityKey(tabKey, item) {
  return `${tabKey}:${item?.id || item?.Id || 'new'}`;
}

function ItemCard({ item, onOpen }) {
  return (
    <button className="manage-item-card" type="button" onClick={onOpen}>
      <div className="manage-item-cover">
        <img
          src={item.coverPreview || item.coverImageUrl || item.image || defaultCoverUrl}
          alt={item.title}
          onError={(event) => {
            event.currentTarget.onerror = null;
            event.currentTarget.src = defaultCoverUrl;
          }}
        />
      </div>
      <div className="manage-item-copy">
        <strong>{item.title || 'Không có tiêu đề'}</strong>
        <p>{item.subtitle || item.description || item.genre || 'Chưa có mô tả'}</p>
      </div>
    </button>
  );
}

function TrackConfirmModal({ confirm, onCancel, onConfirm, dontAskAgain, setDontAskAgain }) {
  if (!confirm) return null;

  return (
    <div className="manage-modal-backdrop">
      <div className="manage-modal">
        <h3>{confirm.title}</h3>
        <p>{confirm.message}</p>
        {confirm.allowSkip ? (
          <label className="manage-modal-checkbox">
            <input
              type="checkbox"
              checked={dontAskAgain}
              onChange={(event) => setDontAskAgain(event.target.checked)}
            />
            <span>Không nhắc lại trong phiên đăng nhập này</span>
          </label>
        ) : null}
        <div className="manage-modal-actions">
          <button type="button" className="manage-secondary" onClick={onCancel}>Hủy</button>
          <button type="button" className="manage-primary" onClick={onConfirm}>Xác nhận</button>
        </div>
      </div>
    </div>
  );
}

export default function ManageStudio({
  onBackToHome,
  isAuthenticated,
  onRequireAuth,
  onDirtyChange,
  initialTab,
  initialEntityId,
  onClearInit,
}) {
  const [activeTab, setActiveTab] = useState('song');
  const [viewMode, setViewMode] = useState('list');
  const [items, setItems] = useState([]);
  const [mediaPool, setMediaPool] = useState([]);
  const [selectedEntity, setSelectedEntity] = useState(null);
  const [draft, setDraft] = useState(createMediaDraft('song'));
  const [trackRows, setTrackRows] = useState([]);
  const [selectedTrackToAdd, setSelectedTrackToAdd] = useState('');
  const [draggingTrackId, setDraggingTrackId] = useState('');
  const [dragOverTrackId, setDragOverTrackId] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [dirty, setDirty] = useState(false);
  const [confirm, setConfirm] = useState(null);
  const [dontAskAgain, setDontAskAgain] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const activeTabConfig = manageTabs.find((tab) => tab.key === activeTab) || manageTabs[0];

  const filteredItems = useMemo(() => {
    if (!searchQuery) return items;
    const query = searchQuery.toLowerCase();
    return items.filter((item) => {
      return (
        (item.title || item.Title || '').toLowerCase().includes(query) ||
        (item.genre || item.Genre || '').toLowerCase().includes(query) ||
        (item.description || item.Description || '').toLowerCase().includes(query)
      );
    });
  }, [items, searchQuery]);

  useEffect(() => {
    if (initialTab && initialTab !== activeTab) {
      setActiveTab(initialTab);
    }
  }, [initialTab]);

  useEffect(() => {
    onDirtyChange?.(dirty);
  }, [dirty, onDirtyChange]);

  const availableTracks = useMemo(() => {
    const selectedIds = new Set(trackRows.map((row) => row.mediaItemId || row.MediaItemId));
    return mediaPool.filter((item) => !selectedIds.has(item.id || item.Id));
  }, [mediaPool, trackRows]);

  const openEditor = async (item = null) => {
    if (dirty) {
      const ok = window.confirm('Bạn đang có thay đổi chưa lưu. Bạn có muốn bỏ thay đổi hiện tại không?');
      if (!ok) return;
    }

    setError('');
    setSelectedTrackToAdd('');
    setDraggingTrackId('');
    setDragOverTrackId('');
    setDirty(false);
    setViewMode('editor');

    if (!item) {
      setSelectedEntity(null);
      setTrackRows([]);
      setDraft(
        activeTabConfig.scope === 'media'
          ? createMediaDraft(activeTab, localStorage.getItem('user_name'))
          : createCollectionDraft(activeTab)
      );
      return;
    }

    setSelectedEntity(item);

    if (activeTabConfig.scope === 'media') {
      setDraft(buildMediaDraft(item, activeTab, localStorage.getItem('user_name')));
      setTrackRows([]);
      return;
    }

    const collectionDraft = buildCollectionDraft(item, activeTab);
    setDraft(collectionDraft);

    const trackList = Array.isArray(item.tracks) ? item.tracks : [];
    if (trackList.length === 0) {
      setTrackRows([]);
      return;
    }

    const rows = await Promise.all(trackList.map(async (track) => {
      const mediaId = track.mediaItemId || track.MediaItemId;
      const media = await getTrackById(mediaId).catch(() => null);
      return {
        ...track,
        mediaItemId: mediaId,
        trackOrder: Number(track.trackOrder || track.TrackOrder || 0),
        media,
      };
    }));

    setTrackRows(rows.sort((a, b) => a.trackOrder - b.trackOrder));
  };

  const refreshTabData = async (nextTab = activeTab) => {
    if (!isAuthenticated) return;
    setLoading(true);
    setError('');

    try {
      const [media, playlists, albums] = await Promise.all([
        getMyMedia().catch(() => []),
        getMyPlaylists().catch(() => []),
        getMyAlbums().catch(() => []),
      ]);

      setMediaPool(media || []);

      const nextKind = (manageTabs.find((tab) => tab.key === nextTab) || {}).scope;
      let loadedItems = [];
      if (nextKind === 'media') {
        loadedItems = (media || []).filter((entry) => isMediaTabItem(nextTab, entry));
      } else if (nextTab === 'playlist') {
        loadedItems = playlists || [];
      } else if (nextTab === 'album') {
        loadedItems = albums || [];
      } else {
        loadedItems = [];
      }
      setItems(loadedItems);

      if (initialEntityId) {
        const matched = loadedItems.find((item) => String(item.id || item.Id) === String(initialEntityId));
        if (matched) {
          setSelectedEntity(matched);
          setViewMode('editor');
          setDirty(false);
          if (nextKind === 'media') {
            setDraft(buildMediaDraft(matched, nextTab, localStorage.getItem('user_name')));
            setTrackRows([]);
          } else {
            const collectionDraft = buildCollectionDraft(matched, nextTab);
            setDraft(collectionDraft);
            const trackList = Array.isArray(matched.tracks) ? matched.tracks : [];
            if (trackList.length > 0) {
              const rows = await Promise.all(trackList.map(async (track) => {
                const mediaId = track.mediaItemId || track.MediaItemId;
                const trackMedia = await getTrackById(mediaId).catch(() => null);
                return {
                  ...track,
                  mediaItemId: mediaId,
                  trackOrder: Number(track.trackOrder || track.TrackOrder || 0),
                  media: trackMedia,
                };
              }));
              setTrackRows(rows.sort((a, b) => a.trackOrder - b.trackOrder));
            } else {
              setTrackRows([]);
            }
          }
        }
        onClearInit?.();
      }
    } catch (err) {
      setError(err?.message || 'Không tải được dữ liệu quản lý.');
      setItems([]);
      setMediaPool([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isAuthenticated) return;
    const timeoutId = window.setTimeout(() => {
      void refreshTabData(activeTab);
    }, 0);

    return () => window.clearTimeout(timeoutId);
  }, [activeTab, isAuthenticated]);

  const handleTabChange = (nextTab) => {
    if (nextTab === activeTab) return;
    if (dirty) {
      const ok = window.confirm('Bạn đang có thay đổi chưa lưu. Chuyển tab sẽ mất dữ liệu hiện tại. Bạn có muốn tiếp tục không?');
      if (!ok) return;
    }

    setActiveTab(nextTab);
    setViewMode('list');
    setSelectedEntity(null);
    setTrackRows([]);
    setDirty(false);
    setError('');
    setSearchQuery('');
  };

  const updateDraft = (patch) => {
    setDraft((current) => ({ ...current, ...patch }));
    setDirty(true);
  };

  const setPreviewFile = async (field, file, previewKind) => {
    if (!file) return;
    const previewUrl = URL.createObjectURL(file);
    const durationSeconds = (previewKind === 'audio' || previewKind === 'video')
      ? await readDurationFromFile(file, previewKind)
      : 0;

    updateDraft({
      [field]: file,
      [`${field.replace('File', 'Preview')}`]: previewUrl,
      ...(field === 'coverFile' ? { coverPreview: previewUrl } : {}),
      ...(field === 'mediaFile' ? { mediaPreview: previewUrl, durationSeconds } : {}),
    });
  };

  const openDeleteConfirm = (message, onConfirmAction, allowSkip = false) => {
    setDontAskAgain(false);
    setConfirm({
      title: 'Xác nhận',
      message,
      onConfirmAction,
      allowSkip,
    });
  };

  const closeConfirm = () => {
    setConfirm(null);
    setDontAskAgain(false);
  };

  const confirmRemoveTrack = (row) => {
    const skipKey = sessionSkipRemoveTrackKey;
    if (sessionStorage.getItem(skipKey) === '1') {
      void handleRemoveTrack(row);
      return;
    }

    openDeleteConfirm(
      'Bạn có chắc muốn xóa track này khỏi danh sách không?',
      async () => {
        if (dontAskAgain) sessionStorage.setItem(skipKey, '1');
        await handleRemoveTrack(row);
      },
      true
    );
  };

  const handleRemoveTrack = async (row) => {
    if (!selectedEntity) return;
    const mediaId = row.mediaItemId || row.MediaItemId;
    try {
      setSaving(true);
      if (activeTab === 'playlist') {
        await removeTrackFromPlaylist(selectedEntity.id || selectedEntity.Id, mediaId);
      } else {
        await removeTrackFromAlbum(selectedEntity.id || selectedEntity.Id, mediaId);
      }
      await openEditor(selectedEntity);
      await refreshTabData(activeTab);
    } catch (err) {
      setError(err?.message || 'Không thể xóa track.');
    } finally {
      setSaving(false);
    }
  };

  const handleMoveTrack = async (row, targetOrder) => {
    if (!selectedEntity) return;
    const mediaId = row.mediaItemId || row.MediaItemId;
    const currentOrder = Number(row.trackOrder || row.TrackOrder || 0);
    const nextOrder = Number(targetOrder);

    if (!Number.isInteger(nextOrder) || nextOrder < 1 || nextOrder > trackRows.length) {
      setError(`Vị trí hợp lệ phải từ 1 đến ${trackRows.length}.`);
      return;
    }

    if (nextOrder === currentOrder) return;

    try {
      setSaving(true);
      if (activeTab === 'playlist') {
        await reorderTrackInPlaylist(selectedEntity.id || selectedEntity.Id, mediaId, nextOrder);
      } else {
        await reorderTrackInAlbum(selectedEntity.id || selectedEntity.Id, mediaId, nextOrder);
      }
      await openEditor(selectedEntity);
      await refreshTabData(activeTab);
    } catch (err) {
      setError(err?.message || 'Không thể thay đổi thứ tự track.');
    } finally {
      setSaving(false);
    }
  };

  const handleDragStart = (row) => {
    setDraggingTrackId(String(row.mediaItemId || row.MediaItemId || ''));
    setDragOverTrackId('');
  };

  const handleDragEnd = () => {
    setDraggingTrackId('');
    setDragOverTrackId('');
  };

  const handleDropTrack = async (sourceRow, targetRow) => {
    if (!sourceRow || !targetRow) return;

    const sourceId = String(sourceRow.mediaItemId || sourceRow.MediaItemId || '');
    const targetId = String(targetRow.mediaItemId || targetRow.MediaItemId || '');
    if (!sourceId || !targetId || sourceId === targetId) return;

    await handleMoveTrack(sourceRow, Number(targetRow.trackOrder || targetRow.TrackOrder || 0));
  };

  const handleAddTrack = async () => {
    if (!selectedEntity || !selectedTrackToAdd) return;
    try {
      setSaving(true);
      if (activeTab === 'playlist') {
        await addTrackToPlaylist(selectedEntity.id || selectedEntity.Id, selectedTrackToAdd);
      } else {
        await addTrackToAlbum(selectedEntity.id || selectedEntity.Id, selectedTrackToAdd);
      }
      setSelectedTrackToAdd('');
      await openEditor(selectedEntity);
      await refreshTabData(activeTab);
    } catch (err) {
      setError(err?.message || 'Không thể thêm track.');
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteEntity = async () => {
    if (!selectedEntity) return;

    const ok = window.confirm('Xóa mục này sẽ ẩn nó khỏi danh sách. Bạn có chắc muốn tiếp tục không?');
    if (!ok) return;

    try {
      setSaving(true);
      if (activeTabConfig.scope === 'media') {
        await deleteMedia(selectedEntity.id || selectedEntity.Id);
      } else if (activeTab === 'playlist') {
        await deletePlaylist(selectedEntity.id || selectedEntity.Id);
      } else {
        await deleteAlbum(selectedEntity.id || selectedEntity.Id);
      }
      setViewMode('list');
      setSelectedEntity(null);
      setDirty(false);
      await refreshTabData(activeTab);
    } catch (err) {
      setError(err?.message || 'Không thể xóa mục này.');
    } finally {
      setSaving(false);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError('');

      if (activeTabConfig.scope === 'media') {
        const payload = {
          Title: draft.title,
          Description: draft.description || '',
          Genre: draft.genre || '',
          Type: getMediaKindFromTab(activeTab),
          AccessLevel: 0,
          IsPublic: draft.isPublic,
          ReleaseDate: fromDateTimeLocal(draft.releaseDate),
          FeaturedArtistIds: [],
        };

        const mediaField = activeTab === 'video' ? 'VideoFile' : 'AudioFile';
        const mediaFile = draft.mediaFile;
        const hasNewMediaFile = Boolean(mediaFile);
        if (selectedEntity ? hasNewMediaFile : !mediaFile) {
          if (!selectedEntity) {
            setError(activeTab === 'video' ? 'Vui lòng chọn file video.' : 'Vui lòng chọn file audio.');
            setSaving(false);
            return;
          }
        }

        if (mediaFile) payload[mediaField] = mediaFile;
        if (draft.coverFile) payload.CoverImage = draft.coverFile;
        if (draft.canvasFile) payload.CanvasFile = draft.canvasFile;

        if (selectedEntity) {
          const updated = await updateMedia(selectedEntity.id || selectedEntity.Id, payload);
          setSelectedEntity(updated);
        } else {
          const created = await uploadMedia(payload, getMediaRouteKind(activeTab));
          setSelectedEntity(created);
        }
      } else if (activeTab === 'playlist') {
        const payload = {
          Title: draft.title,
          Description: draft.description || '',
          IsPublic: draft.isPublic,
          CoverImage: draft.coverFile,
          ContentType: draft.contentType || 'Song',
          ReleaseDate: fromDateTimeLocal(draft.releaseDate),
        };

        if (selectedEntity) {
          await updatePlaylist(selectedEntity.id || selectedEntity.Id, payload);
        } else {
          await createPlaylist(payload);
        }
      } else if (activeTab === 'album') {
        const payload = {
          Title: draft.title,
          Description: draft.description || '',
          IsPublic: draft.isPublic,
          CoverImage: draft.coverFile,
          ContentType: draft.contentType || 'Song',
          ReleaseDate: fromDateTimeLocal(draft.releaseDate),
        };

        if (selectedEntity) {
          await updateAlbum(selectedEntity.id || selectedEntity.Id, payload);
        } else {
          await createAlbum(payload);
        }
      }

      setDirty(false);
      setViewMode('list');
      setSelectedEntity(null);
      setTrackRows([]);
      await refreshTabData(activeTab);
    } catch (err) {
      setError(err?.message || 'Không thể lưu thay đổi.');
    } finally {
      setSaving(false);
    }
  };

  const handleOpenItem = async (item) => {
    await openEditor(item);
  };

  if (!isAuthenticated) {
    return (
      <div className="create-studio">
        <div className="background-glow create-glow">
          <div></div>
          <div></div>
        </div>
        <div className="create-studio-inner">
          <div className="manage-empty-auth">
            <h1>Quản lý media / album / playlist</h1>
            <p>Bạn cần đăng nhập để tạo và chỉnh sửa nội dung.</p>
            <button type="button" className="create-primary" onClick={() => onRequireAuth?.('Đăng nhập để quản lý nội dung.')}>
              Đăng nhập
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="create-studio manage-studio">
      <div className="background-glow create-glow">
        <div></div>
        <div></div>
      </div>

      <div className="create-studio-inner manage-studio-inner">
        <div className="create-studio-header">
          <div>
            <p className="create-kicker">Quản lý nội dung</p>
            <h1>Quản lý media / album / playlist</h1>
          </div>
          <button className="create-back-button" type="button" onClick={() => {
            if (dirty) {
              const ok = window.confirm('Bạn đang có thay đổi chưa lưu. Rời màn hình sẽ mất dữ liệu hiện tại. Bạn có muốn tiếp tục không?');
              if (!ok) return;
            }
            onBackToHome?.();
          }}>
            <span className="material-symbols-outlined">arrow_back</span>
            <span>Quay lại</span>
          </button>
        </div>

        <div className="create-tabs manage-tabs">
          {manageTabs.map((tab) => (
            <button
              className={activeTab === tab.key ? 'active' : ''}
              type="button"
              key={tab.key}
              onClick={() => handleTabChange(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </div>


        {viewMode === 'list' ? (
          <section className="manage-list-layout">
            <div className="manage-list-header">
              <div>
                <h2>{activeTabConfig.label}</h2>
                <p>Chạm vào một mục để chỉnh sửa hoặc tạo mới từ tab này.</p>
              </div>

              <div className="manage-search-container" style={{
                display: 'flex',
                alignItems: 'center',
                background: 'rgba(255, 255, 255, 0.05)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: '12px',
                padding: '8px 14px',
                gap: '8px',
                flexGrow: 1,
                maxWidth: '400px',
                marginLeft: '20px',
                marginRight: 'auto',
                transition: 'border-color 0.25s, background-color 0.25s'
              }}>
                <span className="material-symbols-outlined" style={{
                  fontSize: '20px',
                  color: 'var(--on-surface-variant)',
                  userSelect: 'none'
                }}>search</span>
                <input
                  type="text"
                  placeholder={`Tìm kiếm trong ${activeTabConfig.label.toLowerCase()}...`}
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  style={{
                    background: 'transparent',
                    border: 'none',
                    color: 'var(--on-surface)',
                    fontSize: '14px',
                    outline: 'none',
                    width: '100%',
                    padding: '0'
                  }}
                />
                {searchQuery && (
                  <button
                    type="button"
                    onClick={() => setSearchQuery('')}
                    style={{
                      background: 'transparent',
                      border: 'none',
                      padding: '0',
                      color: 'var(--on-surface-variant)',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}
                  >
                    <span className="material-symbols-outlined" style={{ fontSize: '18px' }}>close</span>
                  </button>
                )}
              </div>

              <button className="create-primary" type="button" onClick={() => openEditor(null)}>
                + Thêm {activeTabConfig.label.toLowerCase()} mới
              </button>
            </div>

            {loading ? (
              <div className="content-state">Đang tải dữ liệu quản lý...</div>
            ) : error ? (
              <div className="content-state content-state-error">{error}</div>
            ) : filteredItems.length === 0 ? (
              <div className="section-empty-state">
                {searchQuery
                  ? `Không tìm thấy kết quả nào phù hợp với "${searchQuery}"`
                  : `Chưa có dữ liệu để hiển thị. Hãy tạo mục mới.`}
              </div>
            ) : (
              <div className="manage-list-grid">
                {filteredItems.map((item) => {
                  const mediaType = normalizeType(item.mediaType || item.type || item.contentType || activeTabConfig.kind);
                  const isMedia = activeTabConfig.scope === 'media';
                  return (
                    <ItemCard
                      key={entityKey(activeTab, item)}
                      item={{
                        title: item.title || item.Title,
                        subtitle: isMedia
                          ? `${item.genre || item.Genre || mediaType}`
                          : `${item.description || item.Description || ''} • ${Array.isArray(item.tracks) ? item.tracks.length : 0} track`,
                        coverPreview: (item.id ? mediaPosterUrl(item.id) : undefined) || normalizeAssetUrl(item.coverImageUrl || item.coverImgUrl || item.CoverImageUrl || item.CoverImgUrl) || defaultCoverUrl,
                      }}
                      onOpen={() => handleOpenItem(item)}
                    />
                  );
                })}
              </div>
            )}
          </section>
        ) : (
          <section className="manage-editor-layout">
            <aside className="manage-preview-panel">
              <div className="create-cover manage-cover">
                <img
                  alt={draft.title || 'Preview'}
                  src={draft.coverPreview || defaultCoverUrl}
                  onError={(event) => {
                    event.currentTarget.onerror = null;
                    event.currentTarget.src = defaultCoverUrl;
                  }}
                />
                <div className="create-cover-overlay">
                  <span className="material-symbols-outlined">add_a_photo</span>
                  <span>Tải ảnh bìa</span>
                </div>
                <div className="create-cover-copy">
                  <strong>{draft.title || 'Untitled'}</strong>
                  <span>{draft.ownerLabel}</span>
                </div>
              </div>

              {activeTabConfig.scope === 'media' ? (
                <div className="manage-preview-card">
                  <span className="manage-preview-label">Thời lượng</span>
                  <strong>{durationToLabel(draft.durationSeconds)}</strong>
                  <span className="manage-preview-muted">
                    Tự động đọc từ file local sau khi bạn chọn file.
                  </span>
                </div>
              ) : null}
            </aside>

            <div className="create-grid manage-editor-grid">
              <section className="create-form-panel">
                <div className="create-field">
                  <label>Tên</label>
                  <input
                    type="text"
                    value={draft.title}
                    placeholder={activeTabConfig.scope === 'media' ? 'Tên bài hát của bạn...' : 'Tên album / playlist...'}
                    onChange={(event) => updateDraft({ title: event.target.value })}
                  />
                </div>

                <div className="create-field">
                  <label>Mô tả</label>
                  <textarea
                    className="manage-textarea"
                    rows="4"
                    value={draft.description}
                    placeholder="Mô tả ngắn..."
                    onChange={(event) => updateDraft({ description: event.target.value })}
                  />
                </div>

                <div className="create-field-row">
                  <div className="create-field">
                    <label>Ngày phát hành</label>
                    <input
                      type="datetime-local"
                      value={draft.releaseDate}
                      onChange={(event) => updateDraft({ releaseDate: event.target.value })}
                    />
                  </div>
                  <div className="create-field">
                    <label>{activeTabConfig.scope === 'media' ? 'Genre' : 'Content type'}</label>
                    {activeTabConfig.scope === 'media' ? (
                      <input
                        type="text"
                        value={draft.genre}
                        placeholder="Electronic, Pop..."
                        onChange={(event) => updateDraft({ genre: event.target.value })}
                      />
                    ) : (
                      <div className="create-select-wrap">
                        <select
                          value={draft.contentType}
                          onChange={(event) => updateDraft({ contentType: event.target.value })}
                        >
                          <option value="Song">Song</option>
                          <option value="Audio">Audio</option>
                          <option value="Video">Video</option>
                        </select>
                        <span className="material-symbols-outlined">expand_more</span>
                      </div>
                    )}
                  </div>
                </div>

                {activeTabConfig.scope === 'media' ? (
                  <>
                    <div className="create-field">
                      <label>{draft.tabKey === 'video' ? 'Video file' : 'Audio file'}</label>
                      <div className="create-upload-zone">
                        <div className="create-upload-icon">
                          <span className="material-symbols-outlined fill-icon">cloud_upload</span>
                        </div>
                        <div>
                          <strong>Kéo thả file từ máy tính hoặc chọn file local</strong>
                          <p>{draft.tabKey === 'video' ? 'Hỗ trợ MP4, WEBM' : 'Hỗ trợ MP3, WAV, FLAC, M4A, OGG'}</p>
                        </div>
                        <label className="manage-file-button">
                          Duyệt file
                          <input
                            type="file"
                            accept={draft.tabKey === 'video' ? 'video/*' : 'audio/*'}
                            onChange={(event) => setPreviewFile('mediaFile', event.target.files?.[0], draft.tabKey === 'video' ? 'video' : 'audio')}
                          />
                        </label>
                      </div>
                      {draft.mediaFile ? <p className="create-help">Đã chọn: {draft.mediaFile.name}</p> : null}
                    </div>

                    <div className="create-field-row">
                      <div className="create-field">
                        <label>Ảnh bìa</label>
                        <label className="manage-file-input">
                          <span>Chọn file ảnh local</span>
                          <input
                            type="file"
                            accept="image/*"
                            onChange={(event) => setPreviewFile('coverFile', event.target.files?.[0])}
                          />
                        </label>
                      </div>
                      {activeTabConfig.scope === 'media' && draft.tabKey !== 'video' && (
                        <div className="create-field">
                          <label>Canvas</label>
                          <label className="manage-file-input">
                            <span>Chọn file video canvas</span>
                            <input
                              type="file"
                              accept="video/*"
                              onChange={(event) => setPreviewFile('canvasFile', event.target.files?.[0], 'video')}
                            />
                          </label>
                        </div>
                      )}
                    </div>
                  </>
                ) : (
                  <>
                    <div className="create-field">
                      <label>Ảnh bìa</label>
                      <label className="manage-file-input">
                        <span>Chọn file ảnh local</span>
                        <input
                          type="file"
                          accept="image/*"
                          onChange={(event) => setPreviewFile('coverFile', event.target.files?.[0])}
                        />
                      </label>
                    </div>

                    <div className="manage-track-panel">
                      <div className="manage-track-panel-header">
                        <h3>Track trong {activeTabConfig.label.toLowerCase()}</h3>
                        <span>{trackRows.length} track</span>
                      </div>

                      {trackRows.length > 0 ? (
                        <div className="manage-track-table">
                          {trackRows.map((row, index) => {
                            const media = row.media || {};
                            return (
                              <div className="manage-track-row" key={`${row.mediaItemId || row.MediaItemId}-${row.trackOrder || row.TrackOrder}`}>
                                <div
                                  className={`manage-track-main${draggingTrackId === String(row.mediaItemId || row.MediaItemId || '') ? ' is-dragging' : ''}${dragOverTrackId === String(row.mediaItemId || row.MediaItemId || '') ? ' is-drop-target' : ''}`}
                                  draggable
                                  onDragStart={() => handleDragStart(row)}
                                  onDragEnd={handleDragEnd}
                                  onDragOver={(event) => {
                                    event.preventDefault();
                                    setDragOverTrackId(String(row.mediaItemId || row.MediaItemId || ''));
                                  }}
                                  onDrop={(event) => {
                                    event.preventDefault();
                                    const sourceRow = trackRows.find((track) => String(track.mediaItemId || track.MediaItemId || '') === draggingTrackId);
                                    void handleDropTrack(sourceRow, row);
                                    handleDragEnd();
                                  }}
                                >
                                  <span className="manage-track-index">{index + 1}</span>
                                  <span className="material-symbols-outlined manage-track-handle" aria-hidden="true">drag_indicator</span>
                                  <img
                                    src={(media.id ? mediaPosterUrl(media.id) : undefined) || normalizeAssetUrl(media.coverImageUrl || media.CoverImageUrl) || defaultCoverUrl}
                                    alt={media.title || row.mediaItemId}
                                  />
                                  <div>
                                    <strong>{media.title || row.mediaItemId}</strong>
                                    <p>{media.genre || media.Genre || media.mediaType || media.type || 'Media'}</p>
                                  </div>
                                </div>

                                <div className="manage-track-actions">
                                  <button type="button" onClick={() => confirmRemoveTrack(row)} aria-label="Xóa track">
                                    <span className="material-symbols-outlined">remove</span>
                                  </button>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      ) : null}

                      {trackRows.length === 0 ? (
                        <div className="section-empty-state">Chưa có track nào trong {activeTabConfig.label.toLowerCase()} này.</div>
                      ) : trackRows.length > 1 ? (
                        <div className="manage-track-dropzone">
                          <span className="material-symbols-outlined">swap_vert</span>
                          <p>Kéo một track và thả lên track khác để đổi thứ tự.</p>
                        </div>
                      ) : null}

                      <div className="manage-add-track">
                        <div className="create-select-wrap">
                          <select
                            value={selectedTrackToAdd}
                            onChange={(event) => setSelectedTrackToAdd(event.target.value)}
                          >
                            <option value="">Chọn track để thêm</option>
                            {availableTracks.map((track) => (
                              <option key={track.id || track.Id} value={track.id || track.Id}>
                                {track.title || track.Title}
                              </option>
                            ))}
                          </select>
                          <span className="material-symbols-outlined">expand_more</span>
                        </div>
                        <button type="button" className="create-secondary" onClick={handleAddTrack} disabled={!selectedTrackToAdd}>
                          Thêm track
                        </button>
                      </div>
                    </div>
                  </>
                )}

                <div className="manage-toggle-row">
                  <div>
                    <strong>Công khai</strong>
                    <p>Ẩn để chỉ chủ tài khoản xem được.</p>
                  </div>
                  <button
                    type="button"
                    className={`manage-switch${draft.isPublic ? ' active' : ''}`}
                    onClick={() => updateDraft({ isPublic: !draft.isPublic })}
                    aria-pressed={draft.isPublic}
                  >
                    <span></span>
                  </button>
                </div>

                <div className="manage-access-note">
                  <strong>Quyền truy cập</strong>
                  <p>Tự động theo tài khoản đang tạo nội dung.</p>
                </div>

                <div className="create-actions">
                  <button type="button" className="create-primary" onClick={handleSave} disabled={saving}>
                    {saving ? 'Đang lưu...' : selectedEntity ? 'Lưu thay đổi' : `Tạo ${activeTabConfig.label.toLowerCase()} mới`}
                  </button>
                  <button type="button" className="create-secondary" onClick={() => openEditor(selectedEntity)}>
                    Làm mới form
                  </button>
                  <button type="button" className="create-secondary" onClick={() => {
                    if (dirty) {
                      const ok = window.confirm('Bạn đang có thay đổi chưa lưu. Rời khỏi form sẽ mất dữ liệu hiện tại. Bạn có muốn tiếp tục không?');
                      if (!ok) return;
                    }
                    setViewMode('list');
                    setSelectedEntity(null);
                    setTrackRows([]);
                    setDirty(false);
                  }}>
                    Quay về danh sách
                  </button>
                  {selectedEntity ? (
                    <button type="button" className="manage-danger" onClick={handleDeleteEntity}>
                      Xóa mục này
                    </button>
                  ) : null}
                </div>
              </section>

              <section className="create-preview-panel">
                <div className="manage-preview-card">
                  <span className="manage-preview-label">Loại đang chỉnh</span>
                  <strong>{activeTabConfig.label}</strong>
                  <span className="manage-preview-muted">
                    {activeTabConfig.scope === 'media'
                      ? 'File sẽ được chọn từ máy tính local và upload lên backend.'
                      : 'Track của album / playlist có thể thêm, xóa, và đổi thứ tự ngay tại đây.'}
                  </span>
                </div>

                <div className="manage-preview-card">
                  <span className="manage-preview-label">Ngày phát hành</span>
                  <strong>{draft.releaseDate ? new Date(draft.releaseDate).toLocaleString('vi-VN') : 'Ngay bây giờ'}</strong>
                </div>

                {activeTabConfig.scope === 'media' ? (
                  <div className="manage-preview-card">
                    <span className="manage-preview-label">File hiện chọn</span>
                    <strong>{draft.mediaFile ? draft.mediaFile.name : 'Chưa chọn file'}</strong>
                    <span className="manage-preview-muted">
                      Duration: {durationToLabel(draft.durationSeconds)}
                    </span>
                  </div>
                ) : (
                  <div className="manage-preview-card">
                    <span className="manage-preview-label">Track count</span>
                    <strong>{trackRows.length}</strong>
                    <span className="manage-preview-muted">Danh sách track được cập nhật theo thứ tự hiện tại.</span>
                  </div>
                )}
              </section>
            </div>
          </section>
        )}

        <TrackConfirmModal
          confirm={confirm}
          dontAskAgain={dontAskAgain}
          setDontAskAgain={setDontAskAgain}
          onCancel={closeConfirm}
          onConfirm={async () => {
            const action = confirm?.onConfirmAction;
            closeConfirm();
            if (typeof action === 'function') {
              await action();
            }
          }}
        />
      </div>
    </div>
  );
}
