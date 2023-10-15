using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductsController _products;
        public HomeController()
        {
            _products = new ProductsController();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            var count = _products.Count().Result.data;
            ViewBag.ProductsCount = count;

            return View();
        }

    }
}
