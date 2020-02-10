﻿using System.ComponentModel.DataAnnotations;

namespace ByteBank.Forum.ViewModels
{
    public class ContraRegistrarViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}