using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultithreadingExample
{
    //class Program
    //{
    //    private static int degreeOfParallelism = int.Parse(ConfigurationManager.AppSettings["DegreeOfParallelism"]);
    //    private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
    //    private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";
    //    private static DateTime startTime;
    //    private static DateTime endTime;
    //    private static CountdownEvent countdownEvent;

    //    static async Task<List<DataModel>> FetchDataFromApi(string apiUrl)
    //    {
    //        using (HttpClient client = new HttpClient())
    //        {
    //            string json = await client.GetStringAsync(apiUrl);
    //            List<DataModel> dataList = JsonConvert.DeserializeObject<List<DataModel>>(json);
    //            return dataList;
    //        }
    //    }

    //    static async Task Main(string[] args)
    //    {
    //        try
    //        {
    //            startTime = DateTime.Now;

    //            /* Fetch Data */
    //            Console.WriteLine("Processing starts on: " + startTime);
    //            Console.WriteLine("Fetching Data is in process ...");
    //            List<DataModel> dataList = await FetchDataFromApi(getApiUrl);
    //            Console.WriteLine("Fetching Data is completed ...");

    //            /* Post Data */
    //            Console.WriteLine("Posting Data is in process ...");

    //            if (degreeOfParallelism == 0)
    //            {
    //                // Use Environment.ProcessorCount to get the maximum available threads
    //                degreeOfParallelism = Environment.ProcessorCount;
    //            }

    //            // Now consume only 20 records
    //            dataList = dataList.Take(20).ToList();

    //            countdownEvent = new CountdownEvent(dataList.Count);

    //            // Create and start threads for parallel processing
    //            List<Thread> threads = new List<Thread>();
    //            foreach (var dataItem in dataList)
    //            {
    //                Thread thread = new Thread(() => PostDataToApi(dataItem));
    //                thread.Start();
    //                threads.Add(thread);
    //            }

    //            // Wait for all threads to complete
    //            foreach (var thread in threads)
    //            {
    //                thread.Join();
    //            }

    //            countdownEvent.Wait(); // Wait for all threads to signal completion

    //            Console.WriteLine("Posting Data is completed ...");

    //            /* Logs */
    //            Console.WriteLine("Processing complete.");

    //            endTime = DateTime.Now;
    //            Console.WriteLine("Processing ends on: " + endTime);
    //            Console.WriteLine("Total Time Consumption in minutes: " + (endTime - startTime).TotalMinutes.ToString("##.##"));
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error: {ex.Message}");
    //        }

    //        Console.Read();
    //    }

    //    static void PostDataToApi(DataModel dataItem)
    //    {
    //        try
    //        {
    //            using (HttpClient client = new HttpClient())
    //            {
    //                var postData = new
    //                {
    //                    title = dataItem.name,
    //                    body = dataItem.path,
    //                    userId = dataItem.code
    //                };

    //                string json = JsonConvert.SerializeObject(postData);

    //                var content = new StringContent(json, Encoding.UTF8, "application/json");

    //                HttpResponseMessage response = client.PostAsync(postApiUrl, content).Result;

    //                if (response.IsSuccessStatusCode)
    //                {
    //                    string responseJson = response.Content.ReadAsStringAsync().Result;
    //                    Console.WriteLine(responseJson);
    //                }
    //                else
    //                {
    //                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
    //                }
    //            }
    //        }
    //        finally
    //        {
    //            countdownEvent.Signal(); // Signal thread completion
    //        }
    //    }
    //}

    //class DataModel
    //{
    //    public string code { get; set; }
    //    public string name { get; set; }
    //    public string path { get; set; }
    //    // Add more properties as needed
    //}

}
