using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Wombat.Extensions.FreeSql.Demo
{
    [Table(Name = "Class2")]
    public class Class2
    {

        [Column(IsPrimary = true)]
        public long Id { get; set; }



    }
}
