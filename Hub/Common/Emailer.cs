using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Common
{
    public class Emailer
    {
        private string smtpServer;
        private string smtpUser;
        private string smtpPassword;

        public Emailer(string smtpServer, string smtpUsername, string smtpPassword) 
        {
            this.smtpServer = smtpServer;
            this.smtpUser = smtpUsername;
            this.smtpPassword = smtpPassword;
        }

        public bool Send(Notification notification, VLogger logger)
        {
            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                string.IsNullOrWhiteSpace(notification.toAddress))
            {
                logger.Log("Could not send email. One of the essential fields is not existent");
                return false;
            }

            MailMessage message = new MailMessage();

            string from = string.IsNullOrEmpty(smtpUser) ? "homeos.placeholder@microsoft.com" : smtpUser;
            message.From = new MailAddress(from);

            message.To.Add(notification.toAddress);

            message.Subject = notification.subject;
            message.Body = notification.body;

            if (notification.attachmentList != null)
            {
                foreach (Attachment attachment in notification.attachmentList)
                {
                    message.Attachments.Add(attachment);
                }
            }

            try
            {
                SmtpClient smtpClient = new SmtpClient(smtpServer);
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(message);
            }
            catch (Exception exception)
            {
                logger.Log("Exception while sending message: {0}", exception.ToString());
                return false;
            }

            return true;
        }

    }
}
