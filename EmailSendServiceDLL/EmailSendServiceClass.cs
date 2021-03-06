﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace EmailSendServiceDLL
{
    public class EmailSendServiceClass
    {
        private string strLogin;
        private string strPassword;
        private string strSmtp = "smtp.yandex.ru";
        private int iSmtpPort = 25;
        private string strBody;
        private string strSubject;

        public EmailSendServiceClass(string sLogin, string sPassword)
        {
            strLogin = sLogin;
            strPassword = sPassword;
        }

        private void SendMail(string mail, string name)
        {
            using (MailMessage message = new MailMessage(strLogin, mail))
            {
                message.Subject = strSubject;
                message.Body = strBody;
                message.IsBodyHtml = false;

                using (SmtpClient client = new SmtpClient(strSmtp, iSmtpPort))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(strLogin, strPassword);
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Невозможно отправить письмо");
                    }
                }
            }
        }
    }
}
