using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using ORM.Attributes;
using ORM.Helpers;

namespace ORM.Context
{
    enum Action
    {
        Insert,
        Update,
        Delete,
        Select
    }

    internal class ApplicationContext : IDisposable
    {
        private readonly string connectionString;
        private readonly StringBuilder query;
        private readonly SqlConnection sqlConnection;
        private SqlTransaction sqlTransaction;

        internal ApplicationContext(string connectionString)
        {
            this.connectionString = connectionString;
            this.query = new StringBuilder();
            this.sqlConnection = new SqlConnection(this.connectionString);
            this.sqlConnection.Open();
            this.sqlTransaction = this.sqlConnection.BeginTransaction();
        }

        public void Insert<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var memberProperties = Helper.GetMemberProperties(item);

            if (Helper.IsNullOrEmptyCollection(memberProperties))
                throw new Exception("The class does not contain Member attributes needed to create the model!");

            query.Clear();
            query.Append($"insert into {tableName} (");
            query.Append($"{string.Join(",", memberProperties.Select(x => x.Name))} ) ");
            query.Append($"values ({string.Join(",", memberProperties.Select(x => "@" + x.Name))}) ");
            query.Append("select @primaryKey = @@identity");

            var command = GenerateSqlCommand(item, this.query.ToString(), Action.Insert, memberProperties);
            
            try
            {
                command.ExecuteNonQuery();

                var pkProperties = Helper.GetPrimaryKeyProperties(item);
                if (pkProperties != null)
                    pkProperties.SetValue(item, command.Parameters["primaryKey"].Value);
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                throw new Exception(ex.Message);
            }
        }

        public void Update<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var modelProperties = Helper.GetMemberProperties(item);
            var pkProperties = Helper.GetPrimaryKeyProperties(item);

            if (Helper.IsNullOrEmptyCollection(modelProperties) || pkProperties == null)
                throw new Exception("The class does not contain Member and PK attributes necessary for updating the model!");

            query.Clear();
            query.Append($"update {tableName} set ");
            query.Append(string.Join(", ", modelProperties.Select(x => x.Name + " = @" + x.Name)));
            query.Append($" where {pkProperties.Name} = {pkProperties.GetValue(item)}");

            var command = GenerateSqlCommand(item, query.ToString(), Action.Update, modelProperties);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                throw new Exception(ex.Message);
            }
        }

        public void Delete<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var pkProperties = Helper.GetPrimaryKeyProperties(item);
      
            if (pkProperties == null)
                throw new Exception("The class does not contain the PK attributes necessary to delete the model!");

            this.query.Clear();
            this.query.Append($"delete from {tableName} ");
            this.query.Append($"where {pkProperties.Name} = {pkProperties.GetValue(item)}");

            var command = GenerateSqlCommand(item, query.ToString(), Action.Delete);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                throw new Exception(ex.Message);
            }
        }

        public List<T> Select<T>() where T : class, new()
        {
            var collection = new List<T>();
            var type = typeof(T);
            var table =  Helper.GetTableName(type);

            query.Clear();
            query.Append($"select * from {table}");

            var cmd = GenerateSqlCommand(type, query.ToString(), Action.Select);
            var da = new SqlDataAdapter { SelectCommand = cmd };
            var ds = new DataSet();
            da.Fill(ds);

            if(ds.Tables[0].Rows.Count > 0)
            {
                var column = ds.Tables[0].Columns;
                var rows = ds.Tables[0].Rows;

                var collectionObjects = Helper.ConvertToObject<T>(column, rows);

                foreach (var item in collectionObjects)
                {
                    var collectionPropertiesOneToOne = Helper.GetPropertiesByRelations<OneToOne>(type);
                    var collectionPropertiesOneToMany = Helper.GetPropertiesByRelations<OneToMany>(type);
                    var collectionPropertiesManyToMany = Helper.GetPropertiesByRelations<ManyToMany>(type);

                    this.GetOneToOne(collectionPropertiesOneToOne, item);
                    this.GetOneToMany(collectionPropertiesOneToMany, item);
                    this.GetManyToMany(collectionPropertiesManyToMany, item);

                    collection.Add(item);
                }
            }

            return collection;
        }

        private void GetOneToOne<T>(IList<PropertyInfo> collectionProperties, T obj) where T : class, new()
        {
            foreach (var item in collectionProperties)
            {
                // User -> Id
                var pkObject = Helper.GetPrimaryKeyProperties(obj);
                var tableObject = Helper.GetTableName(obj);
                // UserInfo
                var typeCollection = item.PropertyType;
                // UserInfo
                var typeCollectionName = typeCollection.Name;
                // properties: UserId About
                var propertiesCollection = typeCollection.GetProperties();
                // UserId
                var fk = (item.GetCustomAttribute(typeof(OneToOne)) as OneToOne).FKName;
                var nameFieldToInsert = (item.GetCustomAttribute(typeof(OneToOne)) as OneToOne).Name;

                query.Clear();
                query.Append($"select {string.Join(", ", propertiesCollection.Select(x => x.Name))} from {typeCollectionName} ");
                query.Append($"inner join {tableObject} on {typeCollectionName}.{fk} = {tableObject}.{pkObject.Name} ");
                query.Append($"where {fk} = {pkObject.GetValue(obj)}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);
                this.AddDataToObject(cmd, typeCollection, nameFieldToInsert, obj, false);
            }
        }

        private void GetManyToMany<T>(IList<PropertyInfo> collectionProperties, T obj) where T : class, new()
        {
            foreach (var item in collectionProperties)
            {
                // Id
                var pkObject = Helper.GetPrimaryKeyProperties(obj);
                // тип коллекции: WorkAddress || User
                var typeCollection = item.PropertyType.GetGenericArguments().FirstOrDefault();
                // DB: WorkAddress || Users
                var typeCollectionName = (typeCollection.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute).Name;
                // WorkAddress: (Id Address) || User (Id Name)
                var propertiesCollection = typeCollection.GetProperties().Where(x => Attribute.IsDefined(x, typeof(MemberAttribute))).ToList();
                // Id
                var pkTypeCollection = propertiesCollection.Where(x => Attribute.IsDefined(x, typeof(PKAttribute))).FirstOrDefault().Name;
                // get all Attributes by "item"
                var attributes = item.GetCustomAttribute(typeof(ManyToMany)) as ManyToMany;
                // UserWorkAddress
                var stagingTable = attributes.StagingTable;
                // UserId
                var fkForThisObject = attributes.FkForThisObject;
                // WorkAddressId
                var fkForAnotherObject = attributes.FkForAnotherObject;
                // куда вставлять 
                var nameFieldCollectionToInsert = attributes.NameCollection;

                query.Clear();
                query.Append($"select {string.Join(", ", propertiesCollection.Select(x => x.Name))} from {typeCollectionName} ");
                query.Append($"left join {stagingTable} on {stagingTable}.{fkForAnotherObject} = {typeCollectionName}.{pkTypeCollection} ");
                query.Append($"where {fkForThisObject} = {pkObject.GetValue(obj)}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);
                this.AddDataToObject(cmd, typeCollection, nameFieldCollectionToInsert, obj);
            }
        }

        private void GetOneToMany<T>(IList<PropertyInfo> collectionProperties, T obj) where T : class, new()
        {
            foreach (var item in collectionProperties)
            {
                var typeCollection = item.PropertyType.GetGenericArguments().First();
                var nameFieldCollectionToInsert = (item.GetCustomAttribute(typeof(OneToMany)) as OneToMany).Name;
                var pkProperties = Helper.GetPrimaryKeyProperties(obj);
                var fkProperties = typeCollection.GetProperties().Where(x => Attribute.IsDefined(x, typeof(FKAttribute))).ToList();

                query.Clear();
                query.Append($"select * from {nameFieldCollectionToInsert} ");
                query.Append($"where {fkProperties.First().Name} = {pkProperties.GetValue(obj)}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);
                this.AddDataToObject(cmd, typeCollection, nameFieldCollectionToInsert, obj);
            }
        }

        private void AddDataToObject<T>(SqlCommand cmd, Type typeCollection, string nameCollectionToInsert, T obj, bool isCollection = true)
        {
            var da = new SqlDataAdapter { SelectCommand = cmd };
            var ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) 
                return;

            var column = ds.Tables[0].Columns;
            var rows = ds.Tables[0].Rows;

            var magicType = Type.GetType("ORM.Helpers.Helper");
            var magicMethod = magicType.GetMethod("ConvertToObject").MakeGenericMethod(new Type[] { typeCollection });
            var result = magicMethod.Invoke(null, new object[] { column, rows });

            if (!isCollection)
                result = (result as IEnumerable<object>).ToList().FirstOrDefault();

            obj.GetType().GetProperty(nameCollectionToInsert).SetValue(obj, result);
        }

        private SqlCommand GenerateSqlCommand<T>(T item, string query, Action action, params IEnumerable<PropertyInfo>[] properties)
        {
            var command = new SqlCommand(query, this.sqlConnection, this.sqlTransaction);

            if (action == Action.Select)
                return command;

            foreach (var collection in properties)
            {
                foreach (var prop in collection)
                {
                    var dbValue = prop.GetValue(item, null);
                    if (dbValue != null)
                        command.Parameters.AddWithValue(prop.Name, dbValue);
                    else
                        command.Parameters.AddWithValue(prop.Name, DBNull.Value);
                }
            }

            if (action == Action.Insert)
                command.Parameters.Add("primaryKey", SqlDbType.Int).Direction = ParameterDirection.Output;

            return command;
        }

        public void Commit()
        {
            this.sqlTransaction.Commit();
        }

        public void Dispose()
        {
            this.sqlTransaction.Dispose();
            this.sqlConnection.Close();
        }
    }
}