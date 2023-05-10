using ExampleClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

string projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(projectDir + @"\appsettings.json", optional: false, reloadOnChange: true);
var configuration = builder.Build();

// Setting API Settings
string serverUrl = configuration["ApiSettings:ServerURL"];
string apikey = configuration["ApiSettings:ApiKey"];
string successUrl = configuration["ApiSettings:SuccessUrl"];
string errorUrl = configuration["ApiSettings:ErrorUrl"];

// Modify here
string uploadDirectory = @"C:\Users\username\examples\";
string downloadDirectory = @"C:\Users\username\signed\";
string templatePath = @"C:\Users\username\TemplateBeispiel.xml";

SignaturboxClient boxClient = new SignaturboxClient(serverUrl, apikey);
Console.WriteLine("Running tests on signautre server: " + serverUrl);

// Uploads example template
int templateId = boxClient.UploadTemplate(templatePath);

if (templateId == -1)
{
    Console.WriteLine("error uploading template");
    return;
}
Console.WriteLine("using template with templateId = " + templateId);

//  Create new batch
string? ticket = boxClient.StartBatchSignature(successUrl, errorUrl);
if (ticket == null)
{
    Console.WriteLine("error opening batch");
    return;
}
Console.WriteLine("received ticket: " + ticket);


string[] filenamesUploadDir = Directory.GetFiles(uploadDirectory, "*.pdf");
Dictionary<int, string> documentMeta = new Dictionary<int, string>();

// Upload documents
int documentId;
string documentName;
foreach (string name in filenamesUploadDir)
{
    //  without specific template
    // int documentId = boxClient.AddDocument(ticket, name, "SigServer", "Signature test");

    //  with specific template
    // int documentId = boxClient.AddDocumentTemplate(ticket, name, templateId, "SigServer", "Signature test");
   
    //  with specific template and position
    documentId = boxClient.AddDocumentTemplateEx(ticket, name, templateId, "SigServer", "Signature test", 1, 50, 50, 296, 180);

    if (documentId == -1)
    {
        Console.WriteLine("error occured when adding a document");
        return;
    }

    documentName = SignaturboxClient.GetLastPartFromUrl(name, @"\");
    documentMeta.Add(documentId, documentName);
    Console.WriteLine("uploaded document: " + documentName + " (DocumentID: " + documentId + " )");
}

// Start batch signing
string? handySigUrl = boxClient.EndBatchSignature(ticket, "");
if (handySigUrl == null)
{
    Console.WriteLine("error starting the signature process");
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
        Console.WriteLine("error getting document");
        return;
    }

    if (!boxClient.WriteToFile(doc.Value, downloadDirectory, signedPDF))
    {
        Console.WriteLine("error writing to file");
        return;
    }
}