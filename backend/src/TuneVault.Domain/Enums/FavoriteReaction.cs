namespace TuneVault.Domain.Enums;

/// <summary>
/// Định nghĩa các loại trạng thái cảm xúc hiển thị khi người dùng tương tác yêu thích một bài hát/vật phẩm phương tiện.
/// </summary>
public enum FavoriteReaction
{
    /// <summary>
    /// Trạng thái không thích bài hát.
    /// </summary>
    Dislike = 0,

    /// <summary>
    /// Trạng thái Thích thông thường (Mặc định).
    /// </summary>
    Like = 1,

    /// <summary>
    /// Trạng thái cực kỳ Yêu thích, đưa vào danh sách đặc biệt (Yêu thích nhất).
    /// </summary>
    Love = 2,

    /// <summary>
    /// Cảm xúc âm nhạc mang lại cảm giác thư giãn, nhẹ nhàng.
    /// </summary>
    Chill = 3,

    /// <summary>
    /// Cảm xúc âm nhạc mang tính chất sôi động, truyền năng lượng và động lực.
    /// </summary>
    Energetic = 4,

    /// <summary>
    /// Trạng thái lưu nội dung để xem hoặc nghe lại sau.
    /// </summary>
    Save = 5,

    /// <summary>
    /// Trạng thái xóa bỏ phản ứng khỏi danh sách yêu thích.
    /// </summary>
    Remove = 6
}
