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

function toTimestamp(value) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 0 : date.getTime();
}

function sortByName(left, right) {
  return String(left.name || '').localeCompare(String(right.name || ''), 'vi');
}

function sortByNewest(left, right) {
  const timeDiff = Number(right.sortTimestamp || 0) - Number(left.sortTimestamp || 0);
  if (timeDiff !== 0) return timeDiff;
  return sortByName(left, right);
}

function mapFriend(friend) {
  const id = pick(friend.userId, friend.UserId, friend.id, friend.Id);
  const handle = pick(friend.idDisplay, friend.IdDisplay, friend.userName, friend.UserName, id);
  const name = pick(friend.displayName, friend.DisplayName, handle, id);
  const avatarUrl = pick(friend.avatarUrl, friend.AvatarUrl);
  const sortTimestamp = toTimestamp(pick(friend.friendsSince, friend.FriendsSince));

  return {
    id,
    name,
    handle,
    status: `Bạn bè từ ${toDateText(pick(friend.friendsSince, friend.FriendsSince))}`,
    avatar: normalizeAssetUrl(avatarUrl) || defaultAvatarUrl,
    time: 'Bạn bè',
    sortTimestamp,
  };
}

function mapRequest(request, kind) {
  const requestId = pick(request.requestId, request.RequestId, request.id, request.Id);
  const id = pick(request.userId, request.UserId);
  const handle = pick(request.idDisplay, request.IdDisplay, request.userName, request.UserName, id);
  const name = pick(request.displayName, request.DisplayName, handle, id);
  const avatarUrl = pick(request.avatarUrl, request.AvatarUrl);
  const sortTimestamp = toTimestamp(pick(request.requestedAt, request.RequestedAt));

  return {
    requestId,
    id,
    name,
    handle,
    status: kind === 'incoming' ? 'Đang chờ bạn xác nhận' : 'Đã gửi lời mời',
    avatar: normalizeAssetUrl(avatarUrl) || defaultAvatarUrl,
    time: toDateText(pick(request.requestedAt, request.RequestedAt)),
    sortTimestamp,
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

export default function FriendActivity({
  onClose,
  onOpenProfile,
  shareMode = false,
  shareItemTitle = '',
  shareItemType = '',
  onShareConfirm,
  onCancelShare,
}) {
  const [friends, setFriends] = useState([]);
  const [incomingRequests, setIncomingRequests] = useState([]);
  const [sentRequests, setSentRequests] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [query, setQuery] = useState('');
  const [activeTab, setActiveTab] = useState('friends');
  const [requestStatus, setRequestStatus] = useState('');
  const [loadNotice, setLoadNotice] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [actionKey, setActionKey] = useState('');
  const [error, setError] = useState('');
  const [selectedShareUser, setSelectedShareUser] = useState(null);
  const [shareMessage, setShareMessage] = useState('');
  const [shareActionStatus, setShareActionStatus] = useState('');

  const reloadFriendData = async () => {
    const [friendsResult, incomingResult, sentResult] = await Promise.allSettled([
      getMyFriends(),
      getIncomingFriendRequests(),
      getSentFriendRequests(),
    ]);

    const notices = [];

    if (friendsResult.status === 'fulfilled') {
      setFriends(friendsResult.value.map(mapFriend).sort(sortByNewest));
    } else {
      setFriends([]);
      notices.push('danh sách bạn bè');
    }

    if (incomingResult.status === 'fulfilled') {
      setIncomingRequests(incomingResult.value.map((request) => mapRequest(request, 'incoming')).sort(sortByNewest));
    } else {
      setIncomingRequests([]);
      notices.push('thông báo đã nhận');
    }

    if (sentResult.status === 'fulfilled') {
      setSentRequests(sentResult.value.map((request) => mapRequest(request, 'sent')).sort(sortByNewest));
    } else {
      setSentRequests([]);
      notices.push('thông báo đã gửi');
    }

    setLoadNotice(notices.length > 0
      ? `Một số danh sách bạn bè chưa tải được: ${notices.join(', ')}.`
      : '');

    if (notices.length === 3) {
      throw new Error('Không tải được dữ liệu bạn bè.');
    }
  };

  useEffect(() => {
    let isMounted = true;

    async function loadFriendData() {
      setIsLoading(true);
      setError('');
      setLoadNotice('');

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
      setSearchResults([]);
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

  useEffect(() => {
    if (!shareMode) {
      setSelectedShareUser(null);
      setShareMessage('');
      setShareActionStatus('');
    }
  }, [shareMode]);

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

  const tabCounts = {
    friends: friends.length,
    incoming: incomingRequests.length,
    sent: sentRequests.length,
  };

  const handleQueryChange = (value) => {
    setQuery(value);
    setRequestStatus('');
    if (!shareMode) {
      setShareActionStatus('');
    }
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
    if (!user?.id) return;

    const relation = relationMap.get(user.id);

    if (relation?.type === 'friend') {
      await refreshAndReport(
        `Đã xóa bạn bè với ${user.name}.`,
        () => removeFriend(user.id),
        `search:remove:${user.id}`,
      );
      return;
    }

    if (relation?.type === 'incoming') {
      if (!requireRequestId(relation.requestId)) return;
      await refreshAndReport(
        `Đã chấp nhận lời mời của ${user.name}.`,
        () => acceptFriendRequest(relation.requestId),
        `search:accept:${relation.requestId}`,
      );
      return;
    }

    if (relation?.type === 'sent') {
      if (!requireRequestId(relation.requestId)) return;
      await refreshAndReport(
        `Đã hủy lời mời gửi đến ${user.name}.`,
        () => cancelFriendRequest(relation.requestId),
        `search:cancel:${relation.requestId}`,
      );
      return;
    }

    await refreshAndReport(
      `Đã gửi lời mời kết bạn tới ${user.name}.`,
      () => sendFriendRequest(user.id),
      `search:add:${user.id}`,
    );
  };

  const selectShareUser = (user) => {
    if (!shareMode || !user?.id) return;
    setSelectedShareUser(user);
    setShareActionStatus(`Đã chọn ${user.name}.`);
  };

  const handleShareConfirm = async () => {
    if (!shareMode) return;
    if (!selectedShareUser?.id) {
      setShareActionStatus('Chọn một người nhận trước khi xác nhận.');
      return;
    }

    if (!onShareConfirm) {
      setShareActionStatus('Thiếu callback gửi chia sẻ.');
      return;
    }

    try {
      setActionKey(`share:${selectedShareUser.id}`);
      await onShareConfirm({
        receiverId: selectedShareUser.id,
        message: shareMessage.trim() || null,
      });
      setShareActionStatus('Đã gửi chia sẻ thành công.');
      setSelectedShareUser(null);
      setShareMessage('');
      onCancelShare?.();
    } catch (error) {
      setShareActionStatus(error instanceof Error ? error.message : 'Không thể chia sẻ nội dung.');
    } finally {
      setActionKey('');
    }
  };

  const shareLabel = shareItemType ? String(shareItemType).toUpperCase() : 'NỘI DUNG';

  return (
    <aside className="friend-activity">
      <div className="friend-activity-header">
        <span>{shareMode ? 'Chọn người nhận' : 'Bạn bè'}</span>
        <button type="button" aria-label="Đóng bạn bè" onClick={onClose}>
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      {shareMode ? (
        <div className="friend-share-banner">
          <strong>Chia sẻ {shareLabel}</strong>
          <span>{shareItemTitle || 'Nội dung đang phát'}</span>
          <button type="button" className="friend-share-banner-button" onClick={onCancelShare}>Hủy chia sẻ</button>
        </div>
      ) : null}

      {!shareMode ? (
        <label className="friend-activity-search">
          <span className="material-symbols-outlined">search</span>
          <input
            type="text"
            placeholder="Tìm theo ID hoặc tên"
            value={query}
            onChange={(event) => handleQueryChange(event.target.value)}
          />
        </label>
      ) : null}

      {requestStatus && <div className="friend-activity-status">{requestStatus}</div>}
      {loadNotice && <div className="friend-activity-status muted">{loadNotice}</div>}
      {shareActionStatus && <div className="friend-activity-status muted">{shareActionStatus}</div>}

      {isLoading ? (
        <div className="friend-activity-empty">Đang tải dữ liệu bạn bè...</div>
      ) : error ? (
        <div className="friend-activity-empty error">{error}</div>
      ) : null}

      {!shareMode ? (
        <div className="friend-tabs">
          <button type="button" className={activeTab === 'friends' ? 'active' : ''} onClick={() => setActiveTab('friends')}>
            <span>Bạn bè</span>
            <strong>{tabCounts.friends}</strong>
          </button>
          <button type="button" className={activeTab === 'incoming' ? 'active' : ''} onClick={() => setActiveTab('incoming')}>
            <span>Đã nhận</span>
            <strong>{tabCounts.incoming}</strong>
          </button>
          <button type="button" className={activeTab === 'sent' ? 'active' : ''} onClick={() => setActiveTab('sent')}>
            <span>Đã gửi</span>
            <strong>{tabCounts.sent}</strong>
          </button>
        </div>
      ) : null}

      <div className="friend-activity-list">
        {!shareMode && activeTab === 'friends' && incomingRequests.length > 0 && (
          <div className="friend-activity-notice">
            <div className="friend-activity-notice-copy">
              <span className="material-symbols-outlined">notifications</span>
              <div>
                <strong>Có {incomingRequests.length} lời mời kết bạn đang chờ</strong>
                <small>Chuyển sang tab Đã nhận để xử lý các thông báo này.</small>
              </div>
            </div>
            <button type="button" className="friend-activity-notice-button" onClick={() => setActiveTab('incoming')}>
              Xem Đã nhận
            </button>
          </div>
        )}

        {!shareMode && activeTab === 'friends' && activeTabItems.map((friend) => (
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
              className="friend-activity-action-button secondary"
              disabled={Boolean(actionKey)}
              onClick={() => {
                if (!friend?.id) return;
                void refreshAndReport(`Đã xóa bạn bè với ${friend.name}.`, () => removeFriend(friend.id), `friend:${friend.id}`);
              }}
            >
              Xóa bạn
            </button>
          </article>
        ))}

        {!shareMode && filteredSearchResults.length > 0 && (
          <>
            <div className="friend-activity-search-label">{shareMode ? 'Người có thể nhận' : 'Kết quả tìm kiếm'}</div>
            {filteredSearchResults.map((user) => {
              const relation = relationMap.get(user.id);
              const actionLabel = shareMode
                ? (selectedShareUser?.id === user.id ? 'Đã chọn' : 'Chọn')
                : relation?.type === 'friend'
                  ? 'Xóa bạn'
                  : relation?.type === 'incoming'
                    ? 'Chấp nhận'
                    : relation?.type === 'sent'
                      ? 'Hủy lời mời'
                      : 'Kết bạn';

              return (
                <article className={`friend-activity-item ${shareMode ? 'suggested' : ''} ${selectedShareUser?.id === user.id ? 'selected' : ''}`} key={user.id}>
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
                  <button
                    type="button"
                    className="friend-activity-profile-trigger friend-activity-copy-trigger"
                    onClick={() => (shareMode ? selectShareUser(user) : openProfile(user))}
                  >
                    <strong>{user.name}</strong>
                    <span>{shareMode ? 'Chọn người nhận' : relation?.type === 'friend' ? 'Đã là bạn bè' : user.status}</span>
                    <small>@{user.handle} • {shareMode ? 'Chọn để chia sẻ' : 'Kết quả tìm kiếm'}</small>
                  </button>
                  <button
                    type="button"
                    className="friend-activity-action-button"
                    disabled={Boolean(actionKey)}
                    onClick={() => (shareMode ? selectShareUser(user) : handleSearchUserAction(user))}
                  >
                    {actionLabel}
                  </button>
                </article>
              );
            })}
          </>
        )}

        {!shareMode && !isLoading && !error && query.trim().length < 2 && activeTabItems.length === 0 && (
          <div className="friend-activity-empty">{activeTabEmptyMessage}</div>
        )}

        {shareMode && friends.map((friend) => (
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
            <button type="button" className="friend-activity-action-button" disabled={Boolean(actionKey)} onClick={() => selectShareUser(friend)}>
              Chọn
            </button>
          </article>
        ))}

        {!shareMode && activeTab === 'incoming' && activeTabItems.map((request) => (
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

        {!shareMode && activeTab === 'sent' && activeTabItems.map((request) => (
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

      {shareMode ? (
        <div className="friend-share-confirm">
          <div className="friend-share-confirm-copy">
            <strong>{selectedShareUser ? `Gửi tới ${selectedShareUser.name}` : 'Chọn một người nhận'}</strong>
            <span>{selectedShareUser ? `@${selectedShareUser.handle}` : 'Nhấn chọn ở kết quả tìm kiếm để xác nhận.'}</span>
          </div>
          <label className="friend-share-message">
            <span>Lời nhắn</span>
            <textarea
              rows={3}
              value={shareMessage}
              onChange={(event) => setShareMessage(event.target.value)}
              placeholder="Gửi kèm lời nhắn..."
            />
          </label>
          <div className="friend-share-actions">
            <button type="button" className="friend-share-cancel" onClick={onCancelShare}>Hủy</button>
            <button type="button" className="friend-share-confirm-button" onClick={handleShareConfirm} disabled={Boolean(actionKey)}>
              {actionKey ? 'Đang gửi...' : 'Xác nhận'}
            </button>
          </div>
        </div>
      ) : null}
    </aside>
  );
}
