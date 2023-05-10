# Setup
## 1. Download the required package from a NuGet Package source of your choice

<br/>

## 2. Configure your environment variables

Modify the local file "**.env**":
- **API_KEY**: Your API-Key for your SigBox
- **SERVER_URL**: The Server URL under which the signature server is reachable
- **SUCCESS_URL**: the URL to which you will be redirected, when the signature process was successful
- **ERROR_URL**: the URL to which you will be redirected, when the signature process failed

<br/>

## 3. Modify "uploadDirectory" / "downloadDirectory" and add PDF documents to it
Modify the following variables in your code according to your setup:

- **uploadDirectory**: All files inside this directory are uploaded to the batch
- **downloadDirectory**: When the batch has been signed, you will find all signed documents in this directory
- **templatePath**: The path at which a template.xml file can be found (an example: TemplateBeispiel.xml, can be found in the project directory.).

<br/>

Add PDF documents that you want to be signed into the uploadDirectory. You can find your signed documents in the downloadDirectory.

<br/>

## 4. Run the example code in Program.cs