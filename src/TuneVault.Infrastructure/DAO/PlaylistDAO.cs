using Dapper;
using TuneVault.Domain.Entities;

namespace TuneVault.Infrastructure.DAO
{
    public class PlaylistDAO
    {
        private readonly DapperContext _context;

        public PlaylistDAO(DapperContext context)
        {
            _context = context;
        }

        // Lấy tất cả playlist của 1 user
        public async Task<IEnumerable<Playlist>> GetByOwnerAsync(string ownerId)
        {
            var sql = "SELECT * FROM Playlists WHERE OwnerId = @OwnerId";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Playlist>(sql, new { OwnerId = ownerId });
        }

        // Lấy playlist theo Id
        public async Task<Playlist?> GetByIdAsync(string id)
        {
            var sql = "SELECT * FROM Playlists WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Playlist>(sql, new { Id = id });
        }

        // Tạo playlist mới
        public async Task CreateAsync(Playlist playlist)
        {
            var sql = @"
                INSERT INTO Playlists (Id, OwnerId, Title, CoverImgUrl, IsPublic, CreatedAt)
                VALUES (@Id, @OwnerId, @Title, @CoverImgUrl, @IsPublic, @CreatedAt)";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, playlist);
        }

        // Sửa playlist
        public async Task UpdateAsync(Playlist playlist)
        {
            var sql = @"
                UPDATE Playlists SET
                    Title       = @Title,
                    CoverImgUrl = @CoverImgUrl,
                    IsPublic    = @IsPublic
                WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, playlist);
        }

        // Xóa playlist
        public async Task DeleteAsync(string id)
        {
            var sql = "DELETE FROM Playlists WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // Đặt công khai / riêng tư
        public async Task SetVisibilityAsync(string id, bool isPublic)
        {
            var sql = "UPDATE Playlists SET IsPublic = @IsPublic WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id, IsPublic = isPublic });
        }

        // Thêm bài vào playlist
        public async Task AddTrackAsync(PlaylistTrack track)
        {
            var sql = @"
                INSERT INTO PlaylistTracks (PlaylistId, MediaItemId, TrackOrder, AddedAt)
                VALUES (@PlaylistId, @MediaItemId, @TrackOrder, @AddedAt)";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, track);
        }

        // Xóa bài khỏi playlist
        public async Task RemoveTrackAsync(string playlistId, string mediaItemId)
        {
            var sql = "DELETE FROM PlaylistTracks WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { PlaylistId = playlistId, MediaItemId = mediaItemId });
        }

        // Sắp xếp lại bài trong playlist
        public async Task ReorderTrackAsync(string playlistId, string mediaItemId, int newOrder)
        {
            var sql = @"
                UPDATE PlaylistTracks SET TrackOrder = @TrackOrder
                WHERE PlaylistId = @PlaylistId AND MediaItemId = @MediaItemId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { PlaylistId = playlistId, MediaItemId = mediaItemId, TrackOrder = newOrder });
        }

        // Lấy danh sách bài trong playlist
        public async Task<IEnumerable<MediaItem>> GetTracksAsync(string playlistId)
        {
            var sql = @"
                SELECT m.* FROM MediaItems m
                INNER JOIN PlaylistTracks pt ON m.Id = pt.MediaItemId
                WHERE pt.PlaylistId = @PlaylistId
                ORDER BY pt.TrackOrder";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MediaItem>(sql, new { PlaylistId = playlistId });
        }
    }
}