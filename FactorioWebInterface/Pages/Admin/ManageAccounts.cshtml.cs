using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FactorioWebInterface.Data;
using FactorioWebInterface.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FactorioWebInterface.Pages.Admin
{
    public class ManageAccountsModel : PageModel
    {

        private readonly IWebAccountManager _accountManager;
        public ManageAccountsModel(IWebAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        public List<ApplicationUser> getUsers()
        {
            return _accountManager.GetUsers();
        }

        public void OnGet()
        {
        }
    }
}
