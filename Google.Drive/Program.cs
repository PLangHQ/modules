using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using PLang.Errors;
using PLang.Errors.Runtime;
using PLang.Interfaces;
using PLang.Modules;
using PLang.SafeFileSystem;
using System;
using System.IO;

namespace Google.Drive
{
	public class Program : BaseProgram
	{
		private readonly ISettings settings;
		private readonly PLangAppContext context;
		private readonly IPLangFileSystem fileSystem;
		private readonly string AccountKeyJsonKey = "__Google.Drive.DefaultAccount__";
		public Program(ISettings settings, PLangAppContext context, IPLangFileSystem fileSystem)
		{
			this.settings = settings;
			this.context = context;
			this.fileSystem = fileSystem;
			context.AddOrReplace(AccountKeyJsonKey, "default");
		}

		public async Task SetServiceAccountKey(string name)
		{
			context.AddOrReplace(AccountKeyJsonKey, name);
		}

		private DriveService GetDriveService()
		{
			string serviceAccountKeyFile = context.GetOrDefault<string>(AccountKeyJsonKey, "default")!;
			string serviceAccountJson = settings.Get<string>(GetType(), serviceAccountKeyFile, "", "Google Service account key json file. Read about it at https://github.com/PLangHQ/modules/tree/main/Google.Drive");

			GoogleCredential credential = GoogleCredential.FromJson(serviceAccountJson)
							  .CreateScoped(DriveService.Scope.DriveReadonly);


			var service = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Google Drive API with Service Account",
			});
			return service;
		}

		public async Task<IList<Apis.Drive.v3.Data.File>> GetFiles(string folderId)
		{		
			var service = GetDriveService();

			var request = service.Files.List();
			request.Q = $"'{folderId}' in parents";
			request.Fields = "nextPageToken, files(id, name)";

			var result = request.Execute();
			return result.Files;

		}

		public async Task<IError?> DownloadFile(string fileId, string saveTo, bool createFolder = true, bool overwriteFile = false)
		{
			var saveToPath = GetPath(saveTo);
			if (!overwriteFile && fileSystem.File.Exists(saveToPath))
			{
				return new ProgramError($"File already exists at {saveToPath}", goalStep, function);

			}

			var service = GetDriveService();
			var request = service.Files.Get(fileId);
			using (var memoryStream = new MemoryStream())
			{
				var progress = await request.DownloadAsync(memoryStream);

				if (progress.Status == DownloadStatus.Completed)
				{
					await SaveStreamToFile(memoryStream, saveToPath, createFolder);
				}
				else
				{
					return new ProgramError($"Failed to download {saveToPath}", goalStep, function);
				}
			}
			return null;
		}

		private async Task SaveStreamToFile(MemoryStream stream, string filePath, bool createFolder = true)
		{
			var dirName = fileSystem.Path.GetDirectoryName(filePath);
			if (createFolder && !Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}

			using (var fileStream = fileSystem.FileStream.New(filePath, FileMode.Create, FileAccess.Write))
			{
				stream.Seek(0, SeekOrigin.Begin);
				await stream.CopyToAsync(fileStream);
			}
		}

	}
}
