using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebApplication1_NoteMarketPlace.DBModel;

namespace WebApplication1_NoteMarketPlace.Models
{
    public class UserProfileModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailID { get; set; }

        public Nullable<DateTime> DOB { get; set; }

        public Nullable<int> Gender { get; set; }

        public string PhoneNumber_CountryCode { get; set; }

        public string PhoneNumber { get; set; }

        public string ProfilePicture { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public int Country { get; set; }

        public string University { get; set; }

        public string College { get; set; }

        public List<GenderModel> genderModel { get; set; }

        public List<CountryModel> countryModel { get; set; }

        public List<CountryModel> CountryCodeModel { get; set; }

        public void MaptoModel(tblUser user, tblUserProfile details)
        {
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.EmailID = EmailID;
            details.DOB = DOB;
            details.Gender = Gender;
            details.PhoneNumber_CountryCode = PhoneNumber_CountryCode;
            details.PhoneNumber = PhoneNumber;
            details.ProfilePicture = ProfilePicture;
            details.AddressLine1 = AddressLine1;
            details.AddressLine2 = AddressLine2;
            details.City = City;
            details.State = State;
            details.ZipCode = ZipCode;
            details.Country = Country;
            details.College = College;
            details.University = University;
        }
    }

    public class GenderModel
    {
        public int Gender_Id { get; set; }
        public string Gender_Val { get; set; }
    }

    public class CountryModel
    {
        public int Country_Id { get; set; }
        public string Country_Val { get; set; }
        public string Country_Code { get; set; }
    }
}