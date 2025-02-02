using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoStreamingAPI.Models;
using VideoStreamingAPI.Services;

namespace VideoStreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ManageRolesController : Controller
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager<AppUserModel> _userManager;
        private IServiceProvider _serviceProvider;

        public ManageRolesController(RoleManager roleManager, UserManager<AppUserModel> userManager, IServiceProvider serviceProvider)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("addRole")]
        public async Task<IActionResult> Index()
        {
            try
            {
                await _roleManager.Initialize(_serviceProvider);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("AssignRole")]
        public async Task<IActionResult> AssingRole()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Ten użytkownik nie istnieje");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok("Rola przypisana poprawnie");
            }
            return BadRequest(result.Errors);
        }
    }
}