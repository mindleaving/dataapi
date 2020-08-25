import React from 'react';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom';
import Layout from './Layout';
import HomePage from './scripts/Pages/HomePage';
import CollectionsPage from './scripts/Pages/CollectionsPage';
import CreateEditCollectionPage from './scripts/Pages/CreateEditCollectionPage';
import DataProjectsPage from './scripts/Pages/DataProjectsPage';
import TagsPage from './scripts/Pages/TagsPage';
import DataProcessingServicePage from './scripts/Pages/DataProcessingServicePage';
import DataServicesPage from './scripts/Pages/DataServicesPage';
import UserManagementPage from './scripts/Pages/UserManagementPage';

function App() {
  return (
    <Router>
      <Layout>
        <Switch>
          <Route 
            exact path="/" 
            render={(props) => <HomePage {...props} />}
          />
          <Route 
            exact path="/explore/collections" 
            render={(props) => <CollectionsPage {...props} />}
          />
          <Route
            exact path="/explore/collections/new"
            render={(props) => <CreateEditCollectionPage {...props} />}
          />
          <Route
            exact path="/explore/collections/edit/:collectionName"
            render={(props) => <CreateEditCollectionPage {...props} />}
          />
          <Route 
            exact path="/explore/dataprojects" 
            render={(props) => <DataProjectsPage {...props} />}
          />
          <Route 
            exact path="/explore/tags" 
            render={(props) => <TagsPage {...props} />}
          />
          <Route 
            exact path="/automation" 
            render={(props) => <DataProcessingServicePage {...props} />}
          />
          <Route 
            exact path="/distribution" 
            render={(props) => <DataServicesPage {...props} />}
          />
          <Route 
            exact path="/users" 
            render={(props) => <UserManagementPage {...props} />}
          />
          <Route 
            path="*" 
            render={(props) => <HomePage {...props} />}
          />
        </Switch>
      </Layout>
    </Router>
  );
}

export default App;
