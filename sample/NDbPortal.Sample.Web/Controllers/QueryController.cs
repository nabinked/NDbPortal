using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NDbPortal.Sample.Web.Controllers
{
    public class QueryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}