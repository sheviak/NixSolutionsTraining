using ORM.Attributes;

namespace AppClient.Core
{
    public abstract class BaseEntity
    {
        [PK]
        [Member]
        public int Id { get; set; }
    }
}