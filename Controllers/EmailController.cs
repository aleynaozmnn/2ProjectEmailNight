using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Project2_EmailNight.Dtos;

namespace Project2_EmailNight.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult SendEmail(string receiverEmail,string subject,string categoryName)
        {
            var model = new MailRequestDto
            {
                ReceiverEmail=receiverEmail,
                Subject=subject,
            };
            return View();
        }
        [HttpPost]
        public IActionResult SendEmail(MailRequestDto mailRequestDto)
        {
            MimeMessage mimeMessage = new MimeMessage();//Mailkit ile birlikte çalışıyor
            MailboxAddress mailboxAddressFrom = new MailboxAddress("Identity Admin", "aleynaozmenn629@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("user",mailRequestDto.ReceiverEmail);
            mimeMessage.To.Add(mailboxAddressTo);
            
            mimeMessage.Subject=mailRequestDto.Subject;

            /*Önce MimeMessage ile paketi hazırladım, 
             sonra SmtpClient ile Gmail sunucusuna bağlandım, kimliğimi doğruladım,
             paketi fırlattım ve kapıyı kapatıp çıktım. 
             En son da kullanıcıyı listeye yönlendirdim.
             
            */
            var bodyBuilder =new BodyBuilder(); //Bodybuilder:Genellikle meyin tabanlı içeriklerde içeriğin sadece metin kısmını almak için kullanılır
            bodyBuilder.TextBody = mailRequestDto.MessageDetail;
            mimeMessage.Body=bodyBuilder.ToMessageBody();
            
            SmtpClient smtpClient = new SmtpClient();//SmtpClient:
            smtpClient.Connect("smtp.gmail.com",587,false);//587:türkiye port no
            smtpClient.Authenticate("aleynaozmenn629@gmail.com", "n m z u k f k c d r p e p a i n");
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);

            
            
            return RedirectToAction("Inbox","Message");
        }
    }
}
