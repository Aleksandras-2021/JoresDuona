﻿using PosShared.Models;


namespace PosShared;

public static class ApiRoutes
{
    public const string ApiBaseUrl = "https://localhost:5000";
    public const string ClientBaseUrl = "https://localhost:5001";


    public static class Auth
    {
        public static string Login => $"{ApiBaseUrl}/api/Auth/login";
        public static string ChangePassword => $"{ApiBaseUrl}/api/Auth/Change-Password";
    }
    
    public static class Order
    {
        public static string GetPaginated(int pageNumber, int pageSize) => $"{ApiBaseUrl}/api/Order?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string GetOrderVariations(int orderId) => $"{ApiBaseUrl}/api/Order/{orderId}/Variations";
        public static string GetOrderServices(int orderId) => $"{ApiBaseUrl}/api/Order/{orderId}/Services";
        public static string GetOrderPayments(int orderId) => $"{ApiBaseUrl}/api/Payment/Order/{orderId}";
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
        public static string GetItemsPaginated(int pageNumber, int pageSize) => $"{ApiBaseUrl}/api/Items?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string GetItemById(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}";
        public static string CreateItem => $"{ApiBaseUrl}/api/Items";
        public static string UpdateItem(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}";
        public static string DeleteItem(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}";
        public static string GetItemVariations(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}/Variations";
        public static string GetItemVariationById(int varId) => $"{ApiBaseUrl}/api/Items/Variations/{varId}";
        public static string CreateVariation(int itemId) => $"{ApiBaseUrl}/api/Items/{itemId}/Variations";
        public static string UpdateVariation(int variationId) => $"{ApiBaseUrl}/api/Items/Variations/{variationId}";
        public static string DeleteVariations(int variationId) => $"{ApiBaseUrl}/api/Items/Variations/{variationId}";
    }
    
    public static class Tax
    {
        public static string List => $"{ApiBaseUrl}/api/Tax";
        public static string GetById(int taxId) => $"{ApiBaseUrl}/api/Tax/{taxId}";
        public static string Create => $"{ApiBaseUrl}/api/Tax";
        public static string Update(int taxId) => $"{ApiBaseUrl}/api/Tax/{taxId}";
        public static string Delete(int itemId) => $"{ApiBaseUrl}/api/Tax/{itemId}";
    }

    public static class Business
    {
        public static string ListPaginated(int pageNumber,int pageSize) =>  $"{ApiBaseUrl}/api/Businesses?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string Create =>  $"{ApiBaseUrl}/api/Businesses";
        public static string GetById(int businessId) =>  $"{ApiBaseUrl}/api/Businesses/{businessId}";
        public static string Update(int businessId) =>  $"{ApiBaseUrl}/api/Businesses/{businessId}";
        public static string Delete(int businessId) =>  $"{ApiBaseUrl}/api/Businesses/{businessId}";
    }

    public static class User
    {
        public static string ListPaginated(int pageNumber,int pageSize) =>  $"{ApiBaseUrl}/api/Users?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string Create =>  $"{ApiBaseUrl}/api/Users";
        public static string GetById(int id) =>  $"{ApiBaseUrl}/api/Users/{id}";
        public static string Update(int id) =>  $"{ApiBaseUrl}/api/Users/{id}";
        public static string Delete(int id) =>  $"{ApiBaseUrl}/api/Users/{id}";
        
    }
    
    public static class Reservation
    {
        public static string ListPaginated(int pageNumber,int pageSize) =>  $"{ApiBaseUrl}/api/Reservation?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string Create =>  $"{ApiBaseUrl}/api/Reservation";
        public static string GetById(int id) =>  $"{ApiBaseUrl}/api/Reservation/{id}";
        public static string Update(int id) =>  $"{ApiBaseUrl}/api/Reservation/{id}";
        public static string Delete(int id) =>  $"{ApiBaseUrl}/api/Reservation/{id}";
        
    }
    public static class Service
    {
        public static string List =>  $"{ApiBaseUrl}/api/Service";
        public static string Create =>  $"{ApiBaseUrl}/api/Service";
        public static string GetById(int id) =>  $"{ApiBaseUrl}/api/Service/{id}";
        public static string Update(int id) =>  $"{ApiBaseUrl}/api/Service/{id}";
        public static string Delete(int id) =>  $"{ApiBaseUrl}/api/Service/{id}";
    }
    
    public static class Payment
    {
        public static string Get =>  $"{ApiBaseUrl}/api/Payment";
        public static string Create =>  $"{ApiBaseUrl}/api/Payment";
        public static string CreateRefund(int paymentId) =>  $"{ApiBaseUrl}/api/Payment/Refund/{paymentId}";
        public static string GetById(int id) =>  $"{ApiBaseUrl}/api/Payment/{id}";
        public static string Update(int id) =>  $"{ApiBaseUrl}/api/Payment/{id}";
        public static string Delete(int id) =>  $"{ApiBaseUrl}/api/Payment/{id}";
    }
    
    public static class DefaultShiftPattern
    {
        public static string List =>  $"{ApiBaseUrl}/api/DefaultShiftPattern";
        public static string Create =>  $"{ApiBaseUrl}/api/DefaultShiftPattern";
        public static string GetById(int id) =>  $"{ApiBaseUrl}/api/DefaultShiftPattern/{id}";
        public static string Update(int id) =>  $"{ApiBaseUrl}/api/DefaultShiftPattern/{id}";
        public static string Delete(int id) =>  $"{ApiBaseUrl}/api/DefaultShiftPattern/{id}";
        
    }

    

}
