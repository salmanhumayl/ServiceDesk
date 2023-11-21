using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IRepository<T> where T : class
    {
        T Insert(T entity);

        void Update(T entity);

        IEnumerable<T> GetModel();

        string TableName { get; set; }

        T GetDataByID (int id);

    }
}
