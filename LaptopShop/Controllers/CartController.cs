using System.Security.Claims;
using System.Text;
using LaptopShop.Models.database;
using LaptopShop.Models.reposatorys;
using LaptopShop.Models.servive;
using LaptopShop.Models.servive.CartService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LaptopShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {

        LaptopService laptopSetvice;
        UserManager<User> userManager;
        private readonly IMemoryCache _cache;
        private readonly ICartService cartService;
        public ILogger<CartController> _Logger;

        public CartController( LaptopService lp , UserManager<User> userManager , ILogger<CartController> logger, IMemoryCache memory, ICartService cartService)
        {
            this. userManager = userManager;
            _Logger = logger;
            laptopSetvice = lp;
            _cache = memory;
            this.cartService = cartService;
        }




        public async Task<IActionResult> viewCart(string username)
        {

            int totla = 0;
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value; 
            
            var  data = cartService.getUsersLaptops(userID);
           
            foreach (var item in data)
            {
                totla += item.price;
            }
            ViewBag.Total = totla;

            return View(data);
          
        }








        public async Task<IActionResult> AddToCard(string username, int Id)
        {
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Cart cart = cartService.makeCart(UserId, Id);

            cartService.AddToCart(cart);
            
            _Logger.LogInformation("{username} added this Product ID {id} to his Cart", username,Id);
            
            return RedirectToAction("Index", "Laptop");
        }
        








        public async Task< ActionResult> deletitem(int Laptopid, string username)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();


            cartService.DeleteFromCart(userId, Laptopid);

            
            _Logger.LogInformation("{username} deleted this Product ID {id} to his Cart", username, Laptopid);


            return RedirectToAction ("viewCart","Cart",username);
        }














        public IActionResult buy()
        {
            return View();
        }
    }
}
