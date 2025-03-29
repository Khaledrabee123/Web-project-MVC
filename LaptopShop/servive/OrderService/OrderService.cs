using LaptopShop.Models.database;
using LaptopShop.Models.interfaces;

namespace LaptopShop.servive.order
{
    public class OrderService : IOrderService
    {
        IOrderRepository orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public void addOrder(Order order)
        {
            orderRepository.addOrder(order);
        }

        public Order MakeOrder(string userID, decimal total)
        {
           return orderRepository.MakeOrder(userID, total);
        }

        public Order oredr(decimal total, string id, string username)
        {
            return orderRepository.oredr(total, id, username);  
        }

        public void removeOrder(Order order)
        {
            orderRepository.removeOrder(order);
        }

        public void updateOrder(Order order)
        {
            orderRepository.updateOrder(order);
        }
    }
}
