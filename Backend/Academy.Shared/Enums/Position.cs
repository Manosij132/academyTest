using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.Enums
{
    public enum Position
    {
        [Display(Name = "Project Manager")]
        ProjectManager,

        [Display(Name = "Operations Manager")]
        OperationsManager,

        [Display(Name = "Product Manager")]
        ProductManager,

        [Display(Name = "Delivery Manager")]
        DeliveryManager
    }
}
