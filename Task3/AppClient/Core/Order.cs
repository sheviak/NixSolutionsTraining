using ORM.Attributes;
using System;

namespace AppClient.Core
{
    [Table("Orders")]
    public class Order : BaseEntity
    {
        [FK]
        [Member]
        public int UserId { get; set; }

        [Member]
        public DateTime? CreatedAt { get; set; }
    }
}   