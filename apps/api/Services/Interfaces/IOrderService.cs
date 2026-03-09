using RestaurantSaas.Api.DTOs.Orders;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IOrderService
{
    List<OrderDto> GetOrders();
}
