using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using WebApi.Handlers;
using WebApi.Models;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace WebApi.Controllers
{
    [RoutePrefix("api/order")]
    public class OrderController : ApiController
    {
        private readonly string filePath = Path.Combine(HostingEnvironment.MapPath("~/App_Data/Orders"));
        private readonly string url = "https://localhost:44368/api/order/download-pdf/";

        [Route("add")]
        [HttpPost]
        public async Task<JsonResponse<string>> CreateOrder([FromBody] Order order)
        {
            try
            {
                order.Id = Guid.NewGuid().ToString();
                GenerateOrderPdf(order, filePath);

                var mailMessage = CreateEmailMessage(order);
                await SendEmailAsync(mailMessage);

                return new JsonResponse<string> { data = "Order# " + order.Id + " created and email sent successfully.", success = true, message = "" };
            }
            catch (Exception ex)
            {
                // Handle exceptions and return an error response
                return new JsonResponse<string> { data = "Something went wrong! Please try later.", success = false, message = ex.Message };
            }
        }

        [Route("download-pdf/{id}")]
        [HttpGet]
        public IHttpActionResult DownloadOrderPdf(string id)
        {
            var file = Path.Combine(filePath, id + ".pdf");
            if (File.Exists(file))
            {
                var fileBytes = File.ReadAllBytes(file);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(fileBytes)
                };
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "order.pdf" // Change the filename as needed
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                return ResponseMessage(response);
            }
            else
            {
                return NotFound();
            }
        }


        /* Private Methods */

        // Method to create the email message
        private MailMessage CreateEmailMessage(Order order)
        {
            order.Url = url;

            if (true)
            {
                order.Url = order.Url + order.Id;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress("guest.cloudscourt@gmail.com", "Infobip-Testing"),
                Subject = "New Order",
                Body = "",
                IsBodyHtml = true,
            };

            // You can add an HTML part with an iframe to display the PDF link if desired.
            string htmlBody = $@"<html>
        <body>
            <p>Dear User,</p>
            <p>A new order {order.Id} has been placed.</p>
            <p>{order.Message}</p>
            <p>PDF URL: <a href='{order.Url}' target='_blank'>Open PDF</a></p>
            <p><iframe src='{order.Url}' width='800' height='600'></iframe></p>
            <p><i>*Note: This is an auto-generated email, please do not reply.</i></p>
        </body>
    </html>";

            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlBody;

            mailMessage.To.Add(order.UserEmail); // Replace with the recipient's email address

            return mailMessage;
        }

        // Method to send the email
        private async Task SendEmailAsync(MailMessage mailMessage)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("guest.cloudscourt@gmail.com", "---"), // Replace with your Gmail credentials
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = "smtp.gmail.com",
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        // Function to generate and save the PDF
        private void GenerateOrderPdf(Order order, string filePath)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);

            // Create a graphical context for drawing on the page
            XTextFormatter tf = new XTextFormatter(gfx);

            tf.DrawString("Order Details", font, XBrushes.Black, new XRect(100, 100, page.Width, page.Height), XStringFormats.TopLeft);

            // Add order details to the PDF
            tf.DrawString($"Order ID: {order.Id}", font, XBrushes.Black, new XRect(100, 140, page.Width, page.Height), XStringFormats.TopLeft);
            tf.DrawString($"User Email: {order.UserEmail}", font, XBrushes.Black, new XRect(100, 160, page.Width, page.Height), XStringFormats.TopLeft);
            // Add other order details as needed

            var file = Path.Combine(filePath, order.Id + ".pdf");
            if (!File.Exists(file))
            {
                // Get the directory path from the file path
                var directoryPath = Path.GetDirectoryName(file);

                // Check if the directory doesn't exist, then create it
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }

            // Save the PDF to the specified file path
            document.Save(file);
        }

    }
}
