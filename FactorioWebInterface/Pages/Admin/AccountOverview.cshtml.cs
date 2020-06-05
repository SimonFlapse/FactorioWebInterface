using FactorioWebInterface.Data;
using FactorioWebInterface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FactorioWebInterface.Pages.Admin
{
    //[Authorize(Roles = Constants.AdminRole + Constants.RootRole) ]
    public class AccountOverviewModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebAccountManager _accountManager;
        private readonly ILogger<AccountModel> _logger;

        public AccountOverviewModel(
            IWebAccountManager accountManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountModel> logger
            )
        {
            _accountManager = accountManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public ApplicationUser ManagingUser { get; set; }
        public string? GeneratedPassword { get; set; }

        public class AccountRole
        {
            public string Role { get; set; }

            public bool IsSelected { get; set; } = false;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            public ApplicationUser User { get; set; } = default!;

            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string UserName { get; set; } = default!;

            [Display(Name = "Select role")]
            public List<AccountRole> Roles { get; } = GenerateAccountRoleList();

            private static List<AccountRole> GenerateAccountRoleList()
            {
                var adminRole = new AccountRole
                {
                    Role = Constants.AdminRole
                };
                var rootRole = new AccountRole
                {
                    Role = Constants.RootRole
                };
                return new List<AccountRole>() { adminRole, rootRole };
            }
        }

        public async Task<bool> IsInRole(ApplicationUser user, string role)
        {
            return await _accountManager.IsInRoleAsync(user, role);
        }

        public async Task<IActionResult> OnGetAsync(string userId, string generatedPassword)
        {
            var user = await _accountManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            ManagingUser = await _accountManager.FindByIdAsync(userId);

            GeneratedPassword = generatedPassword;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId)
        {
            var user = await _accountManager.GetUserAsync(User);
            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "accountoverview");
                return RedirectToPage("signIn");
            }

            ManagingUser = await _accountManager.FindByIdAsync(userId);

            return Page();
        }

        private void PopulateInputModel(ApplicationUser user)
        {
            Input.User = user;
            Input.UserName = user.UserName;
        }

        public async Task<IActionResult> OnPostUpdateAccountAsync()
        {

            var user = await _accountManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.UserName != Input.User.UserName)
            {
                await _accountManager.ChangeUsernameAsync(Input.User, Input.UserName);
            }
            foreach (var role in Input.Roles)
            {
                var inRole = await _accountManager.IsInRoleAsync(Input.User, role.Role);
                if (role.IsSelected && inRole)
                {
                    break;
                }

                if (role.IsSelected && !inRole)
                {
                    var result =  await _accountManager.AddRoleAsync(Input.User, role.Role);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return Page();
                    }
                    break;
                }

                if (!role.IsSelected && inRole)
                {
                    var result = await _accountManager.RemoveRoleAsync(Input.User, role.Role);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return Page();
                    }
                }
            }

            _logger.LogInformation($"The account {UserName} has been updated");

            return RedirectToPage(new {UserId = Input.User.Id, PasswordReset = false});
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            var user = await _accountManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var password = await _accountManager.ResetPasswordAsync(user);

            _logger.LogInformation($"User {user.UserName} changed password");

            return RedirectToPage(new { Password = true });
        }
    }
}