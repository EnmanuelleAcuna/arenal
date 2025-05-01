using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace plani.Identity;

public class EmailSender : IEmailSender
{
	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		try
		{
			var smtpClient = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential("sandiconsultores@gmail.com", "zann yqbc cvve nyhn")
			};

			await smtpClient.SendMailAsync(
				new MailMessage("sandiconsultores@gmail.com", email, subject, htmlMessage)
				{
					IsBodyHtml = true
				}
			);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}
}
