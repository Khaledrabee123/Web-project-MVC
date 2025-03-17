using LaptopShop.Models.database;
using LaptopShop.Models.servive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MediatR;
using LaptopShop.CQRS.Queries;

namespace LaptopShop.Controllers
{
    [Authorize]
    public class LaptopController : Controller
    {
      private readonly   IMediator mediator;
        ILogger<LaptopController> _logger;
        IMemoryCache _Cache;
		public LaptopController(ILogger<LaptopController> logger, IMemoryCache cache, IMediator mediator)
		{
			_logger = logger;
			_Cache = cache;
			this.mediator = mediator;
		}

		public IActionResult Index()
        {
            string key = "GetAllLaptops";
            if (_Cache.TryGetValue(key, out List<Laptop> data))
            {
                _logger.LogInformation("found in the cache");
                return View(data);

            }
           
            
            var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.Normal);
             data = mediator.Send(new GetAllLaptopsQuery()).Result;
            _Cache.Set(key, data, cacheOptions);
            
            
            _logger.LogInformation("Not found in the cache");
            
            
            return View(data);
        }







        public IActionResult gitbyid(int Id)
        {
            return View(mediator.Send(new GetLaptopByIdQuery(Id)).Result);
        }

        public IActionResult getcatagory(String catagory)
        {
            string key = "Catagorty" + catagory;
            if (_Cache.TryGetValue(key, out List<Laptop> data))
            {
                _logger.LogInformation("found in the cache");
                return View("Index", data);

            }
            
            var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.Normal);
            data = mediator.Send(new GetLaptopsByCategorieQuery(catagory)).Result;
            _Cache.Set(key, data, cacheOptions);
            
            
            _logger.LogInformation("Not found in the cache");
            
            
            
            return View("Index", data);
        }


    }
}
