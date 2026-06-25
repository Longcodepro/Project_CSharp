type ApiEnvelope<T> = {
  success?: boolean;
  data?: T;
  message?: string;
};

type RequestOptions = RequestInit & {
  skipAuthRefresh?: boolean;
};

type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  userId: string;
  idDisplay: string;
  roles: string[];
};

type PlaylistPayload = {
  Title?: string;
  title?: string;
  Description?: string | null;
  description?: string | null;
  IsPublic?: boolean;
  isPublic?: boolean;
  CoverImage?: File | null;
  coverImage?: File | null;
  ContentType?: string | null;
  contentType?: string | null;
  ReleaseDate?: string | null;
  releaseDate?: string | null;
};

type AlbumPayload = PlaylistPayload;

type MediaPayload = {
  Title?: string;
  title?: string;
  Description?: string | null;
  description?: string | null;
  Genre?: string | null;
  genre?: string | null;
  Type?: string | null;
  type?: string | null;
  AccessLevel?: number | string | null;
  accessLevel?: number | string | null;
  IsPublic?: boolean;
  isPublic?: boolean;
  ReleaseDate?: string | null;
  releaseDate?: string | null;
  FeaturedArtistIds?: string[];
  featuredArtistIds?: string[];
  AudioFile?: File | null;
  audioFile?: File | null;
  VideoFile?: File | null;
  videoFile?: File | null;
  CoverImage?: File | null;
  coverImage?: File | null;
  CanvasFile?: File | null;
  canvasFile?: File | null;
};

type NormalizedItem = {
  id: string;
  title?: string;
  artist?: string;
  image?: string;
  audioUrl?: string | null;
  videoUrl?: string | null;
  mediaType?: string;
  durationSeconds?: number;
  progress?: number;
  [key: string]: unknown;
  };
 
export type UserProfile = {
  id?: string;
  Id?: string;
  idDisplay?: string;
  IdDisplay?: string;
  displayName?: string;
  DisplayName?: string;
  userName?: string;
  UserName?: string;
  name?: string;
  Name?: string;
  handle?: string; // Added handle property
  email?: string;
  Email?: string;
  avatarUrl?: string | null;
  AvatarUrl?: string | null;
  bio?: string;
  Bio?: string;
};

const DEFAULT_API_BASE_URL = 'http://localhost:5128/api';
const rawApiBaseUrl = (import.meta.env?.VITE_API_BASE_URL as string | undefined) || DEFAULT_API_BASE_URL;
const API_BASE_URL = rawApiBaseUrl.replace(/\/$/, '');
const API_ORIGIN = (() => {
  try {
    return new URL(API_BASE_URL).origin;
  } catch {
    return window.location.origin;
  }
})();

const AUTH_SESSION_KEY = 'auth_session';
const AUTH_ACCESS_TOKEN_KEY = 'auth_access_token';
const AUTH_REFRESH_TOKEN_KEY = 'auth_refresh_token';
const AUTH_USER_ID_KEY = 'auth_user_id';
const AUTH_ID_DISPLAY_KEY = 'auth_id_display';
const AUTH_ROLES_KEY = 'user_roles';

function joinApiPath(endpoint: string): string {
  if (/^https?:\/\//i.test(endpoint)) {
    return endpoint;
  }

  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  return `${API_BASE_URL}${normalizedEndpoint}`;
}

function extractData<T>(payload: ApiEnvelope<T> | T | null | undefined): T {
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as ApiEnvelope<T>).data as T;
  }

  return payload as T;
}

function normalizeArray<T>(payload: unknown): T[] {
  const data = extractData(payload as ApiEnvelope<T[]> | T[] | null | undefined);
  return Array.isArray(data) ? data : [];
}

function buildHeaders(options: RequestInit): HeadersInit {
  const headers = new Headers(options.headers || {});
  const isFormData = typeof FormData !== 'undefined' && options.body instanceof FormData;

  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json');
  }

  if (!isFormData && options.body != null && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  const token = localStorage.getItem(AUTH_ACCESS_TOKEN_KEY);
  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  return headers;
}

async function parseResponse<T>(response: Response): Promise<T> {
  const contentType = response.headers.get('content-type') || '';

  if (response.status === 204) {
    return undefined as T;
  }

  if (contentType.includes('application/json')) {
    return response.json() as Promise<T>;
  }

  const text = await response.text();
  return (text ? (JSON.parse(text) as T) : undefined) as T;
}

async function request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
  const { skipAuthRefresh = false, ...fetchOptions } = options;

  const response = await fetch(joinApiPath(endpoint), {
    ...fetchOptions,
    credentials: 'include',
    headers: buildHeaders(fetchOptions),
  });

  if (response.status === 401 && !skipAuthRefresh) {
    const refreshed = await refreshAuthSession().catch(() => null);
    if (refreshed) {
      return request<T>(endpoint, { ...options, skipAuthRefresh: true });
    }
  }

  if (!response.ok) {
    let message = `HTTP error ${response.status}`;
    try {
      const errorPayload = await parseResponse<ApiEnvelope<unknown>>(response.clone());
      message = errorPayload?.message || message;
    } catch {
      const text = await response.text().catch(() => '');
      if (text) message = text;
    }

    throw new Error(message);
  }

  return parseResponse<T>(response);
}

function asString(value: unknown): string {
  return String(value ?? '').trim();
}

function normalizeRoleList(roles: unknown): string[] {
  if (Array.isArray(roles)) return roles.map((role) => String(role).trim()).filter(Boolean);
  if (typeof roles === 'string') {
    return roles.split(',').map((role) => role.trim()).filter(Boolean);
  }
  return [];
}

function setSessionState(auth: AuthResponse | null): void {
  if (!auth) {
    clearAuthSession();
    return;
  }

  localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(auth));
  localStorage.setItem(AUTH_ACCESS_TOKEN_KEY, auth.accessToken || '');
  localStorage.setItem(AUTH_REFRESH_TOKEN_KEY, auth.refreshToken || '');
  localStorage.setItem(AUTH_USER_ID_KEY, auth.userId || '');
  localStorage.setItem(AUTH_ID_DISPLAY_KEY, auth.idDisplay || '');
  localStorage.setItem(AUTH_ROLES_KEY, JSON.stringify(auth.roles || []));
}

function buildFormData(payload: Record<string, unknown> | FormData | undefined | null): FormData {
  if (payload instanceof FormData) return payload;

  const formData = new FormData();
  Object.entries(payload || {}).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') {
      return;
    }

    if (Array.isArray(value)) {
      value.forEach((item) => {
        if (item !== undefined && item !== null && item !== '') {
          formData.append(key, String(item));
        }
      });
      return;
    }

    if (value instanceof File || value instanceof Blob) {
      formData.append(key, value);
      return;
    }

    formData.append(key, String(value));
  });

  return formData;
}

function readString(...values: unknown[]): string {
  for (const value of values) {
    const resolved = asString(value);
    if (resolved) return resolved;
  }
  return '';
}

function readNumber(...values: unknown[]): number {
  for (const value of values) {
    const resolved = Number(value);
    if (Number.isFinite(resolved)) return resolved;
  }
  return 0;
}

function normalizeMediaType(value: unknown): string {
  const numericMap = ['audio', 'video', 'podcast', 'song'];
  if (typeof value === 'number') return numericMap[value] || '';

  const normalized = asString(value).toLowerCase();
  if (/^\d+$/.test(normalized)) return numericMap[Number(normalized)] || normalized;
  return normalized;
}

export function normalizeAssetUrl(url?: string | null): string {
  const value = asString(url);
  if (!value) return '';
  if (/^(https?:|data:|blob:|\/\/)/i.test(value)) return value;
  if (value.startsWith('/')) return `${API_ORIGIN}${value}`;
  return `${API_ORIGIN}/${value}`;
}

export function mediaPosterUrl(mediaId?: string | null): string {
  const id = asString(mediaId);
  return id ? `${API_BASE_URL}/media/${id}/poster` : '';
}

export function mediaAudioStreamUrl(mediaId?: string | null): string {
  const id = asString(mediaId);
  return id ? `${API_BASE_URL}/media/${id}/audio/stream` : '';
}

export function mediaVideoStreamUrl(mediaId?: string | null): string {
  const id = asString(mediaId);
  return id ? `${API_BASE_URL}/media/${id}/video/stream` : '';
}

export function saveAuthSession(auth: AuthResponse): AuthResponse {
  setSessionState(auth);
  return auth;
}

export function clearAuthSession(): void {
  localStorage.removeItem(AUTH_SESSION_KEY);
  localStorage.removeItem(AUTH_ACCESS_TOKEN_KEY);
  localStorage.removeItem(AUTH_REFRESH_TOKEN_KEY);
  localStorage.removeItem(AUTH_USER_ID_KEY);
  localStorage.removeItem(AUTH_ID_DISPLAY_KEY);
  localStorage.removeItem(AUTH_ROLES_KEY);
}

export async function refreshAuthSession(): Promise<AuthResponse | null> {
  try {
    const payload = await request<ApiEnvelope<AuthResponse>>('/auth/refresh', {
      method: 'POST',
      skipAuthRefresh: true,
    });

    const auth = extractData(payload);
    if (auth?.accessToken) {
      setSessionState(auth);
      return auth;
    }

    return null;
  } catch {
    clearAuthSession();
    return null;
  }
}

export async function logoutUser(): Promise<void> {
  try {
    await request<ApiEnvelope<object | null>>('/auth/logout', {
      method: 'POST',
      skipAuthRefresh: true,
    });
  } finally {
    clearAuthSession();
  }
}

export async function loginUser(idDisplay: string, password: string): Promise<AuthResponse> {
  const payload = await request<ApiEnvelope<AuthResponse>>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ idDisplay, password }),
    skipAuthRefresh: true,
  });

  const auth = extractData(payload);
  if (!auth?.accessToken) {
    throw new Error('Không nhận được thông tin đăng nhập từ máy chủ.');
  }

  return auth;
}

export async function registerUser(requestBody: {
  email: string;
  otpCode: string;
  idDisplay: string;
  displayName: string;
  password: string;
}): Promise<AuthResponse> {
  const payload = await request<ApiEnvelope<AuthResponse>>('/auth/register', {
    method: 'POST',
    body: JSON.stringify(requestBody),
    skipAuthRefresh: true,
  });

  const auth = extractData(payload);
  if (!auth?.accessToken) {
    throw new Error('Không nhận được thông tin đăng ký từ máy chủ.');
  }

  return auth;
}

export async function sendOtp(email: string, purpose: 'register' | 'reset_password' | 'change_password'): Promise<void> {
  await request<ApiEnvelope<object | null>>('/auth/send-otp', {
    method: 'POST',
    body: JSON.stringify({ email, purpose }),
    skipAuthRefresh: true,
  });
}

export async function changePassword(requestBody: {
  email: string;
  oldPassword: string;
  otpCode: string;
  newPassword: string;
}): Promise<void> {
  await request<ApiEnvelope<object | null>>('/auth/change-password', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  });
}

export async function getMyProfile() {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>('/users/me/profile');
  return extractData(payload);
}

export async function updateProfile(
  displayName: string,
  idDisplay: string,
  avatarFile: File | null,
  bio: string,
  removeAvatar: boolean,
) {
  const formData = buildFormData({
    DisplayName: displayName,
    IdDisplay: idDisplay,
    Bio: bio,
    AvatarFile: avatarFile,
    RemoveAvatar: removeAvatar,
  });

  const payload = await request<ApiEnvelope<Record<string, unknown>>>('/users/me/profile', {
    method: 'PUT',
    body: formData,
  });

  return extractData(payload);
}

export async function getUserById(userId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/users/${encodeURIComponent(userId)}`);
  return extractData(payload);
}

export async function getArtists(): Promise<UserProfile[]> {
  const payload = await request<ApiEnvelope<UserProfile[]>>('/users/artists');
  return normalizeArray(payload);
}

export async function searchUsers(keyword: string, _page = 1, _pageSize = 10) {
  const trimmed = asString(keyword).toLowerCase();
  if (!trimmed) return [];

  const users = await getArtists().catch(() => []);
  return users.filter((user) => {
    const idDisplay = readString(user.idDisplay, user.IdDisplay, user.userName, user.UserName, user.id, user.Id).toLowerCase();
    const displayName = readString(user.displayName, user.DisplayName, user.name, user.Name).toLowerCase();
    return idDisplay.includes(trimmed) || displayName.includes(trimmed);
  });
}

export async function followUser(followeeId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>('/users/follow', {
    method: 'POST',
    body: JSON.stringify({ followeeId }),
  });
}

export async function unfollowUser(followeeId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>('/users/unfollow', {
    method: 'DELETE',
    body: JSON.stringify({ followeeId }),
  });
}

export async function checkFollowStatus(followeeId: string): Promise<boolean> {
  const payload = await request<ApiEnvelope<boolean>>(`/users/is-following/${encodeURIComponent(followeeId)}`);
  return Boolean(extractData(payload));
}

export async function getMyMedia() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/media/my-media');
  return normalizeArray(payload);
}

export async function getMedia(page = 1, pageSize = 10) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/media?page=${page}&pageSize=${pageSize}`);
  return normalizeArray(payload);
}

export async function getTrackById(mediaId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/media/${encodeURIComponent(mediaId)}`);
  return extractData(payload);
}

export async function getMediaById(mediaId: string) {
  return getTrackById(mediaId);
}

export async function getArtistMedia(userId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/media/artist/${encodeURIComponent(userId)}`);
  return normalizeArray(payload);
}

export async function getTrendingTracks(top = 10) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/search/trending?top=${top}`);
  return normalizeArray(payload);
}

export async function getMediaReactionCount(mediaId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/favorites/${encodeURIComponent(mediaId)}/reaction-count`);
  return extractData(payload);
}

export async function getAlbumReactionCount(albumId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/favorites/albums/${encodeURIComponent(albumId)}/reaction-count`);
  return extractData(payload);
}

export async function getPlaylistReactionCount(playlistId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/favorites/playlists/${encodeURIComponent(playlistId)}/reaction-count`);
  return extractData(payload);
}

export async function getFavoriteReactions() {
  const payload = await request<ApiEnvelope<Array<{ name: string; value: number }>>>('/favorites/reactions', {
    skipAuthRefresh: true,
  });
  return normalizeArray(payload);
}

export async function getFavoriteStatus(mediaId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/favorites/status/${encodeURIComponent(mediaId)}`);
  return extractData(payload);
}

export async function toggleFavorite(mediaId: string, reaction: string | null = 'Love'): Promise<void> {
  if (reaction === null) {
    await request<ApiEnvelope<object | null>>(`/favorites/${encodeURIComponent(mediaId)}`, {
      method: 'DELETE',
    });
    return;
  }

  await request<ApiEnvelope<object | null>>(`/favorites/${encodeURIComponent(mediaId)}`, {
    method: 'PUT',
    body: JSON.stringify({ reaction }),
  });
}

export async function getLikedSongs() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/favorites/me');
  return normalizeArray(payload);
}

export async function getRecentCollectionLikes(limit = 3) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/collection-likes/recent?limit=${limit}`);
  return normalizeArray(payload);
}

export async function toggleCollectionLike(targetId: string, targetType: string) {
  const payload = await request<ApiEnvelope<boolean>>('/collection-likes', {
    method: 'PUT',
    body: JSON.stringify({ targetId, targetType }),
  });
  return Boolean(extractData(payload));
}

export async function getFeaturedPlaylists(limit = 10) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/playlists?limit=${limit}`);
  return normalizeArray(payload);
}

export async function getMyPlaylists() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/playlists/me');
  return normalizeArray(payload);
}

export async function getPlaylistById(id: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/playlists/${encodeURIComponent(id)}`);
  return extractData(payload);
}

export async function createPlaylist(payload: PlaylistPayload | FormData) {
  const formData = buildFormData(payload as Record<string, unknown>);
  const response = await request<ApiEnvelope<Record<string, unknown>>>('/playlists', {
    method: 'POST',
    body: formData,
  });
  return extractData(response);
}

export async function updatePlaylist(id: string, payload: PlaylistPayload | FormData) {
  const formData = buildFormData(payload as Record<string, unknown>);
  const response = await request<ApiEnvelope<Record<string, unknown>>>(`/playlists/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: formData,
  });
  return extractData(response);
}

export async function deletePlaylist(id: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/playlists/${encodeURIComponent(id)}`, {
    method: 'DELETE',
  });
}

export async function addTrackToPlaylist(playlistId: string, mediaItemId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/playlists/${encodeURIComponent(playlistId)}/tracks`, {
    method: 'POST',
    body: JSON.stringify({ mediaItemId }),
  });
}

export async function removeTrackFromPlaylist(playlistId: string, mediaItemId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/playlists/${encodeURIComponent(playlistId)}/tracks/${encodeURIComponent(mediaItemId)}`, {
    method: 'DELETE',
  });
}

export async function reorderTrackInPlaylist(playlistId: string, mediaItemId: string, newOrder: number): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/playlists/${encodeURIComponent(playlistId)}/tracks/${encodeURIComponent(mediaItemId)}/order?newOrder=${encodeURIComponent(String(newOrder))}`, {
    method: 'PATCH',
  });
}

export async function getFeaturedAlbums(limit = 10) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/albums?limit=${limit}`);
  return normalizeArray(payload);
}

export async function getMyAlbums() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/albums/me');
  return normalizeArray(payload);
}

export async function getAlbumById(id: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/albums/${encodeURIComponent(id)}`);
  return extractData(payload);
}

export async function createAlbum(payload: AlbumPayload | FormData) {
  const formData = buildFormData(payload as Record<string, unknown>);
  const response = await request<ApiEnvelope<Record<string, unknown>>>('/albums', {
    method: 'POST',
    body: formData,
  });
  return extractData(response);
}

export async function updateAlbum(id: string, payload: AlbumPayload | FormData) {
  const formData = buildFormData(payload as Record<string, unknown>);
  const response = await request<ApiEnvelope<Record<string, unknown>>>(`/albums/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: formData,
  });
  return extractData(response);
}

export async function deleteAlbum(id: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/albums/${encodeURIComponent(id)}`, {
    method: 'DELETE',
  });
}

export async function addTrackToAlbum(albumId: string, mediaItemId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/albums/${encodeURIComponent(albumId)}/tracks`, {
    method: 'POST',
    body: JSON.stringify({ mediaItemId }),
  });
}

export async function removeTrackFromAlbum(albumId: string, mediaItemId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/albums/${encodeURIComponent(albumId)}/tracks/${encodeURIComponent(mediaItemId)}`, {
    method: 'DELETE',
  });
}

export async function reorderTrackInAlbum(albumId: string, mediaItemId: string, newOrder: number): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/albums/${encodeURIComponent(albumId)}/tracks/${encodeURIComponent(mediaItemId)}/order?newOrder=${encodeURIComponent(String(newOrder))}`, {
    method: 'PATCH',
  });
}

export async function uploadMedia(payload: MediaPayload | FormData, routeKind?: 'audio' | 'video' | 'mixed') {
  const formData = buildFormData(payload as Record<string, unknown>);
  let endpoint = '/media/upload';
  if (routeKind === 'audio') endpoint = '/media/upload/audio';
  if (routeKind === 'video') endpoint = '/media/upload/video';

  const response = await request<ApiEnvelope<Record<string, unknown>>>(endpoint, {
    method: 'POST',
    body: formData,
  });
  return extractData(response);
}

export async function updateMedia(mediaId: string, payload: MediaPayload | FormData) {
  const formData = buildFormData(payload as Record<string, unknown>);
  const response = await request<ApiEnvelope<Record<string, unknown>>>(`/media/${encodeURIComponent(mediaId)}`, {
    method: 'PUT',
    body: formData,
  });
  return extractData(response);
}

export async function deleteMedia(mediaId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/media/${encodeURIComponent(mediaId)}`, {
    method: 'DELETE',
  });
}

export async function getNotifications(limit = 50) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/notifications?limit=${limit}`);
  return normalizeArray(payload);
}

export async function getUnreadNotifications(limit = 50) {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>(`/notifications/unread?limit=${limit}`);
  return normalizeArray(payload);
}

export async function getUnreadNotificationCount() {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>('/notifications/unread-count');
  return extractData(payload);
}

export async function markNotificationAsRead(notificationId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/notifications/${encodeURIComponent(notificationId)}/read`, {
    method: 'PATCH',
  });
}

export async function markAllNotificationsAsRead(): Promise<void> {
  await request<ApiEnvelope<number>>('/notifications/read-all', {
    method: 'PATCH',
  });
}

export async function deleteNotification(notificationId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/notifications/${encodeURIComponent(notificationId)}`, {
    method: 'DELETE',
  });
}

export async function getMyFriends() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/friends/me');
  return normalizeArray(payload);
}

export async function getIncomingFriendRequests() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/friends/requests/inbox');
  return normalizeArray(payload);
}

export async function getSentFriendRequests() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/friends/requests/sent');
  return normalizeArray(payload);
}

export async function sendFriendRequest(receiverId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>(`/friends/requests/${encodeURIComponent(receiverId)}`, {
    method: 'POST',
  });
}

export async function acceptFriendRequest(requestId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>(`/friends/requests/${encodeURIComponent(requestId)}/accept`, {
    method: 'POST',
  });
}

export async function rejectFriendRequest(requestId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>(`/friends/requests/${encodeURIComponent(requestId)}/reject`, {
    method: 'POST',
  });
}

export async function cancelFriendRequest(requestId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>(`/friends/requests/${encodeURIComponent(requestId)}`, {
    method: 'DELETE',
  });
}

export async function removeFriend(friendUserId: string): Promise<void> {
  await request<ApiEnvelope<object | null>>(`/friends/${encodeURIComponent(friendUserId)}`, {
    method: 'DELETE',
  });
}

export async function getRecentHistory() {
  const payload = await request<ApiEnvelope<Record<string, unknown>[]>>('/history/recent');
  return normalizeArray(payload);
}

export async function recordPlayHistory(mediaId: string): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/history/${encodeURIComponent(mediaId)}`, {
    method: 'POST',
  });
}

export async function recordPlaybackStop(mediaId: string, stoppedAt: number): Promise<void> {
  await request<ApiEnvelope<boolean>>(`/history/${encodeURIComponent(mediaId)}/stop`, {
    method: 'PATCH',
    body: JSON.stringify({ stoppedAt }),
  });
}

export async function getResumeInfo(mediaId: string) {
  const payload = await request<ApiEnvelope<Record<string, unknown>>>(`/history/${encodeURIComponent(mediaId)}/resume`);
  return extractData(payload);
}

export async function getMediaCollection(collectionId: string, collectionType: string) {
  const normalizedType = asString(collectionType).toLowerCase();
  const collection = normalizedType === 'album'
    ? await getAlbumById(collectionId).catch(() => null)
    : await getPlaylistById(collectionId).catch(() => null);

  const tracks = Array.isArray(collection?.tracks) ? collection.tracks : [];
  const queueItems = await Promise.all(
    tracks
      .map((track: Record<string, unknown>) => ({
        mediaItemId: readString(track.mediaItemId, track.MediaItemId),
        trackOrder: readNumber(track.trackOrder, track.TrackOrder),
      }))
      .filter((track) => Boolean(track.mediaItemId))
      .sort((left, right) => left.trackOrder - right.trackOrder)
      .map(async (track) => {
        const media = await getTrackById(track.mediaItemId).catch(() => null);
        if (!media) {
          return null;
        }

        return normalizePlayableMedia(media, {
          collectionId,
          collectionType: normalizedType,
          trackOrder: track.trackOrder,
        });
      }),
  );

  return queueItems.filter(Boolean);
}

export async function getMediaDetails(mediaId: string) {
  return getTrackById(mediaId);
}

function normalizePlayableMedia(
  media: Record<string, unknown>,
  extra: Record<string, unknown> = {},
): NormalizedItem {
  const mediaId = readString(media.id, media.Id, extra.id);
  const title = readString(media.title, media.Title, extra.title) || mediaId;
  const ownerId = readString(media.ownerId, media.OwnerId);
  const artists = Array.isArray(media.artists) ? media.artists : Array.isArray(media.Artists) ? media.Artists : [];
  const artistName = readString(
    media.artist,
    media.Artist,
    media.artistName,
    media.ArtistName,
    artists.map((artist: Record<string, unknown>) => readString(artist.artistId, artist.ArtistId)).join(', '),
    ownerId,
  ) || 'TuneVault';
  const cover = normalizeAssetUrl(
    readString(media.coverImageUrl, media.CoverImageUrl, media.image, media.Image) || undefined,
  );
  const audioUrl = normalizeAssetUrl(readString(media.audioUrl, media.AudioUrl) || undefined);
  const videoUrl = normalizeAssetUrl(readString(media.videoUrl, media.VideoUrl) || undefined);
  const mediaType = normalizeMediaType(media.mediaType ?? media.MediaType ?? media.type ?? media.Type);
  const durationSeconds = readNumber(media.durationSeconds, media.DurationSeconds);

  return {
    ...media,
    ...extra,
    id: mediaId,
    title,
    artist: artistName,
    image: cover || mediaPosterUrl(mediaId),
    audioUrl,
    videoUrl,
    mediaType,
    durationSeconds,
    progress: readNumber(media.progress, media.Progress, extra.progress),
    collectionId: extra.collectionId,
    collectionType: extra.collectionType,
    trackOrder: extra.trackOrder,
  };
}

export const MediaService = {
  loginUser,
  registerUser,
  sendOtp,
  changePassword,
  saveAuthSession,
  clearAuthSession,
  logoutUser,
  refreshAuthSession,
  normalizeAssetUrl,
  mediaPosterUrl,
  mediaAudioStreamUrl,
  mediaVideoStreamUrl,
  getMyProfile,
  updateProfile,
  getUserById,
  getArtists,
  searchUsers,
  followUser,
  unfollowUser,
  checkFollowStatus,
  getMyMedia,
  getMedia,
  getTrackById,
  getMediaById,
  getArtistMedia,
  getTrendingTracks,
  getMediaReactionCount,
  getAlbumReactionCount,
  getPlaylistReactionCount,
  getFavoriteReactions,
  getFavoriteStatus,
  toggleFavorite,
  getLikedSongs,
  getRecentCollectionLikes,
  toggleCollectionLike,
  getFeaturedPlaylists,
  getMyPlaylists,
  getPlaylistById,
  createPlaylist,
  updatePlaylist,
  deletePlaylist,
  addTrackToPlaylist,
  removeTrackFromPlaylist,
  reorderTrackInPlaylist,
  getFeaturedAlbums,
  getMyAlbums,
  getAlbumById,
  createAlbum,
  updateAlbum,
  deleteAlbum,
  addTrackToAlbum,
  removeTrackFromAlbum,
  reorderTrackInAlbum,
  uploadMedia,
  updateMedia,
  deleteMedia,
  getNotifications,
  getUnreadNotifications,
  getUnreadNotificationCount,
  markNotificationAsRead,
  markAllNotificationsAsRead,
  deleteNotification,
  getMyFriends,
  getIncomingFriendRequests,
  getSentFriendRequests,
  sendFriendRequest,
  acceptFriendRequest,
  rejectFriendRequest,
  cancelFriendRequest,
  removeFriend,
  getRecentHistory,
  recordPlayHistory,
  recordPlaybackStop,
  getResumeInfo,
  getMediaCollection,
  getMediaDetails,
};

export default MediaService;
