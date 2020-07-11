import React, { Component } from 'react';
import Table from 'react-bootstrap/Table';
import { NotificationManager } from 'react-notifications';
import { dataApiClient } from '../../Communication/DataApiClient';
import { DataAPI } from '../../../types/dataApiDataStructures.d';

interface UserListProps {}
interface UserListState {
    isLoading: boolean;
    users: DataAPI.DataStructures.UserManagement.UserProfile[];
}

class UserList extends Component<UserListProps, UserListState> {

    constructor(props: UserListProps) {
        super(props);

        this.state = {
            isLoading: true,
            users: []
         };
    }

    async componentDidMount() {
        await this.loadUsers();
    }

    loadUsers = async () => {
        try {
            this.setState({
                isLoading: true
            });
            const users = await dataApiClient.getUserProfiles();
            this.setState({
                users: users
            });
        } catch(e) {
            NotificationManager.error('Could not load users', '', 5000)
        } finally {
            this.setState({
                isLoading: false
            });
        }
    }

    render() {
        let rows = null;
        if(this.state.isLoading) {
            rows = (<tr>
                <td rowSpan={5} className="text-center">Loading...</td>
            </tr>);
        } else if(this.state.users.length === 0) {
            rows = (<tr>
                <td rowSpan={5} className="text-center">No users</td>
            </tr>);
        } else {
            rows = this.state.users.map(user => 
                <tr>
                    <td></td>
                    <td>{user.username}</td>
                    <td>{user.firstName} {user.lastName}</td>
                    <td>{user.email}</td>
                    <td></td>
                </tr>
            );
        }
        return (
            <Table>
                <thead>
                    <tr>
                        <th></th>
                        <th>Username</th>
                        <th>Name</th>
                        <th>Email</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {rows}
                </tbody>
            </Table>
        );
    }

}

export default UserList;