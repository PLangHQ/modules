using NUnit.Framework;

namespace QRCode;


[TestFixture]
public class Test
{
	[Test]
	public async Task GenerateQRCodeToAscii_ValidText_ReturnsAsciiQRCode()
	{
		// Arrange
		var text = "https://www.example.com";
		var expectedSubstring = "██"; // Part of the expected output

		var qrCodeGenerator = new Program(); // Replace with the class containing GenerateQRCodeToAscii

		// Act
		var result = await qrCodeGenerator.GenerateQRCodeToAscii(text);

		int i = 0;
	}
}

