using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetEnv;

namespace dotNetProject
{
    class Program
    {
        static async Task Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a category! Like restaurants or hotels");
                Environment.Exit(1);
            }

            string categoryInput = args[0].ToLower();
            Console.WriteLine($"Fetching data for {categoryInput}");
            string? limit = args[1];
            if (limit == null)
            {
                limit = "10";
            }

            string geoapifyCategory = categoryInput switch
            {
                "restaurants" => "catering.restaurant",
                "hotels" => "accomodation.hotel",
                "beaches" => "beach",
                _ => ""
            };

            if (geoapifyCategory == "")
            {
                Console.WriteLine("Unknown category!");
                Environment.Exit(1);
            }

            await FetchPlaces(geoapifyCategory, limit);

        }

        static async Task FetchPlaces(string category, string limit)
        {
            // Load dotenv, and store api key
            Env.Load();
            string? apiKey = Environment.GetEnvironmentVariable("API_KEY");
            if (apiKey == null)
            {
                Console.WriteLine("No api key found! Please add an api key to .env, naming it API_KEY");
                Environment.Exit(1);
            }

            string url = $"https://api.geoapify.com/v2/places?categories={category}&filter=circle:25.1442,35.3387,5000&limit={limit}&apiKey={apiKey}";

            // This line creates the http client. Always reuse clients when possible!
            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage res = await client.GetAsync(url);

                res.EnsureSuccessStatusCode();

                string json = await res.Content.ReadAsStringAsync();
                

                Console.Write("Response from the API:");
                Console.Write($"{json}\n");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Request failed!");
                Console.WriteLine(e.Message);
            }

        }
    }
}