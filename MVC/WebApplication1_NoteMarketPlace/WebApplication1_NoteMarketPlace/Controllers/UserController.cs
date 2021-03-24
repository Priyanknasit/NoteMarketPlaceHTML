using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1_NoteMarketPlace.DBModel;
using WebApplication1_NoteMarketPlace.Models;

namespace WebApplication1_NoteMarketPlace.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult SearchNotes()
        {
            return View();
        }

        [Authorize]
        [Route("User/Dashboard")]
        public ActionResult Dashboard()
        {
            using(var _Context = new NotesMarketPlaceEntities())
            {
                // current user
                var currentUser = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name);

                // my total earning
                var earning = (from download in _Context.tblDownloads
                               where download.Seller == currentUser.ID && download.IsSellerHasAllowedDownload == true
                               group download by download.Seller into total
                               select total.Sum(model => model.PurchasedPrice)).ToList();
                ViewBag.TotalEarning = earning.Count() == 0 ? 0 : earning[0];

                // my total sold notes
                var soldNotes = (from download in _Context.tblDownloads
                                 where download.Seller == currentUser.ID
                                 group download by download.Seller into total
                                 select total.Count()).ToList();
                ViewBag.TotalSoldNotes = soldNotes.Count() == 0 ? 0 : soldNotes[0];

                // my download notes
                var downloadNotes = (from download in _Context.tblDownloads
                                     where download.Downloader == currentUser.ID
                                     group download by download.Downloader into total
                                     select total.Count()).ToList();
                ViewBag.TotalDownloadNotes = downloadNotes.Count() == 0 ? 0 : downloadNotes[0];

                // my rejected notes
                var rejectedNotes = (from notes in _Context.tblSellerNotes
                                     join status in _Context.tblReferenceDatas on notes.Status equals status.ID
                                     where status.RefCategory == "Notes Status" && status.Values == "Rejected" && notes.SellerID == currentUser.ID
                                     group notes by notes.SellerID into total
                                     select total.Count()).ToList();
                ViewBag.TotalRejectedNotes = rejectedNotes.Count() == 0 ? 0 : rejectedNotes[0];

                // Buyer request
                var buyerRequest = (from download in _Context.tblDownloads
                                    where download.IsSellerHasAllowedDownload == false && download.Seller == currentUser.ID
                                    group download by download.Seller into total
                                    select total.Count()).ToList();
                ViewBag.TotalBuyerRequest = buyerRequest.Count() == 0 ? 0 : buyerRequest[0];

                // In progress notes
                var progressNotes = (from notes in _Context.tblSellerNotes
                                     join category in _Context.tblNoteCategories on notes.Category equals category.ID
                                     join status in _Context.tblReferenceDatas on notes.Status equals status.ID
                                     where status.RefCategory == "Notes Status" && notes.SellerID == currentUser.ID &&
                                     (status.Values == "Draft" || status.Values == "Submitted" || status.Values == "In Review")
                                     select new DashboardInProgressModel
                                     {
                                         ID = notes.ID,
                                         Title = notes.Title,
                                         Category = category.Name,
                                         Status = status.Values,
                                         CreatedDate = (DateTime)notes.CreatedDate
                                     }).OrderByDescending(model => model.CreatedDate).ToList();

                ViewBag.ProgressNotes = progressNotes;

                // published notes
                var publishedNotes = (from notes in _Context.tblSellerNotes
                                      join category in _Context.tblNoteCategories on notes.Category equals category.ID
                                      join status in _Context.tblReferenceDatas on notes.Status equals status.ID
                                      where status.RefCategory == "Notes Status" && status.Values == "Published" && notes.SellerID == currentUser.ID
                                      select new DashboardInPublishedModel
                                      {
                                          ID = notes.ID,
                                          Title = notes.Title,
                                          Category = category.Name,
                                          Price = (decimal)notes.SellingPrice,
                                          SellType = (notes.SellingPrice == 0 ? "Free" : "Paid"),
                                          CreatedDate = (DateTime)notes.CreatedDate
                                      }).OrderByDescending(model => model.CreatedDate).ToList();

                ViewBag.PublishNotes = publishedNotes;

                return View();
            } 
        }

        [Authorize]
        public ActionResult AddNote(int? edit)
        {
            using(var _Context = new NotesMarketPlaceEntities())
            {
                // all type
                var type = _Context.tblNoteTypes.ToList();

                // all category
                var category = _Context.tblNoteCategories.ToList();

                // all country
                var country = _Context.tblCountries.ToList();

                ViewBag.CategoryList = category;
                ViewBag.NoteTypeList = type;
                ViewBag.CountryList = country;
                ViewBag.Edit = false;

                // edit details
                if(!edit.Equals(null))
                {
                    var note = (from Notes in _Context.tblSellerNotes
                                join Attachment in _Context.tblSellerNotesAttachements on Notes.ID equals Attachment.NoteID
                                where Notes.ID == edit && Notes.Status == 6
                                select new AddNoteModel
                                {
                                    ID = Notes.ID,
                                    Title = Notes.Title,
                                    Category = Notes.Category,
                                    UploadNotes = Attachment.FileName,
                                    NoteType = (int)Notes.NoteType,
                                    NumberofPages = Notes.NumberofPages,
                                    Country = Notes.Country,
                                    UniversityName = Notes.UniversityName,
                                    Course = Notes.Course,
                                    CourseCode = Notes.CourseCode,
                                    Professor = Notes.Professor,
                                    Description = Notes.Description,
                                    SellType = Notes.IsPaid == true ? "Free" : "Paid",
                                    SellingPrice = (decimal)Notes.SellingPrice,
                                    NotePreview = Notes.NotesPreview
                                }).FirstOrDefault<AddNoteModel>();

                    ViewBag.Edit = true;
                    return View(note);
                }

                return View();
            } 
        }

        // draft saving note
        [HttpPost]
        [Route("User/Save")]
        public ActionResult Save(int? id, AddNoteModel note)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("AddNote");
            }

            using(var _Context = new NotesMarketPlaceEntities())
            {
                // get current userID
                var currentuserID = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name).ID;

                // default image for book
                // string bookDefaultImg = _Context.tblSystemConfigurations.FirstOrDefault(model => model.Key == "DefaultNoteDisplayPicture").Values;

                // edit draft details
                if(!id.Equals(null))
                {
                    var noteDetails = _Context.tblSellerNotes.SingleOrDefault(model => model.ID == id && model.Status == 6);
                    var attachment = _Context.tblSellerNotesAttachements.SingleOrDefault(model => model.NoteID == id);

                    note.MaptoModel(noteDetails, attachment);
                    noteDetails.ModificationDate = DateTime.Now;
                    attachment.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard");
                }
                // draft note
                else
                {
                    var noteDetails = _Context.tblSellerNotes;

                    noteDetails.Add(new tblSellerNote
                    {
                        Title = note.Title,
                        SellerID = currentuserID,
                        Category = note.Category,
                        //DisplayPicture = note.DisplayPicture == null ? bookDefaultImg : note.DisplayPicture,
                        NoteType = note.NoteType,
                        NumberofPages = note.NumberofPages,
                        Description = note.Description,
                        UniversityName = note.UniversityName,
                        Country = note.Country,
                        Course = note.Course,
                        CourseCode = note.CourseCode,
                        Professor = note.Professor,
                        SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice,
                        IsPaid = note.SellType == "Free" ? false : true,
                        NotesPreview = note.NotePreview,
                        Status = 6,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    }) ;

                    _Context.SaveChanges();

                    var CreatedNote = noteDetails.FirstOrDefault(model => model.SellerID == currentuserID && model.Title == note.Title).ID;

                    // make folder
                    string path = MakeDirectory(currentuserID, CreatedNote);

                    var attachments = _Context.tblSellerNotesAttachements;
                    attachments.Add(new tblSellerNotesAttachement
                    {
                        NoteID = CreatedNote,
                        FileName = note.UploadNotes,
                        FilePath = path,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");
                
                }
            }
        }

        public string MakeDirectory(int userID, int noteID)
        {
            string path = @"C:\Users\ASUS\source\repos\WebApplication1_NoteMarketPlace\WebApplication1_NoteMarketPlace\Members\" + userID + "\\" + noteID + "\\Attachment";
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }

        // draft notes
        [HttpPost]
        [Route("User/Publish")]
        public ActionResult Publish(int? id, AddNoteModel note)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("AddNote");
            }

            using (var _Context = new NotesMarketPlaceEntities())
            {
                // get current userID
                var currentuserID = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name).ID;

                // default book image
                // var bookDefaultImg = _Context.tblSystemConfigurations.FirstOrDefault(model => model.Key == "DefaultNoteDisplayPicture").Values;

                // pusblish as a draft notes
                if(!id.Equals(null))
                {
                    var noteDraft = _Context.tblSellerNotes.FirstOrDefault(model => model.ID == id && model.Status == 6);
                    var draftAttachment = _Context.tblSellerNotesAttachements.FirstOrDefault(model => model.NoteID == id);
                    note.MaptoModel(noteDraft, draftAttachment);

                    noteDraft.Status = 7;
                    noteDraft.ModificationDate = DateTime.Now;
                    draftAttachment.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");    
                }
                else
                {
                    var noteDetails = _Context.tblSellerNotes;
                    noteDetails.Add(new tblSellerNote
                    {
                        Title = note.Title,
                        SellerID = currentuserID,
                        Category = note.Category,
                        //DisplayPicture = note.DisplayPicture == null ? bookDefaultImg : note.DisplayPicture,
                        NoteType = note.NoteType,
                        NumberofPages = note.NumberofPages,
                        Description = note.Description,
                        UniversityName = note.UniversityName,
                        Country = note.Country,
                        Course = note.Course,
                        CourseCode = note.CourseCode,
                        Professor = note.Professor,
                        SellingPrice = note.SellType == "Free" ? 0 : note.SellingPrice,
                        IsPaid = note.SellType == "Free" ? false : true,
                        NotesPreview = note.NotePreview,
                        Status = 7,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    var createdNote = noteDetails.FirstOrDefault(model => model.SellerID == currentuserID && model.Title == note.Title).ID;

                    string path = MakeDirectory(currentuserID, createdNote);

                    var attachments = _Context.tblSellerNotesAttachements;
                    attachments.Add(new tblSellerNotesAttachement
                    {
                        NoteID = createdNote,
                        FileName = note.UploadNotes,
                        FilePath = path,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                    _Context.SaveChanges();

                    return RedirectToAction("Dashboard", "User");
                }


            }
        }

        // delete note
        [HttpPost]
        [Route("User/delete")]
        public string Delete(int id)
        {
            using(var _Context = new NotesMarketPlaceEntities())
            {
                // get current user
                var currentuserID = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name).ID;

                var note = _Context.tblSellerNotes.SingleOrDefault(model => model.ID == id && model.Status == 6 && model.SellerID == currentuserID);

                var attachment = _Context.tblSellerNotesAttachements.SingleOrDefault(model => model.NoteID == id);

                _Context.tblSellerNotesAttachements.Remove(attachment);
                _Context.tblSellerNotes.Remove(note);
                _Context.SaveChanges();

            }

            return "Dashboard";
        }

        public ActionResult UserProfile()
        {
            using (var _Context = new NotesMarketPlaceEntities())
            {
                // Gender list for dropdown
                var gender = _Context.tblReferenceDatas.Where(model => model.RefCategory == "Gender").ToList();

                // country 
                var country = _Context.tblCountries.ToList();

                // get current userID
                var currentuserID = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name);

                // user details 
                var detailsAvailable = _Context.tblUserProfiles.FirstOrDefault(model => model.UserID == currentuserID.ID);
                   

                var UserProfile = new UserProfileModel();

                // for details available or not
                if(detailsAvailable != null)
                {
                    UserProfile = (from Detail in _Context.tblUserProfiles
                                   join User in _Context.tblUsers on Detail.UserID equals User.ID
                                   join Country in _Context.tblCountries on Detail.Country equals Country.ID
                                   where Detail.UserID == currentuserID.ID
                                   select new UserProfileModel
                                   {
                                       FirstName = User.FirstName,
                                       LastName = User.LastName,
                                       EmailID = User.EmailID,
                                       Gender = Detail.Gender,
                                       DOB = Detail.DOB,
                                       PhoneNumber_CountryCode = Detail.PhoneNumber_CountryCode,
                                       PhoneNumber = Detail.PhoneNumber,
                                       ProfilePicture = Detail.ProfilePicture,
                                       AddressLine1 = Detail.AddressLine1,
                                       AddressLine2 = Detail.AddressLine2,
                                       City = Detail.City,
                                       State = Detail.State,
                                       ZipCode = Detail.ZipCode,
                                       Country = Detail.Country,
                                       University = Detail.University,
                                       College = Detail.College
                                   }).FirstOrDefault<UserProfileModel>();

                    UserProfile.genderModel = gender.Select(m => new GenderModel { Gender_Id = m.ID, Gender_Val = m.Values }).ToList();
                    UserProfile.countryModel = country.Select(m => new CountryModel { Country_Id = m.ID, Country_Val = m.Name }).ToList();
                    UserProfile.CountryCodeModel = country.Select(m => new CountryModel { Country_Code = m.CountryCode }).ToList();

                    return View(UserProfile);
                }
                else
                {
                    UserProfile.FirstName = currentuserID.FirstName;
                    UserProfile.LastName = currentuserID.LastName;
                    UserProfile.EmailID = currentuserID.EmailID;
                    UserProfile.genderModel = gender.Select(m => new GenderModel { Gender_Id = m.ID, Gender_Val = m.Values }).ToList();
                    UserProfile.countryModel = country.Select(m => new CountryModel { Country_Id = m.ID, Country_Val = m.Name }).ToList();
                    UserProfile.CountryCodeModel = country.Select(m => new CountryModel { Country_Code = m.CountryCode }).ToList();
                    return View(UserProfile);
                } 
            }
        }

        [HttpPost]
        public ActionResult UserProfile(UserProfileModel user)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }
            
            using(var _Context = new NotesMarketPlaceEntities())
            {
                // current userID
                int currentUser = _Context.tblUsers.FirstOrDefault(model => model.EmailID == User.Identity.Name).ID;

                // user details
                var userdetails = _Context.tblUserProfiles.FirstOrDefault(model => model.UserID == currentUser);

                // check user available or not
                if(userdetails != null && user != null)
                {
                    // update details
                    var userUpdate = _Context.tblUsers.FirstOrDefault(model => model.ID == currentUser);
                    var update = _Context.tblUserProfiles.FirstOrDefault(model => model.UserID == currentUser);

                    user.MaptoModel(userUpdate, update);
                    userUpdate.ModifiedDate = DateTime.Now;
                    update.ModifiedDate = DateTime.Now;

                    _Context.SaveChanges();

                    return RedirectToAction("UserProfile");
                    
                }
                else
                {
                    // add new details
                    var add = _Context.tblUserProfiles;
                    add.Add(new tblUserProfile
                    {
                        UserID = currentUser,
                        DOB = user.DOB,
                        Gender = user.Gender,
                        PhoneNumber_CountryCode = user.PhoneNumber_CountryCode,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePicture = user.ProfilePicture,
                        AddressLine1 = user.AddressLine1,
                        AddressLine2 = user.AddressLine2,
                        City = user.City,
                        State = user.State,
                        ZipCode = user.ZipCode,
                        Country = user.Country,
                        University = user.University,
                        College = user.College,
                        CreatedDate = DateTime.Now
                    });

                    _Context.SaveChanges();

                    MakeDirectory(add.FirstOrDefault(model => model.UserID == currentUser).ID);
                    return RedirectToAction("UserProfile");
                }
            }

            
        }

        public string MakeDirectory(int userID)
        {
            string path = @"C:\Users\ASUS\source\repos\WebApplication1_NoteMarketPlace\WebApplication1_NoteMarketPlace\Members\" + userID;

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return path;
            }
            else
            {
                return null;
            }
        }
    }
}