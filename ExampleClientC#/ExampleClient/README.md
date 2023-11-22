# Setup
## 1. Download the required package from a NuGet Package source of your choice

<br/>

## 2. Configure your environment variables

Modify the local file "**appsettings.json**":
- **API_KEY**: Your API-Key for your SigBox
- **SERVER_URL**: The Server URL under which the signature server is reachable
- **SUCCESS_URL**: the URL to which you will be redirected, when the signature process was successful
- **ERROR_URL**: the URL to which you will be redirected, when the signature process failed

If you do not have an API-Key please contact the A-Trust sales team (sales@a-trust.at).

<br/>

## 3. Add PDF documents to your upload directory if needed
Add PDF-documents you want to be signed to the ./uploadDocuments directory. PDF-Documents from this directory will be signed with this program.

<br/>

You can find your signed documents after the successful signing process in the signedDocuments directory. Be aware: The signedDocuments directory is in the same directory as your executable.

<br/>

## 4. Run the example code in Program.cs