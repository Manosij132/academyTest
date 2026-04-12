using Academy.Core.PredicateBuilder;
using Academy.Shared.DTO;

namespace Academy.Core.Abstraction.Factories
{
    public interface IPredicateFactory
    {
        AbstractAdminPredicate PredicateGenerator(List<Role> roles);
    }
}
