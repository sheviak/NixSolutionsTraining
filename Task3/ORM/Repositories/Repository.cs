using ORM.Context;
using ORM.Interfaces;
using System;
using System.Collections.Generic;

namespace ORM.Repositories
{
    public class Repository<T> : IDisposable, IRepository<T> where T : class, new()
    {
        private ApplicationContext context;

        public Repository(string connectionString)
        {
            this.context = new ApplicationContext(connectionString);
        }

        public void Delete(T item)
        {
            this.context.Delete(item);
        }

        public void Insert(T item)
        {
            this.context.Insert(item);
        }

        public IEnumerable<T> Select()
        {
            return this.context.Select<T>();
        }

        public void Update(T item)
        {
            this.context.Update(item);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Commit()
        {
            this.context.Commit();
        }
    }
}