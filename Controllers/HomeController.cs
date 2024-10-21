using IdmhProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IdmhProject.Controllers
{
    public class HomeController : Controller
    {
        

        public IActionResult Index()
        {
            return View();
        }
         public IActionResult Architecture()
        {
            return View();
        }
        public IActionResult InteriorArchitecture()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
