using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedId.Entity
{
    [Table(Name = "RedisLockdTest")]
    public class RedisLockdTest
    {
        public int Id { get; set; }
        public int Num { get; set; }

    }
}
