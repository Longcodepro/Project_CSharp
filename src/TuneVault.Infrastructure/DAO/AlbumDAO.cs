using Dapper;
using TuneVault.Domain.Entities;

namespace TuneVault.Infrastructure.DAO
{
    public class AlbumDAO
    {
        private readonly DapperContext _context;

        public AlbumDAO(DapperContext context)
        {
            _context = context;
        }

        // Lấy tất cả album của 1 user
        public async Task<IEnumerable<Album>> GetByOwnerAsync(string ownerId)
        {
            var sql = "SELECT * FROM Albums WHERE OwnerId = @OwnerId";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Album>(sql, new { OwnerId = ownerId });
        }

        // Lấy album theo Id
        public async Task<Album?> GetByIdAsync(string id)
        {
            var sql = "SELECT * FROM Albums WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Album>(sql, new { Id = id });
        }

        // Tạo album mới
        public async Task CreateAsync(Album album)
        {
            var sql = @"
                INSERT INTO Albums (Id, OwnerId, Title, CoverImgUrl, ReleaseDate, IsPublic)
                VALUES (@Id, @OwnerId, @Title, @CoverImgUrl, @ReleaseDate, @IsPublic)";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, album);
        }

        // Sửa album
        public async Task UpdateAsync(Album album)
        {
            var sql = @"
                UPDATE Albums SET
                    Title       = @Title,
                    CoverImgUrl = @CoverImgUrl,
                    ReleaseDate = @ReleaseDate,
                    IsPublic    = @IsPublic
                WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, album);
        }

        // Xóa album
        public async Task DeleteAsync(string id)
        {
            var sql = "DELETE FROM Albums WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // Đặt công khai / riêng tư
        public async Task SetVisibilityAsync(string id, bool isPublic)
        {
            var sql = "UPDATE Albums SET IsPublic = @IsPublic WHERE Id = @Id";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id, IsPublic = isPublic });
        }

        // Thêm bài vào album
        public async Task AddTrackAsync(AlbumTrack track)
        {
            var sql = @"
                INSERT INTO AlbumTracks (AlbumId, MediaItemId, TrackOrder)
                VALUES (@AlbumId, @MediaItemId, @TrackOrder)";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, track);
        }

        // Xóa bài khỏi album
        public async Task RemoveTrackAsync(string albumId, string mediaItemId)
        {
            var sql = "DELETE FROM AlbumTracks WHERE AlbumId = @AlbumId AND MediaItemId = @MediaItemId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { AlbumId = albumId, MediaItemId = mediaItemId });
        }

        // Sắp xếp lại bài trong album
        public async Task ReorderTrackAsync(string albumId, string mediaItemId, int newOrder)
        {
            var sql = @"
                UPDATE AlbumTracks SET TrackOrder = @TrackOrder
                WHERE AlbumId = @AlbumId AND MediaItemId = @MediaItemId";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { AlbumId = albumId, MediaItemId = mediaItemId, TrackOrder = newOrder });
        }

        // Lấy danh sách bài trong album
        public async Task<IEnumerable<MediaItem>> GetTracksAsync(string albumId)
        {
            var sql = @"
                SELECT m.* FROM MediaItems m
                INNER JOIN AlbumTracks at ON m.Id = at.MediaItemId
                WHERE at.AlbumId = @AlbumId
                ORDER BY at.TrackOrder";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MediaItem>(sql, new { AlbumId = albumId });
        }
    }
}