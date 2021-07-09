using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1_NoteMarketPlace.DBModel;
using WebApplication1_NoteMarketPlace.Models;

namespace WebApplication1_NoteMarketPlace.Controllers
{
    public class HomeController : Controller
    {
        NotesMarketPlaceEntities objUserDBEntities = new NotesMarketPlaceEntities();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Faq()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {1}</p><p>Hello,</p><p>{2}</p><br><p>Regards,</p><p>{0}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("kanokalo345@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("kanokalo345@gmail.com");  // replace with valid value
                message.Subject = model.Subject;
                message.Body = string.Format(body, model.FullName, model.EmailID, model.Comments);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "kanokalo345@gmail.com",  // replace with valid value
                        Password = ""  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    
                }
            }
            return View(model);
        }

       
    }

}

    
