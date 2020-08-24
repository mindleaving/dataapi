import React from 'react';
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom';
import Layout from './Layout';
import HomePage from './scripts/Pages/HomePage';
import CollectionsPage from './scripts/Pages/CollectionsPage';
import CreateEditCollection from './scripts/Components/Collections/Create/CreateEditCollection';
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
            path="/explore/collections" 
            render={(props) => <CollectionsPage {...props} />}
          />
          <Route
            path="/collections/new"
            render={(props) => <CreateEditCollection {...props} />}
          />
          <Route 
            path="/explore/dataprojects" 
            render={(props) => <DataProjectsPage {...props} />}
          />
          <Route 
            path="/explore/tags" 
            render={(props) => <TagsPage {...props} />}
          />
          <Route 
            path="/automation" 
            render={(props) => <DataProcessingServicePage {...props} />}
          />
          <Route 
            path="/distribution" 
            render={(props) => <DataServicesPage {...props} />}
          />
          <Route 
            path="/users" 
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
