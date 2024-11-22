using PosShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.DTOs;

public class VariationsDTO
{
    public int Id { get; set; }
    public int ItemId { get; set; }

    public string Name { get; set; }

    public decimal AdditionalPrice { get; set; }

}
