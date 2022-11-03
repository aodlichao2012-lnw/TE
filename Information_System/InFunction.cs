using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Mail;

namespace Information_System
{
    public class InFunction
    {
        public string connString = "data source=10.145.163.10; initial catalog=db_employee; user id=sa; password=p@ssw0rd002;MultipleActiveResultSets=True;";
        public string conRTCStr = "data source=10.145.163.10; initial catalog=db_rtc; user id=sa; password=p@ssw0rd002;MultipleActiveResultSets=True;";

        public string GetSHA1(string text)
        {
            var result = default(string);
            using (var algo = new SHA1Managed())
            {
                result = GenerateHashString(algo, text);
            }
            return result;
        }

        public string GenerateHashString(HashAlgorithm algo, string text)
        {
            algo.ComputeHash(Encoding.UTF8.GetBytes(text));
            var result = algo.Hash;
            return string.Join(
            string.Empty,
            result.Select(x => x.ToString("x2")));
        }

        public string getSQL(string text)
        {
            string return_text;

            return_text = "'" + text + "'";

            return return_text;
        }

        public string newID()
        {
            string newID;
            using (SqlConnection conn = new SqlConnection(conRTCStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Clear();
                    sql.AppendLine(" SELECT NEWID() AS NEWID  ");
                    cmd.CommandText = sql.ToString();
                    newID = cmd.ExecuteScalar().ToString();
                }
                conn.Close();
            }
            return newID;
        }

        public Boolean sendMail(string mailSubject, List<string> mailTo, string mailContent, List<string> atth = null, List<string> mailCC = null, string action = null)
        {
            Boolean result = false;
            var myMail = new MailMessage();
            myMail.From = new MailAddress("Information System<ssimsystem@citizen.co.jp>");

            myMail.Subject = mailSubject;

            foreach (string mail in mailTo)
            {
                myMail.To.Add(new MailAddress(mail));
            }
            if (mailCC != null)
            {
                foreach (string mail in mailCC)
                {
                    myMail.CC.Add(new MailAddress(mail));
                }
            }
            if (atth != null)
            {
                if (action == "APPROVED")
                {
                    foreach (string atthf in atth)
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.145.163.10/IS/" + atthf);
                        request.Credentials = new NetworkCredential("sodick", "Rtc0000", "rtc");
                        request.Method = WebRequestMethods.Ftp.DownloadFile;
                        Stream contentStream = request.GetResponse().GetResponseStream();
                        myMail.Attachments.Add(new Attachment(contentStream, atthf));
                    }
                }
                else
                {
                    foreach (string atthf in atth)
                    {
                        myMail.Attachments.Add(new Attachment(@atthf));
                    }
                }
            }

            myMail.IsBodyHtml = true;
            myMail.BodyEncoding = System.Text.Encoding.UTF8;
            myMail.Body = mailContent;

            var credential = new NetworkCredential("ssimsystem@citizen.co.jp", "47050ssi");
            var smtpClient = new SmtpClient();
            smtpClient.Port = 25;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credential;
            smtpClient.Host = "mailsv.citizen.co.jp";
            smtpClient.EnableSsl = false;

            try
            {
                smtpClient.Send(myMail);
                smtpClient.Dispose();
                myMail.Dispose();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
    }
}