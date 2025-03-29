using System.Security.Claims;
using LaptopShop.Models.database;
using LaptopShop.Models.reposatorys;
using LaptopShop.Models.servive;
using LaptopShop.servive.order;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace LaptopShop.Controllers
{
    public class OrderController : Controller
	{
		
		private readonly UserManager<User> userManager;
		ILogger<OrderController> _logger;
		public IOrderService orderService { get; }

		public OrderController(UserManager<User> userManager, IOrderService orderService, ILogger<OrderController> logger)
		{
			_logger = logger;
			this.userManager = userManager;
            this.orderService = orderService;
		}


		public IActionResult Order(Order order,string Username)
		{
			
			return View(order);
		}

		public async Task<IActionResult> AddOrder(int id,  int TotalAmount, string Username) {

			string Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

			Order order = orderService.MakeOrder(Id, TotalAmount);

            orderService.addOrder(order);
            
			_logger.LogInformation("{user} has orderd {@oredr}", Username, order);
            
			ViewBag.UserName = Username;
            return View(order);

        }
	}
}
