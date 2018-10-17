using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.MultiStore.Models.Manage
{
    /// <summary>
    /// 
    /// </summary>
    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "代码")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "电话号码")]
        public string PhoneNumber { get; set; }
    }
}