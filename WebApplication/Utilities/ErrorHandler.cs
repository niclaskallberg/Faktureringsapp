using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;

namespace WebApplication.Utilities
{
    public class ErrorHandler
    {
        public enum ErrorTypes
        {
            RetrieveFailed,
            DeleteFailed,
            InvoiceCreationFailed
        }


        public static string GetErrorMessage(ErrorTypes error, Exception ex)
        {

            Dictionary<ErrorTypes, string> ErrorMessages = new Dictionary<ErrorTypes, string>()
            {
                { ErrorTypes.RetrieveFailed, "Kunde ej ansluta till kundregistret!" },
                { ErrorTypes.DeleteFailed, "Något gick fel vid radering!" },
                { ErrorTypes.InvoiceCreationFailed, "Kunde ej ansluta till kundregistret!\nInformationen har inte sparats." }
            };

            string errorMessage = ErrorMessages.TryGetValue(error, out string message) ? message : "Ett okänt fel uppstod!";

            if (ex != null)
                errorMessage += $"\n\nFelmeddelande:\n\n{ex.Message}";



            return errorMessage;
        }





        //_fileLock används för att 2 personer ska kunna öppna logg-filen samma millisekund
        private static readonly object _fileLock = new object();

        private static DateTime _lastEmailSent = DateTime.MinValue;



        public static void HandleError(ErrorTypes errorType, Exception ex)
        {

            JsHelpers.ShowAlert(GetErrorMessage(errorType, ex));




            void LogToFile(Exception secondEx)
            {
                if (Default.canLogErrors != 1) return;


                try
                {
                    lock (_fileLock)
                    {

                        string logEntry = $"--------------------------------------------------{Environment.NewLine}" +
                                            $"Date: {DateTime.Now}{Environment.NewLine}" +
                                            $"Message: {secondEx.Message}{Environment.NewLine}" +
                                            $"Stack Trace: {secondEx.StackTrace}{Environment.NewLine}" +
                                            $"--------------------------------------------------{Environment.NewLine}";


                        File.AppendAllText(HostingEnvironment.MapPath("~/error_log.txt"), logEntry);
                    }
                }
                catch
                {
                    // Fail silently so a logging failure doesn't crash the entire website
                }
            }

            LogToFile(ex);






            if (Default.canEmailErrors != 1) return;


            // Set your cooldown
            TimeSpan cooldown = TimeSpan.FromMinutes(60);


            //Om tiden just nu minus tid när förra e-posten skickades är mindre än cooldown, stanna här
            if (DateTime.Now - _lastEmailSent < cooldown) return;



            try
            {
                MailAddress fromAddress = new MailAddress("", "Allservice Error Bot");
                MailAddress toAddress = new MailAddress("");
                const string fromPassword = "";
                string subject = "Critical App Error Alert";
                string body = $"Time: {DateTime.Now}\nMessage: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

                SmtpClient smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (MailMessage mailMessage = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                {
                    smtpClient.Send(mailMessage);
                }

                // Update the timestamp ONLY after a successful send
                _lastEmailSent = DateTime.Now;



            }
            catch (Exception emailException)
            {
                // Om e-posten inte skickas, logga misslyckandet att skicka e-post
                LogToFile(emailException);
            }
        }
    }
}
