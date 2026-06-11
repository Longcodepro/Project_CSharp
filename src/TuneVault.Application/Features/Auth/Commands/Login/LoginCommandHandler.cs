// using MediatR;
// using TuneVault.Domain.Interfaces;
// using TuneVault.Application.Interfaces;

// namespace TuneVault.Application.Features.Auth.Commands.Login;

// /// <summary>
// /// Bộ xử lý nghiệp vụ cho tính năng đăng nhập hệ thống TuneVault.
// /// Tiến hành xác thực tài khoản song song dựa trên Email đối với Admin hoặc IdDisplay đối với Người dùng,
// /// đối chiếu mật khẩu, trích xuất vai trò chi tiết và gọi dịch vụ để cấp phát chuỗi mã thông báo bảo mật JWT.
// /// </summary>
// public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
// {
//     private readonly IUserRepository _userRepository;
//     private readonly IAdminRepository _adminRepository;
//     private readonly IJwtTokenGenerator _jwtTokenGenerator;

//     /// <summary>
//     /// Hàm khởi tạo (Constructor) để nạp các dịch vụ và kho lưu trữ dữ liệu cần thiết từ DI Container.
//     /// Sử dụng cơ chế Dependency Injection (DI) để tự động truyền các đối tượng thực thi vào.
//     /// </summary>
//     /// <param name="userRepository">Giao diện tương tác với kho lưu trữ người dùng (User) ở tầng Domain.</param>
//     /// <param name="adminRepository">Giao diện tương tác với kho lưu trữ quản trị viên (Admin) ở tầng Domain.</param>
//     /// <param name="jwtTokenGenerator">Giao diện dịch vụ hỗ trợ sinh và đóng gói mã thông báo bảo mật JWT ở tầng Application.</param>
//     public LoginCommandHandler(
//         IUserRepository userRepository, 
//         IAdminRepository adminRepository, 
//         IJwtTokenGenerator jwtTokenGenerator)
//     {
//         _userRepository = userRepository;
//         _adminRepository = adminRepository;
//         _jwtTokenGenerator = jwtTokenGenerator;
//     }

//     /// <summary>
//     /// Phương thức cốt lõi thực hiện luồng logic kiểm tra thông tin đăng nhập và phân quyền tài khoản chi tiết.
//     /// </summary>
//     /// <param name="request">Đối tượng chứa gói dữ liệu đầu vào gồm IdDisplay (hoặc Email của Admin) và Password từ Client.</param>
//     /// <param name="cancellationToken">Mã thông báo hỗ trợ hủy bỏ tác vụ bất đồng bộ khi luồng kết nối bị ngắt đột ngột.</param>
//     /// <returns>Một chuỗi ký tự (string) đại diện cho mã thông báo JWT hợp lệ chứa đầy đủ danh sách quyền hạn.</returns>
//     /// <exception cref="UnauthorizedAccessException">Ném ra ngoại lệ nếu tài khoản không tồn tại trên cả 2 hệ thống hoặc sai mật khẩu.</exception>
//     public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
//     {
//         var roles = new List<string>();
//         string userId = string.Empty;
//         string tokenUsername = string.Empty;
//         string passwordHashInDb = string.Empty;

//         // BƯỚC 1: Thử tìm kiếm tài khoản trong hệ thống bảng Admin trước
//         // Vì Admin không có cột IdDisplay hay Username, ta sẽ tìm kiếm dựa trên cột Email trong cơ sở dữ liệu
//         var admin = await _adminRepository.GetByUsernameAsync(request.IdDisplay);

//         if (admin != null)
//         {
//             userId = admin.Id;
//             tokenUsername = admin.Email; // Sử dụng Email làm định danh đại diện trong Token cho tài khoản Admin
//             passwordHashInDb = admin.PasswordHash;

//             // Đóng gói quyền hạn cho Admin: Gồm nhãn "Admin" chung và chức vụ hành chính cụ thể lưu trong DB
//             roles.Add("Admin"); 
//             roles.Add(admin.Role); // Nhận các giá trị thực tế như: "SuperAdmin", "ContentModerator", "SupportRep"
//         }
//         else
//         {
//             // BƯỚC 2: Nếu không tìm thấy ở bảng Admin, tiếp tục tìm kiếm ở bảng User thông thường theo IdDisplay
//             var user = await _userRepository.GetByUsernameAsync(request.IdDisplay);

//             // Nếu cả hai bảng đều không tìm thấy bản ghi nào khớp -> Báo lỗi đồng nhất để bảo mật thông tin
//             if (user == null)
//             {
//                 throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");
//             }

//             userId = user.Id;
//             tokenUsername = user.IdDisplay; // Sử dụng IdDisplay chuẩn của User làm tên hiển thị trên Token
//             passwordHashInDb = user.PasswordHash;

//             // Phân loại vai trò người dùng dựa trên thuộc tính IsArtist kiểu bool trong bảng Users
//             if (user.IsArtist)
//             {
//                 roles.Add("Artist");       // Quyền dành riêng cho nghệ sĩ (Đăng nhạc, quản lý Album...)
//             }
//             else
//             {
//                 roles.Add("Listener");     // Quyền dành cho người nghe nhạc phổ thông (Nghe nhạc, tạo Playlist...)
//             }
//         }

//         // BƯỚC 3: Tiến hành so khớp chuỗi mật khẩu lấy được từ cơ sở dữ liệu
//         if (passwordHashInDb != request.Password)
//         {
//             throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");
//         }

//         // BƯỚC 4: Gọi dịch vụ cấp phát chuỗi Token mang theo toàn bộ danh sách vai trò (roles) đã xác thực
//         var token = _jwtTokenGenerator.GenerateToken(userId, tokenUsername, roles);

//         // BƯỚC 5: Trả chuỗi token về cho tầng phía trên (API Controller)
//         return token;
//     }
// }