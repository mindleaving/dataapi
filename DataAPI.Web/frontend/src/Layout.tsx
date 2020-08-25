import React from 'react';
import { NotificationContainer } from 'react-notifications';
import Navbar from 'react-bootstrap/Navbar';
import Nav from 'react-bootstrap/Nav';
import NavDropdown from 'react-bootstrap/NavDropdown';
import { dataApiClient } from './scripts/Communication/DataApiClient';
import { useHistory } from 'react-router-dom';
import Button from 'react-bootstrap/Button';

interface LayoutProps {
    onLogout?: () => void;
}

const Layout : React.FC<LayoutProps> = (props) => {
    const history = useHistory();

    const logout = async () => {
        await dataApiClient.logout();
        if(props.onLogout) {
            props.onLogout();
        }        
        history.push('/login');
    }

    return (
        <>
            <NotificationContainer />
            <Navbar bg="light" expand="lg">
                <Navbar.Brand href="/">DataAPI</Navbar.Brand>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="navbar-collapse">
                    <Nav className="mr-auto">
                        <NavDropdown title="Explore" id="explore-nav-dropdown">
                            <NavDropdown.Item href="/explore/collections">Collections</NavDropdown.Item>
                            <NavDropdown.Item href="/explore/dataprojects">Data projects</NavDropdown.Item>
                            <NavDropdown.Item href="/explore/tags">Tags</NavDropdown.Item>
                        </NavDropdown>
                        <Nav.Link href="/automation">Automation</Nav.Link>
                        <Nav.Link href="/distribution">Distribution</Nav.Link>
                    </Nav>
                    <Nav>
                        <Nav.Link href="/users">Configuration</Nav.Link>
                        <Nav.Link className="text-dark" disabled><b>Hello, {dataApiClient.getLoggedInUsername()}!</b></Nav.Link>
                        <Button className="mx-2" size="sm" onClick={logout}>Logout</Button>
                    </Nav>
                </Navbar.Collapse>
            </Navbar>
            <div id="main">
                {props.children}
            </div>
        </>
    );
}

export default Layout;