using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using WebApi.Handlers;
using WebApi.Models;

namespace WebApi.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private readonly string filePath = Path.Combine(HostingEnvironment.MapPath("~/App_Data/Products/products.json"));

        // GET api/products
        [HttpGet]
        [Route("getall")]
        public async Task<JsonResponse<List<Product>>> GetAll()
        {
            List<Product> existingProducts = await GetProducts();

            return new JsonResponse<List<Product>>() { data = existingProducts, success = true, message = "" };
        }

        // GET api/products/5
        [HttpGet]
        [Route("get/{take}")]
        public async Task<JsonResponse<List<Product>>> Get(int take)
        {
            List<Product> existingProducts = await GetProducts();

            if (existingProducts.Count > take)
            {
                existingProducts = existingProducts.Take(take).ToList(); // Use ToList() to create a new list
            }

            return new JsonResponse<List<Product>>() { data = existingProducts, success = true, message = "" };
        }

        // GET api/products/count
        [Route("count")]
        [HttpGet]
        public async Task<JsonResponse<int>> Count()
        {
            List<Product> existingProducts = await GetProducts();
            return new JsonResponse<int>() { data = existingProducts.Count, success = true, message = "" };
        }

        // GET api/products/add/1
        [HttpGet]
        [Route("add/{qty}")]
        public async Task<IHttpActionResult> Add(int qty = 20)
        {
            try
            {
                List<Product> existingProducts = await GetProducts();
                int uniqueId = 0;
                if (existingProducts.Count > 0)
                {
                    uniqueId = existingProducts.Max(p => p.id) + 1;
                }

                while (qty > 0)
                {
                    var response = await GetFakeStoreApiData();

                    foreach (var product in response)
                    {
                        product.id = uniqueId+1;
                        product.title = product.title +" SKU" + "00"+product.id;

                        existingProducts.Add(product);
                        qty--;

                        if (qty <= 0)
                            break;
                    }
                }

                SaveProductsToFile(existingProducts);
                return Ok("Data retrieved and added successfully.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/products/delete/1
        [HttpGet]
        [Route("delete/{qty}")]
        public async Task<IHttpActionResult> Delete(int qty)
        {
            //List<Product> existingProducts = (List<Product>)GetProducts();
            //var productsToDelete = existingProducts.Take(qty)
            //    .ToList();
            //existingProducts = existingProducts.Skip(qty).ToList();
            //SaveProductsToFile(existingProducts);
            //return Ok(productsToDelete);
            List<Product> existingProducts = await GetProducts();

            if (qty >= existingProducts.Count)
            {
                existingProducts.Clear();
            }
            else
            {
                existingProducts.RemoveRange(0, qty);
            }

            SaveProductsToFile(existingProducts);
            return Ok("Products deleted successfully.");
        }

        /* Private Methods */

        private async Task<List<Product>> GetFakeStoreApiData()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"https://fakestoreapi.com/products");
                return JsonConvert.DeserializeObject<List<Product>>(response);
            }
        }

        private void SaveProductsToFile(List<Product> products)
        {
            var json = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private Task<List<Product>> GetProducts()
        {
            List<Product> existingProducts;
            if (!File.Exists(filePath))
            {
                // Get the directory path from the file path
                var directoryPath = Path.GetDirectoryName(filePath);

                // Check if the directory doesn't exist, then create it
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Create the file
                using (File.Create(filePath))
                {
                    // The file is created and immediately closed
                }
            }
            var jsonData = File.ReadAllText(filePath);
            existingProducts = JsonConvert.DeserializeObject<List<Product>>(jsonData);

            if (existingProducts == null)
                existingProducts = new List<Product>();

            return Task.FromResult(existingProducts);
        }

    }
}
