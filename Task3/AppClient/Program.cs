using AppClient.Core;
using ORM.Repositories;
using System;

namespace AppClient
{
    class Program
    {
        private static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Task3;Integrated Security=True";

        static void Main(string[] args)
        {
            using (var repository = new Repository<User>(connectionString))
            {
                var obj = new User { Name = "Test User" };

                try
                {
                    repository.Insert(obj);
                    repository.Commit();

                    var temp = repository.Select();
                } 
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.Read();
        }
    }
}