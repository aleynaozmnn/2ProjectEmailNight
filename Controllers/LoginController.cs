using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2_EmailNight.Dtos;
using Project2_EmailNight.Entities;

namespace Project2_EmailNight.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }
        [HttpPost]

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginDto userLoginDto)
        {
            var user = await _userManager.FindByNameAsync(userLoginDto.UserName);

            if (user != null)
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

                if (passwordCheck)
                {
                    if (!user.EmailConfirmed)
                    {
                        // RegisterController'daki ConfirmEmail mail beklediği için 'mail' olarak gönderdik
                        return RedirectToAction("ConfirmEmail", "Register", new { mail = user.Email });
                    }

                    var result = await _signInManager.PasswordSignInAsync(userLoginDto.UserName, userLoginDto.Password, true, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Profile");
                    }
                }
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View();
        }
    }
}
