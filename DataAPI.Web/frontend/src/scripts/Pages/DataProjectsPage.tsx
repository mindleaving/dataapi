import React, { Component } from 'react';

interface DataProjectsPageProps {}
interface DataProjectsPageState {}

class DataProjectsPage extends Component<DataProjectsPageProps, DataProjectsPageState> {

    constructor(props: DataProjectsPageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
        <>
            <h1>Data Projects</h1>
        </>
        );
    }

}

export default DataProjectsPage;