using PLang.Modules;
using QRCoder;
using System.Text;

namespace QRCode; 

public class Program : BaseProgram
{
	public async Task<string> GenerateQRCodeToAscii(string text)
	{

		using (var qrGenerator = new QRCoder.QRCodeGenerator())
		{
			var qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
			return ConvertQrCodeDataToAscii(qrCodeData);
		}
		static string ConvertQrCodeDataToAscii(QRCodeData qrCodeData)
		{
			var moduleMatrix = qrCodeData.ModuleMatrix;
			StringBuilder sb = new StringBuilder();

			foreach (var row in moduleMatrix)
			{
				foreach (var module in row)
				{
					sb.Append((bool) module ? "██" : "  "); // Use double-width characters for better proportion
				}
				sb.AppendLine();
			}

			return sb.ToString();
		}
	}



}