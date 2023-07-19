import json
import requests
from typing import Union

class SigBox:    
    def __init__(self, api_key: str, server_url : str) -> None:
        """Initializes a SigBox object

        Args:
            - api_key (str): API_KEY for your SigBox
            - server_url (str): the URL of your SigBox Server
        """
        self.header = {'X-API-KEY': api_key}
        self.SERVER = server_url
        

    def _get(self, url: str):
        """Sends a GET-Request

        Args:
            url (str): GET-Request URL

        Raises:
            TypeError: returned the incorrect datatype

        Returns:
            _type_: JSON of GET-Request
        """
        ret = requests.get(self._combineUrl(url),  headers = self.header)
        if not ret.ok:
            raise TypeError(f"error getting {url=} {ret.status_code=}")
        return json.loads(ret.content)
    

    def _get_raw(self, url: str):
        """Sends a GET-Request

        Args:
            url (str): GET-Request URL

        Raises:
            TypeError: returned the incorrect datatype

        Returns:
            _type_: JSON of GET-Request
        """
        ret = requests.get(self._combineUrl(url),  headers = self.header)
        if not ret.ok:
            raise TypeError(f"error getting {url=} {ret.status_code=}")
        return ret.content

    def _post(self, url: str, **kwargs):
        """Sends a POST-Request

        Args:
            - url (str): POST-Request URL

        Raises:
            TypeError: request returned the incorrect datatype

        Returns:
            _type_: JSON of POST-Request
        """
        ret = requests.post(self._combineUrl(url), headers=self.header, **kwargs)
        if not ret.ok:
            raise TypeError(f"error posting {url=} {ret.status_code=}")
        return ret
    
      
    def _put(self, url: str, **kwargs):
        """Sends a PUT-Request

        Args:
            - url (str): PUT-Request URL

        Raises:
            TypeError: request returned the incorrect datatype

        Returns:
            _type_: JSON of PUT-Request
        """
        ret = requests.put(self._combineUrl(url), headers=self.header, **kwargs)
        if not ret.ok:
            raise TypeError(f"error putting {url=} {ret.status_code=}")
        return ret

    def _delete(self, url: str):
        """Sends a DELETE-Request

        Args:
            - url (str): DELETE-Request URL
        """
        ret = requests.delete(self._combineUrl(url), headers=self.header)
        if not ret.ok:
            raise TypeError(f"error deleting {url=} {ret.status_code=}")
        return ret

    def _combineUrl(self, url : str):
        serverEndingWithSlash = self.SERVER.endswith("/")
        urlStartingWithSlash = url.startswith("/")
        
        if (serverEndingWithSlash and urlStartingWithSlash):
            print(self.SERVER + url[1:])
            return self.SERVER + url[1:]
        elif (serverEndingWithSlash and not urlStartingWithSlash or 
              not serverEndingWithSlash and urlStartingWithSlash):
            print(self.SERVER + url) 
            return self.SERVER + url
        else:
            print(url + "/" + self.SERVER)
            return self.SERVER + "/" + url
        
    def add_template(self, filename: str):
        """Uploads a new template

          Args:
            - filename (str): The filename of the template
        """
        with open(filename, 'r') as file:
            buf = file.read()
            ret = self._post('/templates', files={'template': (filename, buf), })
        return ret            
    
    def replace_template(self, filename: str, template_id: int):
        """Replaces a template on the signature server
        
          Args:
            - filename (str): The filename of the template
            - template_id (str): The id of the template that should be overwritten
        """
        with open(filename, 'r') as file:
            buf = file.read()        
            ret = self._put(f"/templates/{template_id}", files={'template': (filename, buf)})
        return ret

    def delete_template(self, template_id: str):
        """Deletes a template on the signature server

          Args:
            - template_id (str): The id of the template that should be deleted        
        """
        ret = self._delete('/templates/{template_id}')

    def get_templates(self) -> list[dict[str, Union[int, str]]]:
        """Gets a list of all available templates on the signature server

        Returns:
            list[dict[str, Union[int, str]]]: list of all templates as dict
        """
        ret = self._get('/templates')
        return ret['templateList']
        
    def get_template(self, template_id: int):
        """Gets a template from the signature server
        Args:
            - template_id (int): the templateID of a template

        Returns:
            _type_: a singular template
        """
        ret = self._get_raw(f'/templates/{template_id}')
        return ret

     # # # Batch Signature
    def start_signature(self, success_url: str, error_url: str):
        """Creates a new batch. Enables you to upload documents that should be signed

        Args:
            - success_url (str): The URL to redirect to, when the signature has completed successfully
            - error_url (str): The URL to redirect to, when an error occures

        Returns:
            _type_: TicketID for this batch
        """
        ret = self._post('/signaturebatches', files={'RedirectUrl': (None, success_url), 'ErrorUrl': (None, error_url)})
        return ret.headers['Location'].split('/')[-1]

    def add_document(self, ticket_id: str, filename: str) -> str:
        """Adds a document to the batch 

        Args:
            - ticket_id (str): TicketID of the batch to which the document is added to
            - filename (str): filename of the document

        Returns:
            str: The DocumentID
        """
        with open(filename, 'rb') as file:
            buf = file.read()
            ret = self._post(f'/signaturebatches/{ticket_id}/documents', files={'location': (None, 'Austria'),
                                                                                'reason': (None, ''),
                                                                                'document': (filename, buf), })
            return ret.headers['Location'].split('/')[-1]
        
    def add_document_template(self, ticket_id: str, filename: str, template_id: str) -> str:
        """Adds a document to the batch using the specified template

        Args:
            - ticket_id (str): TicketID of the batch to which the document is added to
            - filename (str): filename of the document
            - template_id (str): templateID of the template that should be used

        Returns:
            str: URL location of the uploaded document
        """
        with open(filename, 'rb') as file:
            buf = file.read()
            ret = self._post(f'/signaturebatches/{ticket_id}/documents', files={'location': (None, 'Austria'),
                                                                                'reason': (None, ''),
                                                                                'document': (filename, buf), 
                                                                                'template': (None, template_id), })
            return ret.headers['Location']

    def add_document_template_ex(self, ticket_id: str, filename: str, template_id: str, page: str, x: str, y: str, w: str, h: str) -> str:
        """Adds a document to the batch using the specified template and coordinates

        Args:
            - ticket_id (str): TicketID of the batch to which the document is added to
            - filename (str): filename of the document
            - template_id (str): templateID of the template that should be used
            - page (str): page of the signature seal
            - x (str): x0 coordinate in userspace units
            - y (str): y0 coordinate in userspace units
            - w (str): x1 coordinate in userspace units
            - h (str): y1 coordinate in userspace units

        Returns:
            str: URL location of the uploaded document
        """
        with open(filename, 'rb') as file:
            buf = file.read()
            ret = self._post(f'/signaturebatches/{ticket_id}/documents', files={'location': (None, 'Ausria'),
                                                                                'reason': (None, ''),
                                                                                'document': (filename, buf), 
                                                                               'template': (None, template_id), 
                                                                               'page': (None, page),
                                                                               'x': (None, x),
                                                                               'y': (None, y),
                                                                               'w': (None, w),
                                                                               'h': (None, h), })
            return ret.headers['Location']

    def finalize(self, ticket_id: str) -> str:
        """Start signature process for a batch

        Args:
            - ticket_id (str): TicketID of the batch to be signed

        Returns:
            str: URL location of where to sign the batch
        """
        ret = self._post(f'/signaturebatches/{ticket_id}/mobilesignature')
        return ret.headers['Location']

    def get_document(self, ticket_id: str, document_id: str):
        """Get the signed docume
        nts from your SigBox. Documents are deleted on the remote SigBox Server. Has to be executed after the signature process. 

        Args:
            - ticket_id (str): TicketID of a signed batch
            - doc_id (str): DocumentID of a document
        """
        return self._delete(f'/signaturebatches/{ticket_id}/documents/{document_id}')
