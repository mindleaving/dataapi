import React, { useState } from 'react';
import { Switch, Route, Redirect } from 'react-router-dom';
import Layout from './Layout';
import HomePage from './scripts/Pages/HomePage';
import CollectionsPage from './scripts/Pages/CollectionsPage';
import CreateEditCollectionPage from './scripts/Pages/CreateEditCollectionPage';
import DataProjectsPage from './scripts/Pages/DataProjectsPage';
import TagsPage from './scripts/Pages/TagsPage';
import DataProcessingServicePage from './scripts/Pages/DataProcessingServicePage';
import DataServicesPage from './scripts/Pages/DataServicesPage';
import UserManagementPage from './scripts/Pages/UserManagementPage';
import { dataApiClient } from './scripts/Communication/DataApiClient';
import CreateEditDataObjectPage from './scripts/Pages/CreateEditDataObjectPage';
import LoginPage from './scripts/Pages/LoginPage';

function App() {

  const [ isLoggedIn, setIsLoggedIn ] = useState(dataApiClient.isLoggedIn());

  if (!isLoggedIn) {
    return (
      <Switch>
        <Route exact path="/login">
          <LoginPage onLoginSuccess={() => setIsLoggedIn(true)} />
        </Route>
        <Route path="*">
          <Redirect to="/login" />
        </Route>
      </Switch>
    );
  }

  return (
    <Layout onLogout={() => setIsLoggedIn(false)}>
      <Switch>
        <Route
          exact path="/"
          render={(props) => <HomePage {...props} />}
        />
        <Route
          exact path="/explore/collections/new"
          render={(props) => <CreateEditCollectionPage {...props} />}
        />
        <Route
          exact path="/explore/collections/:selectedCollection?"
          render={(props) => <CollectionsPage {...props} />}
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
          exact path="/edit/:dataType/:id?"
          render={(props) => <CreateEditDataObjectPage {...props} />}
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
  );
}

export default App;
