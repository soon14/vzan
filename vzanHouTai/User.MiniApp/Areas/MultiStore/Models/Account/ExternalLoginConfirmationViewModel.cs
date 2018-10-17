using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.MiniAppAdmin.Areas.MultiStore.Models.Account
{
    /// <summary>
    /// 
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }
}
