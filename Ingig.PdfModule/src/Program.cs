﻿using PLang.Modules;
using System.ComponentModel;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Content;
using System.Text;
using PLang.Interfaces;
using Microsoft.Extensions.Logging;
using PLang.Errors.Runtime;
using PLang.Errors;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics;
using System.Drawing.Imaging;

namespace PdfModule
{
    [Description("It should be able to read pdf and get text out of it, get page count, read specific page, get form data from pdf, create pdf")]
    public class Program : BaseProgram
    {
        private readonly IPLangFileSystem fileSystem;
        private readonly ILogger logger;

        public Program(IPLangFileSystem fileSystem, ISettings settings, ILogger logger) : base()
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

		/*
         * plang: - read file.pdf into %content%
         */
		public async Task<(string?, IError?)> ReadPdfFile(string path)
        {
            var absolutePath = GetPath(path);

            if (!fileSystem.File.Exists(absolutePath))
            {
                var error = new ProgramError("File not found", goalStep, function)
                {
                    FixSuggestion = "Ensure the file path is correct.",
                    HelpfulLinks = "https://github.com/UglyToad/PdfPig"
                };
                return (null, error);
            }

            using (var document = PdfDocument.Open(absolutePath))
            {
                var text = new StringBuilder();
                foreach (var page in document.GetPages())
                {
                    text.Append(page.Text);
                }
                return (text.ToString(), null);
            }
        }

		/*
         * plang: - read page 1 from file.pdf, write into %page%
         */
		public async Task<(string?, IError?)> ReadPdfPage(string path, int pageNumber)
        {
            var absolutePath = GetPath(path);

            if (!fileSystem.File.Exists(absolutePath))
            {
                var error = new ProgramError("File not found", goalStep, function)
                {
                    FixSuggestion = "Ensure the file path is correct.",
                    HelpfulLinks =  "https://github.com/UglyToad/PdfPig" 
                };
                return (null, error);
            }

            using (var document = PdfDocument.Open(absolutePath))
            {
                var page = document.GetPage(pageNumber);
                return (page.Text, null);
            }
        }

		/*
         * plang: - read form data from file.pdf, write into %formData%
         */
		public async Task<(int?, IError?)> GetPdfPageCount(string path)
        {
            var absolutePath = GetPath(path);

            if (!fileSystem.File.Exists(absolutePath))
            {
                var error = new ProgramError("File not found", goalStep, function)
                {
                    FixSuggestion = "Ensure the file path is correct.",
                    HelpfulLinks =  "https://github.com/UglyToad/PdfPig"
                };
                return (null, error);
            }

            using (var document = PdfDocument.Open(absolutePath))
            {
                return (document.NumberOfPages, null);
            }
        }

		/*
         * plang: - get page count from file.pdf, write to %pageCount%
         */
		public async Task<(Dictionary<string, object>?, IError?)> GetPdfFormData(string path)
        {
            var absolutePath = GetPath(path);

            if (!fileSystem.File.Exists(absolutePath))
            {
                var error = new ProgramError("File not found", goalStep, function)
                {
                    FixSuggestion = "Ensure the file path is correct.",
                    HelpfulLinks = "https://github.com/UglyToad/PdfPig"
                };
                return (null, error);
            }

            using (var document = PdfDocument.Open(absolutePath))
            {
                if (document.TryGetForm(out var form))
                {
                    var formData = new Dictionary<string, object>();
                    foreach (var field in form.Fields)
                    {
                        int i = 0;
                        //formData.Add(field.Informatio($"{field.Name}: {field.Value}");
                    }
                    return (formData, null);
                }
                else
                {
                    return (null, new ProgramError("No form data found.", goalStep, function));
                }
            }
        }


		/*
         * plang: - create pdf with content %content%, save to %path%
         */
		public async Task WritePdfFile(string path, string content)
        {
            var absolutePath = GetPath(path);
            var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);
            page.AddText(content, 12, new PdfPoint(25, 700), font);
            var documentBytes = builder.Build();
            await fileSystem.File.WriteAllBytesAsync(absolutePath, documentBytes);
        }


        public async Task<(string?, IError?)> SavePageAsImage(string path, int pageNr, string saveToPath)
		{
            var absolutePath = GetPath(path);
            if (!fileSystem.File.Exists(absolutePath))
            {
                return (null, new ProgramError($"File {path} was not found. The full path of file should be {absolutePath}", goalStep, function));
            }

			using (var document = PdfiumViewer.PdfDocument.Load(absolutePath))
			{
				using (var image = document.Render(pageNr, 300, 300, true)) // 0 = first page, 300 DPI
				{
                    var abosluteSaveToPath = GetPath(saveToPath);
					image.Save(abosluteSaveToPath, ImageFormat.Png);
                    return (abosluteSaveToPath, null);
				}
			}
            
		}

        public async Task<(List<string>?, IError?)> SavePdfAsImages(string path, string saveToFolder)
        {
			var absolutePath = GetPath(path);
			if (!fileSystem.File.Exists(absolutePath))
			{
				return (null, new ProgramError($"File {path} was not found. The full path of file should be {absolutePath}", goalStep, function));
			}
            List<string> paths = new List<string>();
            string fileName = fileSystem.Path.GetFileNameWithoutExtension(path);
            string extension = fileSystem.Path.GetExtension(path) ?? ".pdf";

			using (var document = PdfiumViewer.PdfDocument.Load(absolutePath))
			{
                for (int i=0;i<document.PageCount;i++) 
                {
                    using (var image = document.Render(i, 300, 300, true)) // 0 = first page, 300 DPI
                    {
                        string fullFileName = fileSystem.Path.Join(saveToFolder, fileName + "_" + i + extension);
                        var abosluteSaveToPath = GetPath(fullFileName);
                        image.Save(abosluteSaveToPath, ImageFormat.Png);
                        paths.Add(abosluteSaveToPath);

					}
                }
			}
            return (paths, null);
		}
    }
}
