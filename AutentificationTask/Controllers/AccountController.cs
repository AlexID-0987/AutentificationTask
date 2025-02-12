using AutentificationTask.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutentificationTask.Controllers
{
    public class AccountController : Controller
    {
        private UserContext _userContext;
        public AccountController(UserContext userContext)
        {
            _userContext = userContext;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                User user = await _userContext.Users.Include(u=>u.Role).FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                if(user!=null)
                {
                    await Authentificate(user);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(" ", "Incorrect login and(or) password");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        public async Task<IActionResult> Myusers()
        {
            List<User> us = _userContext.Users.ToList();
            return View(us);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerView)
        {
            if (ModelState.IsValid)
            {
                User user=await _userContext.Users.FirstOrDefaultAsync(u=>u.Email== registerView.Email);
                {
                    if (user==null)
                    {
                        //_userContext.Add(new User { Email= registerView.Email, Password=registerView.Password});

                        user = new User { Email = registerView.Email, Password = registerView.Password };
                        Role userRole = await _userContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                        _userContext.Users.Add(user);
                        _userContext.SaveChanges();
                        if(userRole!=null)
                        {
                            user.Role = userRole;
                        }
                        await _userContext.SaveChangesAsync();
                        await Authentificate(user);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(" ", "Incorrect login and(or) password");
                    }
                }
            }
                return View(registerView);
        }
        private async Task Authentificate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
            };

            ClaimsIdentity id=new ClaimsIdentity(claims, "ApplicationCookis", ClaimsIdentity.DefaultNameClaimType,ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
