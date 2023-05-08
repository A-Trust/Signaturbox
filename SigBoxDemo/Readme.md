# Setup
## 1. Create and activate your virtual environment 

```python
python3 -m venv venv
.venv\scripts\activate
```
<br/>

## 2. Install the required Libraries
```
pip3 install -r .\requirements.txt
```

<br/>

## 3. Configure your environment variables

Modify the local file "**.env**":
- **API_KEY**: Your API-Key for your SigBox
- **SERVER_URL**: The Server URL under which the SigBox Server is reachable
- **SUCCESS_URL**: the URL to which you will be redirected, when the signature process was successful
- **ERROR_URL**: the URL to which you will be redirected, when the signature process failed

<br/>

## 4. Add your PDFs to "./example"
Add PDF documents that should be signed into the directory: "./example". Signed documents are saved to the "./signed" directory. Modify uploadDir or downloadDir in main.py to change the used directories.

<br/>

## 5. Run example code

```
python3 main.py
```

<br/>

# Information
When running the example code in **main.py** without the use of break points, be aware, you will skip the manual login process needed for a valid signature

One way to go around this would be to stop execution until you press enter in the command line:

```
webbrowser.open(sig_url)    # Opens web browser
input("continue?/")
```