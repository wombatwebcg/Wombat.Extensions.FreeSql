using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{
    public interface IDB3Repository : IRepositoryKey
    {
    }

    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class DB3Repository : BaseRepository<Class3, IDB3Repository>, IDB3Repository
    {
        public DB3Repository(IServiceProvider service) : base(service)
        {
        }
    }
}
