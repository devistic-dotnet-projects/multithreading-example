using Newtonsoft.Json;
//using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Threading;
using System.Net.Http;

namespace Example2
{
    class Program
    {
        private static readonly string getApiUrl = "https://countrycode.org/api/countryCode/countryMenu";
        private static readonly string postApiUrl = "https://jsonplaceholder.typicode.com/posts";
        private static DateTime startTime;
        private static DateTime endTime;
        private static int TotalRecorsCount = 0;
        private static int UpdateRecorsCount = 0;

        static List<Record> FetchDataFromApi(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = client.GetStringAsync(apiUrl).Result;
                List<Record> dataList = JsonConvert.DeserializeObject<List<Record>>(json);
                return dataList;
            }
        }

        static void Main(string[] args)
        {
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

                List<Record> dataList = FetchDataFromApi(getApiUrl);
                //Console.WriteLine("Fetching Data is completed ...");
                Log.Information("Fetching Data is completed ...");

                int workerThreads;
                int completionPortThreads;
                ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);

                /* Post Data */
                startTime = DateTime.Now;

                //Console.WriteLine("Processing starts with " + degreeOfParallelism + " Degree Of Parallelism on: " + startTime);
                Log.Information($"Processing starts with {workerThreads} threads on: {startTime}");
                //Console.WriteLine("Posting Data is in process ...");
                Log.Information("Posting Data is in process ...");

                //dataList = dataList.Take(100).ToList();
                dataList = dataList.ToList();
                TotalRecorsCount = dataList.Count;

                ProcessRecords(dataList, workerThreads);

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

        public static void ProcessRecords(IEnumerable<Record> records, int numberOfThreads)
        {
            //TaskFactory threadPool = new TaskFactory(TaskCreationOptions.LongRunning, TaskCreationOptions.None, TaskScheduler.Default);
            if (numberOfThreads > records.Count())
            {
                numberOfThreads = records.Count();
            }

            var queues = new ConcurrentQueue<Record>[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                queues[i] = new ConcurrentQueue<Record>();
            }

            int index = 0;
            foreach (var record in records)
            {
                queues[index++ % numberOfThreads].Enqueue(record);
            }

            var tasks = new List<Task>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                ConcurrentQueue<Record> queue = queues[i];
                tasks.Add(Task.Factory.StartNew(() => ProcessRecordsInQueue(queue), TaskCreationOptions.LongRunning));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("Number of Tasks completed: " + tasks.Count);
            }
            catch (AggregateException ex)
            {
                Exception innerException = ex.InnerException;
                Console.WriteLine(ex.Message);
            }
        }

        private static void ProcessRecordsInQueue(ConcurrentQueue<Record> queue)
        {
            while (queue.TryDequeue(out Record record))
            {
                ProcessRecord(record);
            }
        }

        private static void ProcessRecord(Record record)
        {
            // Your ProcessRecord logic here
            using (HttpClient client = new HttpClient())
            {
                var postData = new
                {
                    title = record.name,
                    body = record.path,
                    userId = record.code
                };

                string json = JsonConvert.SerializeObject(postData);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(postApiUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = response.Content.ReadAsStringAsync().Result;
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

   
    class Record
    {
        public string code { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        // Add more properties as needed
    }

}
