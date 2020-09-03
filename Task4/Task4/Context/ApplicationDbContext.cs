using Task4.Models;
using ORM.Repositories;

namespace Task4.Context
{
    public class ApplicationDbContext : Repository<CustomUser>
    {
        public ApplicationDbContext(string options) : base(options)
        {
        }
    }
}