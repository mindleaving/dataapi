import http.client
import ssl
import json

class DataApiClient(object):
    server_address = ""
    server_port = 443
    accesstoken = None

    def __init__(self, server_address = "", server_port = 443):
        self.server_address = server_address
        self.server_port = server_port


    def isavailable(self):
        url = self.build_url("api/servicestatus/ping")
        response, content = self._run_get_request(url)
        return response.status == 200


    def register(self, username, firstname, lastname, email, password):
        url = self.build_url( "api/account/register")
        body = {"Username" : username, "FirstName" : firstname, "LastName" : lastname, "Email" : email, "Password" : password}
        response, content = self._run_post_request(url, body = body)
        return response.status == 200


    def login(self, username, password):
        url = self.build_url( "api/account/login")
        body = {"Username" : username, "Password" : password}
        response, content = self._run_post_request(url, body = body)
        authentication_result = json.loads(content)
        if authentication_result["IsAuthenticated"]:
            self.accesstoken = authentication_result["AccessToken"]
        return authentication_result["IsAuthenticated"]


    def logout(self):
        self.accesstoken = None
        return True
    

    def changepassword(self, username, password):
        url = self.build_url( "api/account/changepassword")
        body = {"Username" : username, "Password" : password}
        response, content = self._run_post_request(url, body)
        return response.status == 200


    def deleteuser(self, username):
        url = self.build_url( "api/account/deleteuser", {"username" : username})
        response, content = self._run_delete_request(url)
        return response.status == 200


    def listcollections(self, include_hidden = False):
        url = self.build_url( "api/dataio/listcollections", {"includeHidden":include_hidden})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def listcollectionnames(self, include_hidden = False):
        url = self.build_url( "api/dataio/listcollectionnames", {"includeHidden":include_hidden})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def getcollectioninformation(self, collectionname):
        url = self.build_url( "api/dataio/getcollectioninformation", {"collectionName":collectionname})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def getcollectionpermissions(self, collectionname):
        url = self.build_url( "api/dataio/getcollectionpermissions", {"collectionName":collectionname})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def exists(self, datatype, id):
        url = self.build_url( "api/dataio/exists", {"dataType" : datatype, "id" : id})
        response, content = self._run_get_request(url)
        return response.status == 200


    def get(self, datatype, id):
        url = self.build_url( "api/dataio/get", {"dataType" : datatype, "id" : id})
        response, content = self._run_get_request(url)
        if(response.status != 200):
          raise Exception("Could not find data of type " + datatype + " and ID '" + id + "': " + response.reason + " (" + str(content) + ")")
        return json.loads(content)


    def getmany(self, datatype, query, limit = -1):
        url = self.build_url( "api/dataio/getmany", {"dataType" : datatype, "whereArguments" : query, "limit" : limit})
        response, content = self._run_get_request(url)
        if(response.status != 200):
          raise Exception("Could not get data of type " + datatype + ": " + response.reason + " (" + str(content) + ")")
        return self._read_json_lines(content)


    def search(self, query, result_format = "Json"):
        url = self.build_url( "api/dataio/search")
        body = {"Query" : query, "Format" : result_format}
        response, content = self._run_post_request(url, body)
        if(response.status != 200):
          raise Exception("Search with query '" + query + "' resulted in an error " + response.status + ": " + response.reason + " (" + str(content) + ")")
        return self._read_json_lines(content)


    def insert(self, datatype, obj, id = None):
        url = self.build_url( "api/dataio/submit")
        body = {"DataType" : datatype, "Id" : id, "Overwrite" : False, "Data" : obj}
        response, content = self._run_post_request(url, body)
        if(response.status != 200):
          raise Exception("Could not store data of type " + datatype + ": " + response.reason + " (" + str(content) + ")")
        return content


    def replace(self, datatype, obj, id = None):
        url = self.build_url( "api/dataio/submit")
        body = {"DataType" : datatype, "Id" : id, "Overwrite" : True, "Data" : obj}
        response, content = self._run_post_request(url, body)
        if(response.status != 200):
          raise Exception("Could not store data of type " + datatype + ": " + response.reason + " (" + str(content) + ")")
        return content


    def delete(self, datatype, id):
        url = self.build_url( "api/dataio/delete", {"dataType" : datatype, "id" : id})
        response, content = self._run_delete_request(url)
        return response.status == 200


    def subscribe(self, datatype, modificationTypes, filter):
        url = self.build_url( "api/subscription/subscribe")
        body = {"DataType" : datatype, "ModificationTypes" : modificationTypes, "Filter" : filter}
        response, content = self._run_post_request(url, body)
        return content


    def unsubscribe(self, subscriptionid):
        url = self.build_url( "api/subscription/unsubscribe", {"id":subscriptionid})
        response, content = self._run_get_request(url)
        return response.status == 200


    def unsubscribeall(self, datatype):
        url = self.build_url( "api/subscription/unsubscribeall", {"dataType":datatype})
        response, content = self._run_get_request(url)
        return response.status == 200


    def getsubscriptions(self, datatype = None):
        url = self.build_url( "api/subscription/getsubscriptions", {"dataType":datatype})
        response, content = self._run_get_request(url)
        return self._read_json_lines(content)


    def getsubscribedobjects(self, datatype = None):
        url = self.build_url( "api/subscription/getsubscribedobjects", {"dataType":datatype})
        response, content = self._run_get_request(url)
        return self._read_json_lines(content)


    def deletenotification(self, notificationid):
        url = self.build_url( "api/subscription/deletenotification", {"notificationId":notificationid})
        response, content = self._run_delete_request(url)
        return response.status == 200


    def reportto(self, recipient, datatype, id):
        url = self.build_url( "api/subscription/reportto", {"recipient":recipient, "dataType":datatype, "id":id})
        response, content = self._run_get_request(url)
        return response.status == 200


    def validatevalidator(self, datatype, validator):
        url = self.build_url( "api/validator/validate")
        body = {"ValidatorDefinition":{"DataType" : datatype, "ValidatorType" : "TextRules", "Ruleset" : validator}}
        response, content = self._run_post_request(url, body)
        return json.loads(content)


    def submitvalidator(self, datatype, validator):
        url = self.build_url( "api/validator/submit")
        body = {"ValidatorDefinition":{"DataType" : datatype, "ValidatorType" : "TextRules", "Ruleset" : validator}}
        response, content = self._run_post_request(url, body)
        return response.status == 200


    def getvalidator(self, validatorid):
        url = self.build_url( "api/validator/get", {"validatorId":validatorid})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def listvalidators(self, collectionname = None):
        url = self.build_url( "api/validator/getall", {"collectionName":collectionname})
        response, content = self._run_get_request(url)
        return json.loads(content)


    def approvevalidator(self, validatorid):
        url = self.build_url( "api/validator/approve", {"validatorId":validatorid})
        response, content = self._run_get_request(url)
        return response.status == 200


    def deletevalidator(self, validatorid):
        url = self.build_url( "api/validator/delete", {"validatorId":validatorid})
        response, content = self._run_delete_request(url)
        return response.status == 200


    def createview(self, query, viewid = None):
        url = self.build_url( "api/view/create")
        body = {"Query":query, "ViewId":viewid}
        response, content = self._run_post_request(url, body)
        return json.loads(content)


    def getview(self, viewid, result_format = "Json", placeholders = {}):
        query_params = {"viewId":viewid, "resultFormat":result_format}
        query_params.update(placeholders)
        url = self.build_url( "api/view/get", query_params)
        response, content = self._run_get_request(url)
        return self._read_json_lines(content)


    def deleteview(self, viewid):
        url = self.build_url( "api/view/delete", {"viewId":viewid})
        response, content = self._run_delete_request(url)
        return response.status == 200


#-----------------------------------------------
# Helper methods

        
    def build_url(self, resource_path, query = {}):
        full_query = "/" + resource_path
        first_param = True
        for key in query:
            if first_param:
                full_query += "?"
                first_param = False
            else:
                full_query += "&"
            full_query += key + "=" + str(query[key])
        return full_query
        

    def _run_get_request(self, resource_path):
        connection = http.client.HTTPSConnection(self.server_address, self.server_port)
        if(self.accesstoken == None):
            connection.request("GET", resource_path);
        else:
            headers = { "Authorization" : "Bearer " + self.accesstoken }
            connection.request("GET", resource_path, headers=headers)
        response = connection.getresponse()
        content = response.read().decode('utf-8')
        connection.close()
        return response, content


    def _run_delete_request(self, resource_path):
        connection = http.client.HTTPSConnection(self.server_address, self.server_port)
        if(self.accesstoken == None):
            connection.request("DELETE", resource_path);
        else:
            headers = { "Authorization" : "Bearer " + self.accesstoken }
            connection.request("DELETE", resource_path, headers=headers)
        response = connection.getresponse()
        content = response.read().decode('utf-8')
        connection.close()
        return response, content


    def _run_post_request(self, resource_path, body):
        connection = http.client.HTTPSConnection(self.server_address, self.server_port)
        headers = { "Content-Type": "application/json" }
        if(self.accesstoken == None):
            connection.request("POST", resource_path, body=json.dumps(body), headers=headers);
        else:
            headers.update({ "Authorization" : "Bearer " + self.accesstoken })
            connection.request("POST", resource_path, body=json.dumps(body), headers=headers)
        response = connection.getresponse()
        content = response.read().decode('utf-8')
        connection.close()
        return response, content


    def _read_json_lines(self, content):
        lines = content.splitlines()
        search_result = []
        for line in lines:
          parsed_item = json.loads(line)
          search_result.append(parsed_item)
        
        return search_result
