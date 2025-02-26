using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Nettside.Controllers;
using Nettside.Models;
using Nettside.Models.ViewModel;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Encodings.Web;


namespace Nettside.Controllers
{
    /// <summary>
    /// Controller to handle account-related actions like registration, login and profile management. 
    /// </summary>

    [Authorize] // requires authentication for all actions by default unless otherwise specified 
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager; // service provided by asp.net core identity
        private readonly UserManager<Users> userManager;    // service provided by asp.net core identity
        private readonly IEmailSender _emailSender;



        /// <summary>
        /// Constructor to initialize SignInManager and UserManager
        /// </summary>
        /// <param name="signInManager">a service to manage user sign-in operations</param>
        /// <param name="userManager">a service to manage user interactions</param>
        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, IEmailSender emailSender) // constructor to inject signInManager and UserManager
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _emailSender = emailSender;
        }


        // Displays the registration page
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }



        /// <summary> 
        /// Handles user registration form submission and creates a new user account
        /// </summary>
        /// <param name="registerViewModel">the user registration details</param>
        /// <returns>redirects to login on success or reloads the registration page on failure</returns>
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel); // Returner skjemaet med feilmeldinger
            }

            try
            {
                var createUser = new Users
                {
                    FirstName = registerViewModel.FirstName,
                    LastName = registerViewModel.LastName,
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.Username
                };

                var applicationResult = await userManager.CreateAsync(createUser, registerViewModel.Password);

                if (applicationResult.Succeeded)
                {
                    var applicationIdentityResult = await userManager.AddToRoleAsync(createUser, "PrivateUser");

                    if (applicationIdentityResult.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                }

                foreach (var error in applicationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                // Vis en generell feilmelding til brukeren
                ModelState.AddModelError(string.Empty, "En uventet feil oppstod. Vennligst prøv igjen.");
            }

            return View(registerViewModel);
        }



        // displays the registration page for a caseworker.
        [HttpGet]
        public async Task<ActionResult> RegisterCaseWorker()
        {

            return View();
        }


        /// <summary>
        /// handles caseworker registration form submission. 
        /// </summary>
        /// <param name="registerViewModel">the caseworker registration details.</param>
        /// <returns>redirects to login on success or reloads the registration page on failure </returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> RegisterCaseworker(RegisterViewModel registerViewModel)
        {
            var createUser = new Users
            {
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Email = registerViewModel.Email,
                UserName = registerViewModel.Username
            };

            // Attempt to create the user
            var applicationResult = await userManager.CreateAsync(createUser, registerViewModel.Password);

            if (applicationResult.Succeeded)
            {
                // Add the user to the "Caseworker" role
                var applicationIdentityResult = await userManager.AddToRoleAsync(createUser, "Caseworker");

                if (applicationIdentityResult.Succeeded)
                {
                    // Redirect to Login if successful
                    return RedirectToAction("Login");
                }
            }

            // If any of the operations fail, return the Register view with errors
            foreach (var error in applicationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registerViewModel);
        }









        // Displays the login page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }


        /// <summary>
        /// handles login form submission and authenticates the user
        /// </summary>
        /// <param name="loginViewmodel">the login details</param>
        /// <returns>redirects to home on success or reloads the login page on failure.</returns>
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewmodel)
        {
            if (!ModelState.IsValid)
            {

                return View();
            }


            var signInresult = await signInManager.PasswordSignInAsync(loginViewmodel.UserName, loginViewmodel.Password, false, false);

            // attempt to log in the user


            if (signInresult != null && signInresult.Succeeded)
            {

                return RedirectToAction("RegisterAreaChange", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Email or password is incorrect.");

                return View(loginViewmodel);
            }


        }



        // displays the employee or map user selection page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmployeeOrMapUser()
        {
            return View();
        }



        /// <summary>
        /// assigns a role to the logged-in user based on the selected option.
        /// </summary>
        /// <param name="userRole">the selected role.</param>
        /// <returns>redirects to the role selection page if the role is invalid or assigns the role successfully.</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EmployeeOrMapUser(string userRole)
        {
            // checks if the role is valid
            if (string.IsNullOrEmpty(userRole))
            {
                // handle invalid input
                return RedirectToAction("EmployeeOrMapUser");
            }

            // get the current logged-in user
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // add role to the user
            var result = await userManager.AddToRoleAsync(user, userRole);



            return View();
        }


      


        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Dont reveal that the user does not exist
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            Console.WriteLine("Reset Password URL: " + callBackUrl);

            await _emailSender.SendEmailAsync(model.Email, "Reset password",
                $"Click <a href='{(callBackUrl)}'>here</a> to reset your password.");


            return RedirectToAction("ForgotPasswordConfirmation");
        }





        // Confirmation page after sending email

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest("Invalid password reset request");
            }

                return View(new ResetPasswordViewModel { Token = token, Email = email });

            }

           
        



        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

              
            

          





      [AllowAnonymous]
      public IActionResult ResetPasswordConfirmation()
       {
          return View();
       }



        

       // only authenticated users can change their password
       [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            return View();
        }





        /// <summary>
        /// displays the changepassword page
        /// </summary>
        /// <param name="model">the username associated with the account.</param>
        /// <returns></returns>

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }



            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                return RedirectToAction("ProfilePage", new { Message = "Your password has been changed." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        // displays the AccessDenied page for unauthorized users.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // Logs the user out and redirects to the home page
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
