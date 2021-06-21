using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OEEMetricGenerator
{
    public class Program
    {
        /// <param name="args">
        /// Array must contains next arguments
        ///     -url=http://localhost:51803
        ///     -product=TestProduct
        ///     -station=TestStation
        ///     -breakDuration=00:00:00
        ///     -idealDuration=00:00:50.131
        ///     -totalProducts=140
        ///     -maxTimeout=20
        ///     -maxTimeout=90
        /// </param>
        static void Main(string[] args)
        {
            var (
                url,
                product,
                station,
                breakDuration,
                idealDuration,
                totalProducts,
                minTimeout,
                maxTimeout
            ) = ParseArgs(args);

            var httpHandler = new HttpClientHandler();
            var client = new HttpClient(httpHandler, true)
            {
                BaseAddress = new Uri(url)
            };

            var createdDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 17, 0, 0, DateTimeKind.Utc);

            var rnd = new Random();

            while (true)
            {
                createdDate = createdDate.AddSeconds(rnd.Next(minTimeout, maxTimeout));

                var json = JsonSerializer.Serialize(new
                {
                    ProductName = product,
                    StationName = station,
                    ProductionBreakDuration = breakDuration,
                    ProductionIdealDuration = idealDuration,
                    TotalProducts = totalProducts,
                    CreateTime = createdDate
                });

                var data = new StringContent(json, Encoding.UTF8, "application/json");
                client.PutAsync("api/oee/add", data).Wait();

                Console.WriteLine(createdDate);

                if (createdDate >= endDate)
                    break;
            }
        }

        private static (string, string, string, string, string, int, int, int) ParseArgs(string[] args)
        {
            if (args.Length != 8)
                throw new ArgumentException();

            var url = args[0].Split("=")[1];
            var product = args[1].Split("=")[1];
            var station = args[2].Split("=")[1];
            var breakDuration = args[3].Split("=")[1];
            var idealDuration = args[4].Split("=")[1];
            var totalProducts = int.Parse(args[5].Split("=")[1]);
            var minTimeout = int.Parse(args[6].Split("=")[1]);
            var maxTimeout = int.Parse(args[7].Split("=")[1]);

            return (url, product, station, breakDuration, idealDuration, totalProducts, minTimeout, maxTimeout);
        }
    }
}
