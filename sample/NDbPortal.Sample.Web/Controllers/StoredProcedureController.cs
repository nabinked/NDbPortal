using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NDbPortal.Query;
using NDbPortal.Sample.Web.LtvDev;
using NDbPortal.Sample.Web.LtvDev.Views;

namespace NDbPortal.Sample.Web.Controllers
{
    public class StoredProcedureController : Controller
    {
        private readonly IStoredProcedure _storedProcedure;

        public StoredProcedureController(IStoredProcedure storedProcedure)
        {
            _storedProcedure = storedProcedure;
        }

        public IActionResult Get(int id, int culture)
        {
            var model = _storedProcedure.Get<PublishedNewsArticlesView, Object>("get_top_story", new { culture = culture, newsCatId = id });
            return View("Index", model);
        }

        public IActionResult GetList(int id, int culture)
        {
            var model = _storedProcedure.GetList<PublishedNewsArticlesView, Object>("get_top_stories", new { culture = culture, newsCatId = id });
            return View("Index", model);
        }


        public IActionResult GetPagedList(int statusId, int culture, long page)
        {
            var model = _storedProcedure.GetPagedList<NewsArticlesView, Object>("get_admin_news", page, new { cultureId = culture, statusId = statusId });
            return View("Index", model);
        }
    }
}