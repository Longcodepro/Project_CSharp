using Dapper;
using TuneVault.Domain.Entities;

namespace TuneVault.Infrastructure.DAO
{
    public class SearchDAO
    {
        private readonly DapperContext _context;

        public SearchDAO(DapperContext context)
        {
            _context = context;
        }

        // Tìm kiếm bài hát / podcast
        public async Task<IEnumerable<MediaItem>> SearchMediaAsync(string keyword)
        {
            var sql = @"
                SELECT * FROM MediaItems
                WHERE IsPublic = 1
                AND Title LIKE @Keyword";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MediaItem>(sql, new { Keyword = $"%{keyword}%" });
        }

        // Tìm kiếm theo thể loại / genre
        public async Task<IEnumerable<MediaItem>> SearchByGenreAsync(string genre)
        {
            var sql = @"
                SELECT * FROM MediaItems
                WHERE IsPublic = 1
                AND Genre = @Genre";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MediaItem>(sql, new { Genre = genre });
        }

        // Tìm kiếm nghệ sĩ
        public async Task<IEnumerable<User>> SearchArtistAsync(string keyword)
        {
            var sql = @"
                SELECT * FROM Users
                WHERE UserName LIKE @Keyword
                OR DisplayName LIKE @Keyword";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<User>(sql, new { Keyword = $"%{keyword}%" });
        }

        // Tìm kiếm album
        public async Task<IEnumerable<Album>> SearchAlbumAsync(string keyword)
        {
            var sql = @"
                SELECT * FROM Albums
                WHERE IsPublic = 1
                AND Title LIKE @Keyword";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Album>(sql, new { Keyword = $"%{keyword}%" });
        }

        // Tìm kiếm playlist
        public async Task<IEnumerable<Playlist>> SearchPlaylistAsync(string keyword)
        {
            var sql = @"
                SELECT * FROM Playlists
                WHERE IsPublic = 1
                AND Title LIKE @Keyword";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Playlist>(sql, new { Keyword = $"%{keyword}%" });
        }

        // Trending — bài nghe nhiều nhất
        public async Task<IEnumerable<MediaItem>> GetTrendingAsync(int top = 10)
        {
            var sql = @"
                SELECT TOP (@Top) * FROM MediaItems
                WHERE IsPublic = 1
                ORDER BY ViewCount DESC";
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MediaItem>(sql, new { Top = top });
        }
    }
}