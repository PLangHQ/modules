
using PLang.Errors;
using PLang.Interfaces;
using PLang.Modules;
using Renci.SshNet;

namespace SshModule
{

	class SshClientConnection
	{
		SshClient sshClient;
		MemoryStream? privateKeyStream = null;
		ForwardedPortLocal? forwardedPortLocal;

		public SshClientConnection(SshClient sshClient, MemoryStream? privateKeyStream, ForwardedPortLocal? forwardedPortLocal)
		{
			this.sshClient = sshClient;
			this.privateKeyStream = privateKeyStream;
			this.forwardedPortLocal = forwardedPortLocal;
		}

		public SshClient SshClient { get { return sshClient; } }

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
		Dictionary<string, SshClientConnection> sshClients;
		public Program(PLangAppContext context)
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
		}
		public async Task<IError?> Connect(string sshHost, int sshPort = 22, string? sshUser = null, string? sshPassword = null,
			string? privateKey = null, string? privateKeyPassphrase = null, string nameOfSshClientInstance = "default",
			string? boundForwardHost = "127.0.0.1", uint? boundForwardPort = null, string? forwardHost = null, uint? forwardPort = null
			)
		{

			if (sshClients.ContainsKey(nameOfSshClientInstance))
			{
				return new PLang.Errors.Error($"The ssh client with the name {nameOfSshClientInstance} already exists", FixSuggestion: "Depending on your usage, either close the connection to the ssh client before creating a new connection or add a name to your client in your step");
			}


			SshClient sshClient;
			MemoryStream? privateKeyStream = null;
			if (!string.IsNullOrEmpty(privateKey))
			{
				privateKeyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(privateKey));
				var privateKeyFile = new PrivateKeyFile(privateKeyStream, privateKeyPassphrase);
				var authenticationMethod = new PrivateKeyAuthenticationMethod(sshUser, privateKeyFile);
				var connectionInfo = new ConnectionInfo(sshHost, sshPort, sshUser, authenticationMethod);

				sshClient = new SshClient(connectionInfo);
			}
			else
			{
				sshClient = new SshClient(sshHost, sshPort, sshUser, sshPassword);
			}


			sshClient.Connect();

			ForwardedPortLocal? forwardedPortLocal = null;
			if (forwardPort != null || boundForwardPort != null)
			{
				if (forwardPort == null && boundForwardPort != null) forwardPort = boundForwardPort;
				if (boundForwardPort == null && forwardPort != null) boundForwardPort = forwardPort;

				var portForwarded = new ForwardedPortLocal(boundForwardHost, (uint)boundForwardPort, forwardHost, (uint)forwardPort);
				sshClient.AddForwardedPort(portForwarded);
				portForwarded.Start();
			}

			sshClients.Add(nameOfSshClientInstance, new SshClientConnection(sshClient, privateKeyStream, forwardedPortLocal));


			return null;

		}

		public void Dispose()
		{

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
				return (null, new PLang.Errors.Error($"The ssh client with the name {nameOfSshClientInstance} does not exist", FixSuggestion: "Fix the name of the ssh client in your step"));
			}

			var client = sshClients[nameOfSshClientInstance];
			if (!client.SshClient.IsConnected)
			{
				return (null, new PLang.Errors.Error($"The ssh client with the name {nameOfSshClientInstance} is not connected", FixSuggestion: "Create a step in your code that connects to ssh client, e.g. '- connect ssh to 127.0.0.1, username: %Settings.SshUsername%, password: %Settings.SshPassword%"));
			}

			var output = client.SshClient.RunCommand(command);
			return (output.Result, null);
		}


	}
}
