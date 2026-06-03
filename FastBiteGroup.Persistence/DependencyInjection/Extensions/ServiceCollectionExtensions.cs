using FastBiteGroup.Domain.Abstractions;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Persistence.DependencyInjection.Options;
using FastBiteGroup.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastBiteGroup.Persistence.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSqlServerPersistence(this IServiceCollection services)
        {
            services.AddDbContextPool<DbContext, ApplicationDbContext>((provider, builder) =>
            {
                //var auditableInterceptor = provider.GetService<UpdateAuditableEntitiesInterceptor>();

                var configuration = provider.GetRequiredService<IConfiguration>();
                var options = provider.GetRequiredService<IOptionsMonitor<SqlServerRetryOptions>>();

                #region ============ SQL-SERVER-STRATEGY-1 ============

                builder
                    .EnableDetailedErrors(true)
                    .EnableSensitiveDataLogging(true)
                    .UseLazyLoadingProxies(true) // If UseLazyLoadingProxies, all of the navigation fields should be VIRTUAL
                    .UseSqlServer(
                        connectionString: configuration.GetConnectionString("DefaultConnection"),
                        sqlServerOptionsAction: optionsBuilder
                            => optionsBuilder.ExecutionStrategy(
                                dependencies => new SqlServerRetryingExecutionStrategy(
                                    dependencies: dependencies,
                                    maxRetryCount: options.CurrentValue.MaxRetryCount,
                                    maxRetryDelay: options.CurrentValue.MaxRetryDelay,
                                    errorNumbersToAdd: options.CurrentValue.ErrorNumbersToAdd))
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name))
                    .AddInterceptors();

                #endregion ============ SQL-SERVER-STRATEGY-1 ============
            });

            //services.AddIdentityCore<AppUser>(options =>
            //{
            //    // Default Lockout settings.
            //    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //    // options.Lockout.MaxFailedAccessAttempts = 5;
            //    // options.Lockout.AllowedForNewUsers = true;

            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //    options.Lockout.MaxFailedAccessAttempts = 3;
            //    options.Lockout.AllowedForNewUsers = true;

            //}).AddRoles<AppRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            //var passwordValidatorOptions =
            //    services.BuildServiceProvider().GetRequiredService<IOptionsMonitor<PasswordValidatorOptions>>();

            //services.Configure<IdentityOptions>(options =>
            //{


            //    /*
            //     * Property                                     Description
            //     * RequiredDigit                Requires a number between 0-9 in the password.
            //     * RequiredLength               The minimum length of the password.
            //     * RequiredLowercase            Requires a lowercase character in the password.
            //     * RequiredUppercase            Requires an uppercase character in the password.
            //     * RequiredNonAlphanumeric      Requires a non-alphanumeric character in the password.
            //     * RequiredUniqueChars          (Only applies to ASP.NET Core 2.0 or later.) Requires the number of distinct characters in the
            //    */

            //    // Default Password settings.
            //    options.Password.RequireDigit = true;
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequireNonAlphanumeric = true;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequiredUniqueChars = 1;

            //    options.Password.RequireDigit = passwordValidatorOptions.CurrentValue.RequireDigitLength >= 1 ? true : false;
            //    options.Password.RequireLowercase = passwordValidatorOptions.CurrentValue.RequireLowercaseLength >= 1 ? true : false;
            //    options.Password.RequireNonAlphanumeric = passwordValidatorOptions.CurrentValue.RequireNonLetterOrDigitLength >= 1 ? true : false;
            //    options.Password.RequireUppercase = passwordValidatorOptions.CurrentValue.RequireUppercaseLength >= 1 ? true : false;
            //    options.Password.RequiredLength = passwordValidatorOptions.CurrentValue.RequiredMinLength;
            //    options.Password.RequiredUniqueChars = 1;

            //    // Enabling Email Confirmation
            //    options.User.RequireUniqueEmail = true;
            //    options.SignIn.RequireConfirmedEmail = true;
            //});

        }
        public static void AddRepositoryPersistence(this IServiceCollection services)
        {
            services.AddTransient(typeof(IUnitOfWork), typeof(EFUnitOfWork));
            services.AddTransient(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));

        }
        public static void AddInterceptorPersistence(this IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
        }
        //public static OptionsBuilder<SqlServerRetryOptions> ConfigureSqlServerRetryOptionsPersistence(this IServiceCollection services, IConfigurationSection section)
        //   => services.AddOptions<SqlServerRetryOptions>()
        //        .Bind(section)
        //        .ValidateDataAnnotations()
        //        .ValidateOnStart();

    }
}
