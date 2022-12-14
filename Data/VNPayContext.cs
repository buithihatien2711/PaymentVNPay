using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestPaymentMVC.Models;

namespace TestPaymentMVC.Data
{
    public class VNPayContext : DbContext
    {
        public VNPayContext(DbContextOptions<VNPayContext> options) : base(options) { }
        
        public DbSet<OrderInfo> OrderInfo { get; set; }
    }
}