using QRCoder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class QrService : IQrService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly string _imagePath;

    public QrService(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
        _imagePath = PathUtils.CreateOrReturnWbRootPath(env, "qr-codes");
    }

    public string GenerateAuthQR()
    {
        var apiKey = _config.GetValue<string>("Authentication:ApiKey");
        var baseUrl = _config.GetValue<string>("Authentication:BaseUrl");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            throw new Exception("Missing Authentication configuration.");

        string setupData = $"{baseUrl}|{apiKey}";

        // We'll name it uniquely so concurrent requests don't overwrite each other
        string fileName = $"setup-{Guid.NewGuid()}.png";
        string fullPath = Path.Combine(_imagePath, fileName);

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(setupData, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
            File.WriteAllBytes(fullPath, qrCodeAsPngByteArr);
        }

        return fullPath;
    }
}