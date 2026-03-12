using Project2_EmailNight.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2_EmailNight.Entities;
using Project2_EmailNight.Context;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace Project2_EmailNight.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailContext _context;

        public ProfileController(UserManager<AppUser> userManager, EmailContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 81 İl Listesi (Merkezi Tanım)
        private List<string> CityList => new List<string> {
            "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
            "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
            "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari",
            "Hatay", "Isparta", "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
            "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
            "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
            "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
            "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
        };

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "Account");

            var values = await _userManager.FindByNameAsync(userName);
            if (values == null) return RedirectToAction("Login", "Account");

            // --- İstatistikler ve Şehir Listesi ---
            ViewBag.InboundCount = _context.Messages.Count(x => x.ReceiverEmail == values.Email);
            ViewBag.SentCount = _context.Messages.Count(x => x.SenderEmail == values.Email && x.IsDraft == false);
            ViewBag.DraftCount = _context.Messages.Count(x => x.SenderEmail == values.Email && x.IsDraft == true);
            ViewBag.Cities = CityList;

            // --- Dinamik Profil Doluluk Hesaplama ---
            double totalFields = 7.0;
            double filledFields = 0.0;

            if (!string.IsNullOrEmpty(values.Name)) filledFields++;
            if (!string.IsNullOrEmpty(values.Surname)) filledFields++;
            if (!string.IsNullOrEmpty(values.Email)) filledFields++;
            if (!string.IsNullOrEmpty(values.PhoneNumber)) filledFields++;
            if (!string.IsNullOrEmpty(values.City)) filledFields++;
            if (!string.IsNullOrEmpty(values.ImageUrl)) filledFields++;
            if (!string.IsNullOrEmpty(values.Description)) filledFields++;

            ViewBag.ProfileProgress = Math.Round((filledFields / totalFields) * 100);

            // --- DTO Eşleme ---
            UserEditDto userEditDto = new UserEditDto()
            {
                Name = values.Name,
                Surname = values.Surname,
                ImageUrl = values.ImageUrl,
                Email = values.Email,
                Description = values.Description,
                PhoneNumber = values.PhoneNumber,
                City = values.City
            };

            return View(userEditDto);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserEditDto userEditDto)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return RedirectToAction("Login", "Account");

            // --- Veritabanı Güncelleme ---
            user.Name = userEditDto.Name;
            user.Surname = userEditDto.Surname;
            user.Description = userEditDto.Description;
            user.PhoneNumber = userEditDto.PhoneNumber;
            user.City = userEditDto.City;

            // Şifre Değişikliği
            if (!string.IsNullOrEmpty(userEditDto.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userEditDto.Password);
            }

            // Fotoğraf Yükleme Mantığı
            if (userEditDto.Image != null)
            {
                var resource = Directory.GetCurrentDirectory();
                var extension = Path.GetExtension(userEditDto.Image.FileName);
                var imageName = Guid.NewGuid() + extension;
                var saveLocation = Path.Combine(resource, "wwwroot/UserImages", imageName);

                if (!Directory.Exists(Path.Combine(resource, "wwwroot/UserImages")))
                    Directory.CreateDirectory(Path.Combine(resource, "wwwroot/UserImages"));

                using (var stream = new FileStream(saveLocation, FileMode.Create))
                {
                    await userEditDto.Image.CopyToAsync(stream);
                }
                user.ImageUrl = "/UserImages/" + imageName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellenmiştir.";
                return RedirectToAction("Index");
            }

            // --- HATA DURUMU: Sayfa tekrar yüklenirse verilerin kaybolmaması için ---
            ViewBag.Cities = CityList;
            ViewBag.InboundCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email);
            ViewBag.SentCount = _context.Messages.Count(x => x.SenderEmail == user.Email && x.IsDraft == false);
            ViewBag.DraftCount = _context.Messages.Count(x => x.SenderEmail == user.Email && x.IsDraft == true);

            // Doluluk oranını hata durumunda da tekrar hesapla
            double totalFields = 7;
            double filledFields = 0;
            if (!string.IsNullOrEmpty(user.Name)) filledFields++;
            if (!string.IsNullOrEmpty(user.Surname)) filledFields++;
            if (!string.IsNullOrEmpty(user.Email)) filledFields++;
            if (!string.IsNullOrEmpty(user.PhoneNumber)) filledFields++;
            if (!string.IsNullOrEmpty(user.City)) filledFields++;
            if (!string.IsNullOrEmpty(user.ImageUrl)) filledFields++;
            if (!string.IsNullOrEmpty(user.Description)) filledFields++;
            ViewBag.ProfileProgress = Math.Round((filledFields / totalFields) * 100);

            return View(userEditDto);
        }

        public async Task<IActionResult> Dashboard()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return RedirectToAction("Login", "Account");

            var userEmail = user.Email;

            // 1. Üstteki 4'lü Ana Kartlar İçin Veriler
            ViewBag.InboundCount = _context.Messages.Count(x => x.ReceiverEmail == userEmail);
            ViewBag.SentCount = _context.Messages.Count(x => x.SenderEmail == userEmail && !x.IsDraft);
            ViewBag.DraftCount = _context.Messages.Count(x => x.SenderEmail == userEmail && x.IsDraft);
            ViewBag.StarredCount = _context.Messages.Count(x => (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail) && x.IsStarred);

            // 2. Kategori Kartları ve Halka Grafik İçin Veriler (EKSİK OLAN KISIM)
            ViewBag.WorkCount = _context.Messages.Count(x => x.Category == "İş" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));
            ViewBag.EducationCount = _context.Messages.Count(x => x.Category == "Eğitim" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));
            ViewBag.SocialCount = _context.Messages.Count(x => x.Category == "Sosyal" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));
            ViewBag.FinanceCount = _context.Messages.Count(x => x.Category == "Finans" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));
            ViewBag.ImportantCount = _context.Messages.Count(x => x.Category == "Önemli" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));
            ViewBag.OtherCount = _context.Messages.Count(x => x.Category == "Diğer" && (x.ReceiverEmail == userEmail || x.SenderEmail == userEmail));

            // 3. Haftalık Mesaj Trafiği (Çizgi Grafik) İçin Veriler
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Now.Date.AddDays(-i)).OrderBy(d => d).ToList();
            var counts = new List<int>();
            foreach (var day in last7Days)
            {
                var nextDay = day.AddDays(1);
                counts.Add(_context.Messages.Count(m => m.SendDate >= day && m.SendDate < nextDay && (m.ReceiverEmail == userEmail || m.SenderEmail == userEmail)));
            }
            ViewBag.ChartData = string.Join(",", counts);
            ViewBag.ChartLabels = string.Join(",", last7Days.Select(d => $"'{d.ToString("dd MMM")}'"));

            // Profil Bilgileri
            UserEditDto userEditDto = new UserEditDto()
            {
                Name = user.Name,
                Surname = user.Surname,
                ImageUrl = user.ImageUrl,
                Email = user.Email
            };

            return View(userEditDto);
        }
    }
}