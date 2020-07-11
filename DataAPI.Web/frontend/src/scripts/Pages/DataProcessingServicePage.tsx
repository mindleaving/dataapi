import React, { Component } from 'react';

interface DataProcessingServicePageProps {}
interface DataProcessingServicePageState {}

class DataProcessingServicePage extends Component<DataProcessingServicePageProps, DataProcessingServicePageState> {

    constructor(props: DataProcessingServicePageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
        <>
            <h1>Automation</h1>
        </>
        );
    }

}

export default DataProcessingServicePage;