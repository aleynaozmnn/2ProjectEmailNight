using Microsoft.AspNetCore.Identity;

namespace Project2_EmailNight.Models
{
    //IdentityErrorDescriber:Hataların otomatik olarak gelmesini sağlayan sınıf
    public class CustomIdentityValidator:IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = "PasswordToShort",
                Description="Parola min 6 karakter olmalıdır!"
            };
        }
        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = "PasswordToShort",
                Description = "Lütfen en az 1 adet küçük harf giriniz!"
            };
        }
        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = "PasswordToShort",
                Description= "Lütfen en az 1 adet büyük harf giriniz!"
            };

        }
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresDigit",
                Description="Lütfen en az 1 adet rakam giriniz!"
            };

        }
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code="PasswordRequiresNonAlphanumeric",
                Description="Lütfen en az 1 adet sembol giriniz!"
            };
            
        }
    }
}
