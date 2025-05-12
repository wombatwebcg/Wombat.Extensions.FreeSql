using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{
    public interface IDB3Repository : IRepositoryKey
    {
    }

    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class DB3Repository : BaseRepository<Class3, DB3>
    {
        public DB3Repository(IServiceProvider service) : base(service)
        {
        }
    }

    public interface IDB4Repository : IRepositoryKey
    {
    }

    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class DB4Repository : BaseRepository<Class4, DB3>
    {
        public DB4Repository(IServiceProvider service) : base(service)
        {
        }
    }
}
