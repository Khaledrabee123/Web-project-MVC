using LaptopShop.Models.database;
using LaptopShop.Models.interfaces;
using LaptopShop.Models.servive.CartService;

namespace LaptopShop.Models.servive.CartService
{
    public class CartService :ICartService
    {
        ICartRepository cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            this.cartRepository = cartRepository;
        }

        public void AddToCart(Cart cart)
        {
            cartRepository.AddToCart(cart);
        }

        public Task DeleteFromCart(string UserID, int laptopId)
        {
            return cartRepository.DeleteFromCart(UserID, laptopId);
        }

        public List<Laptop> getUsersLaptops(string UserID)
        {
            return cartRepository.getUsersLaptops(UserID);
        }

        public Cart makeCart(string userID, int laptopID)
        {
            return cartRepository.makeCart(userID, laptopID);
        }
    }
}
