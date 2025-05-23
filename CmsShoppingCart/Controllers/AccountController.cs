using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CmsShoppingCart.Services;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;

namespace CmsShoppingCart.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IPasswordHasher<AppUser> passwordHasher;
        private readonly IEmailSender emailSender; // Agregar un servicio para enviar correos electrónicos

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 IPasswordHasher<AppUser> passwordHasher,
                                 IEmailSender emailSender) // Inyectar el servicio de envío de correos
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.passwordHasher = passwordHasher;
            this.emailSender = emailSender; // Asignar el servicio de correos
        }

        //GET /account/register
        [AllowAnonymous]
        public IActionResult Register() => View();

        //POST /Account/register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser
                {
                    UserName = user.Nombre,
                    Email = user.Correo
                };

                IdentityResult result = await userManager.CreateAsync(appUser, user.Contraseña);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(user);
        }

        //GET /Account/login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            Login login = new Login
            {
                ReturnUrl = returnUrl
            };

            return View(login);
        }

        // POST /account/login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (ModelState.IsValid)
            {
                // Buscar usuario por correo
                AppUser appUser = await userManager.FindByEmailAsync(login.Correo);
                if (appUser != null)
                {
                    // Verificar contraseña
                    var result = await signInManager.PasswordSignInAsync(appUser, login.Contraseña, false, false);

                    if (result.Succeeded)
                    {
                        // Obtener roles
                        var roles = await userManager.GetRolesAsync(appUser);

                        // Crear lista de claims
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, appUser.Email), // Usamos el correo como "Name"
                    new Claim(ClaimTypes.Email, appUser.Email)
                };

                        // Agregar roles a los claims
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        // Crear la identidad y ClaimsPrincipal
                        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        // Autenticar
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                        // Redirigir según rol
                        if (roles.Contains("Cliente"))
                        {
                            return RedirectToAction("Page", "Pages", new { slug = "home" });
                        }
                        else
                        {
                            return RedirectToAction("Page", "Pages", new { slug = "home" });
                            // Default
                        }
                    }
                }

                ModelState.AddModelError("", "Inicio de sesión fallido, correo o contraseña incorrectas.");
            }

            return View(login);
        }

        //GET /account/logout
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return Redirect("/");
        }

        //GET /account/edit
        public async Task<IActionResult> Edit()
        {
            // Buscar el usuario por el correo (User.Identity.Name ahora contiene el correo)
            AppUser appUser = await userManager.FindByEmailAsync(User.Identity.Name);

            // Verificar si el usuario existe
            if (appUser == null)
            {
                // Si el usuario no se encuentra, redirigir a la página de inicio de sesión
                return RedirectToAction("Login", "Account"); // O cualquier otra acción de error
            }

            // Crear el objeto UserEdit con los datos del usuario
            UserEdit user = new UserEdit(appUser);

            return View(user); // Pasar el modelo a la vista
        }

        //POST /Account/edit
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEdit user)
        {
            AppUser appUser = await userManager.FindByEmailAsync(User.Identity.Name);
            if (ModelState.IsValid)
            {
                appUser.Email = user.Correo;
                if (user.Contraseña != null)
                {
                    appUser.PasswordHash = passwordHasher.HashPassword(appUser, user.Contraseña);
                }

                IdentityResult result = await userManager.UpdateAsync(appUser);
                if (result.Succeeded)
                    TempData["Success"] = "Tu información ha sido editada!";
               
            }
            return View();
        }

        // Método temporal para generar el hash de una contraseña
        [AllowAnonymous]
        public IActionResult GeneratePasswordHash(string password)
        {
            var appUser = new AppUser(); // Crear una instancia vacía de AppUser
            string hashedPassword = passwordHasher.HashPassword(appUser, password); // Generar el hash

            // Mostrar el hash en pantalla
            return Content($"Hash generado para '{password}': {hashedPassword}");
        }

        // Acción para mostrar la página de Olvidaste tu Contraseña (GET)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Acción para manejar el formulario de Olvidaste tu Contraseña (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "El correo es obligatorio.");
                return View();
            }

            // Buscar al usuario con el correo proporcionado
            AppUser user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "No se ha encontrado una cuenta con ese correo.");
                return View();
            }

            // Generar un token para restablecer la contraseña
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            // Crear el enlace para la recuperación
            var resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = user.Email }, Request.Scheme);

            // Enviar el correo con el enlace de recuperación
            await emailSender.SendEmailAsync(user.Email, "Recuperación de Contraseña", $"Haz clic en el siguiente enlace para restablecer tu contraseña: <a href=\"{resetLink}\">Restablecer Contraseña</a>");

            TempData["SuccessMessage"] = "Te hemos enviado un correo con instrucciones para recuperar tu contraseña.";
            return RedirectToAction("Login");
        }

        // Acción para mostrar el formulario de Restablecer Contraseña (GET)
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            // Validar que el token y el correo no sean nulos o vacíos
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("El token o el correo electrónico no son válidos.");
            }

            // Pasar los valores al modelo
            var model = new ResetPasswordModel
            {
                Token = WebUtility.UrlDecode(token), // Decodificar el token en caso de ser necesario
                Email = email
            };
            return View(model);
        }

        // Acción para manejar el formulario de Restablecer Contraseña (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            // Validar si el modelo es válido
            if (!ModelState.IsValid)
            {
                // Opción para depurar y verificar el token y el email
                var token = model.Token;
                var email = model.Email;
                return View(model);
            }

            // Buscar el usuario por email
            AppUser user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No se encontró el usuario.");
                return View(model);
            }

            // Intentar restablecer la contraseña usando el token
            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            // Verificar el resultado de la operación
            if (result.Succeeded)
            {
                // Cerrar la sesión del usuario después de restablecer la contraseña
                await signInManager.SignOutAsync();

                // Mostrar mensaje de éxito y redirigir al login
                TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida con éxito. Por favor, inicia sesión con la nueva contraseña.";
                return RedirectToAction("Login");
            }

            // Agregar errores al modelo si el restablecimiento falla
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View(); // Devuelve la vista AccessDenied.cshtml
        }
    }
}
