using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Andoromeda.RushHour.Models;

namespace Andoromeda.RushHour.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        [Route("sign-in")]
        public IActionResult SignIn() => View();

        [HttpPost]
        [Route("sign-in")]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(string phone)
        {
            var code = Random.Next(100000, 999999);
            HttpContext.Session.SetString("SignInCode", code.ToString());
            HttpContext.Session.SetString("SignInPhone", phone);
            HttpContext.Session.SetString("Expire", DateTime.UtcNow.AddMinutes(5).ToTimeStamp().ToString());
            // TODO: Send SMS
            return RedirectToAction("SignIn2");
        }

        [HttpGet]
        [Route("sign-in-2")]
        public IActionResult SignIn2()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("SignInCode")))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Invalid Operation"];
                    x.Details = SR["Please back to sign in page and try again."];
                    x.StatusCode = 400;
                });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn2(string code)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("SignInCode")))
            {
                return Prompt(x =>
                {
                    x.Title = SR["Invalid Operation"];
                    x.Details = SR["Please back to sign in page and try again."];
                    x.StatusCode = 400;
                });
            }

            var _code = HttpContext.Session.GetString("SignInCode");
            var expire = new DateTime(Convert.ToInt64(HttpContext.Session.GetString("Expire")));
            if (DateTime.UtcNow > expire)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Code is expired"];
                    x.Details = SR["Your code is expired, please back to sign in page and try again."];
                    x.StatusCode = 400;
                });
            }
            
            if (code != _code)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Code is incorrect"];
                    x.Details = SR["The code you inputed is mismatching the one sent you."];
                    x.StatusCode = 400;
                });
            }

            var id = HttpContext.Session.GetString("SignInPhone");
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("UserId", id);

            return RedirectToAction("Binding", new { id });
        }

        [HttpGet]
        [Route("{id}/binding")]
        public async Task<IActionResult> Binding(string id, CancellationToken cancellationToken)
        {
            if (!IsSignedIn)
            {
                return RedirectToAction("SignIn");
            }

            if (id != UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Forbidden"];
                    x.Details = SR["You have no permission to the user {0}", id];
                    x.StatusCode = 403;
                });
            }

            var bindings = await DB.Bindings
                .Where(x => x.UserId == UserId)
                .ToListAsync(cancellationToken);

            return View(bindings);
        }

        [HttpPost]
        [Route("{id}/binding/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBinding(string id, string account, CancellationToken cancellationToken)
        {
            if (!IsSignedIn)
            {
                return RedirectToAction("SignIn");
            }

            if (id != UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Forbidden"];
                    x.Details = SR["You have no permission to the user {0}", id];
                    x.StatusCode = 403;
                });
            }

            var limit = Convert.ToInt32(Configuration["BindingLimit:" + CurrentUser.Plan.ToString()]);
            if (await DB.Bindings.CountAsync(x => x.UserId == UserId, cancellationToken) >= limit)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Binding failed"];
                    x.Details = SR["Your current plan allows bind up to {0} accounts.", CurrentUser.Plan.ToString()];
                    x.StatusCode = 400;
                });
            }

            DB.Bindings.Add(new Binding
            {
                UserId = UserId,
                Account = account
            });

            await DB.SaveChangesAsync();

            return RedirectToAction("Binding", new { id });
        }

        [HttpPost]
        [Route("{id}/binding/remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBinding(string id, string account, CancellationToken cancellationToken)
        {
            if (!IsSignedIn)
            {
                return RedirectToAction("SignIn");
            }

            if (id != UserId)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Forbidden"];
                    x.Details = SR["You have no permission to the user {0}", id];
                    x.StatusCode = 403;
                });
            }

            var binding = await DB.Bindings
                .SingleOrDefaultAsync(x => x.UserId == UserId && x.Account == account, cancellationToken);

            if (binding == null)
            {
                return Prompt(x =>
                {
                    x.Title = SR["Binding not found"];
                    x.Details = SR["Your phone has not binded to {0}", account];
                    x.StatusCode = 404;
                });
            }

            DB.Bindings.Remove(binding);
            await DB.SaveChangesAsync();

            return RedirectToAction("Binding", new { id });
        }

        private Random Random = new Random();

        private string UserId => HttpContext.Session.GetString("UserId");

        private User _currentUser;

        private User CurrentUser
        {
            get
            {
                if (_currentUser == null && !string.IsNullOrEmpty(UserId))
                {
                    _currentUser = DB.Users.SingleOrDefault(x => x.Id == UserId);
                }
                return _currentUser;
            }
        }

        private bool IsSignedIn => CurrentUser != null;
    }
}
