﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineShop.Models
{
    public class discount_for_user : IEntit
    {

   

      public  int percent { get; set; }

        public  ApplicationUser User{ get; set; }



    }
}