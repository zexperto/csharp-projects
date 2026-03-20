using System;
using System.Net.Http;
using System.Text;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        string serviceLayerUrl = "https://localhost:50000/b1s/v2/Attachments2";
        string sessionId = "8a4263cf-f548-4a2b-84f4-ee83b4616374-66640";

        // Boundary MUST match exactly what SAP expects
        string boundary = "WebKitFormBoundaryUmZoXOtOBNCTLyxT";

        // Build the raw multipart content manually
        var body = new StringBuilder();

        body.AppendLine($"--{boundary}");
        body.AppendLine(@"Content-Disposition: form-data; name=""files""; filename=""line1.txt""");
        body.AppendLine("Content-Type: text/plain");
        body.AppendLine();
        body.AppendLine("Introduction");
        body.AppendLine("B1 Service Layer (SL) is a new generation of extension API for consuming B1 objects and services");
        body.AppendLine("via web service with high scalability and high availability.");

        body.AppendLine($"--{boundary}");
        body.AppendLine(@"Content-Disposition: form-data; name=""files""; filename=""line2.txt""");
        body.AppendLine("Content-Type: image/jpeg");
        body.AppendLine();
        body.AppendLine("This is demo text for the learning.");

        body.AppendLine($"--{boundary}--");

        // Convert to bytes
        var content = new StringContent(body.ToString(), Encoding.UTF8);
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
        content.Headers.ContentType.Parameters.Add(
            new System.Net.Http.Headers.NameValueHeaderValue("boundary", boundary));

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; // ignore SSL for localhost

        using (var client = new HttpClient(handler))
        {
            // Add SAP session cookie
            client.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sessionId}");

            var response = await client.PostAsync(serviceLayerUrl, content);

            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Response:");
            Console.WriteLine(result);
        }
    }
}
