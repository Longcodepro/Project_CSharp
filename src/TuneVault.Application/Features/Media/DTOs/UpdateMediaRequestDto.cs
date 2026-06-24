namespace TuneVault.Application.Features.Media.DTOs;

/// <summary>
/// DTO nhận dữ liệu cập nhật thông tin bài hát từ client.
/// Chỉ cho phép thay đổi các thông tin metadata — không thay đổi file media gốc.
/// </summary>
/// <param name="Title">Tiêu đề mới (tối đa 30 ký tự, bắt buộc).</param>
/// <param name="Description">Mô tả mới (nullable, tối đa 500 ký tự).</param>
/// <param name="Genre">Thể loại mới (nullable).</param>
/// <param name="MediaUrl">URL file media mới (nullable — null để giữ file gốc).</param>
/// <param name="CoverImageUrl">URL ảnh bìa mới (nullable — null để xóa ảnh bìa).</param>
/// <param name="CanvasUrl">URL canvas mới (nullable — null để xóa canvas).</param>
/// <param name="IsPublic">Trạng thái hiển thị công khai.</param>
/// <param name="AccessLevel">Cấp độ truy cập: Normal (0) hoặc Premium (1).</param>
public sealed record UpdateMediaRequestDto(
    string Title,
    string? Description,
    string? Genre,
    string? MediaUrl,
    string? CoverImageUrl,
    string? CanvasUrl,
    bool IsPublic,
    int AccessLevel
);
