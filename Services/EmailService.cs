using MimeKit;
using MailKit.Net.Smtp;


namespace Project2_EmailNight.Services
{
    public class EmailService
    {
        public void SendEmail(string receiverMail,string confirmCode)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress mailboxAddressFrom = new MailboxAddress("Proje onayı","aleynaozmenn629@gmail.com");
            message.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("Kullanıcı",receiverMail);
            message.To.Add(mailboxAddressTo);

            message.Subject = "E posta onay kodunuz";
            var bodyBuilder=new BodyBuilder();

            bodyBuilder.TextBody = "Merhaba,kayıt işlemini tamamlamak için onay kodunuz:" + confirmCode;

            message.Body=bodyBuilder.ToMessageBody();
            using(var client=new SmtpClient())
            {
                client.Connect("smtp.gmail.com",587,false);
                client.Authenticate("aleynaozmenn629@gmail.com", "z g l r h v c l b m v u z f x h");
                client.Send(message);
                client.Disconnect(true);
            
            }
        
        }
    }
}
