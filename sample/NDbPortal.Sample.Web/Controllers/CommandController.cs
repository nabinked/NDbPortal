using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NDbPortal.Command;
using NDbPortal.Sample.Web.LtvDev;

namespace NDbPortal.Sample.Web.Controllers
{
    public class CommandController : Controller
    {
        private readonly ICommand<User, long> _userCommand;

        public CommandController(ICommand<User, long> userCommand)
        {
            _userCommand = userCommand;
        }
        public IActionResult Add()
        {
            var newUserId = _userCommand.Add(new User()
            {
                Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                FullName = "Add" + DateTime.Now.ToString("t"),
                Password = "password",
                UserName = Guid.NewGuid().ToString(),
                Salt = "noon",
                RoleId = 1
            });
            return View("Index", newUserId);
        }

        public IActionResult AddRange()
        {
            var newUserId = _userCommand.AddRange(new List<User>()
            {
                new User()
                {
                    Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                    FullName = "AddRange"+ DateTime.Now.ToString("t"),
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1
                },
                new User()
                {
                    Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                    FullName = "AddRange",
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1
                }
            });
            return View("Index", newUserId);
        }

        public IActionResult AddRangeRollBack()
        {
            var email = $"{Guid.NewGuid().ToString()}@outlook.com";
            var newUserId = _userCommand.AddRange(new List<User>()
            {
                new User()
                {
                    Email = email,
                    FullName = "AddRangeRollBack"+ DateTime.Now.ToString("t"),
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1
                },
                new User()
                {
                    Email = email,
                    FullName = "AddRangeRollBack",
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1
                }
            });
            return View("Index", newUserId);
        }

        public IActionResult Update()
        {
            var newUserId = _userCommand.Update(new User()
            {
                Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                FullName = "Update" + DateTime.Now.ToString("t"),
                Password = "password",
                UserName = Guid.NewGuid().ToString(),
                Salt = "noon",
                RoleId = 1,
                Id = 2
            });
            return View("Index", newUserId);
        }

        public IActionResult UpdateRange()
        {
            var newUserId = _userCommand.UpdateRange(new List<User>()
            {
                new User()
                {
                    Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                    FullName = "UpdateRange"+ DateTime.Now.ToString("t"),
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1,
                    Id = 2
                },
                new User()
                {
                    Email = $"{Guid.NewGuid().ToString()}@outlook.com",
                    FullName = "UpdateRange"+ DateTime.Now.ToString("t"),
                    Password = "password",
                    UserName = Guid.NewGuid().ToString(),
                    Salt = "noon",
                    RoleId = 1,
                    Id = 1
                }
            });
            return View("Index", newUserId);
        }
    }
}