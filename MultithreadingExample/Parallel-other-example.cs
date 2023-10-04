//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MultithreadingExample
//{
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

//}

//--------------------------

//using Newtonsoft.Json;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MultithreadingExample
//{
//    class Program
//    {
//        private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
//        private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";
//        private static DateTime startTime;
//        private static DateTime endTime;
//        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//        static async Task<List<DataModel>> FetchDataFromApi(string apiUrl)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                string json = await client.GetStringAsync(apiUrl);
//                List<DataModel> dataList = JsonConvert.DeserializeObject<List<DataModel>>(json);
//                return dataList;
//            }
//        }

//        static async Task Main(string[] args)
//        {
//            // Configure Serilog for logging to the console and a log file
//            Log.Logger = new LoggerConfiguration()
//                .WriteTo.Console()
//                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
//                .CreateLogger();

//            try
//            {
//                /* Fetch Data */
//                Log.Information("Fetching Data is in process ...");

//                List<DataModel> dataList = await FetchDataFromApi(getApiUrl);
//                Log.Information("Fetching Data is completed ...");

//                /* Post Data */
//                startTime = DateTime.Now;
//                Log.Information($"Processing starts on: {startTime}");
//                Log.Information("Posting Data is in process ...");

//                // Break the dataList into batches based on the available threads
//                int batchSize = (int)Math.Ceiling((double)dataList.Count / Environment.ProcessorCount);
//                List<List<DataModel>> dataBatches = new List<List<DataModel>>();
//                for (int i = 0; i < dataList.Count; i += batchSize)
//                {
//                    List<DataModel> batch = dataList.GetRange(i, Math.Min(batchSize, dataList.Count - i));
//                    dataBatches.Add(batch);
//                }

//                await PostDataToApiParallel(dataBatches);

//                Log.Information("Posting Data is completed ...");

//                /* Logs */
//                Log.Information("Processing complete.");

//                endTime = DateTime.Now;
//                Log.Information("Processing ends on: {EndTime}", endTime);
//                Log.Information("Total Time Consumption in minutes: {TotalMinutes}", (endTime - startTime).TotalMinutes.ToString("##.##"));
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex, "Error: {ErrorMessage}", ex.Message);
//            }
//            finally
//            {
//                // Close and flush the Serilog logger
//                Log.CloseAndFlush();
//            }

//            Console.Read();
//        }

//        static async Task PostDataToApiParallel(List<List<DataModel>> dataBatches)
//        {
//            // Create a list of tasks for posting data
//            List<Task> postingTasks = new List<Task>();

//            // Create a CancellationToken to allow for cancellation
//            CancellationToken cancellationToken = cancellationTokenSource.Token;

//            try
//            {
//                // Parallelize the processing using Parallel.ForEach
//                Parallel.ForEach(dataBatches, batch =>
//                {
//                    // Check for cancellation before starting each task
//                    cancellationToken.ThrowIfCancellationRequested();

//                    // Create a task for each batch
//                    Task postingTask = Task.Run(async () =>
//                    {
//                        foreach (var dataItem in batch)
//                        {
//                            using (HttpClient client = new HttpClient())
//                            {
//                                var postData = new
//                                {
//                                    title = dataItem.name,
//                                    body = dataItem.path,
//                                    userId = dataItem.code
//                                };

//                                string json = JsonConvert.SerializeObject(postData);

//                                var content = new StringContent(json, Encoding.UTF8, "application/json");

//                                HttpResponseMessage response = await client.PostAsync(postApiUrl, content);

//                                if (response.IsSuccessStatusCode)
//                                {
//                                    string responseJson = await response.Content.ReadAsStringAsync();
//                                    Log.Information("Record Update:(" + responseJson + ");");
//                                }
//                                else
//                                {
//                                    Log.Error($"Error: {response.StatusCode} - {response.ReasonPhrase}");
//                                }
//                            }
//                        }
//                    }, cancellationToken);

//                    postingTasks.Add(postingTask);
//                });

//                // Wait for all of the posting tasks to complete
//                await Task.WhenAll(postingTasks);
//            }
//            catch (OperationCanceledException)
//            {
//                Log.Warning("Processing was canceled.");
//            }
//        }
//    }

//    class DataModel
//    {
//        public string code { get; set; }
//        public string name { get; set; }
//        public string path { get; set; }
//        // Add more properties as needed
//    }
//}

