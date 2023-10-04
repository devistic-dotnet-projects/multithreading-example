using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadingExample
{
    class Program
    {
        private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
        private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";

        static async Task<List<DataModel>> FetchDataFromApi(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(apiUrl);
                List<DataModel> dataList = JsonConvert.DeserializeObject<List<DataModel>>(json);
                return dataList;
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                List<DataModel> dataList = await FetchDataFromApi(getApiUrl);
                await PostDateToApi(dataList);

                Console.WriteLine("Processing complete.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.Read();
        }


        static async Task PostDateToApi(List<DataModel> dataList)
        {
            foreach (var dataItem in dataList)
            {
                var postData = new
                {
                    title = dataItem.name,
                    body = dataItem.path,
                    userId = dataItem.code
                };

                string json = JsonConvert.SerializeObject(postData);

                using (HttpClient client = new HttpClient())
                {
                    // Create a new StringContent object and set the "Content-Type" header here
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send the POST request with the content
                    HttpResponseMessage response = await client.PostAsync(postApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseJson);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
        }
    }

    class DataModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        // Add more properties as needed
    }
}
