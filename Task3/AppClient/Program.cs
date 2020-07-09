using AppClient.Core;
using ORM.Context;
using System;
using System.Collections.Generic;

namespace AppClient
{
    class Program
    {
        private static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Task3;Integrated Security=True";
        private static ApplicationContext appContext;

        static void Main(string[] args)
        {
            appContext = new ApplicationContext(connectionString);

            var obj = new User
            {
                Name = "Юзер",
                Orders = new List<Order>
                {
                    new Order { CreatedAt = DateTime.Now },
                    new Order { CreatedAt = DateTime.Now },
                },
                WorkAddresses = new List<WorkAddress> { }
            };

            var temp = appContext.Select<WorkAddress>();

            Console.Read();
        }
    }
}
