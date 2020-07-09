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
    public enum Action
    {
        Insert,
        Update,
        Delete,
        Select
    }

    public class ApplicationContext : IDisposable
    {
        private readonly string connectionString;
        private readonly StringBuilder query;
        private readonly SqlConnection sqlConnection;

        public ApplicationContext(string connectionString)
        {
            this.connectionString = connectionString;
            this.query = new StringBuilder();
            this.sqlConnection = new SqlConnection(this.connectionString);
            this.sqlConnection.Open();
        }

        public void Insert<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var memberProperties = Helper.GetMemberProperties(item);

            if (Helper.IsNullOrEmptyCollection(memberProperties))
                throw new Exception("Класс не содержит атрибутов Member необходимых для создания модели");

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
                    pkProperties.SetValue(item, command.Parameters["primaryKey"].Value as object);
            }
            catch (Exception ex)
            {
                //sqlTransaction.Rollback();
                throw new Exception(ex.Message);
            }

            // получили ключ ток что вставленной записи 
            //var pk = (int)sqlCommand.Parameters["primaryKey"].Value;
            //return pk;

            /**
             * 
            // получаем коллекции объекта
            var modelCollections = Helpers.Helpers.GetModelCollections(item);

            foreach (var collection in modelCollections)
            {
                var collect = collection.GetValue(item) as IEnumerable<object>;

                if (collect != null && collect.Any())
                {
                    tableName = Helpers.Helpers.GetTableName(collect.First());

                    foreach (var obj in collect)
                    {
                        stringBuilder.Clear();
                        modelProperties = Helpers.Helpers.GetModelProperties(obj, insertFilter);

                        stringBuilder.Append("insert into " + tableName + "(");
                        stringBuilder.Append(string.Join(",", modelProperties.Select(x => x.Name)) + ")" + separator);
                        stringBuilder.Append("values (" + string.Join(",", modelProperties.Select(x => "@" + x.Name)) + ")" + separator);

                        sqlCommand.CommandText = stringBuilder.ToString();

                        if (modelProperties != null && modelProperties.Any())
                        {
                            foreach (var prop in modelProperties)
                            {
                                var value = prop.GetValue(obj, null);
                                var isFK = prop.IsDefined(typeof(FKAttribute));

                                if (value != null)
                                    sqlCommand.Parameters.AddWithValue(prop.Name, isFK ? pk : value);
                                else
                                    sqlCommand.Parameters.AddWithValue(prop.Name, DBNull.Value);
                            }
                        }

                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.Parameters.Clear();
                    }
                }
            }
            */
        }

        public void Update<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var modelProperties = Helper.GetMemberProperties(item);
            var pkProperties = Helper.GetPrimaryKeyProperties(item);

            if (Helper.IsNullOrEmptyCollection(modelProperties) || pkProperties == null)
                throw new Exception("Класс не содержит атрибутов Member/PK необходимых для обновленяи модели");

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
                //sqlTransaction.Rollback();
                throw new Exception(ex.Message);
            }
        }

        public void Delete<T>(T item)
        {
            var tableName = Helper.GetTableName(item);
            var pkProperties = Helper.GetPrimaryKeyProperties(item);
      
            if (pkProperties == null)
                throw new Exception("Класс не содержит атрибутов PK необходимых для удаления модели!");

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
                //sqlTransaction.Rollback();
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

            SqlCommand cmd = GenerateSqlCommand(type, query.ToString(), Action.Select);
            SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd };
            DataSet ds = new DataSet();
            da.Fill(ds);

            if(ds.Tables[0].Rows.Count > 0)
            {
                var column = ds.Tables[0].Columns;
                var rows = ds.Tables[0].Rows;

                var collectionObjects = Helper.ConvertToObject<T>(column, rows).FirstOrDefault();

                var collectionProperties0 = Helper.GetOneToOneProperties(type);
                var collectionProperties1 = Helper.GetCollectionOneToManyProperties(type);
                var collectionProperties2 = Helper.GetCollectionManyToManyProperties(type);

                this.GetOneToOne(collectionProperties0, collectionObjects);
                this.GetOneToMany(collectionProperties1, collectionObjects);
                this.GetManyToMany(collectionProperties2, collectionObjects);

                collection.Add(collectionObjects);
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
                var fk = (item.GetCustomAttribute(typeof(OneToOneAttribute)) as OneToOneAttribute).FKName;

                query.Clear();
                query.Append($"select {string.Join(", ", propertiesCollection.Select(x => x.Name))} from {typeCollectionName} ");
                query.Append($"inner join {tableObject} on {typeCollectionName}.{fk} = {tableObject}.{pkObject.Name} ");
                query.Append($"where {fk} = {pkObject.GetValue(obj)}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);

                SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd };
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var column = ds.Tables[0].Columns;
                    var rows = ds.Tables[0].Rows;

                    var magicType = Type.GetType("ORM.Helpers.Helper");
                    var magicMethod = magicType.GetMethod("ConvertToObject").MakeGenericMethod(new Type[] { typeCollection });
                    var result = magicMethod.Invoke(null, new object[] { column, rows });

                    var convert = (result as IEnumerable<object>).ToList().FirstOrDefault();

                    var nameField = (item.GetCustomAttribute(typeof(OneToOneAttribute)) as OneToOneAttribute).Name;
                    obj.GetType().GetProperty(nameField).SetValue(obj, convert);
                }
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
                var propertiesCollection = typeCollection.GetProperties().Where(x => Attribute.IsDefined(x, typeof(MemberAttribute))).ToList(); // typeCollection.GetProperties();
                // Id
                var pkTypeCollection = propertiesCollection.Where(x => Attribute.IsDefined(x, typeof(PKAttribute))).FirstOrDefault().Name;

                var attributes = item.GetCustomAttribute(typeof(ManyToManyAttribute)) as ManyToManyAttribute;
                // UserWorkAddress
                var stagingTable = attributes.StagingTable;
                // UserId
                var fkForThisObject = attributes.FkForThisObject;
                // WorkAddressId
                var fkForAnotherObject = attributes.FkForAnotherObject;
                // куда вставлять 
                var nameCollectionToInsert = attributes.NameCollection;

                query.Clear();
                query.Append($"select {string.Join(", ", propertiesCollection.Select(x => x.Name))} from {typeCollectionName} ");
                query.Append($"left join {stagingTable} on {stagingTable}.{fkForAnotherObject} = {typeCollectionName}.{pkTypeCollection} ");
                query.Append($"where {fkForThisObject} = {pkObject.GetValue(obj)}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);

                SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd };
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var column = ds.Tables[0].Columns;
                    var rows = ds.Tables[0].Rows;

                    var magicType = Type.GetType("ORM.Helpers.Helper");
                    var magicMethod = magicType.GetMethod("ConvertToObject").MakeGenericMethod(new Type[] { typeCollection });
                    var result = magicMethod.Invoke(null, new object[] { column, rows });

                    obj.GetType().GetProperty(nameCollectionToInsert).SetValue(obj, result);
                }
            }
        }

        private void GetOneToMany<T>(IList<PropertyInfo> collectionProperties, T obj) where T : class, new()
        {
            foreach (var item in collectionProperties)
            {
                var typeCollection = item.PropertyType.GetGenericArguments().First();
                var collectTable = (item.GetCustomAttribute(typeof(OneToManyAttribute)) as OneToManyAttribute).Name;
                var pkProperties = Helper.GetPrimaryKeyProperties(obj);
                var fkProperties = typeCollection.GetProperties().Where(x => Attribute.IsDefined(x, typeof(FKAttribute))).ToList();

                query.Clear();
                query.Append($"select * from {collectTable} ");
                query.Append($"where {fkProperties.First().Name} = {pkProperties.GetValue(obj)}");
                //query.Append($"{this.separator}where{this.separator}{string.Join(" and ", fkProperties.Select(x => x.Name + " = @" + x.Name))}");

                var cmd = GenerateSqlCommand(obj, query.ToString(), Action.Select);
                SqlDataAdapter da = new SqlDataAdapter { SelectCommand = cmd };
                DataSet ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var column = ds.Tables[0].Columns;
                    var rows = ds.Tables[0].Rows;

                    var magicType = Type.GetType("ORM.Helpers.Helper");
                    var magicMethod = magicType.GetMethod("ConvertToObject").MakeGenericMethod(new Type[] { typeCollection });
                    var result = magicMethod.Invoke(null, new object[] { column, rows });

                    obj.GetType().GetProperty(collectTable).SetValue(obj, result);
                }
            }
        }

        private SqlCommand GenerateSqlCommand<T>(T item, string query, Action action, params IEnumerable<PropertyInfo>[] properties)
        {
            var command = new SqlCommand(query, this.sqlConnection);

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
            {
                command.Parameters.Add("primaryKey", SqlDbType.Int).Direction = ParameterDirection.Output;
            }

            return command;
        }

        public void Dispose()
        {
            sqlConnection.Close();
        }
    }
}