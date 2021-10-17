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
    public class RegisterController : Controller
    {
        AppUserRepository _auRep;
        UserProfileRepository _upRep;
        public RegisterController()
        {
            _auRep = new AppUserRepository();
            _upRep = new UserProfileRepository();
        }
        // GET: Register
        public ActionResult RegisterNow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterNow(AppUser appUser, UserProfile userProfile)
        {
            appUser.Password = DantexCrypt.Crypt(appUser.Password);
            if (_auRep.Any(x => x.UserName == appUser.UserName))
            {
                ViewBag.Registered = "Kullanıcı İsmi Daha Önce Alınmış";
                return View();
            }
            else if (_auRep.Any(x => x.Email == appUser.Email))
            {
                ViewBag.Registered = "Email Zaten Kayıtlı";
                return View();
            }

            string emailToBeSent = "Tebrikler Kayıt İşlemleriniz Başarıyla Gerçekleştirildi. Hesbınızı Aktive Etmek İçin https://localhost:44347/Register/Activation/" + appUser.ActivationCode + "Linkine Tıklayınız";

            MailService.Send(appUser.Email, body: emailToBeSent, subject: "Hesap Aktivasyonu");
            _auRep.Add(appUser);
            if (!string.IsNullOrEmpty(userProfile.FirstName.Trim()) || !string.IsNullOrEmpty(userProfile.LastName.Trim()))
            {
                userProfile.ID = appUser.ID;
                _upRep.Add(userProfile);
            }

            return View("RegisterOk");
        }

        public ActionResult Activation(Guid id)
        {
            AppUser toBeActived = _auRep.FirstOrDefault(x => x.ActivationCode == id);
            if (toBeActived != null)
            {
                toBeActived.Active = true;
                _auRep.Update(toBeActived);
                TempData["ActiveOrNot"] = "Hesabınız Aktifleştirildi Giriş Yapabilirsiniz";
                return RedirectToAction("Login", "Home");
            }
            TempData["ActiveOrNot"] = "Hesap Bulunamadı";
            return RedirectToAction("Login", "Home");
        }

        public ActionResult RegisterOk()
        {
            return View();
        }
    }
}