namespace TuneVault.Domain.Entities;

/// <summary>
/// Định nghĩa các loại trạng thái cảm xúc hiển thị khi người dùng tương tác yêu thích một bài hát/vật phẩm phương tiện.
/// </summary>
public enum FavoriteReaction
{
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
    Energetic = 4
}