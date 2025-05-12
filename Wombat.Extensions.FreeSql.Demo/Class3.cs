using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Wombat.Extensions.FreeSql.Demo
{
    [Table(Name = "Class3")]
    public class Class3
    {
        [Column(IsPrimary = true)]

        public long Id { get; set; }
    }
}
