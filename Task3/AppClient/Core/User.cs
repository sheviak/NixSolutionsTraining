using ORM.Attributes;
using System.Collections.Generic;

namespace AppClient.Core
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Member]
        public string Name { get; set; }

        [OneToOne(fkName: "UserId")]
        public UserInfo UserInfo { get; set; }

        [OneToMany]
        public List<Order> Orders { get; set; }
        
        [ManyToMany(stagingTable: "UserWorkAddress", fkForThisObject: "UserId", fkForAnotherObject: "WorkAddressId", nameCollection: "WorkAddresses")]
        public List<WorkAddress> WorkAddresses { get; set; }
    }   
}