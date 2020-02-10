using System.ComponentModel.DataAnnotations;

namespace ByteBank.Forum.ViewModels
{
    public class ContaLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Continuar Logado")]
        public bool ContinuarLogado { get; set; }
    }
}