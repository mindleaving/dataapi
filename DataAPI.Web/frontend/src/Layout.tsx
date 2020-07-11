import React from 'react';
import { NotificationContainer } from 'react-notifications';
import Navbar from 'react-bootstrap/Navbar';
import Nav from 'react-bootstrap/Nav';
import NavDropdown from 'react-bootstrap/NavDropdown';

interface LayoutProps {

}

const Layout : React.FC<LayoutProps> = (props) => {
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