// using System;
// using System.Collections.Generic;
// using System.Data; // Thư viện gốc của ADO.NET / Dapper
// using System.Threading;
// using System.Threading.Tasks;
// using Dapper; // Bộ gõ Dapper siêu nhẹ
// using TuneVault.Domain.Entities;
// using TuneVault.Domain.Interfaces;

// namespace TuneVault.Infrastructure.Repositories;

// /// <summary>
// /// Triển khai kho lưu trữ dữ liệu Media bằng Dapper.
// /// </summary>
// public class MediaRepository : IMediaRepository
// {
//     private readonly IDbConnection _dbConnection;

//     /// <summary>
//     /// Khởi tạo Repository với kết nối Database được cấu hình sẵn.
//     /// </summary>
//     public MediaRepository(IDbConnection dbConnection)
//     {
//         _dbConnection = dbConnection;
//     }

//     /// <summary>
//     /// Ví dụ một hàm truy vấn thực tế bằng Dapper nếu sau này bạn cần dùng.
//     /// </summary>
//     public async Task<IReadOnlyCollection<MediaItem>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
//     {
//         const string query = "SELECT * FROM MediaItems WHERE Title LIKE @SearchTerm";
        
//         // Dapper map data trực tiếp từ Query
//         var results = await _dbConnection.QueryAsync<MediaItem>(query, new { SearchTerm = $"%{searchTerm}%" });
        
//         return results.AsList().AsReadOnly();
//     }

//     // =========================================================================
//     // CÁC HÀM BẠN CHƯA LÀM TỚI: Ném Exception để bypass qua bộ build của hệ thống
//     // =========================================================================

//     public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
//         => throw new NotImplementedException();

//     public async Task AddAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
//         => throw new NotImplementedException();

//     public async Task UpdateAsync(MediaItem mediaItem, CancellationToken cancellationToken = default)
//         => throw new NotImplementedException();

//     public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
//         => throw new NotImplementedException();
// }