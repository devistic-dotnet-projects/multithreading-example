using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.Controllers
{
    public class ProductsController : ApiController
    {
        // GET api/products
        public IEnumerable<string> Get()
        {
            return new string[] { "products1", "products2" };
        }

        // GET api/products/5
        public string Get(int id)
        {
            return "products";
        }

        // POST api/products
        public void Post([FromBody] string products)
        {
        }

        // PUT api/products/5
        public void Put(int id, [FromBody] string products)
        {
        }

        // DELETE api/products/5
        public void Delete(int id)
        {
        }
    }
}
