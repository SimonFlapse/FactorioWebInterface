using FactorioWebInterface.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FactorioWebInterface.Services
{
    public interface IWebAccountManager
    {
        Task<ApplicationUser> FindByNameAsync(string username);
        Task<ApplicationUser> FindByIdAsync(string id);
        Task<ApplicationUser> GetUserAsync(ClaimsPrincipal user);
        Task CreateAccountAsync(string username, string password, string[] roles);
        Task ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);
        Task AddRoleAsync(ApplicationUser user, string role);
        Task RemoveRoleAsync(ApplicationUser user, string role);
        Task SuspendAccountAsync(ApplicationUser user, bool suspended = true);
    }

    public class WebAccountManager : IWebAccountManager
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WebAccountManager> _logger;

        public WebAccountManager(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<WebAccountManager> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task AddRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("The user: " + user.UserName + " has been added to the role: " + role);
        }

        public async Task ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword)
        {
            await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            _logger.LogInformation("The user: " + user.UserName + " has changed their password");
        }

        public async Task CreateAccountAsync(string username, string password, string[] roles)
        {
            var id = Guid.NewGuid().ToString();
            var user = new ApplicationUser()
            {
                Id = id,
                UserName = username
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("User account couldn't be created");
            }
            foreach (string role in roles) {
                await AddRoleAsync(user, role);
            }
            _logger.LogInformation("User account created with username: " + username + " and id: " + id);
        }

        public async Task<ApplicationUser> FindByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser> FindByNameAsync(string username)
        {
            return await _userManager.FindByIdAsync(username);
        }

        public async Task<ApplicationUser> GetUserAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }

        public async Task RemoveRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.RemoveFromRoleAsync(user, role);
            _logger.LogInformation("The user: " + user.UserName + " has been removed from the role: " + role);
        }

        public async Task SuspendAccountAsync(ApplicationUser user, bool suspended = true)
        {
            user.Suspended = suspended;
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("The user: " + user.UserName + " has been suspended");
        }
    }
}
