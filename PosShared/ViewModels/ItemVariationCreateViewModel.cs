﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosShared.ViewModels;

public class ItemVariationCreateViewModel
{
    public int ItemId { get; set; }

    public string Name { get; set; }

    public decimal AdditionalPrice { get; set; }
}
