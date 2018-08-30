using Microsoft.AspNetCore.Mvc;
using NDbPortal.Query;
using NDbPortal.Sample.Web.LtvDev;

namespace NDbPortal.Sample.Web.Controllers
{
    public class QueryController : Controller
    {
        private readonly IQuery<User> _userQuery;

        public QueryController(IQuery<User> userQuery)
        {
            _userQuery = userQuery;
        }
        public IActionResult Get(long id)
        {
            var model = _userQuery.Get(id);
            return View("Index", model);
        }

        public IActionResult GetAll()
        {
            var model = _userQuery.GetAll();
            return View("Index", model);
        }

        public IActionResult Find(string userName)
        {
            var model = _userQuery.Find(u => u.UserName == userName);
            return View("Index", model);
        }
        public IActionResult FindAll(string userName)
        {
            var model = _userQuery.FindAll(u => u.UserName.ToLower() == userName.ToLower());
            return View("Index", model);
        }
        public IActionResult GetPagedList(int page)
        {
            var model = _userQuery.GetPagedList(page);
            return View("Index", model);
        }
    }
}