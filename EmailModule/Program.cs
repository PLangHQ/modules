using MailKit.Net.Pop3;
using Microsoft.Extensions.Hosting;
using MimeKit;
using Newtonsoft.Json;
using PLang.Errors;
using PLang.Errors.Runtime;
using PLang.Interfaces;
using PLang.Services.SigningService;
using PLang.Utils;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace PLang.Modules.EmailModule
{
	public class Program : BaseProgram
	{

		private readonly ISettings settings;
		private readonly PLangAppContext context;

		public Program(ISettings settings, PLangAppContext context)
		{
			this.settings = settings;
			this.context = context;


		}

		public async Task SetSmptpHostAndPort(string host, int port = 587)
		{
			settings.Set<string>(GetType(), "smptHost", host);
			settings.Set<int>(GetType(), "smptPort", port);
		}

		public async Task SetPop3HostAndPort(string host, int port = 995)
		{
			settings.Set<string>(GetType(), "pop3Host", host);
			settings.Set<int>(GetType(), "pop3Port", port);
		}

		public async Task<IError?> SendEmail(string fromEmail, string toEmail, string subject, string body, bool isHtmlBody = false, string? smtpHost = null, int? smtpPort = null, string? username = null, string? password = null)
		{
			MailMessage mm = new MailMessage(fromEmail, toEmail, subject, body);
			mm.IsBodyHtml = isHtmlBody;

			if (smtpHost == null)
			{
				smtpHost = settings.Get<string>(GetType(), "smptHost", "", "What is the SMTP host address");
			}

			if (smtpPort == null)
			{
				smtpPort = settings.Get<int>(GetType(), "smptPort", 587, "What is the SMTP port address");
			}
			if (username == null)
			{
				username = settings.Get<string>(GetType(), smtpHost + "_username", "", "Username for SMTP server");
			}
			if (password == null)
			{
				password = settings.Get<string>(GetType(), smtpHost + "_password", "", "Password for SMTP server");
			}
			

			using (SmtpClient sc = new SmtpClient(smtpHost, smtpPort.Value))
			{
				sc.Credentials = new NetworkCredential(username, password);
				sc.EnableSsl = false;
				sc.Host = smtpHost;
				sc.Port = smtpPort.Value;
				try
				{
					await sc.SendMailAsync(mm);
				} catch (Exception ex)
				{
					return new ProgramError(ex.Message, goalStep, this.function);
				}
			}
			return null;

		}

		public async Task<(List<MimeMessage>?, IError?)> GetMessages(int limit = 100, string? pop3Host = null, int? pop3Port = null, string? username = null, string? password = null)
		{
			if (pop3Host == null)
			{
				pop3Host = settings.Get<string>(GetType(), "pop3Host", "", "What is the POP3 host address");
			}

			if (pop3Port == null)
			{
				pop3Port = settings.Get<int>(GetType(), "pop3Port", 995, "What is the POP3 port address");
			}
			if (password == null)
			{
				password = settings.Get<string>(GetType(), pop3Host + "_password", "", "Password for POP3 server");
			}
			if (username == null)
			{
				username = settings.Get<string>(GetType(), pop3Host + "_username", "", "Username for POP3 server");
			}

			using (var client = new Pop3Client())
			{
				try
				{
					
					
					client.Connect(pop3Host, pop3Port.Value, MailKit.Security.SecureSocketOptions.SslOnConnect);

					
					client.Authenticate(username, password);
				
					int messageCount = client.Count;
					List<MimeMessage> messages = new();
					
					for (int i = 0; i < messageCount && i < limit; i++)
					{
						messages.Add(client.GetMessage(i));
					}					

					client.Disconnect(true);

					return (messages, null);
				}
				catch (Exception ex)
				{
					return (null, new ProgramError(ex.Message, goalStep, function, Key: "Pop3GetMessages"));
				}
			}
		}

	}
}
