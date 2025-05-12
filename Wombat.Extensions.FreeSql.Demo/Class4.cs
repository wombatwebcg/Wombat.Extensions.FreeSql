using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Wombat.Extensions.FreeSql.Demo
{
    [Table(Name = "Class4")]
    public class Class4
    {
        [Column(IsPrimary = true)]

        public long Id { get; set; }
    }
}
