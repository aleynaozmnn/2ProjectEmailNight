using Project2_EmailNight.Dtos;
using Project2_EmailNight.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2_EmailNight.Services;
using Microsoft.Identity.Client;

namespace Project2_EmailNight.Controllers
{

    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailService _emailService;
        private readonly SignInManager<AppUser> _signInManager;
        public RegisterController(UserManager<AppUser> userManager, EmailService emailService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }
        //asenkron methodlar+dto
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegisterDto userRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return View(userRegisterDto);
            }

            //O e posta nın veri tabanında var olup olmadığının kontrolü için
            var existingUser = await _userManager.FindByEmailAsync(userRegisterDto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
                return View(userRegisterDto);
            }
            Random random = new Random();
            int code = random.Next(100000, 999999);
            AppUser appUser = new AppUser()
            {
                Name = userRegisterDto.Name,
                Surname = userRegisterDto.Surname,
                UserName = userRegisterDto.UserName,
                Email = userRegisterDto.Email,
                ConfirmCode = code.ToString(),
                ImageUrl = "/Templates/images/avatars/default.png"

            };
            var result = await _userManager.CreateAsync(appUser, userRegisterDto.Password);
            if (result.Succeeded)
            {
                _emailService.SendEmail(userRegisterDto.Email, code.ToString());
                return RedirectToAction("ConfirmEmail", new {email=userRegisterDto.Email});

            }
            else
            {
                foreach (var item in result.Errors)
                {
                    if (item.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("", $"'{userRegisterDto.UserName}' kullanıcı adını alamazsınız");
                    }

                    else if (item.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError("", "Şifreniz en az 6 karaktere sahip olmalıdır!");
                    }
                    else if (item.Code == "PasswordRequiresUpper")
                    {
                        ModelState.AddModelError("", "Şifre en az bir büyük harf (A-Z) içermelidir.");
                    }
                    else if (item.Code == "PasswordRequiresLower")
                    {
                        ModelState.AddModelError("", "Şifre en az bir küçük harf (a-z) içermelidir.");
                    }
                    else if (item.Code == "PasswordRequiresDigit")
                    {
                        ModelState.AddModelError("", "Şifrede en az bir rakam (0-9) bulunmalıdır.");
                    }
                    else if (item.Code == "PasswordRequiresNonAlphanumeric")
                    {
                        ModelState.AddModelError("", "Şifrede en az bir sembol (*, !, ? vb.) bulunmalıdır.");
                    }
                    else
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }
            return View(userRegisterDto);



        }

        [HttpGet]
        public IActionResult ConfirmEmail(string email)
        {
            var model = new ConfirmUserDto { Mail = email };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmUserDto confirmUserDto)
        {
            if (string.IsNullOrEmpty(confirmUserDto.Mail))
            {
                ModelState.AddModelError("", "E-posta adresi sistem tarafından alınamadı. Lütfen tekrar deneyin.");
                return View(confirmUserDto);
            }
            var user = await _userManager.FindByEmailAsync(confirmUserDto.Mail);
            if (user != null && user.ConfirmCode == confirmUserDto.ConfirmCode)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // KRİTİK DOKUNUŞ: Artık hata vermeyecek
                await _signInManager.SignInAsync(user, isPersistent: true);

                return RedirectToAction("Index", "Profile");
            }

            ModelState.AddModelError("", "Girdiğiniz onay kodu hatalıdır!");
            return View(confirmUserDto);
        }

    }
}
