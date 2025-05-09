

using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;


namespace Wombat.Extensions.FreeSql.Demo
{
    public interface IDB2Repository : IRepositoryBase<Class2, DB2>
    {

    }

    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class DB2Repository : RepositoryBase<Class2, DB2>, IDB2Repository
    {
        public DB2Repository(IServiceProvider service) : base(service)
        {
        }
    }
}
