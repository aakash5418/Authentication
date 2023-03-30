using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Authorication
{
    public class LoginModel
    {
        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required")]

        public string Password { get; set; }
    }
}
