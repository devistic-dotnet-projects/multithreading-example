using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace EmailTester
{
    class Program
    {
        public static string NetworkEmail = "guest.cloudscourt@gmail.com";
        public static string NetworkPassword = "---";
        public static string FromEmail = "guest.cloudscourt@gmail.com";
        public static string DisplayName = "Serwvz";
        public static int Port = 587;
        public static string Host = "smtp.gmail.com";
        public static bool EnableSsl = true;
        public string Company { get; set; } = "Company Name And Address";
        public string PoweredBy { get; set; } = "Company Name";
        public string PoweredByUrl { get; set; } = "https://Google.com";
        public static string Logo = "https://waxyellowpages.com/frontend/images/logo.png";
        public static string recipientEmail = "trweszcrpo@pretreer.com";

        public static bool SendMail(string email, string subject, string body)
        {
            try
            {
                var senderEmail = new MailAddress(NetworkEmail, "Serwvz");
                var receiverEmail = new MailAddress(email, "Receiver");
                var password = NetworkPassword;
                var sub = subject;
                var smtp = new SmtpClient
                {
                    Host = Host,
                    Port = Port,
                    EnableSsl = EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    From = new MailAddress(FromEmail, DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(mess);
                }
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        static void Main(string[] args)
        {
            // Replace these values with your actual data
            string subject = "New Order";
            string body = CreateEmailBody();

            bool isEmailSent = SendMail(recipientEmail, subject, body);

            if (isEmailSent)
            {
                Console.WriteLine("Email sent successfully.");
            }
            else
            {
                Console.WriteLine("Failed to send the email. Please check your SMTP settings.");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static string CreateEmailBody()
        {
            var guid = Guid.NewGuid();
            Directory.GetCurrentDirectory();

            // Get the path to the Wellcome.html file in the project directory
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(projectDirectory, "Wellcome.html");

            string templatePath = filePath.Replace("\\bin\\Debug", ""); // Replace with the actual path to your email template
            string body = string.Empty;

            using (StreamReader reader = new StreamReader(templatePath))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{Logo}", Logo);
            body = body.Replace("{Guid}", guid.ToString());
            body = body.Replace("{Url}", "Link to your PDF or order details page");
            body = body.Replace("{OrderContent}", "Details of the order");
            body = body.Replace("{Note}", "This is an auto-generated email, please do not reply.");

            return body;
        }


    }
}
