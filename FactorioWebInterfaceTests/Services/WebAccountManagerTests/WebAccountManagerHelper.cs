using FactorioWebInterface;
using FactorioWebInterface.Data;
using FactorioWebInterface.Services;
using FactorioWebInterfaceTests.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactorioWebInterfaceTests.Services.WebAccountManagerTests
{
    class WebAccountManagerHelper
    {
        public static ServiceProvider MakeDefaultAdminAccountServiceProvider()
        {
            var dbContextFactory = new TestDbContextFactory();

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton(typeof(ILogger<>), typeof(TestLogger<>))
                .AddTransient<ApplicationDbContext>(_ => dbContextFactory.Create<ApplicationDbContext>())
                .AddTransient<WebAccountManager>()
                .AddTransient<UserManager<ApplicationUser>>();
            //Startup.SetupIdentity(serviceCollection); //Waiting for #67

            return serviceCollection.BuildServiceProvider();
        }
    }
}
