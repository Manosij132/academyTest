using Academy.Core.Abstraction.Factories;
using Academy.Core.PredicateBuilder;
using Academy.Shared.DTO;
using Academy.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Academy.Core.Factories
{
    public class PredicateFactory : IPredicateFactory
    {
        private readonly Lazy<Dictionary<string, Lazy<AbstractAdminPredicate>>> _predicate;
        private readonly IServiceProvider _serviceProvider;

        public PredicateFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _predicate = new(() =>
            new()
            {
                { nameof(Roles.SystemAdmin), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<SystemAdminPredicate>()) },
                { nameof(Roles.CommunityAdmin), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<CommunityAdminPredicate>()) },
                { nameof(Roles.EcosystemAdmin), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<EcosystemAdminPredicate>()) },
                { nameof(Roles.TdcAdmin), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<TdcAdminPredicate>()) },
                { nameof(Roles.AccountAdmin), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<AccountAdminPredicate>()) },
                { nameof(Roles.User), new Lazy<AbstractAdminPredicate>(() => _serviceProvider.GetRequiredService<UserPredicate>()) }
            });
        }

        public AbstractAdminPredicate PredicateGenerator(List<Role> roles)
        {
            Role role = roles.FirstOrDefault();
            if (role == null)
            {
                role = new Role() { RoleId = (int)Roles.User, RoleName = Roles.User.ToString() };
            }

            if (_predicate.Value.TryGetValue(role.RoleName, out var generator))
            {
                return generator.Value;
            }

            _predicate.Value.TryGetValue(nameof(Roles.User), out var userRoleGenerator);
            return userRoleGenerator.Value;
        }
    }
}
