using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication1_NoteMarketPlace.DBModel;
using WebApplication1_NoteMarketPlace.Models;

namespace WebApplication1_NoteMarketPlace.Controllers
{
    public class AccountController : Controller
    {
        NotesMarketPlaceEntities objUserDBEntities = new NotesMarketPlaceEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Signup()
        {
            UserModel objUserModel = new UserModel();
            return View(objUserModel);
        }

        [HttpPost]
        public ActionResult Signup(UserModel objUserModel)
        {
            if(ModelState.IsValid)
            {
                if (!objUserDBEntities.tblUsers.Any(model => model.EmailID == objUserModel.EmailID ))
                {
                    tblUser objUser = new DBModel.tblUser();
                    objUser.RoleID = 103;
                    objUser.CreatedDate = DateTime.Now;
                    objUser.FirstName = objUserModel.FirstName;
                    objUser.EmailID = objUserModel.EmailID;
                    objUser.LastName = objUserModel.LastName;
                    objUser.Password = objUserModel.Password;
                    objUserDBEntities.tblUsers.Add(objUser);
                    objUserDBEntities.SaveChanges();
                    int id = objUser.ID;
                    if (id > 0)
                    {
                        SendActivationEmail(objUserModel);
                        ViewBag.Success = "Your account has been successfully created. Please Verify Email.";
                    }


                    return View(objUserModel);
                }
                else
                {
                    ModelState.AddModelError("Error", "Email is Already exists!");
                    return View();
                }
            }
            return View();
        }

        private void SendActivationEmail(UserModel objUserModel)
        {
            using(MailMessage mm = new MailMessage("kanokalo345@gmail.com", objUserModel.EmailID))
            {
                mm.Subject = "Note MarketPlace - Email Verification";

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/EmailTemplate/AccountConfirmation.html")))
                {
                    body = reader.ReadToEnd();
                }

                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = objUserModel.EmailID, pass = objUserModel.Password }, protocol: Request.Url.Scheme);

                body = body.Replace("{Username}", objUserModel.FirstName);
                body = body.Replace("{ConfirmationLink}", confirmationLink);

                mm.Body = body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("kanokalo345@gmail.com", "Tatvasoft@123");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }

        [Route("Account/ConfirmEmail")]
        public ActionResult ConfirmEmail(string userId, string pass)
        {
            var check = objUserDBEntities.tblUsers.Where(model => model.EmailID == userId).FirstOrDefault();

            if (check != null)
            {
                if (check.Password.Equals(pass))
                {
                    check.IsEmailVerified = true;

                    objUserDBEntities.SaveChanges();
                    objUserDBEntities.Dispose();

                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return Content("Invalid Credentials");
                }
            }

            return Content("Invalid Credentials");
        }



        public ActionResult Login()
        {
            LoginModel objLoginModel = new LoginModel();
            return View(objLoginModel);
        }

        [HttpPost]
        public ActionResult Login(LoginModel objLoginModel)
        {
            if(ModelState.IsValid)
            {
                var result = objUserDBEntities.tblUsers.Where(model => model.EmailID == objLoginModel.EmailID && model.Password == objLoginModel.Password).FirstOrDefault();

                if (result == null)
                {
                    ModelState.AddModelError("Error", "Email and password is not matching");
                    return View();
                }
                else if(result.IsEmailVerified == true)
                {
                    
                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, objLoginModel.RememberMe);
                    Session["EmailID"] = objLoginModel.EmailID;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    @ViewBag.Email = "Please verify your account.";
                }

                
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}