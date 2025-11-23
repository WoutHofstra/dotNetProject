using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DotNetEnv;
using System.Text.Json;
using System.Runtime.Versioning;

namespace dotNetProject
{
    class Program
    {
        public class GeoapifyFeatureCollection
        {
            public string? Type { get; set; }
            public List<GeoapifyFeature>? Features { get; set; }
        }

        public class GeoapifyFeature
        {
            public string? Type { get; set; }
            public GeoapifyProperties? Properties { get; set; }
            public GeoapifyGeometry? Geometry { get; set; }
        }

        public class GeoapifyProperties
        {
            public string? Name { get; set; }
            public string? Formatted { get; set; }
            public List<string>? Categories { get; set; }
        }

        public class GeoapifyGeometry
        {
            public string? Type { get; set; }
            public List<double>? Coordinates { get; set; }
        }

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
                "hotels" => "accommodation.hotel",
                "beaches" => "beach",
                _ => ""
            };

            if (geoapifyCategory == "")
            {
                Console.WriteLine("Unknown category!");
                Environment.Exit(1);
            }

            string json = await FetchPlaces(geoapifyCategory, limit);
            if (json == "")
            {
                Console.WriteLine("Http request failed");
                Environment.Exit(1);
            }

            var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            var collection = JsonSerializer.Deserialize<GeoapifyFeatureCollection>(json, options);
            if (collection == null || collection.Features == null)
            {
                Console.WriteLine("collection empty!");
                Environment.Exit(1);
            }


            foreach (var feature in collection.Features)
            {
                if (feature.Properties.Name == null)
                {
                    continue;
                }
                Console.WriteLine($"Name: {feature.Properties.Name}");
                Console.WriteLine($"Address: {feature.Properties.Formatted}");
                Console.WriteLine($"Categories: {string.Join(", ", feature.Properties.Categories)}");
                Console.WriteLine(new string('-', 40));
            }



        }

        static async Task<string> FetchPlaces(string category, string limit)
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

                return json;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Request failed!");
                Console.WriteLine(e.Message);
            }
            return "";
        }


    }
}