using System.ComponentModel.DataAnnotations;

namespace Project2_EmailNight.Dtos
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Ad alanı boş geçilemez.")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Soyad alanı boş geçilemez.")]
        public String Surname { get; set; }
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public String Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı boş geçilemez.")]
        public String Password { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı alanı boş geçilemez.")]
        public String UserName { get; set; }
    }
}
