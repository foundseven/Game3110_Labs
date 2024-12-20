using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using UnityEngine;

public class SendingUnityEmail : MonoBehaviour
{
    [ContextMenu("Send Email")]
    public void SendEmail()
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("emmathornewell@gmail.com", "onfd orme ucqv cxxw"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("emmathornewell@gmail.com"),
                Subject = "TESTING (with crossed out password)",
                Body = "I CODED THIS",
            };

            string filePath = Path.Combine(Application.dataPath, "testFile.txt");

            var attachment = new Attachment(filePath, MediaTypeNames.Text.Plain);

            mailMessage.Attachments.Add(attachment);

            mailMessage.To.Add("Fernando.Restituto@georgebrown.ca");

            smtpClient.Send(mailMessage);
            Debug.Log("Email with TXT sent successfully!");

            //smtpClient.Send("emmathornewell@gmail.com", "rachel@thornbury.net", "TESTING", "i coded this mf emailllll");

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to send email: {ex.Message}");

        }
    }
}
