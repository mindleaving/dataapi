import unittest
import uuid
from datetime import datetime
from dataapi import DataApiClient

class TestDataApiClient(unittest.TestCase):
    
    def test_can_insert_and_get_item_from_id(self):
        sut = self._create_data_api_client()
        sut.login("","")
        data_type = "UnitTestDataObject1"
        test_object = { "Id" : str(uuid.uuid4()), "Timestamp" : datetime.now().strftime('%Y-%m-%d %H:%M:%S'), "Objects" : []}
        ID = sut.insert(data_type,test_object)
        actual = sut.get(data_type, ID)
        self.assertEqual(actual,test_object)
        self.assertTrue(sut.delete(data_type, ID))


    def test_can_list_collections(self):
        sut = self._create_data_api_client()
        actual = sut.listcollections()
        self.assertTrue(len(actual))


    def test_can_getmany(self):
        sut = self._create_data_api_client()
        actual = sut.getmany("Location","")
        self.assertTrue(len(actual))


    def test_can_search(self):
        sut = self._create_data_api_client()
        actual = sut.search("SELECT * FROM Location")
        self.assertTrue(len(actual))
        

    def _create_data_api_client(self):
        sut = DataApiClient(server_port = 44387)
        with open("") as file:
            password = file.read()
        sut.login("unittestadmin",password)
        return sut;


if __name__ == '__main__':
    unittest.main()
