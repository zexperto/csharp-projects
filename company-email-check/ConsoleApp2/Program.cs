using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;

public enum EmailProviderType
{
    FreeProvider,
    GoogleWorkspace,
    Microsoft365,
    KnownProfessionalProvider,
    OtherCustomDomain
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter an email address:");
        var email = Console.ReadLine();

        var result = await DetectAsync(email);

        Console.WriteLine($"Email provider type: {result}");
    }

    private static readonly string[] FreeEmailDomains =
    {
        "gmail.com",
        "yahoo.com",
        "outlook.com",
        "hotmail.com",
        "live.com",
        "icloud.com"
    };

    // Known professional email provider MX patterns
    private static readonly string[] ProfessionalMxPatterns =
    {
        // Zoho
        "zoho.com",
        // Proton
        "protonmail.ch",

        // Fastmail
        "messagingengine.com",

        // Mimecast
        "mimecast.com",

        // Proofpoint
        "pphosted.com",

        // Barracuda
        "barracudanetworks.com",

        // Cisco IronPort
        "iphmx.com",

        // Rackspace
        "emailsrvr.com",

        // Amazon WorkMail
        "awsapps.com",

        // GoDaddy
        "secureserver.net",

        // IONOS (1&1)
        "ionos.com",

        // OVH
        "ovh.net",

        // Yandex
        "yandex.net",

        // Alibaba Cloud
        "aliyun.com",

        // Tencent
        "qq.com",

        // GMX
        "gmx.net"
    };

    public static async Task<EmailProviderType> DetectAsync(string email)
    {
        // 1️⃣ Validate email
        MailAddress address;
        try
        {
            address = new MailAddress(email);
        }
        catch
        {
            return EmailProviderType.OtherCustomDomain;
        }

        var domain = address.Host.ToLowerInvariant();

        // 2️⃣ Free providers
        if (FreeEmailDomains.Contains(domain))
            return EmailProviderType.FreeProvider;

        // 3️⃣ MX lookup
        var lookup = new LookupClient();
        var result = await lookup.QueryAsync(domain, QueryType.MX);

        var mxHosts = result.Answers.MxRecords()
            .OrderBy(r => r.Preference)
            .Select(r => r.Exchange.Value.TrimEnd('.').ToLowerInvariant())
            .ToList();

        if (!mxHosts.Any())
            return EmailProviderType.OtherCustomDomain;

        // 4️⃣ Google Workspace
        if (mxHosts.Any(h => h.EndsWith("l.google.com")))
            return EmailProviderType.GoogleWorkspace;

        // 5️⃣ Microsoft 365
        if (mxHosts.Any(h => h.EndsWith("mail.protection.outlook.com")))
            return EmailProviderType.Microsoft365;

        // 6️⃣ Other known professional providers
        if (mxHosts.Any(h => ProfessionalMxPatterns.Any(p => h.Contains(p))))
            return EmailProviderType.KnownProfessionalProvider;

        // 7️⃣ Everything else
        return EmailProviderType.OtherCustomDomain;
    }
}
