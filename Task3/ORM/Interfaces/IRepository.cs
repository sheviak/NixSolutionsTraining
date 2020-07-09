using System.Collections.Generic;

namespace ORM.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Insert(T item);
        void Update(T item);
        void Delete(T item);
        IEnumerable<T> Select();
        void Commit();
    }
}