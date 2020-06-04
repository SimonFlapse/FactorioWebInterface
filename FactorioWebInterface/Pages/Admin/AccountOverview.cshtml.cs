using FactorioWebInterface.Data;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountModel> _logger;

        public AccountOverviewModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountModel> logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public string UserName { get; set; } = default!;
        public bool HasPassword { get; set; }
        public bool PasswordUpdated { get; set; }

        public string[] AccountRoles { get; set; } = { Constants.AdminRole, Constants.RootRole };

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string UserName { get; set; } = default!;

            [Display(Name = "Select role")]
            public List<string> Roles { get; set; } = new List<string>{Constants.AdminRole};
        }

        public async Task<IActionResult> OnGetAsync(bool passwordUpdated)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            UserName = user.UserName;

            HasPassword = await _userManager.HasPasswordAsync(user);
            PasswordUpdated = passwordUpdated;

            return Page();
        }

        public async Task<IActionResult> OnPostCreatePasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            HasPassword = await _userManager.HasPasswordAsync(user);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (HasPassword)
            {
                return Page();
            }

            /*var result = await _userManager.AddPasswordAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }*/

            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation($"User {user.UserName} created password");

            return RedirectToPage(new { PasswordUpdated = true });
        }

        public async Task<IActionResult> OnPostAsync(string userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "accountoverview");
                return RedirectToPage("signIn");
            }

            var managingUser = await _userManager.FindByIdAsync(userId);

            UserName = managingUser.UserName;
            await PopulateInputModel(managingUser);
            return Page();
        }

        private async Task PopulateInputModel(ApplicationUser user)
        {
            Input.UserName = user.UserName;
            var test = await _userManager.GetRolesAsync(user);
            Input.Roles = test.ToList();
        }

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            HasPassword = await _userManager.HasPasswordAsync(user);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!HasPassword)
            {
                return Page();
            }

            /*var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }*/

            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation($"User {user.UserName} changed password");

            return RedirectToPage(new { PasswordUpdated = true });
        }
    }
}