using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using RepoWrapper;

namespace Data
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        public string TableName { get; set; }
     
        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(ConfigurationManager.ConnectionStrings["SmsQuizConnection"].ConnectionString);
            }
        }

       
        internal virtual dynamic Mapping(T item)
        {
            return item;
        }

        public IEnumerable<T> GetModel()
        {
            using (IDbConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                string sql = " Select * from " + this.TableName;

                var obj = connection.Query<T>(sql).ToList();
             
                return obj;
            }
        }
        // Create Connection in New 
        public T Insert(T entity)
        {


         //   using (IDbConnection cn = Connection)
         //   {
         ///       var parameter = (object)Mapping(entity);
         //       cn.Open();
         //   //    item.ID = cn.Insert<Guid>(TableName, parameters);
         //   }

            var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            if (entity == null)
                throw new ArgumentNullException("entity");

            var parameters = (object)Mapping(entity);
            string sql= DynamicQuery.GetInsertQuery(TableName, parameters);

            //  IEnumerable<T> result =SqlMapper.Query<T>(connection, DynamicQuery.GetInsertQuery(TableName, parameters), parameters);

            IEnumerable<T> result = SqlMapper.Query<T>(connection, sql, entity);
            var obj= result.First();
            return obj;

            

        }

        public T GetDataByID(int id)
        {
            using (IDbConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {

                string sql = " Select * from "  + TableName + " Where ID=" + id ;

                var obj = connection.Query<T>(sql).SingleOrDefault();

                return obj;
            }
        }

        public void Update(T entity)
        {
            var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            if (entity == null)
                throw new ArgumentNullException("entity");

            var parameters = (object)Mapping(entity);
            string sql = DynamicQuery.GetUpdateQuery(TableName, parameters);

            //  IEnumerable<T> result =SqlMapper.Query<T>(connection, DynamicQuery.GetInsertQuery(TableName, parameters), parameters);

            IEnumerable<T> result = SqlMapper.Query<T>(connection, sql, entity);
           // var obj = result.First();
           
        }
    }
}
