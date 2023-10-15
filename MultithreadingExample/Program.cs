using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using MultithreadingExample.Models;
using MultithreadingExample.Handlers;

namespace MultithreadingExample
{
    class Program
    {
        private static readonly bool IsSingleThread = bool.Parse(ConfigurationManager.AppSettings["IsSingleThread"]);
        private static int degreeOfParallelism = int.Parse(ConfigurationManager.AppSettings["DegreeOfParallelism"]);
        //private static readonly string getApiUrl = "https://localhost:44368/api/products/getall";
        private static readonly string getApiUrl = "https://localhost:44368/api/products/get/1";
        private static readonly string postApiUrl = "https://localhost:44368/api/order/add";
        private static DateTime startTime;
        private static DateTime endTime;
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static int TotalRecorsCount = 0;
        private static int UpdateRecorsCount = 0;

        static async Task<List<Product>> FetchDataFromApi(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(apiUrl);
                JsonResponse<List<Product>> dataList = JsonConvert.DeserializeObject<JsonResponse<List<Product>>>(json);
                return dataList.data;
            }
        }

        static async Task Main(string[] args)
        {
            // Define the excluded messages
            //var excludedMessages = new HashSet<string> { "Record Update:" };

            // Configure Serilog for logging to the console and a log file
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day) // Specify the log file name and rolling interval
                .CreateLogger();

            try
            {

                /* Fetch Data */
                //Console.WriteLine("Fetching Data is in process ...");
                Log.Information("Fetching Data is in process ...");

                List<Product> dataList = await FetchDataFromApi(getApiUrl);
                //Console.WriteLine("Fetching Data is completed ...");
                Log.Information("Fetching Data is completed ...");

                /* Post Data */
                startTime = DateTime.Now;

                //Console.WriteLine("Processing starts with " + degreeOfParallelism + " Degree Of Parallelism on: " + startTime);
                Log.Information($"Processing starts on: {startTime}");
                //Console.WriteLine("Posting Data is in process ...");
                Log.Information("Posting Data is in process ...");

                //if (degreeOfParallelism == 0)
                //{
                //    // Use Environment.ProcessorCount to get the maximum available threads
                //    degreeOfParallelism = Environment.ProcessorCount;
                //}
                //Console.WriteLine("Total No. of Threads are: " + degreeOfParallelism);
                //Log.Information($"Total No. of Processor are: {degreeOfParallelism}");

                //Now consume it only for 10 records
                //dataList = dataList.Take(100).ToList();
                dataList = dataList.ToList();
                TotalRecorsCount = dataList.Count;

                if (IsSingleThread)
                {
                    await PostDataToApi(dataList);
                }
                else
                {
                    await PostDataToApiParallel(dataList);
                }

                //Console.WriteLine("Posting Data is completed ...");
                Log.Information("Posting Data is completed ...");

                /* Logs */
                //Console.WriteLine("Processing complete.");
                Log.Information($"Processing complete for Total Records: {UpdateRecorsCount}/{TotalRecorsCount}.");

                endTime = DateTime.Now;
                //Console.WriteLine("Processing ends on: " + endTime);
                Log.Information("Processing ends on: {EndTime}", endTime);

                //Console.WriteLine("Total Time Consumption in minutes: " + (endTime - startTime).TotalMinutes.ToString("##.##"));
                Log.Information("Total Time Consumption in minutes: {TotalMinutes}", (endTime - startTime).TotalMinutes.ToString("##.##"));
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error: {ex.Message}");
                Log.Error(ex, "Error: {ErrorMessage}", ex.Message);
            }
            finally
            {
                // Close and flush the Serilog logger
                Log.CloseAndFlush();
            }

            Console.Read();
        }

        private static async Task PostDataToApi(List<Product> dataList)
        {
            foreach (var item in dataList)
            {
                using (HttpClient client = new HttpClient())
                {
                    var postData = new Order
                    {
                        // I am using temp email from this link https://mail.tm/en/
                        UserEmail = "trweszcrpo@pretreer.com",
                        Url = "",
                        Content = item.title,
                        Message = "Thank you for shopping!",
                        OrderItem = new OrderItem { ProductId = item.id, UserId = item.id },
                    };

                    string json = JsonConvert.SerializeObject(postData);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(postApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        UpdateRecorsCount++;
                        //Console.WriteLine(responseJson);
                        //Log.Information(responseJson);
                        Log.Information($"Record Update {UpdateRecorsCount}:({responseJson.Replace("\n", "").Trim()});");
                    }
                    else
                    {
                        //Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        Log.Error($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
        }

        static async Task PostDataToApiParallel(List<Product> dataList)
        {
            // Create a list of tasks for posting data
            List<Task> postingTasks = new List<Task>();

            // Create a CancellationToken to allow for cancellation
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            try
            {
                // Parallelize the processing using Parallel.ForEach
                Parallel.ForEach(dataList, (item) =>
                {
                    // Check for cancellation before starting each task
                    cancellationToken.ThrowIfCancellationRequested();

                    // Create a task for each data item
                    Task postingTask = Task.Run(async () =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var postData = new Order
                        {
                            // I am using temp email from this link https://mail.tm/en/
                            UserEmail = "trweszcrpo@pretreer.com",
                            Url = "",
                            Content = item.title,
                            Message = "Thank you for shopping!",
                            OrderItem = new OrderItem { ProductId = item.id, UserId = item.id },
                        };

                        string json = JsonConvert.SerializeObject(postData);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(postApiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseJson = await response.Content.ReadAsStringAsync();
                            JsonResponse<string> result = JsonConvert.DeserializeObject<JsonResponse<string>>(responseJson);
                            if (result.success)
                            {
                                result.data = result.data + " Item ID: " + item.id;
                                UpdateRecorsCount++;
                                //Console.WriteLine(responseJson);
                                //Log.Information(responseJson);
                                Log.Information($"Record Update {UpdateRecorsCount}:({result.data.Replace("\n", "").Trim()});");
                            }
                            else
                            {
                                Log.Error($"Error: {result.message}");
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                            Log.Error($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        }
                    }
                }, cancellationToken);

                    postingTasks.Add(postingTask);
                });

                // Wait for all of the posting tasks to complete
                await Task.WhenAll(postingTasks);
            }
            catch (OperationCanceledException)
            {
                //Console.WriteLine("Processing was canceled.");
                Log.Warning("Processing was canceled.");
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
