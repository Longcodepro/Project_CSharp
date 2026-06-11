using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Cung cấp các hàm tiện ích dùng chung để ánh xạ dữ liệu từ DAO hoặc cơ sở dữ liệu sang entity trong tầng Domain.
/// Lớp này hỗ trợ đọc giá trị theo tên cột, chuyển đổi kiểu dữ liệu và gán dữ liệu vào các property có setter riêng tư.
/// </summary>
internal static class RepositoryMappingHelper
{
    /// <summary>
    /// Tạo một entity Domain mà không gọi constructor, sau đó gán giá trị cho các property được chỉ định.
    /// Cách này phù hợp với các entity có constructor hoặc setter bị giới hạn để bảo vệ invariant nghiệp vụ.
    /// </summary>
    public static T CreateEntity<T>(params (string PropertyName, object? Value)[] values) where T : class
    {
        var entity = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

        foreach (var (propertyName, value) in values)
        {
            SetProperty(entity, propertyName, value);
        }

        return entity;
    }

    /// <summary>
    /// Chuyển đổi Guid của tầng Domain sang chuỗi định danh dùng để lưu trữ trong cơ sở dữ liệu.
    /// </summary>
    public static string ToDatabaseId(Guid id) => id.ToString();

    /// <summary>
    /// Đọc giá trị dạng chuỗi từ một dòng dữ liệu theo tên cột.
    /// Nếu giá trị không tồn tại hoặc là DBNull, phương thức sẽ trả về giá trị mặc định.
    /// </summary>
    public static string ReadString(object row, string columnName, string defaultValue = "")
    {
        var value = ReadValue(row, columnName);
        return value == null || value is DBNull ? defaultValue : Convert.ToString(value) ?? defaultValue;
    }

    /// <summary>
    /// Đọc giá trị DateTime từ một dòng dữ liệu theo tên cột.
    /// Nếu dữ liệu rỗng, phương thức sẽ trả về giá trị mặc định hoặc thời điểm UTC hiện tại.
    /// </summary>
    public static DateTime ReadDateTime(object row, string columnName, DateTime? defaultValue = null)
    {
        var value = ReadValue(row, columnName);
        if (value == null || value is DBNull)
            return defaultValue ?? DateTime.UtcNow;

        return value is DateTime dateTime ? dateTime : Convert.ToDateTime(value);
    }

    /// <summary>
    /// Đọc giá trị DateTime có thể null từ một dòng dữ liệu theo tên cột.
    /// </summary>
    public static DateTime? ReadNullableDateTime(object row, string columnName)
    {
        var value = ReadValue(row, columnName);
        if (value == null || value is DBNull)
            return null;

        return value is DateTime dateTime ? dateTime : Convert.ToDateTime(value);
    }

    /// <summary>
    /// Đọc giá trị boolean từ một dòng dữ liệu theo tên cột.
    /// Nếu dữ liệu rỗng, phương thức sẽ trả về giá trị mặc định.
    /// </summary>
    public static bool ReadBool(object row, string columnName, bool defaultValue = false)
    {
        var value = ReadValue(row, columnName);
        if (value == null || value is DBNull)
            return defaultValue;

        return value is bool boolValue ? boolValue : Convert.ToBoolean(value);
    }

    /// <summary>
    /// Đọc và chuyển đổi giá trị enum từ một dòng dữ liệu theo tên cột.
    /// Phương thức hỗ trợ dữ liệu nguồn là enum, chuỗi hoặc giá trị số.
    /// </summary>
    public static TEnum ReadEnum<TEnum>(object row, string columnName, TEnum defaultValue) where TEnum : struct, Enum
    {
        var value = ReadValue(row, columnName);
        if (value == null || value is DBNull)
            return defaultValue;

        if (value is TEnum enumValue)
            return enumValue;

        if (value is string text)
        {
            var normalized = NormalizeEnumText(typeof(TEnum), text);
            return Enum.TryParse<TEnum>(normalized, ignoreCase: true, out var parsed) ? parsed : defaultValue;
        }

        return (TEnum)Enum.ToObject(typeof(TEnum), value);
    }

    /// <summary>
    /// Đọc giá trị thô từ một dòng dữ liệu bằng tên cột hoặc tên property.
    /// Phương thức ưu tiên đọc từ dictionary, sau đó đọc bằng reflection trên object.
    /// </summary>
    private static object? ReadValue(object row, string columnName)
    {
        if (row is IDictionary<string, object> dictionary && dictionary.TryGetValue(columnName, out var dictionaryValue))
            return dictionaryValue;

        var property = row.GetType().GetProperty(columnName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        return property?.GetValue(row);
    }

    /// <summary>
    /// Chuẩn hóa chuỗi enum từ dữ liệu cũ hoặc dữ liệu trong cơ sở dữ liệu sang tên enum hiện tại trong Domain.
    /// Phương thức giúp repository tương thích với các giá trị lưu trữ khác tên so với enum trong code.
    /// </summary>
    private static string NormalizeEnumText(Type enumType, string value)
    {
        if (enumType.Name == "ShareType" && value.Equals("Track", StringComparison.OrdinalIgnoreCase))
            return "MediaItem";

        if (enumType.Name == "NotificationType")
        {
            if (value.Equals("Share", StringComparison.OrdinalIgnoreCase) || value.Equals("Shared", StringComparison.OrdinalIgnoreCase))
                return "MediaShared";

            if (value.Equals("System", StringComparison.OrdinalIgnoreCase))
                return "SystemAlert";
        }

        return value;
    }

    /// <summary>
    /// Gán giá trị cho một property của entity, kể cả khi property có setter private hoặc chỉ có backing field.
    /// </summary>
    private static void SetProperty<T>(T entity, string propertyName, object? value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
            return;

        var convertedValue = ConvertValue(value, property.PropertyType);
        var setter = property.GetSetMethod(nonPublic: true);

        if (setter != null)
        {
            setter.Invoke(entity, new[] { convertedValue });
            return;
        }

        var backingField = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        backingField?.SetValue(entity, convertedValue);
    }

    /// <summary>
    /// Chuyển đổi giá trị nguồn sang kiểu dữ liệu đích của property.
    /// Phương thức xử lý các kiểu phổ biến như nullable, enum, Guid, string, DateTime và bool.
    /// </summary>
    private static object? ConvertValue(object? value, Type destinationType)
    {
        if (value == null || value is DBNull)
            return Nullable.GetUnderlyingType(destinationType) != null || !destinationType.IsValueType
                ? null
                : Activator.CreateInstance(destinationType);

        var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        if (targetType.IsEnum)
        {
            if (value is string text)
            {
                var normalized = NormalizeEnumText(targetType, text);
                return Enum.TryParse(targetType, normalized, ignoreCase: true, out var parsed)
                    ? parsed
                    : Activator.CreateInstance(targetType);
            }

            return Enum.ToObject(targetType, value);
        }

        if (targetType == typeof(Guid))
            return value is Guid guid ? guid : Guid.Parse(Convert.ToString(value)!);

        if (targetType == typeof(string))
            return Convert.ToString(value) ?? string.Empty;

        if (targetType == typeof(DateTime) && value is DateTime dateTime)
            return dateTime;

        if (targetType == typeof(bool) && value is bool boolValue)
            return boolValue;

        return Convert.ChangeType(value, targetType);
    }
}
