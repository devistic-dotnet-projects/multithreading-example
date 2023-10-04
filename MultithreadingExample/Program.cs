using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadingExample
{
    class Program
    {
        private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
        private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";
        private static DateTime startTime;
        private static DateTime endTime;

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
                startTime = DateTime.Now;

                /* Fetch Data */
                Console.WriteLine("Processing starts on: " + startTime);
                Console.WriteLine("Fetching Data is in process ...");
                List<DataModel> dataList = await FetchDataFromApi(getApiUrl);
                Console.WriteLine("Fetching Data is completed ...");

                /* Post Data */
                Console.WriteLine("Posting Data is in process ...");
                await PostDataToApiParallel(dataList); // Use parallel processing
                Console.WriteLine("Posting Data is completed ...");

                /* Logs */
                Console.WriteLine("Processing complete.");

                endTime = DateTime.Now;
                Console.WriteLine("Processing ends on: " + endTime);
                Console.WriteLine("Total Time Consumption in minutes:" + (endTime - startTime).TotalMinutes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.Read();
        }

        static async Task PostDataToApiParallel(List<DataModel> dataList)
        {
            // Parallelize the processing using Parallel.ForEach
            Parallel.ForEach(dataList, async (dataItem) =>
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
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

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
            });
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
