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

namespace MultithreadingExample
{
    class Program
    {
        private static readonly bool IsSingleThread = bool.Parse(ConfigurationManager.AppSettings["IsSingleThread"]);
        private static int degreeOfParallelism = int.Parse(ConfigurationManager.AppSettings["DegreeOfParallelism"]);
        private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
        private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";
        private static DateTime startTime;
        private static DateTime endTime;
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static int TotalRecorsCount = 0;
        private static int UpdateRecorsCount = 0;

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

                List<DataModel> dataList = await FetchDataFromApi(getApiUrl);
                //Console.WriteLine("Fetching Data is completed ...");
                Log.Information("Fetching Data is completed ...");

                /* Post Data */
                startTime = DateTime.Now;

                //Console.WriteLine("Processing starts with " + degreeOfParallelism + " Degree Of Parallelism on: " + startTime);
                Log.Information($"Processing starts with {degreeOfParallelism} Degree Of Parallelism on: {startTime}");
                //Console.WriteLine("Posting Data is in process ...");
                Log.Information("Posting Data is in process ...");

                if (degreeOfParallelism == 0)
                {
                    // Use Environment.ProcessorCount to get the maximum available threads
                    degreeOfParallelism = Environment.ProcessorCount;
                }
                //Console.WriteLine("Total No. of Threads are: " + degreeOfParallelism);
                Log.Information($"Total No. of Threads are: {degreeOfParallelism}");

                //Now consume it only for 10 records
                dataList = dataList.Take(100).ToList();
                TotalRecorsCount = dataList.Count;

                if (IsSingleThread)
                {
                    await PostDataToApi(dataList);
                }
                else
                {
                    await PostDataToApiParallel(dataList, degreeOfParallelism);
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

        private static async Task PostDataToApi(List<DataModel> dataList)
        {
            foreach (var item in dataList)
            {
                using (HttpClient client = new HttpClient())
                {
                    var postData = new
                    {
                        title = item.name,
                        body = item.path,
                        userId = item.code
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

        static async Task PostDataToApiParallel(List<DataModel> dataList, int degreeOfParallelism)
        {
            // Create a list of tasks for posting data
            List<Task> postingTasks = new List<Task>();

            // Create a CancellationToken to allow for cancellation
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            try
            {
                // Parallelize the processing using Parallel.ForEach
                Parallel.ForEach(dataList, new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism }, (dataItem) =>
                {
                    // Check for cancellation before starting each task
                    cancellationToken.ThrowIfCancellationRequested();

                    // Create a task for each data item
                    Task postingTask = Task.Run(async () =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var postData = new
                        {
                            title = dataItem.name,
                            body = dataItem.path,
                            userId = dataItem.code
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
