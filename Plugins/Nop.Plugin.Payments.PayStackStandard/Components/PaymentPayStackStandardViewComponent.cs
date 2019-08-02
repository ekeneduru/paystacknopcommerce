using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayStackStandard.Components
{

    [ViewComponent(Name = "PaymentPayStackStandard")]
    public class PaymentPayStackStandardViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayStackStandard/Views/PaymentInfo.cshtml");
        }
    }
    
}

