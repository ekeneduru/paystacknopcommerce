using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayStackStandard
{
    public class PayStackStandardPaymentSettings: ISettings
    {
        /// <summary>
        /// Gets or sets  SecretKey
        /// </summary>
        public string SecretKey { get; set; }


    }
}
