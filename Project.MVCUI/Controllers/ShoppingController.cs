using PagedList;
using Project.BLL.DesingPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using Project.MVCUI.Models.ShoppingTools;
using Project.MVCUI.VMClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Controllers
{
    public class ShoppingController : Controller
    {
        OrderRepository _oRep;
        ProductRepository _pRep;
        CategoryRepository _cRep;
        OrderDetailRepository _odRep;

        public ShoppingController()
        {
            _oRep = new OrderRepository();
            _pRep = new ProductRepository();
            _cRep = new CategoryRepository();
            _odRep = new OrderDetailRepository();
        }

        // GET: ShoppingList
        public ActionResult ShoppingList(int? page, int? categoryID)
        {
            PaginationVM pavm = new PaginationVM
            {
                PageProducts = categoryID == null ? _pRep.GetActives().ToPagedList(page ?? 1, 9) : _pRep.Where(x => x.CategoryID == categoryID).ToPagedList(page ?? 1, 9),
                Categories = _cRep.GetActives()
            };
            if (categoryID != null) TempData["catID"] = categoryID;
            return View(pavm);
        }

        public ActionResult AddToCart(int id)
        {
            Cart c = Session["scart"] == null ? new Cart() : Session["scart"] as Cart;
            Product eklenecekUrun = _pRep.Find(id);

            CartItem ci = new CartItem
            {
                ID = eklenecekUrun.ID,
                Name = eklenecekUrun.ProductName,
                Price = eklenecekUrun.UnitPrice,
                ImagePath = eklenecekUrun.ImagePath
            };
            c.SepeteEkle(ci);
            Session["scart"] = c;
            return RedirectToAction("ShoppingList");
        }

        public ActionResult CartPage()
        {
            if (Session["scart"] != null)
            {
                CartPageVM cpvm = new CartPageVM();
                Cart c = Session["scart"] as Cart;
                cpvm.Cart = c;
                return View(cpvm);
            }
            TempData["bos"] = "Sepetinizde Ürün Bulunmamaktadır.";
            return RedirectToAction("ShoppingList");
        }

        public ActionResult DeleteFromCart(int id)
        {
            if (Session["scart"] != null)
            {
                Cart c = Session["scart"] as Cart;
                c.SepettenCikar(id);
                if (c.Sepetim.Count == 0)
                {
                    Session.Remove("scart");
                    TempData["sepetbos"] = "Sepetinizde Ürün Bulunmamaktadır.";
                    return RedirectToAction("ShoppingList");
                }
                return RedirectToAction("CartPage");
            }
            return RedirectToAction("ShoppingList");
        }

        public ActionResult SiparisOnayla()
        {
            AppUser mevcutKullanici;
            if (Session["member"] != null)
            {
                mevcutKullanici = Session["member"] as AppUser;
            }
            else TempData["anonim"] = "Kullanıcı Üye Değil";
            return View();
        }

        [HttpPost]
        public ActionResult SiparisOnayla(OrderVM orderVM)
        {
            bool result;
            Cart sepet = Session["scart"] as Cart;
            orderVM.Order.TotalPrice = orderVM.PaymentDTO.ShoppingPrice = sepet.TotalPrice;


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44304/api/");
                Task<HttpResponseMessage> postTask = client.PostAsJsonAsync("Payent/ReceivePayment", orderVM.PaymentDTO);
                HttpResponseMessage sonuc;
                try
                {
                    sonuc = postTask.Result;
                }
                catch (Exception ex)
                {
                    TempData["baglantiRed"] = "Banka bağlantıyı Reddetti";
                    return RedirectToAction("ShoppingList");
                }
                if (sonuc.IsSuccessStatusCode) result = true;
                else result = false;

                if (result)
                {
                    if (Session["member"] != null)
                    {
                        AppUser kullanici = Session["member"] as AppUser;
                        orderVM.Order.AppUserID = kullanici.ID;
                        orderVM.Order.UserName = kullanici.UserName;
                    }
                    else
                    {
                        orderVM.Order.AppUserID = null;
                        orderVM.Order.UserName = TempData["anonim"].ToString();
                    }
                    _oRep.Add(orderVM.Order);
                    foreach (CartItem item in sepet.Sepetim)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderID = orderVM.Order.ID;
                        orderDetail.ProductID = item.ID;
                        orderDetail.TotalPrice = item.SubTotal;
                        orderDetail.Quantity = item.Amount;
                        _odRep.Add(orderDetail);
                        Product stokDus = _pRep.Find(item.ID);
                        stokDus.UnitsInStock -= item.Amount;
                        _pRep.Update(stokDus);

                    }
                    TempData["odeme"] = "Siparişiniz Başarıyla Alınmıştır.";
                    MailService.Send(orderVM.Order.Email, body: $"Siparişiniz Başarıyla Alındı.. {orderVM.Order.TotalPrice}");
                    return RedirectToAction("ShoppingList");
                }
                else
                {
                    TempData["sorun"] = "Ödeme Sırasında Bir Sorun Oluştu Lütfen Bankanız ile İletişime Geçiniz";
                    return RedirectToAction("ShoppingList");
                }


            }

        }





    }
}