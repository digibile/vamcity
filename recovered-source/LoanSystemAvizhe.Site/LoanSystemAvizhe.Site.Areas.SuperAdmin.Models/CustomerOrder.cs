using System.Collections.Generic;
using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Site.Areas.SuperAdmin.Models;

public class CustomerOrder
{
    public List<CustomerOrderModel> CustomerOrders { get; set; }

    public int CountAll { get; set; }
}
