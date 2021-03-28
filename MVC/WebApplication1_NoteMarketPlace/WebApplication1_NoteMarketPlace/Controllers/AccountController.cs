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
                    objUser.RoleID = 103; // here 103 is the member ID
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

        // For email verification send mail
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

        // For email verification
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
                else if(result.IsEmailVerified == true && result.RoleID == 103)
                {
                    
                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, objLoginModel.RememberMe);
                    Session["EmailID"] = objLoginModel.EmailID;

                    var loginfirsttime = objUserDBEntities.tblUserProfiles.FirstOrDefault(model => model.UserID == result.ID);

                    if(loginfirsttime == null)
                    {
                        return RedirectToAction("UserProfile", "User");
                    }
                    else
                    {
                        return RedirectToAction("SearchNotes", "User");
                    }

                    
                }
                else if(result.IsEmailVerified == true && result.RoleID == 102) // Admin
                {
                    FormsAuthentication.SetAuthCookie(objLoginModel.EmailID, objLoginModel.RememberMe);
                    Session["EmailID"] = objLoginModel.EmailID;
                    return RedirectToAction("Dashboard", "Admin");
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
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgotPassword()
        {
            ForgotPasswordModel objLoginModel = new ForgotPasswordModel();
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel tempPassword)
        {
            if (objUserDBEntities.tblUsers.Any(model => model.EmailID == tempPassword.EmailID))
            {

                ForgotPasswordEmail(tempPassword);
                ViewBag.Success = "Your password has been changed successfully and newly generated password is sent on your registered email address.";
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Email Id does not exists.");
                return View();
            }
        }

        private void ForgotPasswordEmail(ForgotPasswordModel pass)
        {
            var check = objUserDBEntities.tblUsers.Where(model => model.EmailID == pass.EmailID).FirstOrDefault();

            using (MailMessage mm = new MailMessage("kanokalo345@gmail.com", pass.EmailID))
            {
                mm.Subject = "NoteMarketPlace - Temporary Password";
                var body = "<p>Hello,</p> <p>Your newly generated password is:<p> <p>{0}</p> <p>Thanks,</p><p>Team Notes MarketPlace</p>";
                string newPassword = GeneratePassword().ToString();
                body = string.Format(body, newPassword);
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
                if (newPassword != null)
                {
                    var change = objUserDBEntities.tblUsers.Where(model => model.EmailID == pass.EmailID).FirstOrDefault();
                    if (change != null)
                    {

                        change.Password = newPassword;

                        objUserDBEntities.SaveChanges();
                    }

                }
            }
        }

        public string GeneratePassword()
        {
            string PasswordLength = "6";
            string NewPassword = "";

            string allowedChars = "";
            allowedChars = "1,2,3,4,5,6,7,8,9,0";
            //allowedChars += "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            //allowedChars += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";

            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string IDString = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < Convert.ToInt32(PasswordLength); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                IDString += temp;
                NewPassword = IDString;
            }
            return NewPassword;
        }

        // change password
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword pwd)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            using (var _Context = new NotesMarketPlaceEntities())
            {
                // get current user
                var currentUser = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name);

                // check old password
                if(!currentUser.Password.Equals(pwd.OldPassword))
                {
                    ModelState.AddModelError("Error", "Old password is wrong");
                    return View();
                }
                else
                {
                    // update password
                    currentUser.Password = pwd.ConfirmPassword;
                    currentUser.ModifiedDate = DateTime.Now;
                    _Context.SaveChanges();

                    FormsAuthentication.SignOut();

                    return RedirectToAction("Login", "Account");
                }
            }
        }

    }
}