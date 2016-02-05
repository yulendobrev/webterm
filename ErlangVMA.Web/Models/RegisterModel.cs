using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ErlangVMA
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Name")]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}