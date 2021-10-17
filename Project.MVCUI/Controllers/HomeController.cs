using Project.BLL.DesingPatterns.GenericRepository.ConcRep;
using Project.COMMON.Tools;
using Project.ENTITIES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project.MVCUI.Controllers
{
    public class HomeController : Controller
    {
        AppUserRepository _apRep;
        public HomeController()
        {
            _apRep = new AppUserRepository();
        }
        // GET: Home
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(AppUser appUser)
        {
            AppUser captured = _apRep.FirstOrDefault(x => x.UserName == appUser.UserName);
            if (captured == null)
            {
                ViewBag.User = "Kullanıcı Bulunamadı";
                return View();
            }
            string decrypted = DantexCrypt.DeCrypt(captured.Password);
            if (appUser.Password == decrypted && captured.Role == ENTITIES.Enums.UserRole.Admin)
            {
                if (!captured.Active) return ActivationControl();
                Session["admin"] = captured;
                return RedirectToAction("CategoriList", "Category", new { area = "admin" });
            }
            else if (captured.Role == ENTITIES.Enums.UserRole.Member && appUser.Password == decrypted)
            {
                if (!captured.Active) return ActivationControl();
                Session["member"] = captured;
                return RedirectToAction("ShoppingList", "Shopping");
            }
            ViewBag.User = "Kullanıcı Bulunamadı";
            return View();
        }

        public ActionResult ActivationControl()
        {
            ViewBag.User = "Lütfen Hesabınızı Aktifleştirin";
            return View("Login");
        }
    }
}