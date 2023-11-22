import configparser
import os
import random
import sys
import webbrowser

from sigbox import SigBox

def main() -> None:
    
    # # # Paste your secret APIKEY to the local .env file
    config = configparser.ConfigParser()
    config.read(".env")

    # # # Enter your Signature Box Server URL / SUCCESS_URL / ERROR_URL
    try:
        API_KEY = config["DEFAULT"]["API_KEY"]
        TESTBOX_SERVER_URL = config["DEFAULT"]['TESTBOX_SERVER_URL']
        SUCCESS_URL = config["DEFAULT"]['SUCCESS_URL']
        ERROR_URL = config["DEFAULT"]['ERROR_URL']
    except KeyError:
        print("Please read readme.md to see how to configure your environment")    
        sys.exit(-1) 

    if not API_KEY:
        print("API-Key was not set in the environment file (.env). \nIf you do not have an API-Key please contact the A-Trust sales team (sales@a-trust.at)");
        sys.exit(-1)

    upload_dir = "examples"
    download_dir = "signed"

    # Initialize
    try:
        sig_box = SigBox(API_KEY, TESTBOX_SERVER_URL)
    except TypeError:
        print("Please read readme.md to see how to configure your environment")    
        sys.exit(-1)
        
    templates = sig_box.get_templates()
    anyTemplate = random.choice(templates)

    # Add template
    # sig_box.add_template("TemplateBeispiel.xml")

    # Replace template
    # sig_box.replace_template("TemplateBeispiel.xml", 262)

    # Create batch
    ticket_id = sig_box.start_signature(success_url=SUCCESS_URL, error_url=ERROR_URL)

    # Upload multiple files
    dir = os.listdir(upload_dir)
    documents = {}
    for filename in dir:
        doc_id = sig_box.add_document(ticket_id, f"{upload_dir}/{filename}")
        
        # with template
        # document_id = sig_box.add_document_template(ticket_id, "./examples/" + doc, anyTemplate["id"])
        
        # with template & position
        # document_id = sig_box.add_document_template_ex(ticket_id, "./examples/" + doc, anyTemplate["id"], 0, 0, 0, 0, 0)
        
        documents[filename] = doc_id
    
    # Sign the batch
    sig_url = sig_box.finalize(ticket_id)
    webbrowser.open(sig_url)

    input("continue?/")
    
    # Download to signed directory
    for filename, doc_id in documents.items():
        content = sig_box.get_document(ticket_id, doc_id).content
        write_to_file(f"{download_dir}/{filename}", content)


"""
Writes binary content into a file 

Args:
    - path (str): write location of the new file
    - content (bytes): binary content written to the new file
"""
def write_to_file(path: str, content: bytes):
        with open(path, "wb") as file:
            print(file, path, content)
            file.write(content)

if __name__ == '__main__':
    main()