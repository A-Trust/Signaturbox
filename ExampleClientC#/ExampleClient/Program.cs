using ExampleClient;
using System.Diagnostics;

string boxurl = @"https://testbox.a-trust.at/testboxrest/v2/";
string apikey = @"API_KEY";

Console.WriteLine("Running tests for boxurl " + boxurl);

string fileName = @"c:\pdf\diagnose.pdf";
string fileNameSigned = @"c:\pdf\diagnose-signed.pdf";
string template = @"c:\temp\template.xml";

SignaturboxClient boxClient = new SignaturboxClient(boxurl, apikey);

int res;

// optional template upload
int templateId = boxClient.UploadTemplate(template);
// int templateId = 263;
if (templateId == -1) 
{
    Console.WriteLine("error uploading template");
    return;
}

Console.WriteLine("using template with templateId = " + templateId);

string? ticket = boxClient.StartBatchSignature("https://testbox.a-trust.at/SignaturboxCallbackHandler/success", "https://testbox.a-trust.at/SignaturboxCallbackHandler/error");
if (ticket == null)
{
    Console.WriteLine("error opening batch");
    return;
}
Console.WriteLine("received ticket: " + ticket);

// without template 
// int documentId = boxClient.AddDocument(ticket, fileName, "SigServer", "Signature test");
// with template    
// int documentId = boxClient.AddDocumentTemplate(ticket, fileName, templateId, "SigServer", "Signature test");
// with template and position
int documentId = boxClient.AddDocumentTemplateEx(ticket, fileName, templateId, "SigServer", "Signature test", 1, 50, 50, 296, 180);
if (documentId == -1)
{
    Console.WriteLine("error adding document");
    return;
}
Console.WriteLine("added document with id: " + documentId);

string? handySigUrl = boxClient.EndBatchSignature(ticket, "");
if (handySigUrl == null)
{
    Console.WriteLine("error starting signature");
    return;
}
Console.WriteLine("received URL for Handy-Signature: " + handySigUrl);

new Process { StartInfo = new ProcessStartInfo(handySigUrl) { UseShellExecute = true } }.Start();

Console.WriteLine("press a key to continue...");
Console.Read();

if (!boxClient.GetDocument(ticket, documentId, out string? name, out byte[]? signedPDF) || name == null || signedPDF == null) 
{
    Console.WriteLine("error getting document");
    return;
}
File.WriteAllBytes(fileNameSigned, signedPDF);
