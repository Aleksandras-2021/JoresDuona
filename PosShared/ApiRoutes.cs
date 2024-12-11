using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared;

public static class ApiRoutes
{
    public const string ApiBaseUrl = "https://localhost:5000";
    public const string ClientBaseUrl = "https://localhost:5001";


    public static class Orders
    {
        public static string GetPaginated(int pageNumber, int pageSize) => $"{ApiBaseUrl}/api/Order?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string GetOrderVariations(int orderId) => $"{ApiBaseUrl}/api/Order/{orderId}/Variations";
        public static string Create => $"{ApiBaseUrl}/api/Order";
        public static string GetById(int id) => $"{ApiBaseUrl}/api/Order/{id}";
        public static string Update(int id) => $"{ApiBaseUrl}/api/Order/{id}";
        public static string Delete(int id) => $"{ApiBaseUrl}/api/Order/{id}";
        public static string UpdateStatus(int id, OrderStatus status) => $"{ApiBaseUrl}/api/Order/{id}/UpdateStatus/{status}";

    }

    public static class OrderItems
    {
        public static string GetOrderItems(int orderId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items";
        public static string AddOrderItem(int orderId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items";
        public static string DeleteOrderItem(int orderId, int itemId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items/{itemId}";

        public static string GetVariations(int orderId, int orderItemId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items/{orderItemId}/Variations";
        public static string AddVariation(int orderId, int orderItemId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items/{orderItemId}/Variations";
        public static string DeleteVariation(int orderId, int orderItemId, int orderItemVariationId) => $"{ApiBaseUrl}/api/Order/{orderId}/Items/{orderItemId}/Variations/{orderItemVariationId}";
    }
    public static class Items
    {
        public static string GetAll => $"{ApiBaseUrl}/api/Items";
        public static string GetVariations(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}/Variations";
    }


}
