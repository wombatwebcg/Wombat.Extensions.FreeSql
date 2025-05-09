using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Wombat.Extensions.FreeSql.Demo
{
    [Table(Name ="Class1")]
    public class Class1
    {

        [Column(IsPrimary = true)]
        public long Id { get; set; }



    }
}
