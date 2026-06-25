import { useEffect, useRef, useState } from 'react';
import Sidebar from './Components/Sidebar';
import Home from './Components/Home';
import PlayerBar from './Components/PlayerBar';
import FriendActivity from './Components/FriendActivity';
import NotificationActivity from './Components/NotificationActivity';
import AuthLoginModal from './Components/AuthLoginModal';
import PlaylistModal from './Components/PlaylistModal'; // Import PlaylistModal
import VideoPlayerView from './Components/VideoPlayerView';
import MediaInfoPanel from './Components/MediaInfoPanel';
import ListeningHistoryActivity from './Components/ListeningHistoryActivity';

import {
  MediaService,
  clearAuthSession,
  getMedia,
  getRecentHistory,
  getResumeInfo,
  getMyProfile,
  mediaAudioStreamUrl,
  mediaVideoStreamUrl,
  normalizeAssetUrl,
  recordPlayHistory,
  recordPlaybackStop,
  logoutUser,
  refreshAuthSession,
  getMediaCollection,
  getMyPlaylists, // Import getMyPlaylists
  addTrackToPlaylist // Import addTrackToPlaylist
} from '../../Services/MediaService.tsx';
import '../CSS/App_style.css';

const fallbackTrack = {
  id: 'demo-track',
  title: 'Midnight Resonance',
  artist: 'Lumina Collective',
  image: normalizeAssetUrl('/uploads/default-cover/Default.png')
    || 'https://images.unsplash.com/photo-1614613535308-eb5fbd3d2c17?auto=format&fit=crop&q=80&w=1000',
  audioUrl: null,
  currentTime: '1:24',
  duration: '4:02',
  progress: 35,
};

function formatTime(seconds = 0) {
  if (!Number.isFinite(seconds) || seconds < 0) return '0:00';
  const totalSeconds = Math.floor(seconds);
  const minutes = Math.floor(totalSeconds / 60);
  const remainingSeconds = String(totalSeconds % 60).padStart(2, '0');
  return `${minutes}:${remainingSeconds}`;
}

function normalizeMediaTypeName(value) {
  const numericMap = ['audio', 'video', 'podcast', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';

  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;

  return normalized;
}

function resolveAudioSource(media) {
  if (!media?.id) return null;
  if (media.id === fallbackTrack.id) return null;

  const explicitAudioUrl = normalizeAssetUrl(media.audioUrl || media.AudioUrl);
  if (explicitAudioUrl) return explicitAudioUrl;

  const mediaType = normalizeMediaTypeName(media.mediaType ?? media.type);
  if (mediaType === 'video') return mediaVideoStreamUrl(media.id);

  return mediaAudioStreamUrl(media.id);
}

export default function App() {
  const [activePanel, setActivePanel] = useState(null);
  const [isNowPlayingExpanded, setIsNowPlayingExpanded] = useState(false);
  const [selectedLibraryItem, setSelectedLibraryItem] = useState(null);
  const [bodyMode, setBodyMode] = useState('home');
  const [manageInitialTab, setManageInitialTab] = useState(null);
  const [manageInitialEntityId, setManageInitialEntityId] = useState(null);
  const [profileTarget, setProfileTarget] = useState(null);
  const [authPrompt, setAuthPrompt] = useState({ isOpen: false, reason: '', initialMode: 'login' });
  const [authVersion, setAuthVersion] = useState(0);
  const [playerTrack, setPlayerTrack] = useState(fallbackTrack);
  const [playerQueue, setPlayerQueue] = useState([]); // New state for the queue
  const [currentQueueIndex, setCurrentQueueIndex] = useState(-1); // Index of the current track in the queue
  const [isPlaying, setIsPlaying] = useState(false);
  const [volume, setVolume] = useState(() => {
    const savedVolume = localStorage.getItem('player_volume');
    return savedVolume !== null ? parseFloat(savedVolume) : 1;
  });
  const [isMuted, setIsMuted] = useState(false);
  const [currentUserAvatarUrl, setCurrentUserAvatarUrl] = useState(null);
  const [currentUserEmail, setCurrentUserEmail] = useState('');
  const [isProfileDirty, setIsProfileDirty] = useState(false);
  const [isManageDirty, setIsManageDirty] = useState(false);
  const [isPlaylistModalOpen, setIsPlaylistModalOpen] = useState(false); // State for playlist modal
  const [userPlaylists, setUserPlaylists] = useState([]); // State for user's playlists
  const [isVideoViewOpen, setIsVideoViewOpen] = useState(false); // State for video view
  const [isInfoPanelOpen, setIsInfoPanelOpen] = useState(false); // State for info panel
  const [availableReactions, setAvailableReactions] = useState([]); // State for available favorite reactions
  const [currentFavoriteReaction, setCurrentFavoriteReaction] = useState(null); // State for current favorite reaction of the playing track
  const [isFavoritePickerOpen, setIsFavoritePickerOpen] = useState(false); // State to control the visibility of the favorite reaction picker
  const [libraryVersion, setLibraryVersion] = useState(0);
  const audioRef = useRef(null);
  const pendingSeekRef = useRef(null);
  const pendingAutoPlayRef = useRef(false);
  const currentAudioObjectUrlRef = useRef(null);
  const currentAudioMediaIdRef = useRef(null);

  const isAuthenticated = Boolean(localStorage.getItem('auth_session'));

  const promptAuth = (initialMode = 'login', reason = 'Bạn cần đăng nhập để tiếp tục thao tác này.') => {
    setAuthPrompt({ isOpen: true, reason, initialMode });
  };

  const promptLogin = (reason = 'Bạn cần đăng nhập để tiếp tục thao tác này.') => {
    promptAuth('login', reason);
  };

  const confirmLeaveCurrentBody = () => {
    const dirtyState = bodyMode === 'profile'
      ? isProfileDirty
      : bodyMode === 'manage'
        ? isManageDirty
        : false;

    if (!dirtyState) return true;

    const ok = window.confirm(
      bodyMode === 'profile'
        ? 'Bạn đang có thay đổi hồ sơ chưa lưu. Rời khỏi màn hình sẽ mất dữ liệu hiện tại. Bạn có muốn tiếp tục không?'
        : 'Bạn đang có thay đổi chưa lưu. Rời khỏi màn hình sẽ mất dữ liệu hiện tại. Bạn có muốn tiếp tục không?'
    );

    return ok;
  };

  const setBodyModeSafely = (nextMode) => {
    if (nextMode === bodyMode) return true;
    if (!confirmLeaveCurrentBody(nextMode)) return false;

    setBodyMode(nextMode);
    return true;
  };

  const requireAuth = (reason, action) => {
    if (!localStorage.getItem('auth_session')) {
      promptLogin(reason);
      return false;
    }

    action?.();
    return true;
  };

  const handleLogout = async () => {
    await logoutUser().catch(() => null);
    audioRef.current?.pause();
    if (audioRef.current) {
      audioRef.current.removeAttribute('src');
      audioRef.current.load();
    }
    if (currentAudioObjectUrlRef.current) {
      URL.revokeObjectURL(currentAudioObjectUrlRef.current);
      currentAudioObjectUrlRef.current = null;
    }
    currentAudioMediaIdRef.current = null;
    clearAuthSession();
    setCurrentUserAvatarUrl(null);
    setIsProfileDirty(false);
    setIsManageDirty(false);
    setProfileTarget(null);
    setCurrentUserEmail('');
    setActivePanel(null);
    setSelectedLibraryItem(null);
    setBodyMode('home');
    setIsNowPlayingExpanded(false);
    setPlayerTrack(fallbackTrack);
    setPlayerQueue([]); // Clear queue on logout
    setCurrentQueueIndex(-1); // Reset index on logout
    setIsPlaying(false);
    setVolume(1); // Reset volume on logout
    setIsMuted(false); // Reset mute on logout
    setUserPlaylists([]); // Clear user playlists on logout
    setIsVideoViewOpen(false); // Close video view on logout
    setIsInfoPanelOpen(false); // Close info panel on logout
    setAuthVersion((version) => version + 1);
  };

  useEffect(() => {
    if (!isAuthenticated) return undefined;

    let cancelled = false;

    const syncCurrentUserProfile = async () => {
      try {
        const profile = await getMyProfile().catch(() => null);
        if (cancelled) return;

        const avatarUrl = normalizeAssetUrl(profile?.avatarUrl || profile?.AvatarUrl || null);
        const email = String(profile?.email || profile?.Email || '').trim();
        setCurrentUserAvatarUrl(avatarUrl || null);
        setCurrentUserEmail(email);

        if (avatarUrl) {
          localStorage.setItem('user_avatar', avatarUrl);
        } else {
          localStorage.removeItem('user_avatar');
        }
        if (email) {
          localStorage.setItem('user_email', email);
        } else {
          localStorage.removeItem('user_email');
        }
      } catch {
        if (!cancelled) {
          setCurrentUserAvatarUrl(null);
          setCurrentUserEmail('');
          localStorage.removeItem('user_avatar');
          localStorage.removeItem('user_email');
        }
      }
    };

    void syncCurrentUserProfile();

    return () => {
      cancelled = true;
    };
  }, [authVersion, isAuthenticated]);

  const togglePanel = (panelName) => {
    if (panelName === 'friends' || panelName === 'notifications') {
      requireAuth('Đăng nhập để xem bạn bè và thông báo của bạn.', () => {
        setActivePanel((currentPanel) => (currentPanel === panelName ? null : panelName));
      });
      return;
    }

    setActivePanel((currentPanel) => (currentPanel === panelName ? null : panelName));
  };

  const toggleLibraryItem = (item) => {
    if (item?.type === 'media') {
      if (!setBodyModeSafely('manage')) return;
      setManageInitialTab(item.mediaKind || 'song');
      setManageInitialEntityId(item.id);
      setSelectedLibraryItem(null);
      setProfileTarget(null);
      setIsNowPlayingExpanded(false);
      return;
    }

    if (item && ['song', 'video', 'audio', 'podcast'].includes(String(item.type).toLowerCase())) {
      MediaService.getMediaById(item.id)
        .then((fullMedia) => {
          if (fullMedia) {
            playMediaItem(fullMedia);
          }
        })
        .catch((err) => {
          console.error('[TuneVault] Error fetching media item details:', err);
          playMediaItem(item);
        });
      return;
    }

    setSelectedLibraryItem((currentItem) => (currentItem?.id === item.id ? null : item));
  };

  const openCollectionItem = (item) => {
    if (!item?.id) return;
    setSelectedLibraryItem(item);
    setBodyMode('home');
    setIsNowPlayingExpanded(false);
  };

  const openUserProfileFromPanel = (user) => {
    if (!user?.id && !user?.handle && !user?.idDisplay) return;
    if (!setBodyModeSafely('profile')) return;
    setSelectedLibraryItem(null);
    setIsNowPlayingExpanded(false);
    setActivePanel(null);
    setProfileTarget(user);
  };

  const fetchCollectionItems = async (collectionId, collectionType) => {
    try {
      return (await getMediaCollection(collectionId, collectionType)) || [];
    } catch (error) {
      console.error(`Error fetching collection items for ${collectionType} ${collectionId}:`, error);
      return [];
    }
  };

  const startPlaybackForTrack = async (track, nextQueue, nextQueueIndex, options = {}) => {
    const { autoplay = true, resumeAt = 0 } = options;
    if (!track?.id) return false;

    const nextAudioSource = resolveAudioSource(track);
    if (!nextAudioSource) return false;

    setPlayerQueue(nextQueue);
    setCurrentQueueIndex(nextQueueIndex);
    pendingAutoPlayRef.current = Boolean(autoplay);
    pendingSeekRef.current = Math.max(0, Number(resumeAt || 0));

    setPlayerTrack({
      id: track.id,
      title: track.title || track.Title || fallbackTrack.title,
      artist: track.artist || track.artistName || track.ownerId || track.OwnerId || 'TuneVault',
      image: track.image || normalizeAssetUrl(track.coverImageUrl || track.CoverImageUrl) || fallbackTrack.image,
      audioUrl: nextAudioSource,
      videoUrl: normalizeAssetUrl(track.videoUrl || track.VideoUrl) || '',
      mediaType: normalizeMediaTypeName(track.mediaType ?? track.type),
      collectionId: track.collectionId || track.CollectionId || null,
      collectionType: track.collectionType || track.CollectionType || null,
      currentTime: formatTime(resumeAt),
      duration: formatTime(Number(track.durationSeconds || track.DurationSeconds || 0)),
      progress: 0,
    });

    try {
      await ensureAudioSource({ ...track, audioUrl: nextAudioSource });
    } catch (error) {
      console.warn('[TuneVault] Không tải được audio stream.', error);
      setIsPlaying(false);
      return false;
    }

    return true;
  };

  const playQueueTrackAtIndex = async (queueIndex, options = {}) => {
    const track = playerQueue[queueIndex];
    if (!track) return;

    const started = await startPlaybackForTrack(track, playerQueue, queueIndex, {
      autoplay: true,
      resumeAt: options.resumeAt ?? 0,
    });

    if (!started) return;

    const audio = audioRef.current;
    if (!audio) return;

    if (audio.readyState >= 1) {
      pendingAutoPlayRef.current = false;
      audio.muted = true;
      audio.play()
        .then(() => {
          setIsPlaying(true);
          window.setTimeout(() => {
            if (audioRef.current) {
              audioRef.current.muted = false;
            }
          }, 0);
        })
        .catch(() => {
          audio.muted = false;
          setIsPlaying(false);
        });
    }
  };

  const playMediaItem = async (media, options = {}) => {
    const { collectionId, collectionType, shuffle = false } = options;

    if (!media?.id) return;

    if (!isAuthenticated) {
      promptLogin('Đăng nhập để phát bài hát này.');
      return;
    }

    // --- Explicit check for video without audio fallback ---
    const mediaType = normalizeMediaTypeName(media.mediaType ?? media.type);
    const hasAudioSource = resolveAudioSource(media);

    if (mediaType === 'video' && !hasAudioSource) {
      console.warn('[TuneVault] Cannot play video without an audio source.');
      // Optionally, show a user-facing message here.
      return; // Prevent playback if it's a video without an audio source.
    }
    // --- End explicit check ---


    audioRef.current?.pause();
    setIsPlaying(false);

    let queue = [];
    let initialIndex = 0;

    if (collectionId && collectionType) {
      const collectionItems = await fetchCollectionItems(collectionId, collectionType);
      if (collectionItems.length > 0) {
        queue = shuffle ? [...collectionItems].sort(() => Math.random() - 0.5) : collectionItems;
        initialIndex = queue.findIndex((item) => item.id === media.id);
        if (initialIndex < 0) {
          queue = [media];
          initialIndex = 0;
        }
      } else {
        queue = [media];
      }
    } else {
      try {
        const allMedia = await getMedia(1, 50).catch(() => []);
        if (allMedia && allMedia.length > 0) {
          const normalizedMedia = allMedia.map((item) => {
            const mediaId = String(item.id || item.Id);
            return {
              ...item,
              id: mediaId,
              title: item.title || item.Title || fallbackTrack.title,
              artist: item.artist || item.artistName || item.ownerId || item.OwnerId || 'TuneVault',
              image: item.image || normalizeAssetUrl(item.coverImageUrl || item.CoverImageUrl) || fallbackTrack.image,
              audioUrl: resolveAudioSource(item),
              videoUrl: normalizeAssetUrl(item.videoUrl || item.VideoUrl) || '',
              mediaType: normalizeMediaTypeName(item.mediaType ?? item.type),
              durationSeconds: Number(item.durationSeconds || item.DurationSeconds || 0),
            };
          });

          queue = shuffle ? [...normalizedMedia].sort(() => Math.random() - 0.5) : normalizedMedia;
          initialIndex = queue.findIndex((item) => item.id === media.id);
          if (initialIndex < 0) {
            queue = [media, ...queue];
            initialIndex = 0;
          }
        } else {
          queue = [media];
        }
      } catch {
        queue = [media];
      }
    }

    const currentTrack = queue[initialIndex];
    const started = await startPlaybackForTrack(currentTrack, queue, initialIndex, { autoplay: true, resumeAt: 0 });
    if (!started) {
      setIsPlaying(false);
      pendingAutoPlayRef.current = false;
      return;
    }

    const audio = audioRef.current;
    if (audio?.readyState >= 1) {
      pendingAutoPlayRef.current = false;
      audio.muted = true;
      audio.play()
        .then(() => {
          setIsPlaying(true);
          window.setTimeout(() => {
            if (audioRef.current) {
              audioRef.current.muted = false;
            }
          }, 0);
        })
        .catch(() => {
          audio.muted = false;
          setIsPlaying(false);
        });
    }

    if (mediaType === 'video') {
      setIsNowPlayingExpanded(true);
    }
  };

  const syncTrackFromMedia = (media, resumeAt = 0) => {
    if (!media) return;

    const durationSeconds = Number(media.durationSeconds || 0);
    const currentSeconds = Math.max(0, Number(resumeAt || 0));
    const progress = durationSeconds > 0
      ? Math.min(100, Math.round((currentSeconds / durationSeconds) * 100))
      : 0;

    const artistLabel = Array.isArray(media.artists) && media.artists.length > 0
      ? media.artists.map((artist) => artist.artistName || artist.ArtistName || artist.artistId || artist.ArtistId).filter(Boolean).join(', ')
      : media.artistName || media.ArtistName || media.artist || media.Artist || media.ownerId || media.OwnerId || 'TuneVault';
    const audioSource = resolveAudioSource(media);

    setPlayerTrack({
      id: media.id,
      title: media.title || media.Title || fallbackTrack.title,
      artist: artistLabel,
      image: media.image || normalizeAssetUrl(media.coverImageUrl || media.CoverImageUrl) || fallbackTrack.image,
      audioUrl: audioSource,
      videoUrl: normalizeAssetUrl(media.videoUrl || media.VideoUrl) || '',
      canvasUrl: normalizeAssetUrl(media.canvasUrl || media.CanvasUrl) || null,
      mediaType: normalizeMediaTypeName(media.mediaType ?? media.type),
      collectionId: media.collectionId || media.CollectionId || null,
      collectionType: media.collectionType || media.CollectionType || null,
      currentTime: formatTime(currentSeconds),
      duration: formatTime(durationSeconds),
      progress,
    });

    pendingSeekRef.current = currentSeconds > 0 ? currentSeconds : null;
  };

  const revokeCurrentAudioObjectUrl = () => {
    if (currentAudioObjectUrlRef.current) {
      URL.revokeObjectURL(currentAudioObjectUrlRef.current);
      currentAudioObjectUrlRef.current = null;
    }
  };

  const ensureAudioSource = async (media) => {
    const mediaId = media?.id;
    if (!audioRef.current || !mediaId) return;

    const nextSource = resolveAudioSource(media);
    if (!nextSource) {
      throw new Error('Media này chưa có audio stream.');
    }

    if (currentAudioMediaIdRef.current === mediaId && currentAudioObjectUrlRef.current) {
      return;
    }

    revokeCurrentAudioObjectUrl();

    try {
      const response = await fetch(nextSource, {
        credentials: 'include',
      });

      if (!response.ok) {
        if (response.status === 401) {
          const refreshed = await refreshAuthSession().catch(() => null);
          if (refreshed) {
            return ensureAudioSource(media);
          }
        }
        console.warn('[TuneVault] Audio stream request failed.', {
          mediaId,
          source: nextSource,
          status: response.status,
        });
        throw new Error(`Không tải được stream audio (${response.status}).`);
      }

      const blob = await response.blob();
      const objectUrl = URL.createObjectURL(blob);
      currentAudioObjectUrlRef.current = objectUrl;
      currentAudioMediaIdRef.current = mediaId;
      audioRef.current.src = objectUrl;
      audioRef.current.load();
    } catch (error) {
      pendingAutoPlayRef.current = false;
      setIsPlaying(false);
      throw error;
    }
  };

  const handleVolumeChange = (newVolume) => {
    const audio = audioRef.current;
    if (audio) {
      audio.volume = newVolume;
      localStorage.setItem('player_volume', newVolume.toString());
      setVolume(newVolume);
      if (newVolume > 0 && isMuted) {
        setIsMuted(false);
      }
    }
  };

  const handleToggleMute = () => {
    const audio = audioRef.current;
    if (audio) {
      if (isMuted) {
        audio.muted = false;
        setIsMuted(false);
        // Restore volume if it was 0 when muted
        if (volume === 0) {
          audio.volume = 0.5; // Default to 0.5 if volume was 0
          setVolume(0.5);
          localStorage.setItem('player_volume', '0.5');
        } else {
          audio.volume = volume;
        }
      } else {
        audio.muted = true;
        setIsMuted(true);
      }
    }
  };

  const handleTogglePlay = async () => {
    if (!isAuthenticated) {
      promptLogin('Đăng nhập để nghe nhạc và sử dụng trình phát.');
      return;
    }

    const audio = audioRef.current;
    if (!audio) return;
    const currentTrack = playerQueue[currentQueueIndex];
    if (!currentTrack) {
      setIsPlaying(false);
      return;
    }

    try {
      await ensureAudioSource(currentTrack);
    } catch (error) {
      console.warn('[TuneVault] Không thể phát media hiện tại.', error);
      setIsPlaying(false);
      return;
    }

    if (audio.paused) {
      try {
        await recordPlayHistory(currentTrack.id).catch(() => null);
        audio.muted = false;
        await audio.play();
        setIsPlaying(true);
      } catch {
        setIsPlaying(false);
      }
      return;
    }

    audio.pause();
    try {
      await recordPlaybackStop(currentTrack.id, audio.currentTime || 0).catch(() => null);
    } finally {
      setIsPlaying(false);
    }
  };

  const playNextTrack = async () => {
    if (playerQueue.length === 0 || currentQueueIndex < 0) return;
    const nextIndex = currentQueueIndex + 1;
    if (nextIndex >= playerQueue.length) {
      audioRef.current?.pause();
      setIsPlaying(false);
      return;
    }

    await playQueueTrackAtIndex(nextIndex);
  };

  const playPreviousTrack = async () => {
    if (playerQueue.length === 0 || currentQueueIndex <= 0) return;
    const prevIndex = currentQueueIndex - 1;
    await playQueueTrackAtIndex(prevIndex);
  };


  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return undefined;

    const syncFromAudio = () => {
      setPlayerTrack((current) => {
        const durationSeconds = Number.isFinite(audio.duration) ? audio.duration : current.duration;
        const currentSeconds = audio.currentTime || 0;
        const progress = durationSeconds > 0
          ? Math.min(100, Math.round((currentSeconds / durationSeconds) * 100))
          : current.progress;

        return {
          ...current,
          currentTime: formatTime(currentSeconds),
          duration: Number.isFinite(durationSeconds) ? formatTime(durationSeconds) : current.duration,
          progress,
        };
      });
    };

    const onLoadedMetadata = () => {
      if (pendingSeekRef.current != null) {
        const target = Math.min(pendingSeekRef.current, audio.duration || pendingSeekRef.current);
        audio.currentTime = Number.isFinite(target) ? target : 0;
        pendingSeekRef.current = null;
      }

      syncFromAudio();

      if (pendingAutoPlayRef.current) {
        pendingAutoPlayRef.current = false;
        audio.muted = true;
        audio.play()
          .then(() => {
            setIsPlaying(true);
            window.setTimeout(() => {
              if (audioRef.current) {
                audioRef.current.muted = false;
              }
            }, 0);
          })
          .catch(() => {
            audio.muted = false;
            setIsPlaying(false);
          });
      }
    };

    const onTimeUpdate = syncFromAudio;
    const onPlay = () => setIsPlaying(true);
    const onPause = () => setIsPlaying(false);
    const onEnded = () => {
      setIsPlaying(false);
      // Automatically play next track when current one ends
      playNextTrack();
    };

    audio.addEventListener('loadedmetadata', onLoadedMetadata);
    audio.addEventListener('timeupdate', onTimeUpdate);
    audio.addEventListener('play', onPlay);
    audio.addEventListener('pause', onPause);
    audio.addEventListener('ended', onEnded);

    const persistProgress = () => {
      if (!isAuthenticated || !playerTrack.id || audio.paused) return;
      recordPlaybackStop(playerTrack.id, audio.currentTime || 0).catch(() => null);
    };

    window.addEventListener('beforeunload', persistProgress);
    document.addEventListener('visibilitychange', persistProgress);

    return () => {
      audio.removeEventListener('loadedmetadata', onLoadedMetadata);
      audio.removeEventListener('timeupdate', onTimeUpdate);
      audio.removeEventListener('play', onPlay);
      audio.removeEventListener('pause', onPause);
      audio.removeEventListener('ended', onEnded);
      window.removeEventListener('beforeunload', persistProgress);
      document.removeEventListener('visibilitychange', persistProgress);
    };
  }, [isAuthenticated, playerTrack.id, playerQueue, currentQueueIndex]); // Added dependencies for queue and index

  // useEffect(() => {
  //   if (!isAuthenticated) return undefined;
  // 
  //   let cancelled = false;
  // 
  //   const timeoutId = window.setTimeout(() => {
  //     void (async () => {
  //       if (cancelled || !localStorage.getItem('auth_session')) return;
  // 
  //       try {
  //         const recentHistory = await getRecentHistory().catch(() => []);
  //         console.log('[App.jsx] Recent History:', recentHistory);
  //         const recentTrack = recentHistory?.[0];
  //         console.log('[App.jsx] Recent Track:', recentTrack);
  //         if (!recentTrack) {
  //           const fallbackMedia = await getMedia(1, 1).catch(() => []);
  //           if (fallbackMedia[0] && !cancelled) {
  //             syncTrackFromMedia(fallbackMedia[0], 0);
  //             await ensureAudioSource(fallbackMedia[0]);
  //             // Initialize queue with fallback media if no history
  //             setPlayerQueue([fallbackMedia[0]]);
  //             setCurrentQueueIndex(0);
  //           }
  //           return;
  //         }
  // 
  //         const resumeInfo = await getResumeInfo(recentTrack.id).catch(() => null);
  //         console.log('[App.jsx] Resume Info:', resumeInfo);
  //         const resumeAt = Number(resumeInfo?.stoppedAt ?? 0);
  //         console.log('[App.jsx] Resume At:', resumeAt);
  //         if (cancelled) return;
  //         syncTrackFromMedia(recentTrack, resumeAt);
  //         await ensureAudioSource(recentTrack);
  // 
  //         // When loading from history, we might want to fetch the entire collection
  //         // to build a queue. This is a simplification for now.
  //         // A more robust solution would fetch the collection based on recentTrack's type.
  //         // For now, we'll just set the current track and assume queue will be built later.
  //         // If recentTrack has collection info, we could use it here.
  //         // Example: if (recentTrack.collectionId) { ... fetch collection ... }
  //         // For now, we'll just set the current track and let playMediaItem handle queueing if needed.
  //         // If we want to load the queue from history, we'd need to store queue info in history/resume.
  //         // For now, let's assume the queue is built when a new item is explicitly played.
  //         // If we want to resume a queue, we'd need to store queue and index in backend.
  // 
  //       } catch {
  //         if (!cancelled) {
  //           setPlayerTrack(fallbackTrack);
  //           setPlayerQueue([]);
  //           setCurrentQueueIndex(-1);
  //         }
  //       }
  //     })();
  //   }, 0);
  // 
  //   return () => {
  //     cancelled = true;
  //     window.clearTimeout(timeoutId);
  //   };
  // }, [authVersion, isAuthenticated]);

  useEffect(() => () => {
    revokeCurrentAudioObjectUrl();
  }, []);

  useEffect(() => {
    if (isVideoViewOpen) {
      audioRef.current?.pause();
      setIsPlaying(false);
    }
  }, [isVideoViewOpen]);

  useEffect(() => {
    if (playerTrack?.id && playerTrack.id !== fallbackTrack.id && isPlaying) {
      const trackId = playerTrack.id;
      const now = Date.now();
      try {
        let history = JSON.parse(localStorage.getItem('listening_history') || '[]');
        if (!Array.isArray(history)) history = [];
        history = history.filter((item) => item.id !== trackId);
        history.unshift({ id: trackId, timestamp: now });
        const cutoff = now - 259200000; // 72 hours
        history = history.filter((item) => item.timestamp >= cutoff);
        localStorage.setItem('listening_history', JSON.stringify(history));
      } catch (e) {
        console.error('Error saving history:', e);
      }
    }
  }, [playerTrack?.id, isPlaying]);


  // Effect to fetch available reactions and current favorite status
  useEffect(() => {
    let cancelled = false;

    const fetchReactionsAndStatus = async () => {
      if (!isAuthenticated) {
        setAvailableReactions([]);
        setCurrentFavoriteReaction(null);
        return;
      }

      try {
        const reactions = await MediaService.getFavoriteReactions();
        if (!cancelled) {
          setAvailableReactions(reactions);
        }
      } catch (error) {
        console.error('Error fetching available reactions:', error);
        if (!cancelled) {
          setAvailableReactions([]);
        }
      }

      if (playerTrack?.id && playerTrack.id !== fallbackTrack.id) {
        try {
          const [status, countResult] = await Promise.all([
            MediaService.getFavoriteStatus(playerTrack.id),
            MediaService.getMediaReactionCount(playerTrack.id).catch(() => null),
          ]);
          if (!cancelled) {
            setCurrentFavoriteReaction(status?.reaction ?? null);
            setPlayerTrack((curr) => ({
              ...curr,
              favoriteCount: Number(countResult?.totalCount ?? 0),
            }));
          }
        } catch (error) {
          console.error(`Error fetching favorite status/count for ${playerTrack.id}:`, error);
          if (!cancelled) {
            setCurrentFavoriteReaction(null);
          }
        }
      } else {
        if (!cancelled) {
          setCurrentFavoriteReaction(null);
        }
      }
    };

    void fetchReactionsAndStatus();

    return () => {
      cancelled = true;
    };
  }, [authVersion, isAuthenticated, playerTrack.id]); // Re-run when auth status or playing track changes

  const closeBodyOverlay = () => {
    if (isVideoViewOpen) {
      setIsVideoViewOpen(false);
      return;
    }
    if (isInfoPanelOpen) {
      setIsInfoPanelOpen(false);
      return;
    }
    if (selectedLibraryItem) {
      setSelectedLibraryItem(null);
      return;
    }

    if (bodyMode !== 'home') {
      if (!setBodyModeSafely('home')) return;
      setProfileTarget(null);
      return;
    }

    setIsNowPlayingExpanded(false);
  };

  // Handler for opening the playlist modal
  const handleOpenPlaylistModal = () => {
    if (!isAuthenticated) {
      promptLogin('Đăng nhập để thêm bài hát vào playlist.');
      return;
    }
    if (!playerTrack?.id) return; // Cannot add if no track is playing

    requireAuth('Đăng nhập để xem danh sách playlist của bạn.', async () => {
      try {
        const playlists = await getMyPlaylists();
        setUserPlaylists(playlists);
        setIsPlaylistModalOpen(true);
      } catch (error) {
        console.error('Error fetching user playlists:', error);
        // Optionally show an error message to the user
      }
    });
  };

  // Handler for adding a track to a playlist from the modal
  const handleAddToPlaylist = async (playlistId) => {
    if (!playerTrack?.id || !playlistId) return;

    try {
      await addTrackToPlaylist(playlistId, playerTrack.id);
      // Optionally show a success message
      setIsPlaylistModalOpen(false); // Close modal on success
    } catch (error) {
      console.error('Error adding track to playlist:', error);
      // Optionally show an error message to the user
    }
  };

  // Handler for opening the video view
  const handleOpenVideoView = () => {
    if (!playerTrack?.id) return; // Need a track to view video for
    if (!playerTrack.mediaType || playerTrack.mediaType !== 'video') {
      // Optionally show a message that this track is not a video
      console.log('This track is not a video.');
      return;
    }
    requireAuth('Đăng nhập để xem video.', () => {
      setIsVideoViewOpen(true);
      setIsInfoPanelOpen(false); // Close info panel if open
    });
  };

  // Handler for opening the media info panel
  const handleOpenInfoPanel = () => {
    if (!playerTrack?.id) return; // Need a track to view info for
    requireAuth('Đăng nhập để xem thông tin media.', () => {
      setIsInfoPanelOpen(true);
      setIsVideoViewOpen(false); // Close info panel if open
    });
  };

  // Handlers for favorite reactions
  const handleToggleFavorite = async (reaction = 'Love') => {
    if (!playerTrack?.id) return;
    if (!isAuthenticated) {
      promptLogin('Đăng nhập để thích bài hát này.');
      return;
    }
    try {
      await MediaService.toggleFavorite(playerTrack.id, reaction);
      setCurrentFavoriteReaction(reaction);
      setIsFavoritePickerOpen(false); // Close picker after action
      setLibraryVersion((v) => v + 1);

      const countResult = await MediaService.getMediaReactionCount(playerTrack.id).catch(() => null);
      setPlayerTrack((curr) => ({
        ...curr,
        favoriteCount: Number(countResult?.totalCount ?? 0),
      }));
    } catch (error) {
      console.error('Error toggling favorite:', error);
      // Optionally show an error message to the user
    }
  };

  const handleSelectFavoriteReaction = async (reaction) => {
    await handleToggleFavorite(reaction);
  };

  const handleUnlikeFavorite = async () => {
    await handleToggleFavorite(null); // null reaction means unlike
  };

  const toggleFavoritePicker = () => {
    setIsFavoritePickerOpen((prev) => !prev);
  };

  return (
    <div className="app-container">
      <div className="main-layout">
        <Sidebar
          key={`${authVersion}-${libraryVersion}`}
          activeItemId={selectedLibraryItem?.id}
          onSelectItem={toggleLibraryItem}
          onAddCreate={() => {
            requireAuth('Đăng nhập để tạo hoặc upload nội dung mới.', () => {
              if (!setBodyModeSafely('manage')) return;
              setSelectedLibraryItem(null);
              setProfileTarget(null);
            });
          }}
        />
        <Home
          audioRef={audioRef}
          activePanel={activePanel}
          onTogglePanel={togglePanel}
          isNowPlayingExpanded={isNowPlayingExpanded}
          selectedLibraryItem={selectedLibraryItem}
          onHomeClick={closeBodyOverlay}
          track={playerTrack} // Pass current track for display
          bodyMode={bodyMode}
          manageInitialTab={manageInitialTab}
          manageInitialEntityId={manageInitialEntityId}
          onClearManageInit={() => {
            setManageInitialTab(null);
            setManageInitialEntityId(null);
          }}
          onBackToHome={() => {
            if (!setBodyModeSafely('home')) return;
            setProfileTarget(null);
          }}
          onOpenProfile={() => {
            requireAuth('Đăng nhập để xem và chỉnh sửa hồ sơ.', () => {
              if (!setBodyModeSafely('profile')) return;
              setSelectedLibraryItem(null);
              setProfileTarget(null);
            });
          }}
          onOpenArtistProfile={(artist) => {
            if (!artist?.id && !artist?.handle) return;
            if (!setBodyModeSafely('profile')) return;
            setSelectedLibraryItem(null);
            setProfileTarget(artist);
          }}
          isAuthenticated={isAuthenticated}
          onRequireAuth={promptLogin}
          onRequestSignup={() => promptAuth('register', '')}
          onLogout={handleLogout}
          isRightPanelOpen={Boolean(activePanel)}
          onPlayMedia={(media, options) => playMediaItem(media, options)} // Pass options for collection/shuffle
          onOpenCollection={openCollectionItem}
          currentUserAvatarUrl={currentUserAvatarUrl}
          profileTarget={profileTarget}
          onProfileDirtyChange={setIsProfileDirty}
          onManageDirtyChange={setIsManageDirty}
          onProfileSaved={(profile) => {
            const avatarUrl = normalizeAssetUrl(profile?.avatarUrl || profile?.AvatarUrl || null);
            const email = String(profile?.email || profile?.Email || '').trim();
            setCurrentUserAvatarUrl(avatarUrl || null);
            setCurrentUserEmail(email);
            if (avatarUrl) {
              localStorage.setItem('user_avatar', avatarUrl);
            } else {
              localStorage.removeItem('user_avatar');
            }
            if (email) {
              localStorage.setItem('user_email', email);
            } else {
              localStorage.removeItem('user_email');
            }
            setIsProfileDirty(false);
          }}
          onOpenChangePassword={() => {
            requireAuth('Đăng nhập để đổi mật khẩu.', () => {
              setAuthPrompt({ isOpen: true, reason: 'Cập nhật mật khẩu cho tài khoản hiện tại.', initialMode: 'change-password' });
            });
          }}
        />
        {activePanel === 'friends' && (
          <FriendActivity
            onClose={() => setActivePanel(null)}
            onOpenProfile={openUserProfileFromPanel}
          />
        )}
        {activePanel === 'notifications' && <NotificationActivity onClose={() => setActivePanel(null)} />}
        {activePanel === 'history' && (
          <ListeningHistoryActivity
            onClose={() => setActivePanel(null)}
            onPlayMedia={(media) => playMediaItem(media)}
          />
        )}
      </div>

      {/* PlayerBar is now always rendered if authenticated and a track is available */}
      {isAuthenticated && playerTrack?.id && playerTrack.id !== fallbackTrack.id ? (
        <PlayerBar
          isExpanded={isNowPlayingExpanded}
          isPlaying={isPlaying}
          onTogglePlay={handleTogglePlay}
          onToggleExpanded={() => {
            requireAuth('Đăng nhập để mở trình phát và nghe nội dung.', () => {
              setIsNowPlayingExpanded((isExpanded) => !isExpanded);
            });
          }}
          onRequireAuth={() => requireAuth('Đăng nhập để nghe nhạc và sử dụng trình phát.')}
          playerTrack={playerTrack}
          onPlayNext={playNextTrack}
          onPlayPrevious={playPreviousTrack}
          isNextDisabled={currentQueueIndex >= playerQueue.length - 1}
          isPreviousDisabled={currentQueueIndex <= 0}
          onVolumeChange={handleVolumeChange}
          volume={volume}
          onToggleMute={handleToggleMute}
          isMuted={isMuted}
          onAddPlaylist={handleOpenPlaylistModal} // Pass handler for add to playlist
          onOpenVideo={handleOpenVideoView} // Pass handler for video view
          onOpenInfo={handleOpenInfoPanel} // Pass handler for info panel
          onToggleFavorite={handleToggleFavorite} // Pass handler for direct favorite click
          currentFavoriteReaction={currentFavoriteReaction} // Pass current favorite reaction
          onSelectFavoriteReaction={handleSelectFavoriteReaction} // Pass handler for selecting other reactions
          availableReactions={availableReactions} // Pass available reactions for the picker
          isFavoritePickerOpen={isFavoritePickerOpen} // Pass state for picker visibility
          onToggleFavoritePicker={toggleFavoritePicker} // Pass handler to toggle picker visibility
        />
      ) : null}

      <VideoPlayerView
        isOpen={isVideoViewOpen}
        onClose={() => setIsVideoViewOpen(false)}
        track={playerTrack}
        audioRef={audioRef}
        isPlaying={isPlaying}
      />

      <MediaInfoPanel
        isOpen={isInfoPanelOpen}
        onClose={() => setIsInfoPanelOpen(false)}
        track={playerTrack}
      />

      <audio ref={audioRef} preload="metadata" />

      <AuthLoginModal
        key={`${authPrompt.isOpen ? 'open' : 'closed'}-${authPrompt.initialMode}-${authVersion}`}
        isOpen={authPrompt.isOpen}
        initialMode={authPrompt.initialMode}
        reason={authPrompt.reason}
        currentUserEmail={currentUserEmail}
        onClose={() => setAuthPrompt({ isOpen: false, reason: '', initialMode: 'login' })}
        onAuthenticated={() => {
          setAuthPrompt({ isOpen: false, reason: '', initialMode: 'login' });
          setAuthVersion((version) => version + 1);
        }}
      />

      {/* Playlist Modal */}
      <PlaylistModal
        isOpen={isPlaylistModalOpen}
        onClose={() => setIsPlaylistModalOpen(false)}
        playlists={userPlaylists}
        onAddTrackToPlaylist={handleAddToPlaylist}
        currentTrackId={playerTrack?.id}
      />

    </div>
  );
}
