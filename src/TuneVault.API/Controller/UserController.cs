using Microsoft.AspNetCore.Mvc;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.API.Controller
{
    /// <summary>
    /// Controller xử lý các API liên quan đến User
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDAO _userDAO;

        // .NET tự inject UserDAO vào đây
        public UserController(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }

        /// <summary>
        /// GET api/user — Lấy tất cả users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userDAO.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// GET api/user/{id} — Lấy user theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userDAO.GetByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User không tồn tại" });

            return Ok(user);
        }
    }
}