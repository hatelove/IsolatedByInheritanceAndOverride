using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;

namespace IsolatedByInheritanceAndOverride
{
    public class OrderService
    {
        private string _filePath = @"C:\temp\joey.csv";

        public void SyncBookOrders()
        {
            var orders = this.GetOrders();

            // only get orders of book
            var ordersOfBook = orders.Where(x => x.Type == "Book");

            // extract method to get BookDao
            var bookDao = this.GetBookDao();
            foreach (var order in ordersOfBook)
            {
                bookDao.Insert(order);
            }
        }

        internal virtual IBookDao GetBookDao()
        {
            return new BookDao();
        }

        protected virtual List<Order> GetOrders()
        {
            // parse csv file to get orders
            var result = new List<Order>();

            // directly depend on File I/O
            using (StreamReader sr = new StreamReader(this._filePath, Encoding.UTF8))
            {
                int rowCount = 0;

                while (sr.Peek() > -1)
                {
                    rowCount++;

                    var content = sr.ReadLine();

                    // Skip CSV header line
                    if (rowCount > 1)
                    {
                        string[] line = content.Trim().Split(',');

                        result.Add(this.Mapping(line));
                    }
                }
            }

            return result;
        }

        private Order Mapping(string[] line)
        {
            var result = new Order
            {
                ProductName = line[0],
                Type = line[1],
                Price = Convert.ToInt32(line[2]),
                CustomerName = line[3]
            };

            return result;
        }
    }

    public class Order
    {
        public string Type { get; set; }

        public int Price { get; set; }

        public string ProductName { get; set; }

        public string CustomerName { get; set; }
    }

    internal class BookDao : IBookDao
    {
        public void Insert(Order order)
        {
            // directly depend on some web service
            var client = new HttpClient();
            client.PostAsync<Order>("http://api.joey.io/Order", order, new JsonMediaTypeFormatter());
        }
    }

    internal interface IBookDao
    {
        void Insert(Order order);
    }
}