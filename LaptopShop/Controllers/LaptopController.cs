using LaptopShop.Models.database;
using LaptopShop.Models.servive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MediatR;
using LaptopShop.CQRS.Queries;
using LaptopShop.servive.LaptopService;

namespace LaptopShop.Controllers
{
    [Authorize]
    public class LaptopController : Controller
    {
        ILaptopService LaptopService;
        ILogger<LaptopController> _logger;
        IMemoryCache _Cache;
        public LaptopController(ILogger<LaptopController> logger, IMemoryCache cache, ILaptopService laptopService)
        {
            _logger = logger;
            _Cache = cache;
            LaptopService = laptopService;
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
            data = LaptopService.getAll();
            _Cache.Set(key, data, cacheOptions);
            
            
            _logger.LogInformation("Not found in the cache");
            
            
            return View(data);
        }







        public IActionResult gitbyid(int Id)
        {
            return View(LaptopService.getLaptopbyid(Id));
        }

        public IActionResult getcatagory(String catagory)
        {
            string key = "Catagorty" + catagory;
            if (_Cache.TryGetValue(key, out List<Laptop> data))
            {
                _logger.LogInformation("found in the cache");
                return View("Index", data);

            }

            data = LaptopService.getbyCategorie(catagory);


            var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.Normal);
            _Cache.Set(key, data, cacheOptions);
            
            
            _logger.LogInformation("Not found in the cache");
            
            
            
            return View("Index", data);
        }


    }
}
