﻿using System.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;
using LaptopShop.Models.database;
using LaptopShop.Models.reposatorys;
using LaptopShop.Models.servive;
using LaptopShop.Views.viewmodels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace LaptopShop.Controllers
{
    public class UserController : Controller
    {
        SignInManager<User> SignInManager;
        UserManager<User> UserManager;
        DBlaptops DBlaptops;
        ILogger<UserController> _logger;
        private IMemoryCache _Cache;
        private readonly ISenderEmail emailSender;
        private readonly Randomreposatory Random;

        public RoleManager<IdentityRole> RoleManager { get; }

        public UserController(UserManager<User> UserManager, SignInManager<User> signInManager, DBlaptops dBlaptops, RoleManager<IdentityRole> roleManager, ILogger<UserController> logger, ISenderEmail emailSender, IMemoryCache memoryCache, Randomreposatory random)
        {
            this.UserManager = UserManager;
            SignInManager = signInManager;
            DBlaptops = dBlaptops;
            RoleManager = roleManager;
            _logger = logger;
            this.emailSender = emailSender;
            this._Cache = memoryCache;
            Random = random;
        }







        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }





        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(ViewUser viewUser)
        {
            if (ModelState.IsValid)
            {



                var IsEmailUsed = await UserManager.FindByEmailAsync(viewUser.Email);
                var IsUsernameUser = await UserManager.FindByEmailAsync(viewUser.UserName);
                if (IsEmailUsed == null && IsUsernameUser == null)
                {
                    viewUser.Id = Random.randomNumber().ToString();
                    string key = "User" + viewUser.Id;
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                        .SetPriority(CacheItemPriority.Normal);
                    _Cache.Set(key, viewUser, cacheOptions);
                    await SendConfirmationEmail(viewUser.Email, viewUser);
                    return RedirectToAction("entercode", "User", new { UserId = viewUser.Id });
                }
                else
                {
                    if (IsUsernameUser == null)
                    {
                        ModelState.AddModelError("Password", "user name must be uniqe");
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Email name must be uniqe");
                    }
                }
            }
            return View("Register");
        }





        [HttpGet]
        public async Task<IActionResult> login()
        {
            var login = new loginViewmodel()
            {
                Schemes = await SignInManager.GetExternalAuthenticationSchemesAsync(),
            };
            return View(login);


        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> login(loginViewmodel login)
        {


            _logger.LogInformation("{user}  is logining", login.Username);

            if (ModelState.IsValid)
            {
                User user = await UserManager.FindByNameAsync(login.Username);
                if (user != null)
                {
                    bool isOK = await UserManager.CheckPasswordAsync(user, login.Password);
                    if (isOK)
                    {
                        await SignInManager.SignInAsync(user, login.RememberMe);
                        _logger.LogInformation("{user} succsesful", login.Username);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogError("{username} invild passwoerd", login.Username);
                        ModelState.AddModelError("", "invild passwoerd");
                    }
                }
                else
                {
                    _logger.LogError("invild username");
                    ModelState.AddModelError("", "invild username");

                }
            }
            login.Schemes = await SignInManager.GetExternalAuthenticationSchemesAsync();


            return View(login);

        }







        public IActionResult logout()
        {
            SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }











    
        private async Task SendConfirmationEmail(string? email, ViewUser? user)
        {

            string key = user.Id.ToString();

            int code = Random.randomNumber();
            var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.Normal);
            _Cache.Set(key, code, cacheOptions);



            var ConfirmationLink = Url.Action("entercode", "User",
            new { UserId = user.Id}, protocol: HttpContext.Request.Scheme);
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, $"enter the code plz", $"Please confirm your account the code is {code} by <a href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);
       
        }

        public IActionResult entercode(string UserId)
        {
            ViewData["id"] = UserId;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId ,int code)
        {
            string key = userId;
            if (_Cache.TryGetValue(key, out int _cachecode))
            {
                _logger.LogInformation("_cache code not expierd found in the cache");
            }
            else
            {
                _logger.LogInformation("_cache code  expierd found in the cache");

            }
            string Userkey ="User"+ userId;
            if (_Cache.TryGetValue(Userkey, out ViewUser? user))
            {
                _logger.LogInformation("user information not expierd found in the cache");
            }
            else
            {
                _logger.LogInformation("user information   expierd found in the cache");

            }




            if (code == null || _cachecode != code)
            {
                ViewBag.Message = "invilid code ";
                return View();

            }
            

            if (_cachecode == code)
            {
                // Create User
                User RegUser = new User();
                RegUser.Email = user.Email;
                RegUser.UserName = user.UserName;
                RegUser.PasswordHash = user.Password;
                RegUser.EmailConfirmed = true;
                ViewBag.Message = "Thank you for confirming your email";
                await UserManager.CreateAsync(RegUser, user.Password);
              
                return View();
            }

            
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }


        public IActionResult ExternalLogin(string provider, string returnUrl = "")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "User", new { ReturnUrl = returnUrl });

            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }









        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "", string remoteError = "")
        {

            var loginVM = new loginViewmodel()
            {
                Schemes = await SignInManager.GetExternalAuthenticationSchemesAsync()
            };

            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError("", $"Error from extranal login provide: {remoteError}");
                return View("Login", loginVM);
            }

            //Get login info
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", $"Error from extranal login provide: {remoteError}");
                return View("Login", loginVM);
            }

            var signInResult = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Home");
            else
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = await UserManager.FindByEmailAsync(userEmail);

                    if (user == null)
                    {
                        user = new Models.database.User()
                        {
                            UserName = userEmail,
                            Email = userEmail,
                            EmailConfirmed = true
                        };

                        await UserManager.CreateAsync(user);
                    }

                    await SignInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

            }

            ModelState.AddModelError("", $"Something went wrong");
            return View("Login", loginVM);
        }

    }

}

