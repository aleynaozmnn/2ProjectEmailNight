using Project2_EmailNight.Entities;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Project2_EmailNight.Context;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ViewComponents;


namespace Project2_EmailNight.Controllers
{
    public class MessageController : Controller
    {
        private readonly EmailContext _emailContext;
        private readonly UserManager<AppUser> _userManager;//inboxta giriş yapan kullanıcı ile eşleştireceğiz

        public MessageController(EmailContext emailContext, UserManager<AppUser> userManager)
        {
            _emailContext = emailContext;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateMessage(string receiver,string subject,string content,string category)
        {
    
            var categories = _emailContext.Categories.ToList();
            ViewBag.CategoryList = categories;

            if (!string.IsNullOrEmpty(receiver)) ViewBag.PageTitle = "Mesajı Yanıtla";
            else if (!string.IsNullOrEmpty(content)) ViewBag.PageTitle = "Mesajı Yönlendir";
            else ViewBag.PageTitle = "Yeni Mesaj Gönder";

            var model = new Message
            {
                ReceiverEmail = receiver,
                Subject = subject,
                
                MessageDetail = content,
                Category = category?.Trim()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMessage(Message message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (message.MessageId != 0)
                {
                    // 1. Önce veritabanındaki mevcut kaydı bul (ID üzerinden)
                    var existingMessage = await _emailContext.Messages.FindAsync(message.MessageId);

                    if (existingMessage != null)
                    {
                        // 2. Sadece değişen alanları güncelle
                        existingMessage.ReceiverEmail = message.ReceiverEmail;
                        existingMessage.Subject = message.Subject;
                        existingMessage.MessageDetail = message.MessageDetail;
                        existingMessage.Category = message.Category;
                        existingMessage.SendDate = DateTime.Now;
                        existingMessage.IsDraft = false; // Taslaklıktan çıkar
                        existingMessage.IsStatus = false;
                        existingMessage.IsStarred = false;

                        _emailContext.Messages.Update(existingMessage);
                    }
                }
                else
                {
                    // 3. Eğer ID 0 ise tamamen yeni mesajdır, doğrudan ekle
                    message.SenderEmail = user.Email;
                    message.SendDate = DateTime.Now;
                    message.IsDraft = false;
                    _emailContext.Messages.Add(message);
                }

                await _emailContext.SaveChangesAsync(); // Hata burada çözülecek

                // SMTP (Mail Gönderme) kodların buraya gelsin...
                // (Burayı aynen koruyabilirsin)

                return RedirectToAction("SentMessages");
            }
            return View(message);
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            // Sadece bana gelenler
            var messagelist = _emailContext.Messages
                .Where(x => x.ReceiverEmail == user.Email)
                .OrderByDescending(x => x.SendDate) // Yeni mesaj en üstte
                .ToList();
            return View(messagelist);
        }
        public async Task<IActionResult> MessageDetail(int id)
        {
            var message = await _emailContext.Messages.FindAsync(id);
            if (message == null) return RedirectToAction("Inbox");
            message.IsStatus = true;
            message.IsRead = true;

            // Gönderen kullanıcının profil resmini çekmek için
            var sender = await _userManager.FindByEmailAsync(message.SenderEmail);
            ViewBag.SenderImage = sender?.ImageUrl ?? "/UserImages/gorsel.jpg"; // Eğer fotoğraf yoksa varsayılan resim

            

            _emailContext.Update(message);
            await _emailContext.SaveChangesAsync();

            return View(message);
        }
        public async Task<IActionResult> SentMessages()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            // Sadece benim gönderdiklerim
            var values = _emailContext.Messages
                .Where(x => x.SenderEmail == user.Email)
                .OrderByDescending(x => x.SendDate)
                .ToList();
            return View(values);
        }
        public async Task<IActionResult> ChangeStarStatus(int id)
        {
            var message = await _emailContext.Messages.FindAsync(id);
            if (message != null)
            {
                message.IsStarred = !message.IsStarred;
                _emailContext.Update(message);
                await _emailContext.SaveChangesAsync();
            }

            // Kullanıcı hangi sayfada (Inbox, Sent, Starred) yıldıza bastıysa oraya döner
            string returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Inbox");
        }
        public async Task<IActionResult> StarredMessages()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

           
            // Şimdi: Alıcısı BEN OLAN VEYA Göndereni BEN OLAN mesajlardan yıldızlıları getir diyoruz.
            var values = _emailContext.Messages
                .Where(x => (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email) && x.IsStarred == true)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(values);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SaveDraft(Message message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // 1. Yeni kayıt olduğu için ID'yi sıfırlıyoruz (Identity hatasını önler)
                message.MessageId = 0;

                message.SenderEmail = user.Email;
                message.SendDate = DateTime.Now;
                message.IsDraft = true;
                message.IsStatus = false;
                message.IsRead = false;
                message.IsStarred = false;

                // 2. En kritik nokta: Eğer içerik boşsa veritabanı hata vermesin diye boş string atıyoruz
                if (string.IsNullOrEmpty(message.MessageDetail))
                {
                    message.MessageDetail = " "; // Veya ""
                }

                _emailContext.Messages.Add(message);
                await _emailContext.SaveChangesAsync();

                return RedirectToAction("DraftMessage");
            }
            return View("CreateMessage", message);
        }

        public async Task<IActionResult>DraftMessage()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var values = _emailContext.Messages.Where(x => x.SenderEmail == user.Email && x.IsDraft == true).OrderByDescending(x => x.SendDate).ToList();
            return View(values);

        }
        public async Task<IActionResult>EditDRaft(int id)
        {
            var message = await _emailContext.Messages.FindAsync(id);
            if(message==null || !message.IsDraft)
            {
                return RedirectToAction("DraftMessage");

            }
            //Şimdi kategorilerimizi geri yükleyelim
            ViewBag.CategoryList = _emailContext.Categories.ToList();
            ViewBag.PageTitle = "Taslağı Düzenle";
            return View("CreateMessage",message);
        }
        public IActionResult DeleteMessage(int id)
        {
            var value = _emailContext.Messages.Find(id);
            if(value!=null)
            {
                _emailContext.Remove(value);
                _emailContext.SaveChanges();
            }

            string referer = Request.Headers["Referer"].ToString();//Kullanıcı geldiği sayfaya geri yolla
            if(!string.IsNullOrEmpty(referer))
            {
                return Redirect (referer);
            }
            return RedirectToAction("Inbox");
        }
    }

    }

