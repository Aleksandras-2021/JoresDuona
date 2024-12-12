using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosShared.DTOs;
public class OrderDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int BusinessId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public OrderStatus Status { get; set; }

    public decimal ChargeAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TipAmount { get; set; }

}
