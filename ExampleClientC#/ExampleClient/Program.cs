using ExampleClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
string workPath = System.IO.Path.GetDirectoryName(executablePath);

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"{workPath}\\appsettings.json", optional: false, reloadOnChange: true);
var configuration = builder.Build();


// Setting API Settings
string serverUrl = configuration["ApiSettings:ServerURL"];
string apikey = configuration["ApiSettings:ApiKey"];
string successUrl = configuration["ApiSettings:SuccessUrl"];
string errorUrl = configuration["ApiSettings:ErrorUrl"];

// Check if API Settings were set correctly
if (string.IsNullOrEmpty(serverUrl))
{
    Console.Error.WriteLine("Please enter the ServerURL for your Signature-Box in \"appsettings.json\"");
    return;
}
if (string.IsNullOrEmpty(apikey))
{
    Console.Error.WriteLine("Please enter your API-Key for your Signature-Box in \"appsettings.json\". If you do not have an API-Key please contact the A-Trust Sales Team (sales@a-trust.at)");
    return;
}
if (string.IsNullOrEmpty(successUrl))
{
    Console.Error.WriteLine("Please enter the successUrl (where user is redirect after successful signature) in \"appsettings.json\"");
    return;
}
if (string.IsNullOrEmpty(errorUrl))
{
    Console.Error.WriteLine("Please enter the errorUrl (where user is redirected after a failed signature) for your Signature-Box in \"appsettings.json\"");
    return;
}

// needed Directories
string uploadDirectory = $"{workPath}\\uploadDocuments\\";
string downloadDirectory = $"{workPath}\\signedDocuments\\";
string templatePath = $"{workPath}\\TemplateBeispiel.xml";

SignaturboxClient boxClient = new SignaturboxClient(serverUrl, apikey);
Console.WriteLine("Running tests on signautre server: " + serverUrl);

// Uploads example template
int templateId = boxClient.UploadTemplate(templatePath);

if (templateId == -1)
{
    Console.Error.WriteLine("error uploading template");
    return;
}
Console.WriteLine("using template with templateId = " + templateId);

//  Create new batch
string? ticket = boxClient.StartBatchSignature(successUrl, errorUrl);
if (ticket == null)
{
    Console.Error.WriteLine("error opening batch");
    return;
}
Console.WriteLine("received ticket: " + ticket);


string[] filepathsUploadDir = Directory.GetFiles(uploadDirectory, "*.pdf");
Dictionary<int, string> documentMeta = new Dictionary<int, string>();

// Upload documents
int documentId;
string documentName;
foreach (string path in filepathsUploadDir)
{
    //  without specific template
    // int documentId = boxClient.AddDocument(ticket, name, "SigServer", "Signature test");

    //  with specific template
    // int documentId = boxClient.AddDocumentTemplate(ticket, name, templateId, "SigServer", "Signature test");
   
    //  with specific template and position
    documentId = boxClient.AddDocumentTemplateEx(ticket, path, templateId, "SigServer", "Signature test", 1, 50, 50, 296, 180);

    if (documentId == -1)
    {
        Console.Error.WriteLine("error occured when adding a document");
        return;
    }

    documentName = SignaturboxClient.GetLastPartFromUrl(path, @"\");
    documentMeta.Add(documentId, documentName);
    Console.WriteLine("uploaded document: " + documentName + " (DocumentID: " + documentId + " )");
}

// Start batch signing
string? handySigUrl = boxClient.EndBatchSignature(ticket, "");
if (handySigUrl == null)
{
    Console.Error.WriteLine("error starting the signature process");
    return;
}

// Open Handy-Signature URL in Browser
Console.WriteLine("received URL for Handy-Signature: " + handySigUrl);

new Process { StartInfo = new ProcessStartInfo(handySigUrl) { UseShellExecute = true } }.Start();

Console.WriteLine("press a key to continue... (After the signing process!)");
Console.Read();

// Download documents
foreach (KeyValuePair<int, string> doc in documentMeta)
{
    if (!boxClient.GetDocument(ticket, doc.Key, out string? name, out byte[]? signedPDF) || name == null || signedPDF == null)
    {
        Console.Error.WriteLine("error getting document");
        Console.Error.WriteLine("Help: Did you forget to sign the documents in the browser before continuing?");
        return;
    }

    if (!boxClient.WriteToFile(doc.Value, downloadDirectory, signedPDF))
    {
        Console.Error.WriteLine("error writing to file");
        return;
    }
}
Console.WriteLine("Done!");
Console.WriteLine("Be aware: signedDocuments are put in the same directory of your executable!");
