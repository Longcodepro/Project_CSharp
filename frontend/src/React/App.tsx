import { useEffect, useRef, useState, type ComponentType } from 'react';
import Sidebar from './Components/Sidebar';
import Home from './Components/Home';
import PlayerBar from './Components/PlayerBar';
import FriendActivity from './Components/FriendActivity';
import NotificationActivity from './Components/NotificationActivity';
import AuthLoginModal from './Components/AuthLoginModal';
import PlaylistModal from './Components/PlaylistModal';
import MediaInfoPanel from './Components/MediaInfoPanel';
import ListeningHistoryActivity from './Components/ListeningHistoryActivity';
import ShareActivity from './Components/ShareActivity';
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
  addTrackToPlaylist, // Import addTrackToPlaylist
  type UserProfile // Import UserProfile type
} from '../../Services/MediaService.tsx';
import '../CSS/App_style.css';

const TypedSidebar = Sidebar as ComponentType<any>;
const TypedHome = Home as ComponentType<any>;
const TypedPlayerBar = PlayerBar as ComponentType<any>;
const TypedFriendActivity = FriendActivity as ComponentType<any>;
const TypedNotificationActivity = NotificationActivity as ComponentType<any>;
const TypedAuthLoginModal = AuthLoginModal as ComponentType<any>;
const TypedPlaylistModal = PlaylistModal as ComponentType<any>;
const TypedMediaInfoPanel = MediaInfoPanel as ComponentType<any>;
const TypedListeningHistoryActivity = ListeningHistoryActivity as ComponentType<any>;
const TypedShareActivity = ShareActivity as ComponentType<any>;


type BodyMode = 'home' | 'profile' | 'manage';
type AuthPromptMode = 'login' | 'register' | 'change-password';
type ActivePanel = 'friends' | 'notifications' | 'history' | 'shares' | null;
type SelectableItem = { id: string;[key: string]: unknown };
type PlaybackOptions = {
  autoplay?: boolean;
  resumeAt?: number;
  collectionId?: string | null;
  collectionType?: string | null;
  shuffle?: boolean;
};
type PlayableMedia = {
  id: string;
  Id?: string;
  title?: string;
  Title?: string;
  artist?: string;
  artistName?: string;
  ownerId?: string;
  OwnerId?: string;
  image?: string;
  coverImageUrl?: string;
  CoverImageUrl?: string;
  audioUrl?: string | null;
  AudioUrl?: string | null;
  videoUrl?: string | null;
  VideoUrl?: string | null;
  mediaType?: string | number;
  type?: string | number;
  collectionId?: string | null;
  CollectionId?: string | null;
  collectionType?: string | null;
  CollectionType?: string | null;
  durationSeconds?: number | string;
  DurationSeconds?: number | string;
  ownerName?: string | null;
  OwnerName?: string | null;
  uploadedAt?: string | null;
  UploadedAt?: string | null;
  releaseDate?: string | null;
  ReleaseDate?: string | null;
  genre?: string | null;
  Genre?: string | null;
  viewCount?: number | string;
  ViewCount?: number | string;
  favoriteCount?: number | string;
  FavoriteCount?: number | string;
  isPublic?: boolean;
  IsPublic?: boolean;
  [key: string]: unknown;
};
type PlayerTrack = {
  id: string;
  title: string;
  artist: string;
  image: string;
  audioUrl: string | null;
  videoUrl: string | null;
  mediaType: string;
  collectionId: string | null;
  collectionType: string | null;
  currentTime: string;
  duration: string;
  durationSeconds: number;
  progress: number;
  favoriteCount?: number;
  viewCount?: number;
  genre?: string | null;
  releaseDate?: string | null;
  ownerName?: string | null;
  isPublic?: boolean;
  canvasUrl?: string | null;
};
type AuthPromptState = {
  isOpen: boolean;
  reason: string;
  initialMode: AuthPromptMode;
};
type ShareDraft = {
  id: string;
  title: string;
  mediaType: string;
};

const fallbackTrack: PlayerTrack = {
  id: 'demo-track',
  title: 'Midnight Resonance',
  artist: 'Lumina Collective',
  image: normalizeAssetUrl('/uploads/default-cover/Default.png')
    || 'https://images.unsplash.com/photo-1614613535308-eb5fbd3d2c17?auto=format&fit=crop&q=80&w=1000',
  audioUrl: null,
  videoUrl: null,
  mediaType: 'audio',
  collectionId: null,
  collectionType: null,
  currentTime: '1:24',
  duration: '4:02',
  durationSeconds: 242,
  progress: 35,
};

function formatTime(seconds = 0) {
  if (!Number.isFinite(seconds) || seconds < 0) return '0:00';
  const totalSeconds = Math.floor(seconds);
  const minutes = Math.floor(totalSeconds / 60);
  const remainingSeconds = String(totalSeconds % 60).padStart(2, '0');
  return `${minutes}:${remainingSeconds}`;
}

function normalizePlaybackSeconds(seconds: number): number {
  if (!Number.isFinite(seconds) || seconds < 0) return 0;
  return Math.floor(seconds);
}

function normalizeMediaTypeName(value: unknown): string {
  const numericMap = ['audio', 'video', '', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';

  const normalized = String(value || '').trim().toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;

  return normalized;
}

function resolveAudioSource(media: PlayableMedia | null | undefined): string | null {
  if (!media?.id) return null;
  if (media.id === fallbackTrack.id) return null;

  const explicitAudioUrl = normalizeAssetUrl(media.audioUrl || media.AudioUrl);
  if (explicitAudioUrl) return explicitAudioUrl;

  const mediaType = normalizeMediaTypeName(media.mediaType ?? media.type);
  if (mediaType === 'video') return mediaVideoStreamUrl(media.id);

  return mediaAudioStreamUrl(media.id);
}

function resolveArtistLabel(media: PlayableMedia | null | undefined): string {
  if (!media) return 'TuneVault';

  return String(
    media.ownerName
    || media.OwnerName
    || media.artist
    || media.Artist
    || media.artistName
    || media.ArtistName
    || media.ownerId
    || media.OwnerId
    || 'TuneVault',
  ).trim() || 'TuneVault';
}

function getMediaSortTimestamp(media: PlayableMedia): number {
  const rawDate = media.uploadedAt || media.UploadedAt || media.releaseDate || media.ReleaseDate;
  if (rawDate) {
    const timestamp = Date.parse(String(rawDate));
    if (Number.isFinite(timestamp)) return timestamp;
  }

  return 0;
}

function getMediaSortNumber(media: PlayableMedia): number {
  const mediaId = String(media.id || media.Id || '');
  const numericId = Number(mediaId.match(/\d+/g)?.join('') || 0);
  return Number.isFinite(numericId) ? numericId : 0;
}

function compareMediaDescending(left: PlayableMedia, right: PlayableMedia): number {
  const timestampDiff = getMediaSortTimestamp(right) - getMediaSortTimestamp(left);
  if (timestampDiff !== 0) return timestampDiff;

  const numericDiff = getMediaSortNumber(right) - getMediaSortNumber(left);
  if (numericDiff !== 0) return numericDiff;

  return String(right.id || right.Id || '').localeCompare(String(left.id || left.Id || ''));
}

function sortDefaultQueueByTypePriority(items: PlayableMedia[], firstMediaType: string): PlayableMedia[] {
  return [...items].sort((left, right) => {
    const leftType = normalizeMediaTypeName(left.mediaType ?? left.type);
    const rightType = normalizeMediaTypeName(right.mediaType ?? right.type);
    const leftSameType = leftType === firstMediaType ? 0 : 1;
    const rightSameType = rightType === firstMediaType ? 0 : 1;

    if (leftSameType !== rightSameType) return leftSameType - rightSameType;

    return compareMediaDescending(left, right);
  });
}

export default function App() {
  const [activePanel, setActivePanel] = useState<ActivePanel>(null);
  const [isNowPlayingExpanded, setIsNowPlayingExpanded] = useState<boolean>(false);
  const [selectedLibraryItem, setSelectedLibraryItem] = useState<SelectableItem | null>(null);
  const [bodyMode, setBodyMode] = useState<BodyMode>('home');
  const [profileTarget, setProfileTarget] = useState<UserProfile | null>(null);

  type NavState = {
    bodyMode: BodyMode;
    selectedLibraryItem: SelectableItem | null;
    isNowPlayingExpanded: boolean;
    profileTarget: UserProfile | null;
  };

  const [navHistory, setNavHistory] = useState<NavState[]>([
    { bodyMode: 'home', selectedLibraryItem: null, isNowPlayingExpanded: false, profileTarget: null }
  ]);
  const [navIndex, setNavIndex] = useState<number>(0);
  const isNavigatingRef = useRef<boolean>(false);

  useEffect(() => {
    if (isNavigatingRef.current) {
      isNavigatingRef.current = false;
      return;
    }

    const currentState: NavState = {
      bodyMode,
      selectedLibraryItem,
      isNowPlayingExpanded,
      profileTarget
    };

    const previousState = navHistory[navIndex];
    if (previousState &&
      previousState.bodyMode === currentState.bodyMode &&
      previousState.selectedLibraryItem?.id === currentState.selectedLibraryItem?.id &&
      previousState.isNowPlayingExpanded === currentState.isNowPlayingExpanded &&
      previousState.profileTarget?.id === currentState.profileTarget?.id) {
      return;
    }

    const newHistory = navHistory.slice(0, navIndex + 1);
    newHistory.push(currentState);
    setNavHistory(newHistory);
    setNavIndex(newHistory.length - 1);
  }, [bodyMode, selectedLibraryItem, isNowPlayingExpanded, profileTarget]);

  const handleNavBack = () => {
    if (navIndex > 0) {
      const prevIndex = navIndex - 1;
      const prevState = navHistory[prevIndex];
      isNavigatingRef.current = true;
      setNavIndex(prevIndex);
      setBodyMode(prevState.bodyMode);
      setSelectedLibraryItem(prevState.selectedLibraryItem);
      setIsNowPlayingExpanded(prevState.isNowPlayingExpanded);
      setProfileTarget(prevState.profileTarget);
    }
  };

  const handleNavForward = () => {
    if (navIndex < navHistory.length - 1) {
      const nextIndex = navIndex + 1;
      const nextState = navHistory[nextIndex];
      isNavigatingRef.current = true;
      setNavIndex(nextIndex);
      setBodyMode(nextState.bodyMode);
      setSelectedLibraryItem(nextState.selectedLibraryItem);
      setIsNowPlayingExpanded(nextState.isNowPlayingExpanded);
      setProfileTarget(nextState.profileTarget);
    }
  };

  const canNavBack = navIndex > 0;
  const canNavForward = navIndex < navHistory.length - 1;
  const [authPrompt, setAuthPrompt] = useState<AuthPromptState>({ isOpen: false, reason: '', initialMode: 'login' });
  const [authVersion, setAuthVersion] = useState(0);
  const [playerTrack, setPlayerTrack] = useState<PlayerTrack>(fallbackTrack);
  const [playerQueue, setPlayerQueue] = useState<PlayableMedia[]>([]); // New state for the queue
  const [currentQueueIndex, setCurrentQueueIndex] = useState<number>(-1); // Index of the current track in the queue
  const [lastCompletedQueueIndex, setLastCompletedQueueIndex] = useState<number | null>(null);
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const [volume, setVolume] = useState<number>(() => {
    const savedVolume = localStorage.getItem('player_volume');
    return savedVolume !== null ? parseFloat(savedVolume) : 1;
  });
  const [isMuted, setIsMuted] = useState<boolean>(false);
  const [currentUserAvatarUrl, setCurrentUserAvatarUrl] = useState<string | null>(null);
  const [currentUserEmail, setCurrentUserEmail] = useState<string>('');
  const [isProfileDirty, setIsProfileDirty] = useState<boolean>(false);
  const [isManageDirty, setIsManageDirty] = useState<boolean>(false);
  const [isPlaylistModalOpen, setIsPlaylistModalOpen] = useState<boolean>(false); // State for playlist modal
  const [userPlaylists, setUserPlaylists] = useState<Record<string, unknown>[]>([]); // State for user's playlists
  const [isInfoPanelOpen, setIsInfoPanelOpen] = useState<boolean>(false); // State for info panel
  const [isFavoriteActive, setIsFavoriteActive] = useState<boolean>(false);
  const [isFavoriteActionLoading, setIsFavoriteActionLoading] = useState<boolean>(false);
  const [favoriteActionError, setFavoriteActionError] = useState<string>('');
  const [shareDraft, setShareDraft] = useState<ShareDraft | null>(null);
  const [playerError, setPlayerError] = useState<string>('');
  const [manageInitialTab, setManageInitialTab] = useState<string | null>(null);
  const [manageInitialEntityId, setManageInitialEntityId] = useState<string | null>(null);
  const [libraryVersion, setLibraryVersion] = useState<number>(0);
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const pendingSeekRef = useRef<number | null>(null);
  const pendingAutoPlayRef = useRef<boolean>(false);
  const currentAudioObjectUrlRef = useRef<string | null>(null);
  const currentAudioMediaIdRef = useRef<string | null>(null);
  const lastKnownPlaybackSecondsRef = useRef<number>(0);
  const favoriteErrorTimerRef = useRef<number | null>(null);

  const isAuthenticated = Boolean(localStorage.getItem('auth_session'));

  const promptAuth = (initialMode: AuthPromptMode = 'login', reason = 'Bạn cần đăng nhập để tiếp tục thao tác này.') => {
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

  const setBodyModeSafely = (nextMode: BodyMode): boolean => {
    if (nextMode === bodyMode) return true;
    if (!confirmLeaveCurrentBody()) return false;

    setBodyMode(nextMode);
    return true;
  };

  const requireAuth = (reason: string, action?: () => void | Promise<void>): boolean => {
    if (!localStorage.getItem('auth_session')) {
      promptLogin(reason);
      return false;
    }

    action?.();
    return true;
  };

  const persistPlaybackPosition = async (stoppedAt?: number): Promise<void> => {
    if (!isAuthenticated) return;
    const currentTrack = playerQueue[currentQueueIndex] || (playerTrack?.id ? playerTrack : null);
    if (!currentTrack?.id || currentTrack.id === fallbackTrack.id) return;

    const audio = audioRef.current;
    const fallbackStoppedAt = Number.isFinite(audio?.currentTime)
      ? normalizePlaybackSeconds(Number(audio?.currentTime ?? 0))
      : lastKnownPlaybackSecondsRef.current;
    const nextStoppedAt = Number.isFinite(stoppedAt as number)
      ? normalizePlaybackSeconds(Number(stoppedAt ?? 0))
      : Math.max(0, fallbackStoppedAt);

    await recordPlaybackStop(currentTrack.id, nextStoppedAt).catch(() => null);
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
    setIsInfoPanelOpen(false); // Close info panel on logout
    setIsFavoriteActive(false);
    setFavoriteActionError('');
    setAuthVersion((version) => version + 1);
  };

  useEffect(() => {
    if (!isAuthenticated) return undefined;

    let cancelled = false;

    const syncCurrentUserProfile = async () => {
      try {
        const profile = (await getMyProfile().catch(() => null)) as UserProfile | null;
        if (cancelled) return;

        const avatarUrl = normalizeAssetUrl(
          profile?.avatarUrl
          || profile?.AvatarUrl
          || profile?.avatar
          || profile?.Avatar
          || profile?.avatarPath
          || profile?.AvatarPath
          || null,
        );
        const email = String(profile?.email || profile?.Email || '').trim();
        setCurrentUserAvatarUrl(typeof avatarUrl === 'string' ? avatarUrl : null);
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

  useEffect(() => {
    if (!isAuthenticated) return undefined;

    let cancelled = false;

    const restoreRecentPlayback = async () => {
      try {
        const recentHistory = await getRecentHistory().catch(() => []);
        if (cancelled) return;

        const recentTrack = Array.isArray(recentHistory) ? recentHistory[0] : null;
        if (!recentTrack?.id) return;

        const resumeInfo = await getResumeInfo(recentTrack.id).catch(() => null);
        if (cancelled) return;

        const resumeAt = Math.max(0, Number(resumeInfo?.stoppedAt ?? 0));
        const recentMedia = await MediaService.getMediaById(recentTrack.id).catch(() => recentTrack);
        if (cancelled || !recentMedia?.id) return;

        const started = await startPlaybackForTrack(
          recentMedia as PlayableMedia,
          [recentMedia as PlayableMedia],
          0,
          { autoplay: false, resumeAt },
        );

        if (!started || cancelled) return;

        setPlayerError('');
        setIsPlaying(false);
        if (normalizeMediaTypeName((recentMedia as PlayableMedia).mediaType ?? (recentMedia as PlayableMedia).type) === 'video') {
          setIsNowPlayingExpanded(true);
        }
      } catch (error) {
        console.error('[TuneVault] Error restoring recent playback:', error);
      }
    };

    void restoreRecentPlayback();

    return () => {
      cancelled = true;
    };
  }, [authVersion, isAuthenticated]);

  const togglePanel = (panelName: string): void => {
    if (panelName === 'friends' || panelName === 'notifications' || panelName === 'shares' || panelName === 'history') {
      const authReason = panelName === 'shares'
        ? 'Đăng nhập để xem và gửi chia sẻ nội dung.'
        : panelName === 'history'
          ? 'Đăng nhập để xem lịch sử nghe của bạn.'
          : 'Đăng nhập để xem bạn bè và thông báo của bạn.';
      requireAuth(authReason, () => {
        setActivePanel((currentPanel) => (currentPanel === panelName ? null : panelName));
      });
      return;
    }

    setActivePanel((currentPanel) => (currentPanel === panelName ? null : panelName));
  };

  const toggleLibraryItem = (item: SelectableItem): void => {
    if (item?.type === 'media') {
      if (!setBodyModeSafely('manage')) return;
      setManageInitialTab(item.mediaKind as string || 'song');
      setManageInitialEntityId(item.id);
      setSelectedLibraryItem(null);
      setProfileTarget(null);
      setIsNowPlayingExpanded(false);
      return;
    }

    if (item && ['song', 'video', 'audio'].includes(String(item.type).toLowerCase())) {
      MediaService.getMediaById(item.id)
        .then((fullMedia) => {
          if (fullMedia) {
            playMediaItem(fullMedia as any);
          }
        })
        .catch((err) => {
          console.error('[TuneVault] Error fetching media item details:', err);
          playMediaItem(item as any);
        });
      return;
    }

    setSelectedLibraryItem((currentItem) => (currentItem?.id === item.id ? null : item));
  };

  const openCollectionItem = (item: SelectableItem): void => {
    if (!item?.id) return;
    setSelectedLibraryItem(item);
    setBodyMode('home');
    setIsNowPlayingExpanded(false);
  };
  const openUserProfileFromPanel = (user: UserProfile | null | undefined) => {
    if (!user?.id && !user?.handle && !user?.idDisplay) return;
    if (!setBodyModeSafely('profile')) return;
    setSelectedLibraryItem(null);
    setIsNowPlayingExpanded(false);
    setActivePanel(null);
    setProfileTarget(user);
  };

  const fetchCollectionItems = async (collectionId: string, collectionType: string): Promise<PlayableMedia[]> => {
    try {
      return (await getMediaCollection(collectionId, collectionType)) as PlayableMedia[] || [];
    } catch (error) {
      console.error(`Error fetching collection items for ${collectionType} ${collectionId}:`, error);
      return [];
    }
  };

  const startPlaybackForTrack = async (
    track: PlayableMedia,
    nextQueue: PlayableMedia[],
    nextQueueIndex: number,
    options: PlaybackOptions = {},
  ): Promise<boolean> => {
    const { autoplay = true, resumeAt = 0 } = options;
    if (!track?.id) return false;

    const nextAudioSource = resolveAudioSource(track);
    if (!nextAudioSource) {
      setPlayerError('Media nay chua co stream hop le de phat.');
      return false;
    }

    setPlayerError('');

    setPlayerQueue(nextQueue);
    setCurrentQueueIndex(nextQueueIndex);
    pendingAutoPlayRef.current = Boolean(autoplay);
    pendingSeekRef.current = normalizePlaybackSeconds(Number(resumeAt || 0));
    lastKnownPlaybackSecondsRef.current = normalizePlaybackSeconds(Number(resumeAt || 0));

    setPlayerTrack({
      id: track.id,
      title: track.title || track.Title || fallbackTrack.title,
      artist: resolveArtistLabel(track),
      image: track.image || normalizeAssetUrl(track.coverImageUrl || track.CoverImageUrl) || fallbackTrack.image,
      audioUrl: nextAudioSource,
      videoUrl: normalizeAssetUrl(track.videoUrl || track.VideoUrl) || '',
      mediaType: normalizeMediaTypeName(track.mediaType ?? track.type),
      collectionId: track.collectionId || track.CollectionId || null,
      collectionType: track.collectionType || track.CollectionType || null,
      genre: track.genre || track.Genre || null,
      releaseDate: track.releaseDate || track.ReleaseDate || null,
      ownerName: track.ownerName || track.OwnerName || null,
      isPublic: typeof track.isPublic === 'boolean' ? track.isPublic : typeof track.IsPublic === 'boolean' ? track.IsPublic : undefined,
      viewCount: Number(track.viewCount || track.ViewCount || 0),
      currentTime: formatTime(resumeAt),
      duration: formatTime(Number(track.durationSeconds || track.DurationSeconds || 0)),
      durationSeconds: Number(track.durationSeconds || track.DurationSeconds || 0),
      progress: 0,
    });

    try {
      await ensureAudioSource({ ...track, audioUrl: nextAudioSource });
      setPlayerError('');
    } catch (error) {
      console.warn('[TuneVault] Không tải được audio stream.', error);
      setPlayerError(error instanceof Error ? error.message : 'Khong the tai stream audio. Vui long thu lai.');
      setIsPlaying(false);
      return false;
    }

    return true;
  };

  const playQueueTrackAtIndex = async (
    queueIndex: number,
    options: Pick<PlaybackOptions, 'resumeAt'> = {},
  ): Promise<void> => {
    const track = playerQueue[queueIndex];
    if (!track) return;

    const started = await startPlaybackForTrack(track, playerQueue, queueIndex, {
      autoplay: true,
      resumeAt: options.resumeAt ?? 0,
    });

    if (!started) return;

    const audio = audioRef.current;
    if (!audio) return;

    await recordPlayHistory(track.id).catch(() => null);

    if (audio.readyState < 1) return;

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
  };

  const playMediaItem = async (media: PlayableMedia, options: PlaybackOptions = {}): Promise<void> => {
    const { collectionId, collectionType, shuffle = false, autoplay = true, resumeAt: requestedResumeAt = 0 } = options;

    if (!media?.id) return;

    if (!isAuthenticated) {
      promptLogin('Đăng nhập để phát bài hát này.');
      return;
    }

    setLastCompletedQueueIndex(null);

    const mediaType = normalizeMediaTypeName(media.mediaType ?? media.type);
    const hasAudioSource = resolveAudioSource(media);

    if (mediaType === 'video' && !hasAudioSource) {
      console.warn('[TuneVault] Cannot play video without an audio source.');
      return;
    }

    audioRef.current?.pause();
    setIsPlaying(false);

    let queue: PlayableMedia[] = [];
    let initialIndex = 0;

    if (collectionId && collectionType) {
      const collectionItems = await fetchCollectionItems(collectionId, collectionType);
      if (collectionItems.length > 0) {
        queue = shuffle ? [...collectionItems].sort(() => Math.random() - 0.5) : collectionItems;
        initialIndex = queue.findIndex((item: PlayableMedia) => item.id === media.id);
        if (initialIndex < 0) {
          queue = [media];
          initialIndex = 0;
        }
      } else {
        queue = [media];
      }
    } else {
      try {
        const allMedia = (await getMedia(1, 50).catch(() => [])) as PlayableMedia[];
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
              viewCount: Number(item.viewCount || item.ViewCount || 0),
              favoriteCount: Number(item.favoriteCount || item.FavoriteCount || 0),
              genre: item.genre || item.Genre || null,
              releaseDate: item.releaseDate || item.ReleaseDate || null,
              ownerName: item.ownerName || item.OwnerName || null,
              isPublic: typeof item.isPublic === 'boolean' ? item.isPublic : typeof item.IsPublic === 'boolean' ? item.IsPublic : undefined,
            } as PlayableMedia;
          });

          const requestedMedia = {
            ...media,
            id: String(media.id || media.Id),
            title: media.title || media.Title || fallbackTrack.title,
            artist: resolveArtistLabel(media),
            image: media.image || normalizeAssetUrl(media.coverImageUrl || media.CoverImageUrl) || fallbackTrack.image,
            audioUrl: resolveAudioSource(media),
            videoUrl: normalizeAssetUrl(media.videoUrl || media.VideoUrl) || '',
            mediaType,
            durationSeconds: Number(media.durationSeconds || media.DurationSeconds || 0),
            viewCount: Number(media.viewCount || media.ViewCount || 0),
            favoriteCount: Number(media.favoriteCount || media.FavoriteCount || 0),
            genre: media.genre || media.Genre || null,
            releaseDate: media.releaseDate || media.ReleaseDate || null,
            ownerName: media.ownerName || media.OwnerName || null,
            isPublic: typeof media.isPublic === 'boolean' ? media.isPublic : typeof media.IsPublic === 'boolean' ? media.IsPublic : undefined,
          } as PlayableMedia;

          const mediaById = new Map<string, PlayableMedia>();
          [...normalizedMedia, requestedMedia].forEach((item) => {
            if (item?.id) {
              mediaById.set(String(item.id), item);
            }
          });

          queue = sortDefaultQueueByTypePriority([...mediaById.values()], mediaType);
          initialIndex = queue.findIndex((item: PlayableMedia) => item.id === requestedMedia.id);
          if (initialIndex < 0) initialIndex = 0;
        } else {
          queue = [media];
        }
      } catch {
        queue = [media];
      }
    }

    const currentTrack = queue[initialIndex] as PlayableMedia;
    let resumeAt = Math.max(0, Number(requestedResumeAt || 0));
    if (!resumeAt) {
      const resumeInfo = await getResumeInfo(currentTrack.id).catch(() => null);
      resumeAt = Math.max(0, Number(resumeInfo?.stoppedAt ?? 0));
    }

    const started = await startPlaybackForTrack(currentTrack, queue, initialIndex, { autoplay, resumeAt });
    if (!started) {
      setIsPlaying(false);
      pendingAutoPlayRef.current = false;
      return;
    }

    if (!autoplay) {
      setIsPlaying(false);
      return;
    }

    const audio = audioRef.current;
    if (!audio) return;

    audio.muted = true;
    await recordPlayHistory(currentTrack.id).catch(() => null);
    if (audio.readyState < 1) return;
    pendingAutoPlayRef.current = false;
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

    if (mediaType === 'video') {
      setIsNowPlayingExpanded(true);
    }
  };

  const syncTrackFromMedia = (media: PlayableMedia | null | undefined, resumeAt = 0): void => {
    if (!media) return;

    const durationSeconds = Number(media.durationSeconds || media.DurationSeconds || 0);
    const currentSeconds = Math.max(0, Number(resumeAt || 0));
    const safeDurationSeconds = Number.isFinite(durationSeconds) ? durationSeconds : 0;
    const safeCurrentSeconds = Number.isFinite(currentSeconds) ? currentSeconds : 0;
    const progress = safeDurationSeconds > 0
      ? Math.min(100, Math.round((safeCurrentSeconds / safeDurationSeconds) * 100))
      : 0;

    const artistLabel = resolveArtistLabel(media);
    const audioSource = resolveAudioSource(media);

    setPlayerTrack({
      id: media.id,
      title: media.title || media.Title || fallbackTrack.title,
      artist: artistLabel,
      image: media.image || normalizeAssetUrl(media.coverImageUrl || media.CoverImageUrl) || fallbackTrack.image,
      audioUrl: audioSource,
      videoUrl: normalizeAssetUrl(media.videoUrl || media.VideoUrl) || '',
      canvasUrl: normalizeAssetUrl((media.canvasUrl as string | null) || (media.CanvasUrl as string | null)) || null,
      mediaType: normalizeMediaTypeName(media.mediaType ?? media.type),
      collectionId: media.collectionId || media.CollectionId || null,
      collectionType: media.collectionType || media.CollectionType || null,
      genre: media.genre || media.Genre || null,
      releaseDate: media.releaseDate || media.ReleaseDate || null,
      ownerName: media.ownerName || media.OwnerName || null,
      isPublic: typeof media.isPublic === 'boolean' ? media.isPublic : typeof media.IsPublic === 'boolean' ? media.IsPublic : undefined,
      viewCount: Number(media.viewCount || media.ViewCount || 0),
      currentTime: formatTime(currentSeconds),
      duration: formatTime(durationSeconds),
      durationSeconds: safeDurationSeconds,
      progress,
    });

    pendingSeekRef.current = safeCurrentSeconds > 0 ? safeCurrentSeconds : null;
  };

  const revokeCurrentAudioObjectUrl = () => {
    if (currentAudioObjectUrlRef.current) {
      URL.revokeObjectURL(currentAudioObjectUrlRef.current);
      currentAudioObjectUrlRef.current = null;
    }
  };

  const ensureAudioSource = async (media: PlayableMedia | null | undefined): Promise<void> => {
    const mediaId = media?.id;
    if (!mediaId) {
      throw new Error('Khong tim thay media de phat.');
    }

    const nextSource = resolveAudioSource(media);
    if (!nextSource) {
      throw new Error('Media nay chua co audio stream.');
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
      const audioElement = audioRef.current;
      if (!audioElement) {
        URL.revokeObjectURL(objectUrl);
        throw new Error('Trinh phat am thanh chua san sang. Vui long thu lai.');
      }

      currentAudioObjectUrlRef.current = objectUrl;
      currentAudioMediaIdRef.current = mediaId;
      audioElement.src = objectUrl;
      audioElement.load();
    } catch (error) {
      pendingAutoPlayRef.current = false;
      setIsPlaying(false);
      throw error;
    }
  };

  const handleVolumeChange = (newVolume: number): void => {
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

  const handleToggleMute = (): void => {
    const audio = audioRef.current;
    if (audio) {
      if (isMuted) {
        audio.muted = false;
        setIsMuted(false);
        if (volume === 0) {
          audio.volume = 0.5;
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

  const handleSeek = (timeInSeconds: number): void => {
    const audio = audioRef.current;
    if (audio && Number.isFinite(timeInSeconds)) {
      audio.currentTime = timeInSeconds;
      lastKnownPlaybackSecondsRef.current = normalizePlaybackSeconds(Number(timeInSeconds || 0));
      setPlayerTrack((current) => {
        const durationSeconds = Number(current.durationSeconds || 0);
        const safeTime = normalizePlaybackSeconds(Number(timeInSeconds || 0));
        return {
          ...current,
          currentTime: formatTime(safeTime),
          progress: durationSeconds > 0
            ? Math.min(100, Math.round((safeTime / durationSeconds) * 100))
            : current.progress,
        };
      });
      void persistPlaybackPosition(timeInSeconds);
    }
  };

  const handleTogglePlay = async (): Promise<void> => {
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
      setPlayerError('');
    } catch (error) {
      console.warn('[TuneVault] Không thể phát media hiện tại.', error);
      setPlayerError(error instanceof Error ? error.message : 'Khong the phat media hien tai.');
      setIsPlaying(false);
      return;
    }

    if (audio.paused) {
      try {
        await recordPlayHistory(currentTrack.id).catch(() => null);
        audio.muted = false;
        await audio.play();
        setPlayerError('');
        setIsPlaying(true);
      } catch {
        setPlayerError('Khong the phat audio. Vui long thu lai.');
        setIsPlaying(false);
      }
      return;
    }

    audio.pause();
    setIsPlaying(false);
  };

  const advanceToNextTrack = async (): Promise<void> => {
    if (playerQueue.length === 0 || currentQueueIndex < 0) return;
    await persistPlaybackPosition();
    const nextIndex = currentQueueIndex + 1;
    if (nextIndex >= playerQueue.length) {
      audioRef.current?.pause();
      setIsPlaying(false);
      return;
    }

    await playQueueTrackAtIndex(nextIndex);
  };

  const playNextTrack = async (): Promise<void> => {
    setLastCompletedQueueIndex(null);
    await advanceToNextTrack();
  };

  const playPreviousTrack = async (): Promise<void> => {
    if (playerQueue.length === 0 || currentQueueIndex < 0) return;

    const completedIndex = lastCompletedQueueIndex;
    const shouldReplayCompletedTrack = completedIndex !== null
      && (currentQueueIndex === completedIndex || currentQueueIndex === completedIndex + 1);
    const prevIndex = shouldReplayCompletedTrack ? completedIndex : currentQueueIndex - 1;
    if (prevIndex < 0) return;

    setLastCompletedQueueIndex(null);
    await persistPlaybackPosition();
    await playQueueTrackAtIndex(prevIndex, { resumeAt: 0 });
  };


  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return undefined;

    const syncFromAudio = () => {
      setPlayerTrack((current) => {
        const durationSeconds = Number.isFinite(audio.duration) ? audio.duration : current.durationSeconds;
        const currentSeconds = normalizePlaybackSeconds(audio.currentTime || 0);
        lastKnownPlaybackSecondsRef.current = currentSeconds;
        const progress = durationSeconds > 0
          ? Math.min(100, Math.round((currentSeconds / durationSeconds) * 100))
          : current.progress;

        return {
          ...current,
          currentTime: formatTime(currentSeconds),
          duration: Number.isFinite(durationSeconds) ? formatTime(durationSeconds) : current.duration,
          durationSeconds: Number.isFinite(durationSeconds) ? durationSeconds : current.durationSeconds,
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
            setPlayerError('');
            window.setTimeout(() => {
              if (audioRef.current) {
                audioRef.current.muted = false;
              }
            }, 0);
          })
          .catch(() => {
            audio.muted = false;
            setPlayerError('Khong the tu dong phat audio. Vui long bam Phat.');
            setIsPlaying(false);
          });
      }
    };

    const onTimeUpdate = syncFromAudio;
    const onPlay = () => setIsPlaying(true);
    const onPause = () => {
      setIsPlaying(false);
      void persistPlaybackPosition(normalizePlaybackSeconds(audio.currentTime || lastKnownPlaybackSecondsRef.current));
    };
    const onEnded = () => {
      setIsPlaying(false);
      setLastCompletedQueueIndex(currentQueueIndex);
      void advanceToNextTrack();
    };

    audio.addEventListener('loadedmetadata', onLoadedMetadata);
    audio.addEventListener('timeupdate', onTimeUpdate);
    audio.addEventListener('play', onPlay);
    audio.addEventListener('pause', onPause);
    audio.addEventListener('ended', onEnded);

    const persistProgress = () => {
      if (!isAuthenticated || !playerTrack.id || playerTrack.id === fallbackTrack.id) return;
      const stoppedAt = Number.isFinite(audio.currentTime)
        ? normalizePlaybackSeconds(audio.currentTime)
        : lastKnownPlaybackSecondsRef.current;
      recordPlaybackStop(playerTrack.id, stoppedAt).catch(() => null);
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState !== 'hidden') return;
      persistProgress();
    };

    const handlePageHide = () => {
      persistProgress();
    };

    window.addEventListener('beforeunload', persistProgress);
    window.addEventListener('pagehide', handlePageHide);
    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      audio.removeEventListener('loadedmetadata', onLoadedMetadata);
      audio.removeEventListener('timeupdate', onTimeUpdate);
      audio.removeEventListener('play', onPlay);
      audio.removeEventListener('pause', onPause);
      audio.removeEventListener('ended', onEnded);
      window.removeEventListener('beforeunload', persistProgress);
      window.removeEventListener('pagehide', handlePageHide);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [isAuthenticated, playerTrack.id, playerQueue, currentQueueIndex]);

  useEffect(() => {
    if (playerTrack?.id && playerTrack.id !== fallbackTrack.id && isPlaying) {
      const trackId = playerTrack.id;
      const now = Date.now();
      try {
        let history = JSON.parse(localStorage.getItem('listening_history') || '[]');
        if (!Array.isArray(history)) history = [];
        history = history.filter((item: any) => item.id !== trackId);
        history.unshift({ id: trackId, timestamp: now });
        const cutoff = now - 259200000;
        history = history.filter((item: any) => item.timestamp >= cutoff);
        localStorage.setItem('listening_history', JSON.stringify(history));
      } catch (e) {
        console.error('Error saving history:', e);
      }
    }
  }, [playerTrack?.id, isPlaying]);

  useEffect(() => () => {
    revokeCurrentAudioObjectUrl();
  }, []);

  useEffect(() => {
    let cancelled = false;

    const syncFavoriteState = async () => {
      if (!isAuthenticated || !playerTrack?.id || playerTrack.id === fallbackTrack.id) {
        if (!cancelled) {
          setIsFavoriteActive(false);
          setFavoriteActionError('');
        }
        return;
      }

      try {
        const [status, countResult] = await Promise.all([
          MediaService.getFavoriteStatus(playerTrack.id),
          MediaService.getMediaReactionCount(playerTrack.id).catch(() => null),
        ]);

        if (cancelled) return;

        setIsFavoriteActive(Boolean(status));
        setPlayerTrack((curr) => ({
          ...curr,
          favoriteCount: Number(countResult?.totalCount ?? 0),
        }));
      } catch (error) {
        console.error(`Error fetching favorite status/count for ${playerTrack.id}:`, error);
        if (!cancelled) {
          setIsFavoriteActive(false);
        }
      }
    };

    void syncFavoriteState();

    return () => {
      cancelled = true;
    };
  }, [authVersion, isAuthenticated, playerTrack.id]);

  const closeBodyOverlay = () => {
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

  const handleOpenPlaylistModal = (): void => {
    if (!isAuthenticated) {
      promptLogin('Đăng nhập để thêm bài hát vào playlist.');
      return;
    }
    if (!playerTrack?.id) return;

    requireAuth('Đăng nhập để xem danh sách playlist của bạn.', async () => {
      try {
        const playlists = await getMyPlaylists();
        setUserPlaylists(playlists as Record<string, unknown>[]);
        setIsPlaylistModalOpen(true);
      } catch (error) {
        console.error('Error fetching user playlists:', error);
      }
    });
  };

  const handleAddToPlaylist = async (playlistId: string): Promise<void> => {
    if (!playerTrack?.id || !playlistId) return;

    try {
      await addTrackToPlaylist(playlistId, playerTrack.id);
      setIsPlaylistModalOpen(false);
    } catch (error) {
      console.error('Error adding track to playlist:', error);
    }
  };

  const handleOpenVideoView = (): void => {
    if (!playerTrack?.id) return;
    requireAuth('Đăng nhập để xem video.', () => {
      setIsNowPlayingExpanded(true);
      setSelectedLibraryItem(null);
      setBodyMode('home');
    });
  };

  useEffect(() => () => {
    if (favoriteErrorTimerRef.current) {
      window.clearTimeout(favoriteErrorTimerRef.current);
    }
  }, []);

  const showFavoriteError = (message: string): void => {
    setFavoriteActionError(message);
    if (favoriteErrorTimerRef.current) {
      window.clearTimeout(favoriteErrorTimerRef.current);
    }
    favoriteErrorTimerRef.current = window.setTimeout(() => {
      setFavoriteActionError('');
      favoriteErrorTimerRef.current = null;
    }, 2600);
  };

  const handleFavoriteAction = async (): Promise<void> => {
    if (!playerTrack?.id) return;
    if (!isAuthenticated) {
      promptLogin('Đăng nhập để thích bài hát này.');
      return;
    }

    setIsFavoriteActionLoading(true);
    setFavoriteActionError('');

    try {
      const nextActive = !isFavoriteActive;
      await MediaService.toggleFavorite(playerTrack.id, nextActive);
      setIsFavoriteActive(nextActive);
      setLibraryVersion((v) => v + 1);

      const countResult = await MediaService.getMediaReactionCount(playerTrack.id).catch(() => null);
      setPlayerTrack((curr) => ({
        ...curr,
        favoriteCount: Number(countResult?.totalCount ?? 0),
      }));
    } catch (error) {
      console.error('Error toggling favorite:', error);
      showFavoriteError('Không thể cập nhật cảm xúc. Vui lòng thử lại.');
    } finally {
      setIsFavoriteActionLoading(false);
    }
  };

  const handleOpenSharePicker = (): void => {
    if (!playerTrack?.id || playerTrack.id === fallbackTrack.id) {
      return;
    }

    if (!isAuthenticated) {
      promptLogin('Đăng nhập để chia sẻ nội dung.');
      return;
    }

    setShareDraft({
      id: playerTrack.id,
      title: playerTrack.title,
      mediaType: playerTrack.mediaType,
    });
    setActivePanel('friends');
  };

  const handleShareCurrentTrack = async (request: { receiverId: string; message?: string | null }): Promise<void> => {
    if (!playerTrack?.id || playerTrack.id === fallbackTrack.id) {
      throw new Error('Chưa có nội dung để chia sẻ.');
    }

    if (!isAuthenticated) {
      promptLogin('Đăng nhập để chia sẻ nội dung.');
      throw new Error('Bạn cần đăng nhập để chia sẻ nội dung.');
    }

    const mediaType = normalizeMediaTypeName(playerTrack.mediaType);
    await MediaService.shareItem({
      receiverId: request.receiverId,
      sharedItemId: playerTrack.id,
      shareType: mediaType === 'video' ? 'video' : 'song',
      message: request.message || null,
    });

    setShareDraft(null);
    setActivePanel(null);
  };

  return (
    <div className="app-container">
      <div className="main-layout">
        <TypedSidebar
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
        <TypedHome
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
          onNavBack={handleNavBack}
          onNavForward={handleNavForward}
          canNavBack={canNavBack}
          canNavForward={canNavForward}
          isPlaying={isPlaying}
          onTogglePlay={handleTogglePlay}
          onPlayNext={playNextTrack}
          onPlayPrevious={playPreviousTrack}
          isNextDisabled={currentQueueIndex >= playerQueue.length - 1}
          isPreviousDisabled={currentQueueIndex <= 0 && lastCompletedQueueIndex === null}
          onSeek={handleSeek}
          onOpenProfile={() => {
            requireAuth('Đăng nhập để xem và chỉnh sửa hồ sơ.', () => {
              if (!setBodyModeSafely('profile')) return;
              setSelectedLibraryItem(null);
              setProfileTarget(null);
            });
          }}
          onOpenArtistProfile={(artist: UserProfile) => {
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
          onPlayMedia={(media: PlayableMedia, options: PlaybackOptions) => playMediaItem(media, options)} // Pass options for collection/shuffle
          onOpenCollection={openCollectionItem}
          currentUserAvatarUrl={currentUserAvatarUrl}
          profileTarget={profileTarget}
          onProfileDirtyChange={setIsProfileDirty}
          onManageDirtyChange={setIsManageDirty}
          onProfileSaved={(profile: UserProfile) => {
            const avatarUrl = normalizeAssetUrl(
              profile?.avatarUrl
              || profile?.AvatarUrl
              || profile?.avatar
              || profile?.Avatar
              || profile?.avatarPath
              || profile?.AvatarPath
              || null,
            );
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
          <TypedFriendActivity
            onClose={() => {
              setShareDraft(null);
              setActivePanel(null);
            }}
            onOpenProfile={openUserProfileFromPanel}
            shareMode={Boolean(shareDraft)}
            shareItemTitle={shareDraft?.title || ''}
            shareItemType={shareDraft?.mediaType || ''}
            onShareConfirm={handleShareCurrentTrack}
            onCancelShare={() => {
              setShareDraft(null);
              setActivePanel(null);
            }}
          />
        )}
        {activePanel === 'notifications' && (
          <TypedNotificationActivity
            onClose={() => setActivePanel(null)}
            onOpenProfile={openUserProfileFromPanel}
          />
        )}
        {activePanel === 'history' && (
          <TypedListeningHistoryActivity
            onClose={() => setActivePanel(null)}
            onPlayMedia={(media: PlayableMedia) => playMediaItem(media)}
          />
        )}
        {activePanel === 'shares' && (
          <TypedShareActivity
            onClose={() => setActivePanel(null)}
            onPlayMedia={(media: PlayableMedia) => playMediaItem(media)}
            onOpenProfile={openUserProfileFromPanel}
          />
        )}
      </div>

      {/* PlayerBar is now always rendered if authenticated and a track is available */}
      {isAuthenticated && playerTrack?.id && playerTrack.id !== fallbackTrack.id ? (
        <TypedPlayerBar
          isExpanded={isNowPlayingExpanded}
          isPlaying={isPlaying}
          onTogglePlay={handleTogglePlay}
          onToggleExpanded={() => {
            requireAuth('Đăng nhập để mở trình phát và nghe nội dung.', () => {
              setIsNowPlayingExpanded((isExpanded) => !isExpanded);
            });
          }}
          onRequireAuth={() => requireAuth('Đăng nhập để nghe nhạc và sử dụng trình phát.')}
          onSeek={handleSeek}
          playerTrack={playerTrack}
          onPlayNext={playNextTrack}
          onPlayPrevious={playPreviousTrack}
          isNextDisabled={currentQueueIndex >= playerQueue.length - 1}
          isPreviousDisabled={currentQueueIndex <= 0 && lastCompletedQueueIndex === null}
          onVolumeChange={handleVolumeChange}
          volume={volume}
          onToggleMute={handleToggleMute}
          isMuted={isMuted}
          onAddPlaylist={handleOpenPlaylistModal} // Pass handler for add to playlist
          onToggleFavorite={handleFavoriteAction} // Pass handler for direct favorite click
          isFavoriteActive={isFavoriteActive}
          isFavoriteActionLoading={isFavoriteActionLoading}
          favoriteActionError={favoriteActionError}
          playerError={playerError}
          onOpenVideo={handleOpenVideoView}
          onShareCurrent={handleOpenSharePicker}
        />
      ) : null}

      <TypedMediaInfoPanel
        isOpen={isInfoPanelOpen}
        onClose={() => setIsInfoPanelOpen(false)}
        track={playerTrack}
      />

      <audio ref={audioRef} preload="metadata" />

      <TypedAuthLoginModal
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
      <TypedPlaylistModal
        isOpen={isPlaylistModalOpen}
        onClose={() => setIsPlaylistModalOpen(false)}
        playlists={userPlaylists}
        onAddTrackToPlaylist={handleAddToPlaylist}
        currentTrackId={playerTrack?.id}
      />

    </div>
  );
}
