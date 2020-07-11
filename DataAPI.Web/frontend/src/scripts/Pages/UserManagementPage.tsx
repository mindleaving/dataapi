import React, { Component } from 'react';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import UserList from '../Components/UserManagement/UserList';

interface UserManagementPageProps {}
interface UserManagementPageState {}

class UserManagementPage extends Component<UserManagementPageProps, UserManagementPageState> {

    constructor(props: UserManagementPageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
            <Container>
                <Row>
                    <Col>
                        <h1>User Management</h1>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <UserList

                        />
                    </Col>
                </Row>
            </Container>
        );
    }

}

export default UserManagementPage;