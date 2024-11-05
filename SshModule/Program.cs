
using Microsoft.Extensions.Logging;
using PLang.Errors;
using PLang.Errors.Runtime;
using PLang.Interfaces;
using PLang.Modules;
using Renci.SshNet;
using System.ComponentModel;
using static System.Net.WebRequestMethods;

namespace SshModule
{
	[Description("Provides ssh & sftp connection. Run command on the ssh")]
	public class SshClientConnection
	{
		BaseClient sshClient;
		MemoryStream? privateKeyStream = null;
		ForwardedPortLocal? forwardedPortLocal;
		

		public SshClientConnection(BaseClient sshClient, MemoryStream? privateKeyStream, ForwardedPortLocal? forwardedPortLocal)
		{
			this.sshClient = sshClient;
			this.privateKeyStream = privateKeyStream;
			this.forwardedPortLocal = forwardedPortLocal;
		}

		public SshClient? SshClient { get { return sshClient as SshClient; } }
		public SftpClient? SftpClient { get { return sshClient as SftpClient; } }

		public void Close()
		{
			
			forwardedPortLocal?.Stop();
			forwardedPortLocal?.Dispose();
			privateKeyStream?.Dispose();
			sshClient.Disconnect();
			sshClient.Dispose();

		}

	}

	public class Program : BaseProgram, IDisposable
	{
		private readonly IPLangFileSystem fileSystem;
		private readonly ILogger logger;
		Dictionary<string, SshClientConnection> sshClients;
		public Program(PLangAppContext context, ILogger logger, IPLangFileSystem fileSystem)
		{

			if (context.ContainsKey("__SshClientManager__"))
			{
				sshClients = context["__SshClientManager__"] as Dictionary<string, SshClientConnection>;
			}

			if (sshClients == null)
			{
				sshClients = new Dictionary<string, SshClientConnection>();
				context.AddOrReplace("__SshClientManager__", sshClients);
			}

			this.logger = logger;
			this.fileSystem = fileSystem;
		}
		public async Task<IError?> Connect(string sshHost, int sshPort = 22, string? sshUser = null, string? sshPassword = null,
			string? privateKey = null, string? privateKeyPassphrase = null, string nameOfSshClientInstance = "default",
			string? boundForwardHost = "127.0.0.1", uint? boundForwardPort = null, string? forwardHost = null, uint? forwardPort = null
			)
		{

			if (sshClients.ContainsKey(nameOfSshClientInstance))
			{
				return new ProgramError($"The ssh client with the name {nameOfSshClientInstance} already exists", goalStep, function, FixSuggestion: "Depending on your usage, either close the connection to the ssh client before creating a new connection or add a name to your client in your step");
			}


			SshClient sshClient;
			MemoryStream? privateKeyStream = null;
			if (!string.IsNullOrEmpty(privateKey))
			{
				logger.LogTrace($"Connecting to {nameOfSshClientInstance} using private key");
				privateKeyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(privateKey));
				var privateKeyFile = new PrivateKeyFile(privateKeyStream, privateKeyPassphrase);
				var authenticationMethod = new PrivateKeyAuthenticationMethod(sshUser, privateKeyFile);
				var connectionInfo = new ConnectionInfo(sshHost, sshPort, sshUser, authenticationMethod);

				sshClient = new SshClient(connectionInfo);
			}
			else
			{
				logger.LogTrace($"Connecting to {nameOfSshClientInstance} using username and password");
				sshClient = new SshClient(sshHost, sshPort, sshUser, sshPassword);
			}

			logger.LogTrace($"Connecting");
			sshClient.Connect();

			ForwardedPortLocal? forwardedPortLocal = null;
			if (forwardPort != null || boundForwardPort != null)
			{
				if (forwardPort == null && boundForwardPort != null) forwardPort = boundForwardPort;
				if (boundForwardPort == null && forwardPort != null) boundForwardPort = forwardPort;

				var portForwarded = new ForwardedPortLocal(boundForwardHost, (uint)boundForwardPort, forwardHost, (uint)forwardPort);
				sshClient.AddForwardedPort(portForwarded);
				portForwarded.Start();

				logger.LogTrace($"Forwarded port");
			}

			sshClients.Add(nameOfSshClientInstance, new SshClientConnection(sshClient, privateKeyStream, forwardedPortLocal));

			logger.LogTrace($"Connected ssh");
			return null;

		}

		public async Task<IError?> ConnectSFtp(string sshHost, int sshPort = 22, string? sshUser = null, string? sshPassword = null,
			string? privateKey = null, string? privateKeyPassphrase = null, string nameOfSftpClientInstance = "default_sftp",
			string? boundForwardHost = "127.0.0.1", uint? boundForwardPort = null, string? forwardHost = null, uint? forwardPort = null
			)
		{

			if (sshClients.ContainsKey(nameOfSftpClientInstance))
			{
				return new ProgramError($"The ssh client with the name {nameOfSftpClientInstance} already exists", goalStep, function, FixSuggestion: "Depending on your usage, either close the connection to the ssh client before creating a new connection or add a name to your client in your step");
			}


			SftpClient sshClient;
			MemoryStream? privateKeyStream = null;
			if (!string.IsNullOrEmpty(privateKey))
			{
				logger.LogTrace($"Connecting to {nameOfSftpClientInstance} using private key");
				privateKeyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(privateKey));
				var privateKeyFile = new PrivateKeyFile(privateKeyStream, privateKeyPassphrase);
				var authenticationMethod = new PrivateKeyAuthenticationMethod(sshUser, privateKeyFile);
				var connectionInfo = new ConnectionInfo(sshHost, sshPort, sshUser, authenticationMethod);

				sshClient = new SftpClient(connectionInfo);
			}
			else
			{
				logger.LogTrace($"Connecting to {nameOfSftpClientInstance} using username and password");
				if (string.IsNullOrEmpty(sshPassword))
				{
					return new ProgramError("Password cannot be empty", goalStep, function);
				}
				if (string.IsNullOrEmpty(sshUser))
				{
					return new ProgramError("Username cannot be empty", goalStep, function);
				}
				sshClient = new SftpClient(sshHost, sshPort, sshUser, sshPassword);
			}

			logger.LogTrace($"Connecting");
			sshClient.Connect();

			sshClients.Add(nameOfSftpClientInstance, new SshClientConnection(sshClient, privateKeyStream, null));

			logger.LogTrace($"Connected with sftp");
			return null;

		}

		public async Task Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			logger.LogTrace($"Closing connections");
			foreach (var client in sshClients)
			{
				var clientInfo = client.Value;
				clientInfo.Close();
			}
			context.Remove("__SshClientManager__");
		}


		public async Task<(string?, IError?)> RunCommand(string command, string nameOfSshClientInstance = "default")
		{
			if (!sshClients.ContainsKey(nameOfSshClientInstance))
			{
				return (null, new ProgramError($"The ssh client with the name {nameOfSshClientInstance} does not exist", goalStep, function, FixSuggestion: "You must connect to a ssh before running this command, e.g. '- connect ssh to 127.0.0.1, username: %Settings.SshUsername%, password: %Settings.SshPassword%"));
			}

			var client = sshClients[nameOfSshClientInstance];

			if (client.SshClient == null || !client.SshClient.IsConnected)
			{
				return (null, new ProgramError($"The ssh client with the name {nameOfSshClientInstance} is not connected", goalStep, function, FixSuggestion: "Create a step in your code that connects to ssh client, e.g. '- connect ssh to 127.0.0.1, username: %Settings.SshUsername%, password: %Settings.SshPassword%"));
			}
			client.SshClient.ErrorOccurred += (s, e) =>
			{
				logger.LogError(e.Exception, e.Exception.Message);
			};

			logger.LogTrace($"Running command '{command}'");
			var output = client.SshClient.RunCommand(command);
			

			if (output.ExitStatus != 0 && !string.IsNullOrWhiteSpace(output.Error))
			{
				return (null, new ProgramError(output.Error, goalStep, function));
			}
			logger.LogTrace($"Command result: {output.Result}");
			return (output.Result, null);
		}

		public async Task<IError?> DownloadFile(string remoteFilePath, string localeFilePAth, string nameOfSftpClientInstance = "default_sftp", bool overwrite = true)
		{
			var obj = GetSftpClient(nameOfSftpClientInstance);
			if (obj.Error != null) return obj.Error;

			using (Stream fileStream = fileSystem.File.OpenWrite(localeFilePAth))
			{
				obj.Client.DownloadFile(remoteFilePath, fileStream);
			}
			return null;
		}


		public async Task<IError?> UploadFile(string localFilePath, string remoteFilePath, string nameOfSftpClientInstance = "default_sftp")
		{
			var path = GetPath(localFilePath);

			var obj = GetSftpClient(nameOfSftpClientInstance);
			if (obj.Error != null) return obj.Error;

			logger.LogTrace($"Uploading file through sftp");
			using (var fileStream = fileSystem.FileStream.New(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				obj.Client.UploadFile(fileStream, remoteFilePath);
			}
			return null;
		}


		private (SftpClient? Client, IError? Error) GetSftpClient(string nameOfSftpClientInstance = "default_sftp")
		{
			if (!sshClients.ContainsKey(nameOfSftpClientInstance))
			{
				return (null, new ProgramError($"The ssh client with the name {nameOfSftpClientInstance} does not exist", goalStep, function, FixSuggestion: "You must connect to a sftp before running this command, e.g. '- connect sftp to 127.0.0.1, username: %Settings.SshUsername%, password: %Settings.SshPassword%\""));
			}

			var client = sshClients[nameOfSftpClientInstance].SftpClient;
			if (client == null || !client.IsConnected)
			{
				return (null, new ProgramError($"The ssh client with the name {nameOfSftpClientInstance} is not connected", goalStep, function, FixSuggestion: "Create a step in your code that connects to ssh client, e.g. '- connect ssh to 127.0.0.1, username: %Settings.SshUsername%, password: %Settings.SshPassword%"));
			}
			return (client, null);
		}
	}
}

