using ORM.Attributes;
using System.Collections.Generic;

namespace AppClient.Core
{
    [Table("WorkAddress")]
    public class WorkAddress : BaseEntity
    {
        [Member]
        public string Address { get; set; }

        [ManyToMany(stagingTable: "UserWorkAddress", fkForThisObject: "WorkAddressId", fkForAnotherObject: "UserId", nameCollection: "Users")]
        public List<User> Users { get; set; }
    }
}