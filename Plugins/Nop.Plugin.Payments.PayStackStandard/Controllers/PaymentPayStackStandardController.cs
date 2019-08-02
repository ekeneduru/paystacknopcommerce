using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.PayStackStandard.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using PayStack.Net;




namespace Nop.Plugin.Payments.PayStackStandard.Controllers
{
    public class PaymentPayStackStandardController : BasePaymentController
    {

        #region Fields

        //private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
       // private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
       // private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
      //  private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public PaymentPayStackStandardController(
            //IGenericAttributeService genericAttributeService,
            
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
           // IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ///ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext
            //ShoppingCartSettings shoppingCartSettings
            )
        {
            //_genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            //_paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
           /// _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            //_shoppingCartSettings = shoppingCartSettings;
        }

        #endregion



        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var payStackStandardPaymentSettings = _settingService.LoadSetting<PayStackStandardPaymentSettings>(storeScope);
            var model = new ConfigurationModel
            {
                SecretKey = payStackStandardPaymentSettings.SecretKey,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.PayStackStandard/Views/Configure.cshtml", model);

            model.SecretKey_OverrideForStore = _settingService.SettingExists(payStackStandardPaymentSettings, x => x.SecretKey, storeScope);

            return View("~/Plugins/Payments.PayStackStandard/Views/Configure.cshtml", model);

        }


        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var payStackStandardPaymentSettings = _settingService.LoadSetting<PayStackStandardPaymentSettings>(storeScope);

            //save settings
            payStackStandardPaymentSettings.SecretKey = model.SecretKey;
           

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(payStackStandardPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }





            public IActionResult HandleCallback()
        {
            try
            {
                var storeScope = _storeContext.ActiveStoreScopeConfiguration;
                var payStackStandardPaymentSettings = _settingService.LoadSetting<PayStackStandardPaymentSettings>(storeScope);
                var strRequest = Request.QueryString.ToString();
                var dict = HttpUtility.ParseQueryString(strRequest);
                var json = JsonConvert.SerializeObject(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
                QueryStringModel quStr = JsonConvert.DeserializeObject<QueryStringModel>(json);
                PayStackApi api = new PayStackApi(payStackStandardPaymentSettings.SecretKey);
                TransactionVerifyResponse response = api.Transactions.Verify(quStr.reference);
                if (response.Status == true && response.Data.Status == "success")
                {


                    var metadata = response.Data.Metadata;
                   

                    string orderGuid = string.Empty;
                    string customOrderNumber = string.Empty;
                    foreach (var item in metadata)
                    {
                        var orderkey = item.Key;
                        if(item.Key== "customOrderNumber")
                            customOrderNumber = item.Value.ToString();
                        if (item.Key == "orderGuid")
                            orderGuid= item.Value.ToString();
                    }

                    var order = _orderService.GetOrderByGuid(Guid.Parse(orderGuid));

                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = response.Data.Reference;
                        //order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                        _orderService.UpdateOrder(order);
                        _orderProcessingService.MarkOrderAsPaid(order);

                    }
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else if (response.Status == true && response.Data.Status == "failed")
                {


                }
                else if (response.Status == true && response.Data.Status == "abandoned")
                {

                }
                return Content(string.Empty);
            }
            catch (Exception ex)
            {
                return Content(string.Empty);
            }
        }



    }
}
