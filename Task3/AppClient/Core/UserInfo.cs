using ORM.Attributes;

namespace AppClient.Core
{
    [Table("UserInfo")]
    public class UserInfo
    {
        [PK]
        [FK]
        [Member]
        public int UserId { get; set; }

        [Member]
        public string About { get; set; }
    }
}