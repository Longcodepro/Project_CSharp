import { useEffect, useMemo, useState } from 'react';
import {
  acceptFriendRequest,
  cancelFriendRequest,
  getIncomingFriendRequests,
  getMyFriends,
  getSentFriendRequests,
  normalizeAssetUrl,
  rejectFriendRequest,
  removeFriend,
  searchUsers,
  sendFriendRequest,
} from '../../../Services/MediaService.tsx';
import '../../CSS/FriendActivity.css';

const defaultAvatarUrl = normalizeAssetUrl('/uploads/avatars/Default.png') || 'https://via.placeholder.com/120?text=%20';

function pick(...values) {
  return values.find((value) => value !== undefined && value !== null && String(value).trim() !== '') || '';
}

function toDateText(value) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 'Không rõ ngày' : date.toLocaleDateString('vi-VN');
}

function mapFriend(friend) {
  const id = pick(friend.userId, friend.UserId, friend.id, friend.Id);
  const handle = pick(friend.idDisplay, friend.IdDisplay, friend.userName, friend.UserName, id);
  const name = pick(friend.displayName, friend.DisplayName, handle, id);
  const avatarUrl = pick(friend.avatarUrl, friend.AvatarUrl);

  return {
    id,
    name,
    handle,
    status: `Bạn bè từ ${toDateText(pick(friend.friendsSince, friend.FriendsSince))}`,
    avatar: normalizeAssetUrl(avatarUrl) || defaultAvatarUrl,
    time: 'Bạn bè',
  };
}

function mapRequest(request, kind) {
  const requestId = pick(request.requestId, request.RequestId, request.id, request.Id);
  const id = pick(request.userId, request.UserId);
  const handle = pick(request.idDisplay, request.IdDisplay, request.userName, request.UserName, id);
  const name = pick(request.displayName, request.DisplayName, handle, id);
  const avatarUrl = pick(request.avatarUrl, request.AvatarUrl);

  return {
    requestId,
    id,
    name,
    handle,
    status: kind === 'incoming' ? 'Đang chờ bạn xác nhận' : 'Đã gửi lời mời',
    avatar: normalizeAssetUrl(avatarUrl) || defaultAvatarUrl,
    time: toDateText(pick(request.requestedAt, request.RequestedAt)),
  };
}

function mapSearchUser(user) {
  const id = pick(user.id, user.Id, user.userId, user.UserId);
  const handle = pick(user.userName, user.UserName, user.idDisplay, user.IdDisplay, id);
  const name = pick(user.displayName, user.DisplayName, handle, id);
  const avatarUrl = pick(user.avatarUrl, user.AvatarUrl);

  return {
    id,
    name,
    handle,
    status: `${Number(user.totalFollowers ?? user.TotalFollowers ?? 0)} follower`,
    avatar: normalizeAssetUrl(avatarUrl) || defaultAvatarUrl,
  };
}

export default function FriendActivity({ onClose, onOpenProfile }) {
  const [friends, setFriends] = useState([]);
  const [incomingRequests, setIncomingRequests] = useState([]);
  const [sentRequests, setSentRequests] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [query, setQuery] = useState('');
  const [activeTab, setActiveTab] = useState('friends');
  const [requestStatus, setRequestStatus] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [actionKey, setActionKey] = useState('');
  const [error, setError] = useState('');

  const reloadFriendData = async () => {
    const [friendsData, incomingData, sentData] = await Promise.all([
      getMyFriends(),
      getIncomingFriendRequests(),
      getSentFriendRequests(),
    ]);

    setFriends(friendsData.map(mapFriend));
    setIncomingRequests(incomingData.map((request) => mapRequest(request, 'incoming')));
    setSentRequests(sentData.map((request) => mapRequest(request, 'sent')));
  };

  useEffect(() => {
    let isMounted = true;

    async function loadFriendData() {
      setIsLoading(true);
      setError('');

      try {
        if (!isMounted) return;
        await reloadFriendData();
      } catch {
        if (!isMounted) return;
        setFriends([]);
        setIncomingRequests([]);
        setSentRequests([]);
        setError('Không tải được dữ liệu bạn bè.');
      } finally {
        if (isMounted) setIsLoading(false);
      }
    }

    loadFriendData();

    return () => {
      isMounted = false;
    };
  }, []);

  useEffect(() => {
    let isMounted = true;
    const trimmedQuery = query.trim();

    if (trimmedQuery.length < 2) {
      return undefined;
    }

    const timeoutId = window.setTimeout(async () => {
      try {
        const users = await searchUsers(trimmedQuery, 1, 10);
        if (!isMounted) return;
        setSearchResults(users.map(mapSearchUser));
        setRequestStatus('');
      } catch {
        if (isMounted) setSearchResults([]);
      }
    }, 320);

    return () => {
      isMounted = false;
      window.clearTimeout(timeoutId);
    };
  }, [query]);

  const relationMap = useMemo(() => {
    const map = new Map();
    friends.forEach((friend) => map.set(friend.id, { type: 'friend' }));
    incomingRequests.forEach((request) => map.set(request.id, { type: 'incoming', requestId: request.requestId }));
    sentRequests.forEach((request) => map.set(request.id, { type: 'sent', requestId: request.requestId }));
    return map;
  }, [friends, incomingRequests, sentRequests]);

  const filteredFriends = useMemo(() => {
    const lowerQuery = query.trim().toLowerCase();
    if (!lowerQuery) return friends;

    return friends.filter((friend) => (
      friend.name.toLowerCase().includes(lowerQuery) ||
      friend.handle.toLowerCase().includes(lowerQuery)
    ));
  }, [friends, query]);

  const filteredIncomingRequests = useMemo(() => {
    const lowerQuery = query.trim().toLowerCase();
    if (!lowerQuery) return incomingRequests;

    return incomingRequests.filter((request) => (
      request.name.toLowerCase().includes(lowerQuery) ||
      request.handle.toLowerCase().includes(lowerQuery)
    ));
  }, [incomingRequests, query]);

  const filteredSentRequests = useMemo(() => {
    const lowerQuery = query.trim().toLowerCase();
    if (!lowerQuery) return sentRequests;

    return sentRequests.filter((request) => (
      request.name.toLowerCase().includes(lowerQuery) ||
      request.handle.toLowerCase().includes(lowerQuery)
    ));
  }, [sentRequests, query]);

  const filteredSearchResults = useMemo(() => {
    const lowerQuery = query.trim().toLowerCase();
    if (!lowerQuery) return [];

    return searchResults.filter((user) => (
      user.name.toLowerCase().includes(lowerQuery) ||
      user.handle.toLowerCase().includes(lowerQuery)
    ));
  }, [query, searchResults]);

  const activeTabItems = activeTab === 'friends'
    ? filteredFriends
    : activeTab === 'incoming'
      ? filteredIncomingRequests
      : filteredSentRequests;

  const activeTabEmptyMessage = activeTab === 'friends'
    ? 'Chưa có bạn bè để hiển thị.'
    : activeTab === 'incoming'
      ? 'Không có lời mời kết bạn nào.'
      : 'Chưa gửi lời mời kết bạn nào.';

  const handleQueryChange = (value) => {
    setQuery(value);
    setRequestStatus('');
  };

  const refreshAndReport = async (message, task, key = 'action') => {
    try {
      setActionKey(key);
      await task();
      await reloadFriendData();
      setRequestStatus(message);
    } catch (error) {
      setRequestStatus(error instanceof Error ? error.message : 'Thao tác không thành công.');
    } finally {
      setActionKey('');
    }
  };

  const openProfile = (user) => {
    if (!user?.id) return;
    onOpenProfile?.({
      id: user.id,
      idDisplay: user.handle,
      handle: user.handle,
      displayName: user.name,
      name: user.name,
      image: user.avatar,
    });
  };

  const requireRequestId = (requestId) => {
    if (requestId) return true;
    setRequestStatus('Không tìm thấy mã lời mời. Vui lòng tải lại danh sách bạn bè.');
    return false;
  };

  const handleSearchUserAction = async (user) => {
    const relation = relationMap.get(user.id);

    if (relation?.type === 'friend') {
      await refreshAndReport('Đã xóa bạn bè.', () => removeFriend(user.id), `friend:${user.id}`);
      return;
    }

    if (relation?.type === 'incoming' && relation.requestId) {
      await refreshAndReport('Đã chấp nhận lời mời kết bạn.', () => acceptFriendRequest(relation.requestId), `incoming:${relation.requestId}`);
      return;
    }

    if (relation?.type === 'sent' && relation.requestId) {
      await refreshAndReport('Đã hủy lời mời kết bạn.', () => cancelFriendRequest(relation.requestId), `sent:${relation.requestId}`);
      return;
    }

    if ((relation?.type === 'incoming' || relation?.type === 'sent') && !relation.requestId) {
      setRequestStatus('Không tìm thấy mã lời mời. Vui lòng tải lại danh sách bạn bè.');
      return;
    }

    await refreshAndReport('Đã gửi lời mời kết bạn.', () => sendFriendRequest(user.id), `send:${user.id}`);
  };

  return (
    <aside className="friend-activity">
      <div className="friend-activity-header">
        <span>Bạn bè</span>
        <button type="button" aria-label="Đóng bạn bè" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      <label className="friend-activity-search">
        <span className="material-symbols-outlined">search</span>
        <input
          type="text"
          placeholder="Tìm theo ID hoặc tên"
          value={query}
          onChange={(event) => handleQueryChange(event.target.value)}
        />
      </label>

      {requestStatus && <div className="friend-activity-status">{requestStatus}</div>}

      {isLoading ? (
        <div className="friend-activity-empty">Đang tải dữ liệu bạn bè...</div>
      ) : error ? (
        <div className="friend-activity-empty error">{error}</div>
      ) : null}

      <div className="friend-tabs">
        <button type="button" className={activeTab === 'friends' ? 'active' : ''} onClick={() => setActiveTab('friends')}>
          Bạn bè
        </button>
        <button type="button" className={activeTab === 'incoming' ? 'active' : ''} onClick={() => setActiveTab('incoming')}>
          Đã nhận
        </button>
        <button type="button" className={activeTab === 'sent' ? 'active' : ''} onClick={() => setActiveTab('sent')}>
          Đã gửi
        </button>
      </div>

      <div className="friend-activity-list">
        {filteredSearchResults.length > 0 && (
          <>
            <div className="friend-activity-search-label">Kết quả tìm kiếm</div>
            {filteredSearchResults.map((user) => {
              const relation = relationMap.get(user.id);
              const actionLabel = relation?.type === 'friend'
                ? 'Xóa bạn'
                : relation?.type === 'incoming'
                  ? 'Chấp nhận'
                  : relation?.type === 'sent'
                    ? 'Hủy lời mời'
                    : 'Kết bạn';

              return (
                <article className="friend-activity-item suggested" key={user.id}>
                  <button type="button" className="friend-activity-profile-trigger avatar-trigger" onClick={() => openProfile(user)} aria-label={`Mở hồ sơ ${user.name}`}>
                    <img
                      src={user.avatar}
                      alt={user.name}
                      onError={(event) => {
                        event.currentTarget.onerror = null;
                        event.currentTarget.src = defaultAvatarUrl;
                      }}
                    />
                  </button>
                  <button type="button" className="friend-activity-profile-trigger friend-activity-copy-trigger" onClick={() => openProfile(user)}>
                    <strong>{user.name}</strong>
                    <span>{relation?.type === 'friend' ? 'Đã là bạn bè' : user.status}</span>
                    <small>@{user.handle} • Kết quả tìm kiếm</small>
                  </button>
                  <button
                    type="button"
                    className="friend-activity-action-button"
                    disabled={Boolean(actionKey)}
                    onClick={() => handleSearchUserAction(user)}
                  >
                    {actionLabel}
                  </button>
                </article>
              );
            })}
          </>
        )}

        {!isLoading && !error && query.trim().length < 2 && activeTabItems.length === 0 && (
          <div className="friend-activity-empty">{activeTabEmptyMessage}</div>
        )}

        {activeTab === 'friends' && activeTabItems.map((friend) => (
          <article className="friend-activity-item" key={friend.id}>
            <button type="button" className="friend-activity-profile-trigger avatar-trigger" onClick={() => openProfile(friend)} aria-label={`Mở hồ sơ ${friend.name}`}>
              <img
                src={friend.avatar}
                alt={friend.name}
                onError={(event) => {
                  event.currentTarget.onerror = null;
                  event.currentTarget.src = defaultAvatarUrl;
                }}
              />
            </button>
            <button type="button" className="friend-activity-profile-trigger friend-activity-copy-trigger" onClick={() => openProfile(friend)}>
              <strong>{friend.name}</strong>
              <span>{friend.status}</span>
              <small>@{friend.handle} • {friend.time}</small>
            </button>
            <button
              type="button"
              className="friend-activity-action-button"
              disabled={Boolean(actionKey)}
              onClick={() => refreshAndReport('Đã xóa bạn bè.', () => removeFriend(friend.id), `friend:${friend.id}`)}
            >
              Xóa
            </button>
          </article>
        ))}

        {activeTab === 'incoming' && activeTabItems.map((request) => (
          <article className="friend-activity-item suggested" key={request.requestId}>
            <button type="button" className="friend-activity-profile-trigger avatar-trigger" onClick={() => openProfile(request)} aria-label={`Mở hồ sơ ${request.name}`}>
              <img
                src={request.avatar}
                alt={request.name}
                onError={(event) => {
                  event.currentTarget.onerror = null;
                  event.currentTarget.src = defaultAvatarUrl;
                }}
              />
            </button>
            <button type="button" className="friend-activity-profile-trigger friend-activity-copy-trigger" onClick={() => openProfile(request)}>
              <strong>{request.name}</strong>
              <span>{request.status}</span>
              <small>@{request.handle} • {request.time}</small>
            </button>
            <div className="friend-activity-action-group">
              <button
                type="button"
                className="friend-activity-action-button"
                disabled={Boolean(actionKey)}
                onClick={() => {
                  if (!requireRequestId(request.requestId)) return;
                  void refreshAndReport('Đã chấp nhận lời mời kết bạn.', () => acceptFriendRequest(request.requestId), `incoming:${request.requestId}`);
                }}
              >
                Chấp nhận
              </button>
              <button
                type="button"
                className="friend-activity-action-button secondary"
                disabled={Boolean(actionKey)}
                onClick={() => {
                  if (!requireRequestId(request.requestId)) return;
                  void refreshAndReport('Đã từ chối lời mời kết bạn.', () => rejectFriendRequest(request.requestId), `incoming:${request.requestId}:reject`);
                }}
              >
                Từ chối
              </button>
            </div>
          </article>
        ))}

        {activeTab === 'sent' && activeTabItems.map((request) => (
          <article className="friend-activity-item suggested" key={request.requestId}>
            <button type="button" className="friend-activity-profile-trigger avatar-trigger" onClick={() => openProfile(request)} aria-label={`Mở hồ sơ ${request.name}`}>
              <img
                src={request.avatar}
                alt={request.name}
                onError={(event) => {
                  event.currentTarget.onerror = null;
                  event.currentTarget.src = defaultAvatarUrl;
                }}
              />
            </button>
            <button type="button" className="friend-activity-profile-trigger friend-activity-copy-trigger" onClick={() => openProfile(request)}>
              <strong>{request.name}</strong>
              <span>{request.status}</span>
              <small>@{request.handle} • {request.time}</small>
            </button>
            <button
              type="button"
              className="friend-activity-action-button secondary"
              disabled={Boolean(actionKey)}
              onClick={() => {
                if (!requireRequestId(request.requestId)) return;
                void refreshAndReport('Đã hủy lời mời kết bạn.', () => cancelFriendRequest(request.requestId), `sent:${request.requestId}`);
              }}
            >
              Hủy
            </button>
          </article>
        ))}

        {!isLoading && !error && query.trim().length >= 2 && filteredSearchResults.length === 0 && (
          <div className="friend-activity-empty">Không tìm thấy người dùng phù hợp.</div>
        )}
      </div>
    </aside>
  );
}
