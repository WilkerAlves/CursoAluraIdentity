﻿using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

        private SignInManager<UsuarioAplicacao, string> _signInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _signInManager = contextOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }

                return _signInManager;
            }
            set { _signInManager = value; }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
            }
        }

        [HttpGet]
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

                if (usuario != null)
                    return View("AguardandoConfirmacao");

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Password);

                if (resultado.Succeeded)
                {
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    AdicionarErros(resultado);
                }

            }
            return View(modelo);
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {
            if (usuarioId == null || token == null)
                return View("Error");

            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");

        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                if(usuario is null)
                    return SenhaOuUsuarioInvalidos();
                
                var signInResultado = await SignInManager.PasswordSignInAsync(
                    usuario.UserName,
                    modelo.Password,
                    isPersistent: modelo.ContinuarLogado,
                    shouldLockout: true
                );

                switch (signInResultado)
                {
                    case SignInStatus.Success:
                        if (!usuario.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("AguardandoConfirmacao");
                        }
                        return RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        var senhaCorreta = await UserManager.CheckPasswordAsync(usuario, modelo.Password);
                        
                        if (senhaCorreta)
                            ModelState.AddModelError("", "A conta está bloqueada");
                        else
                            return SenhaOuUsuarioInvalidos();
                    
                        break;
                    default:
                        return SenhaOuUsuarioInvalidos();
                }
            
            }
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Logoff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EsqueciSenha()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                if (usuario != null)
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);
                    
                    var linkDeCallBack = Url.Action(
                        "ConfirmacaoAlteracaoSenha",
                        "Conta",
                        new { usuarioId = usuario.Id, token = token },
                        Request.Url.Scheme
                    );

                    await UserManager.SendEmailAsync(
                        usuario.Id,
                        "Forum ByteBank - Alteração de Senha",
                        $"Clique aqui {linkDeCallBack}, para alterar a sua senha"
                    );
                }

                return View("EmailAlteracaoSenhaEnviado");
            }
            return View();
        }

        
        public ActionResult ConfirmacaoAlteracaoSenha(string usuarioId, string token)
        {
            var modelo = new ConfirmacaoAlteracaoSenhaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ConfirmacaoAlteracaoSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var resultadoAlteracao = await UserManager.ResetPasswordAsync(modelo.UsuarioId, modelo.Token, modelo.NovoPassword);

                if (resultadoAlteracao.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                AdicionarErros(resultadoAlteracao);
            }

            return View();
        }

        private void AdicionarErros(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkDeCallBack = Url.Action(
                "ConfirmacaoEmail",
                "Conta",
                new { usuarioId = usuario.Id, token = token },
                Request.Url.Scheme
            );

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Forum ByteBank - Confirmacao e-mail",
                $"Bem vindo ao forum ByteBank, use o codigo {linkDeCallBack}, para confirmar o seu endereço de e-mail"
            );
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "credenciais invalidas");
            return View("Login");
        }
    }
}