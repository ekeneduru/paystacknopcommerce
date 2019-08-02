using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.PayStackStandard.Models
{
    public class ConfigurationModel : BaseNopModel
    {

        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Plugins.Payments.PayStackStandard.Fields.SecretKey")]
        [DataType(DataType.Password)]
        [Required]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }
    }
}
