using Vendr.Extensions;
using Vendr.Core.Models;
using Vendr.Core.Api;
using Vendr.RemadeByClive.Web.Dtos;
using Vendr.Common.Validation;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Vendr.RemadeByClive.Web.Controllers
{
    public class CartSurfaceController : SurfaceController
    {
        private readonly IVendrApi _vendrApi;

        public CartSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IVendrApi vendrApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _vendrApi = vendrApi;
        }

        [HttpPost]
        public IActionResult AddToCart(AddToCartDto postModel)
        {
            try
            {
                using (var uow = _vendrApi.Uow.Create())
                {
                    var store = CurrentPage.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);
                    var order = _vendrApi.GetOrCreateCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .AddProduct(postModel.ProductReference, 1);

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch (ValidationException)
            {
                ModelState.AddModelError("", "Failed to add product to cart");

                return CurrentUmbracoPage();
            }

            TempData["SucessMessage"] = "Product successfully added to cart";

            return RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        public IActionResult UpdateCart(UpdateCartDto postModel)
        {
            try
            {
                using (var uow = _vendrApi.Uow.Create())
                {
                    var store = CurrentPage.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);
                    var order = _vendrApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow);

                    foreach (var orderLine in postModel.OrderLines)
                    {
                        order.WithOrderLine(orderLine.Id)
                            .SetQuantity(orderLine.Quantity);
                    }

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch (ValidationException)
            {
                ModelState.AddModelError("", "Failed to update cart");

                return CurrentUmbracoPage();
            }

            TempData["SucessMessage"] = "Cart successfully updated";

            return RedirectToCurrentUmbracoPage();
        }

        [HttpGet]
        public IActionResult RemoveFromCart(RemoveFromCartDto postModel)
        {
            try
            {
                using (var uow = _vendrApi.Uow.Create())
                {
                    var store = CurrentPage.Value<StoreReadOnly>("store", fallback: Fallback.ToAncestors);
                    var order = _vendrApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .RemoveOrderLine(postModel.OrderLineId);

                    _vendrApi.SaveOrder(order);

                    uow.Complete();
                }
            }
            catch (ValidationException)
            {
                ModelState.AddModelError("", "Failed to remove item from cart");

                return CurrentUmbracoPage();
            }

            TempData["SucessMessage"] = "Cart item successfully removed";

            return RedirectToCurrentUmbracoPage();
        }
    }
}
