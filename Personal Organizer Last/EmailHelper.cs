using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public static class EmailHelper
{
    public static async Task SendNewPasswordAsync(string toEmail, string newPassword)
    {
        
        string senderEmail = "mervetaskiran777@gmail.com";
        string appPassword = "uvfbrflstvorhcwu";
       

        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail, "Destek Ekibi"),
            Subject = "Yeni Şifreniz",
            Body = $@"Merhaba,

                Talebiniz üzerine şifreniz sıfırlandı. Yeni şifreniz:

                    {newPassword}

                Lütfen giriş yaptıktan sonra şifrenizi değiştirin.

                İyi çalışmalar."
        };
        mail.To.Add(toEmail);

        using (var smtp = new SmtpClient("smtp.gmail.com", 587))
        {
            smtp.UseDefaultCredentials = false;                        // çok önemli
            smtp.Credentials = new NetworkCredential(
                                           senderEmail,
                                           appPassword);
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Timeout = 10000;                        // 10s timeout
                                                         // TLS 1.2 zorunlu ise:
            System.Net.ServicePointManager
                 .SecurityProtocol = SecurityProtocolType.Tls12;

            await smtp.SendMailAsync(mail);
        }
    }
}
