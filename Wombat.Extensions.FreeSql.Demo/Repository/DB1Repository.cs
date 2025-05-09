

using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{

    public interface IDB1Repository : IRepositoryBase<Class1, DB1>
    {

    }

    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class DB1Repository : RepositoryBase<Class1, DB1>, IDB1Repository
    {
        public DB1Repository(IServiceProvider service) : base(service)
        {
        }
    }
}
