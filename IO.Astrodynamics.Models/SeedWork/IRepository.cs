using IO.Astrodynamics.Models.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.SeedWork
{
    public interface IRepository<T> where T : IEntity
    {
        IUnitOfWork UnitOfWork { get; }

        Task<T> AddAsync(T entity);
        Task DeleteAsync(T entity);
        IEnumerable<T> Get<TKey>(Func<T, bool> whereFunct);
        IEnumerable<T> Get<TKey>(Func<T, TKey> orderbyFunct);
        IEnumerable<T> Get<TKey>(Func<T, TKey> orderbyFunct, int page, int pageSize);
        Task<T> GetAsync(int id);
        Task<IEnumerable<T>> GetAsync();
        T Update(T entity);
        Task<T> UpdateAsync(T entity);
    }
}
