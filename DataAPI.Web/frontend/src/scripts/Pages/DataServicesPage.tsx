import React, { Component } from 'react';

interface DataServicesPageProps {}
interface DataServicesPageState {}

class DataServicesPage extends Component<DataServicesPageProps, DataServicesPageState> {

    constructor(props: DataServicesPageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
        <>
            <h1>Distribution</h1>
        </>
        );
    }

}

export default DataServicesPage;