﻿using FactorioWebInterface.Data;
using FactorioWebInterface.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FactorioWebInterface.Pages.Admin
{
    [Authorize(Roles = Constants.RootRole)]
    public class CreateAccountModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebAccountManager _accountManager;
        private readonly ILogger<CreateAccountModel> _logger;

        public CreateAccountModel(
            IWebAccountManager accountManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<CreateAccountModel> logger
            )
        {
            _accountManager = accountManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public bool AccountCreated { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string[] Roles { get; set; } = { Constants.AdminRole, Constants.RootRole };

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string UserName { get; set; } = default!;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = default!;

            [Display(Name = "Select role")]
            public string Role { get; set; } = Constants.AdminRole;
        }

        public async Task<IActionResult> OnGetAsync(bool accountCreated)
        {
            var user = await _accountManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            AccountCreated = accountCreated;

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAccountAsync()
        {
            var user = await _accountManager.GetUserAsync(User);

            if (user == null || user.Suspended)
            {
                HttpContext.Session.SetString("returnUrl", "account");
                return RedirectToPage("signIn");
            }

            await _accountManager.CreateAccountAsync(Input.UserName, Input.Password, new string[]{Input.Role});

            /*if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }*/

            //await _signInManager.SignInAsync(user, isPersistent: false);

            //_logger.LogInformation($"User {user.UserName} created password");

            return RedirectToPage(new { AccountCreated = true });
        }
    }
}