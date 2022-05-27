There's this few web apps that help you check the current status of the certificates from the domains we use, and we have a few of them.

I created an Azure Function that runs once a day and checks a few of the domains I need to take a look, it's a super simple console application that if the expiration date of the certificate is less than 30 days, it will send an email.

The logic itself of how the function works won't be shown, I'd like to show the function that does the base check of the certificate and what data we get.

## The function

```csharp
static async Task<X509Certificate2> CheckCertificateAsync(string urlPath)
{
    var certificate = new X509Certificate2();
    var httpClientHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (request, cert, chain, policyErrors) =>
            {
                certificate = new X509Certificate2(cert);
                return true;
            }
    };

    using HttpClient httpClient = new HttpClient(httpClientHandler);
    await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, urlPath));

    return certificate;
}
```

This method `CheckCertificateAsync` will return us a `X509Certificate2` certificate that will let us do a bunch of stuff, including taking a look at the expiration date.

## Serialized result

This is the serialized value of the `certificate` object:

```json
{
   "Archived":false,
   "Extensions":[
      {
         "KeyUsages":160,
         "Critical":true,
         "Oid":{
            "Value":"2.5.29.15",
            "FriendlyName":"Key Usage"
         },
         "RawData":"AwIFoA=="
      },
      {
         "EnhancedKeyUsages":[
            {
               "Value":"1.3.6.1.5.5.7.3.1",
               "FriendlyName":"Server Authentication"
            },
            {
               "Value":"1.3.6.1.5.5.7.3.2",
               "FriendlyName":"Client Authentication"
            }
         ],
         "Critical":false,
         "Oid":{
            "Value":"2.5.29.37",
            "FriendlyName":"Enhanced Key Usage"
         },
         "RawData":"MBQGCCsGAQUFBwMBBggrBgEFBQcDAg=="
      },
      {
         "CertificateAuthority":false,
         "HasPathLengthConstraint":false,
         "PathLengthConstraint":0,
         "Critical":true,
         "Oid":{
            "Value":"2.5.29.19",
            "FriendlyName":"Basic Constraints"
         },
         "RawData":"MAA="
      },
      {
         "SubjectKeyIdentifier":"634E1585565AA49402C21642A4A5979A38025797",
         "Critical":false,
         "Oid":{
            "Value":"2.5.29.14",
            "FriendlyName":"Subject Key Identifier"
         },
         "RawData":"BBRjThWFVlqklALCFkKkpZeaOAJXlw=="
      },
      {
         "Critical":false,
         "Oid":{
            "Value":"2.5.29.35",
            "FriendlyName":"Authority Key Identifier"
         },
         "RawData":"MBaAFBQusxe3WFbLrlAJQOYfr52LFMLG"
      },
      {
         "Critical":false,
         "Oid":{
            "Value":"1.3.6.1.5.5.7.1.1",
            "FriendlyName":"Authority Information Access"
         },
         "RawData":"MEcwIQYIKwYBBQUHMAGGFWh0dHA6Ly9yMy5vLmxlbmNyLm9yZzAiBggrBgEFBQcwAoYWaHR0cDovL3IzLmkubGVuY3Iub3JnLw=="
      },
      {
         "Critical":false,
         "Oid":{
            "Value":"2.5.29.17",
            "FriendlyName":"Subject Alternative Name"
         },
         "RawData":"MB6CHGJsb2cuZW1pbGlhbm9tb250ZXNkZW9jYS5jb20="
      },
      {
         "Critical":false,
         "Oid":{
            "Value":"2.5.29.32",
            "FriendlyName":"Certificate Policies"
         },
         "RawData":"MEMwCAYGZ4EMAQIBMDcGCysGAQQBgt8TAQEBMCgwJgYIKwYBBQUHAgEWGmh0dHA6Ly9jcHMubGV0c2VuY3J5cHQub3Jn"
      },
      {
         "Critical":false,
         "Oid":{
            "Value":"1.3.6.1.4.1.11129.2.4.2",
            "FriendlyName":"SCT List"
         },
         "RawData":"BIH0APIAdwBByMqx3yJGShDGoToJQodeTjGLGwPr60vHaPCQYpYG9gAAAYCeJIwYAAAEAwBIMEYCIQCG8sf4iBitUjNCc1dsxVd5mdRQCKapRqqnTHKxSKHjHgIhAJFGNXEZkCHKygT1T7bE4orpd6p2l1+GmifMEIuRsgHbAHcARqVV63X6kSAwtaKJafTzfREsQXS+/Um4havy/HD+bUcAAAGAniSMNgAABAMASDBGAiEAoxv1LBn/vfyR7s67kRLB/n1tq3eicuA/8/V0S2YzQCYCIQDXaS3FZbdIVNxQvKxPFxM1awBO/sGxBXafz0lspOoWSA=="
      }
   ],
   "FriendlyName":"",
   "HasPrivateKey":false,
   "PrivateKey":null,
   "IssuerName":{
      "Name":"CN=R3, O=Let's Encrypt, C=US",
      "Oid":{
         "Value":null,
         "FriendlyName":null
      },
      "RawData":"MDIxCzAJBgNVBAYTAlVTMRYwFAYDVQQKEw1MZXQncyBFbmNyeXB0MQswCQYDVQQDEwJSMw=="
   },
   "NotAfter":"2022-08-05T10:50:35+01:00",
   "NotBefore":"2022-05-07T10:50:36+01:00",
   "PublicKey":{
      "EncodedKeyValue":{
         "Oid":{
            "Value":"1.2.840.113549.1.1.1",
            "FriendlyName":"RSA"
         },
         "RawData":"MIIBCgKCAQEAq8cbDO3GAfjqqbPPCBdPost8NMRmEubv85gXecll7mZMH5qSfTPuB/ouFWL3tPMf1U8usWeoSUK/48yatzBGwmj1KKlkaW9MS2QkydztRp+kH8LvbzbQvGknuOLWGHBALLT17o/3DYxuA5LnXdY+vLvJWygQoFr2N/XhnhUjcm6OaQEJpIykydfbBQGQSEuQIIw4egpgdHkYJjCOYAsXuSSggN8/FADTCec0RzVjfFTSoJ3hV9HLE9M8MCSXjuo0AJ/MbAxq91S8XmDcRjHCCd7Zw+NjHo8cxZCQ6NqGvn3xwx8ahmmbC+CyDEcIyJJZK2Yv+qE4oS8QZfaX/RaHMwIDAQAB"
      },
      "EncodedParameters":{
         "Oid":{
            "Value":"1.2.840.113549.1.1.1",
            "FriendlyName":"RSA"
         },
         "RawData":"BQA="
      },
      "Key":{
         "Key":{
            "Algorithm":{
               "Algorithm":"RSA"
            },
            "AlgorithmGroup":{
               "AlgorithmGroup":"RSA"
            },
            "ExportPolicy":0,
            "Handle":{
               "IsInvalid":false,
               "IsClosed":false
            },
            "IsEphemeral":true,
            "IsMachineKey":false,
            "KeyName":null,
            "KeySize":2048,
            "KeyUsage":16777215,
            "ParentWindowHandle":{
               "value":0
            },
            "Provider":{
               "Provider":"Microsoft Software Key Storage Provider"
            },
            "ProviderHandle":{
               "IsInvalid":false,
               "IsClosed":false
            },
            "UIPolicy":{
               "ProtectionLevel":0,
               "FriendlyName":null,
               "Description":null,
               "UseContext":null,
               "CreationTitle":null
            },
            "UniqueName":null
         },
         "LegalKeySizes":[
            {
               "MinSize":512,
               "MaxSize":16384,
               "SkipSize":64
            }
         ],
         "KeyExchangeAlgorithm":"RSA",
         "SignatureAlgorithm":"RSA",
         "KeySize":2048
      },
      "Oid":{
         "Value":"1.2.840.113549.1.1.1",
         "FriendlyName":"RSA"
      }
   },
   "RawData":"MIIFQDCCBCigAwIBAgISBNwTmwP/RTcrEeIgAdMrpaFtMA0GCSqGSIb3DQEBCwUAMDIxCzAJBgNVBAYTAlVTMRYwFAYDVQQKEw1MZXQncyBFbmNyeXB0MQswCQYDVQQDEwJSMzAeFw0yMjA1MDcwOTUwMzZaFw0yMjA4MDUwOTUwMzVaMCcxJTAjBgNVBAMTHGJsb2cuZW1pbGlhbm9tb250ZXNkZW9jYS5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCrxxsM7cYB+Oqps88IF0+iy3w0xGYS5u/zmBd5yWXuZkwfmpJ9M+4H+i4VYve08x/VTy6xZ6hJQr/jzJq3MEbCaPUoqWRpb0xLZCTJ3O1Gn6Qfwu9vNtC8aSe44tYYcEAstPXuj/cNjG4Dkudd1j68u8lbKBCgWvY39eGeFSNybo5pAQmkjKTJ19sFAZBIS5AgjDh6CmB0eRgmMI5gCxe5JKCA3z8UANMJ5zRHNWN8VNKgneFX0csT0zwwJJeO6jQAn8xsDGr3VLxeYNxGMcIJ3tnD42MejxzFkJDo2oa+ffHDHxqGaZsL4LIMRwjIklkrZi/6oTihLxBl9pf9FoczAgMBAAGjggJZMIICVTAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMAwGA1UdEwEB/wQCMAAwHQYDVR0OBBYEFGNOFYVWWqSUAsIWQqSll5o4AleXMB8GA1UdIwQYMBaAFBQusxe3WFbLrlAJQOYfr52LFMLGMFUGCCsGAQUFBwEBBEkwRzAhBggrBgEFBQcwAYYVaHR0cDovL3IzLm8ubGVuY3Iub3JnMCIGCCsGAQUFBzAChhZodHRwOi8vcjMuaS5sZW5jci5vcmcvMCcGA1UdEQQgMB6CHGJsb2cuZW1pbGlhbm9tb250ZXNkZW9jYS5jb20wTAYDVR0gBEUwQzAIBgZngQwBAgEwNwYLKwYBBAGC3xMBAQEwKDAmBggrBgEFBQcCARYaaHR0cDovL2Nwcy5sZXRzZW5jcnlwdC5vcmcwggEGBgorBgEEAdZ5AgQCBIH3BIH0APIAdwBByMqx3yJGShDGoToJQodeTjGLGwPr60vHaPCQYpYG9gAAAYCeJIwYAAAEAwBIMEYCIQCG8sf4iBitUjNCc1dsxVd5mdRQCKapRqqnTHKxSKHjHgIhAJFGNXEZkCHKygT1T7bE4orpd6p2l1+GmifMEIuRsgHbAHcARqVV63X6kSAwtaKJafTzfREsQXS+/Um4havy/HD+bUcAAAGAniSMNgAABAMASDBGAiEAoxv1LBn/vfyR7s67kRLB/n1tq3eicuA/8/V0S2YzQCYCIQDXaS3FZbdIVNxQvKxPFxM1awBO/sGxBXafz0lspOoWSDANBgkqhkiG9w0BAQsFAAOCAQEAjSEID5MWonbSiyHbmPYWO8ImCCOjkLGxgY8WJODbrWxFy+xU44UwrWOCkqYZUlv2LRmPqSyZDrIeeHK9VMbGh71oXX+XovikgAr6PpI0Mp897nPWj0XvOBaSYG0s+f+CXMtyt0tWCsQOcl+iT82+Ja71f8gbVL6l7xESewEE78pTKEH8EqD22r8VSD7FNICD8EYQr13v3AuVWObSU/R8Td6SrSVEknw1HgJS4e9nvmrMxBGKOJ+aWrAGiUydehg8M9o2gbGckMhz6D7cwB5l618cYaXKkW1dEOYZHl++qUj1/VPK+FNkiDZOPVNN//PbZuOLwAUIlZvhqGWX5/9PBg==",
   "SerialNumber":"04DC139B03FF45372B11E22001D32BA5A16D",
   "SignatureAlgorithm":{
      "Value":"1.2.840.113549.1.1.11",
      "FriendlyName":"sha256RSA"
   },
   "SubjectName":{
      "Name":"CN=blog.emilianomontesdeoca.com",
      "Oid":{
         "Value":null,
         "FriendlyName":null
      },
      "RawData":"MCcxJTAjBgNVBAMTHGJsb2cuZW1pbGlhbm9tb250ZXNkZW9jYS5jb20="
   },
   "Thumbprint":"28CF960F772ABFF22AA193C291492C27F8E13D4D",
   "Version":3,
   "Handle":{
      "value":2658150705632
   },
   "Issuer":"CN=R3, O=Let's Encrypt, C=US",
   "Subject":"CN=blog.emilianomontesdeoca.com"
}
```

## Expiration date

In order to know the expiration time, we need to take a look at the `NotAfter` and `NotBefore`, which are inside this object:

```json
   "NotAfter":"2022-08-05T10:50:35+01:00",
   "NotBefore":"2022-05-07T10:50:36+01:00",
```

## Console application

The following snippet is a simple console application built on .NET 6, that will produce the following result, in which you can check any of the certifications that you want:

```csharp
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

var url = "https://blog.emilianomontesdeoca.com/";

static async Task<X509Certificate2> CheckCertificateAsync(string urlPath)
{
    var certificate = new X509Certificate2();
    var httpClientHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (request, cert, chain, policyErrors) =>
            {
                certificate = new X509Certificate2(cert);
                return true;
            }
    };

    using HttpClient httpClient = new HttpClient(httpClientHandler);
    await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, urlPath));

    return certificate;
}

var cert = await CheckCertificateAsync(url);

var serializedValue = JsonConvert.SerializeObject(cert);

Console.WriteLine(serializedValue);

Console.ReadLine();
```

## Demo project

You can find the console application in my Github, in the repository called [expiration-date-certificate](https://github.com/emimontesdeoca/expiration-date-certificate).
