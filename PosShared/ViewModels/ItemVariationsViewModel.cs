using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace PosShared.ViewModels
{
    public class ItemVariationsViewModel
    {
        public int ItemId { get; set; }  // The ID of the item for which variations are being shown
        public int OrderItemId { get; set; }  // The ID of the order item to associate variations with
        public int OrderId { get; set; }  // The ID of the order item to associate variations with

        public List<ItemVariation>? Variations { get; set; }  // List of variations for the item
        public List<OrderItemVariation>? OrderItemVariations { get; set; }  // List of variations for the item
    }
}
