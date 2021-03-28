using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1_NoteMarketPlace.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email Id is required.")]
        public string EmailID { get; set; }
    }

    public class ChangePassword
    {
        [Required(ErrorMessage = "Old Password is required.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password should be same.")]
        public string ConfirmPassword { get; set; }
    }
}