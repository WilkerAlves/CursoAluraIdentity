using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        private UserManager<UsuarioAplicacao> _userManager;

        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }

                return _userManager;
            }
            set { _userManager = value; }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContraRegistrarViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var novoUsuario = new UsuarioAplicacao();

                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;

                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                if (usuario !=  null)
                    return RedirectToAction("Index", "Home");

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Password);

                if (resultado.Succeeded)
                    return RedirectToAction("Index", "Home");
                else
                    AdicionarErros(resultado);
            }
            return View(modelo);
        }

        private void AdicionarErros(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error);
        }
    }
}