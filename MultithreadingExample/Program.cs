using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultithreadingExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string apiUrl = "https://countrycode.org/api/countryCode/countryMenu";

            try
            {
                List<DataModel> dataList = await FetchDataFromApi(apiUrl);
                await WriteDataToFileAsync(dataList);

                Console.WriteLine("Processing complete.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.Read();
        }

        static async Task<List<DataModel>> FetchDataFromApi(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(apiUrl);
                List<DataModel> dataList = JsonConvert.DeserializeObject<List<DataModel>>(json);
                return dataList;
            }
        }

        static async Task WriteDataToFileAsync(List<DataModel> dataList)
        {
            string execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
#if DEBUG
            // Remove the debug bin folder from the project directory.
            execPath = execPath.Replace("file:\\", "");
            execPath = execPath.Replace("\\bin\\Debug", "");
#endif

            string filePath = Path.Combine(execPath, "output.txt");

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                await writer.WriteLineAsync("-------------------------br-------------------------");

                foreach (var dataItem in dataList)
                {
                    string serializedData = JsonConvert.SerializeObject(dataItem);
                    await writer.WriteLineAsync(serializedData);
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
