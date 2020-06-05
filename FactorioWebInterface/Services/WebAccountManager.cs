using FactorioWebInterface.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FactorioWebInterface.Services
{
    public interface IWebAccountManager
    {
        Task<ApplicationUser> FindByNameAsync(string username);
        Task<ApplicationUser> FindByIdAsync(string id);
        Task<ApplicationUser> GetUserAsync(ClaimsPrincipal user);
        List<ApplicationUser> GetUsers();
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> CreateAccountAsync(string username, string password, string[] roles);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<IdentityResult> AddRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> AddRolesAsync(ApplicationUser user, string[] roles);
        Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> SuspendAccountAsync(ApplicationUser user, bool suspended = true);
        Task<IdentityResult> DeleteAccountAsync(ApplicationUser user);
        Task<IdentityResult> ChangeUsernameAsync(ApplicationUser user, string username);
        Task<string> ResetPasswordAsync(ApplicationUser user);
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

        public async Task<IdentityResult> AddRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} has been added to the role: {role}", user.UserName, role);
            }
            return result;
        }

        public async Task<IdentityResult> AddRolesAsync(ApplicationUser user, string[] roles)
        {
            var result = await _userManager.AddToRolesAsync(user, roles);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} has been added to {roles}", user.UserName, roles);
            }
            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string oldPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} has changed their password", user.UserName);
            }
            return result;
        }

        public async Task<IdentityResult> CreateAccountAsync(string username, string password, string[] roles)
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
                await CancelAccountCreationAsync(user);
                return result;
            }

            result = await AddRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                await CancelAccountCreationAsync(user);
                return result;
            }
            _logger.LogInformation("User account created with username: {username} and id: {id}", username, id);
            return result;
        }

        private async Task<IdentityResult> CancelAccountCreationAsync(ApplicationUser user)
        {
            _logger.LogError("User account couldn't be created");
            return await DeleteAccountAsync(user);
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

        public List<ApplicationUser> GetUsers()
        {
            return _userManager.Users.ToList();
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<IdentityResult> RemoveRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} has been removed from {role}", user.UserName, role);
            }
            return result;
        }

        public async Task<IdentityResult> SuspendAccountAsync(ApplicationUser user, bool suspended = true)
        {
            user.Suspended = suspended;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} has been suspended", user.UserName);
            }
            return result;
        }

        public async Task<IdentityResult> DeleteAccountAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogWarning("{username} has been deleted", user.UserName);
            }
            return result;
        }

        public async Task<IdentityResult> ChangeUsernameAsync(ApplicationUser user, string username)
        {
            var result = await _userManager.SetUserNameAsync(user, username);
            if (result.Succeeded)
            {
                _logger.LogInformation("{username} is now known as {newusername}", user.UserName, username);
            }
            return result;
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        //Todo: Password reset page, and sending this token to the user. E.g via email or copy-pasted
        public async Task<string> ResetPasswordAsync(ApplicationUser user)
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var generatedPassword = Guid.NewGuid().ToString();
            var result = await _userManager.ResetPasswordAsync(user, resetToken, generatedPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("The password of {username} has been reset", user.UserName);
            }
            return generatedPassword;
        }
    }
}
