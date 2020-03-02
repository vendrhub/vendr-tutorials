using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Exceptions;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.RemadeByClive.Web.Dtos;

namespace Vendr.RemadeByClive.Web.Controllers
{
    public class CartSurfaceController : SurfaceController
    {
        private readonly IVendrApi _vendrApi;

        public CartSurfaceController(IVendrApi vendrApi)
        {
            _vendrApi = vendrApi;
        }

        [HttpPost]
        public ActionResult AddToCart(AddToCartDto postModel)
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
        public ActionResult UpdateCart(UpdateCartDto postModel)
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
        public ActionResult RemoveFromCart(RemoveFromCartDto postModel)
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
