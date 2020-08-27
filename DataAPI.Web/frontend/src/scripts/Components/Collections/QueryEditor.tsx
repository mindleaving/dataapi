import React, { Component } from 'react';
import Form from 'react-bootstrap/Form';
import Col from 'react-bootstrap/Col';
import Button from 'react-bootstrap/Button';
import { NotificationManager } from 'react-notifications';

interface QueryEditorProps {
    collectionName: string;
    onRunQuery: (query: string) => void;
    isRunningQuery: boolean;
}
interface QueryEditorState {
    query: string;
}

class QueryEditor extends Component<QueryEditorProps, QueryEditorState> {

    constructor(props: QueryEditorProps) {
        super(props);

        this.state = {
            query: 'SELECT *\nLIMIT 3'
        };
    }

    runQuery = () => {
        if(!this.isQueryValid(this.state.query)) {
            NotificationManager.error('Query is invalid', '', 5000);
            return;
        }
        const completedQuery = this.completeQuery(this.state.query);
        this.props.onRunQuery(completedQuery)
    }

    completeQuery = (query: string) => {
        return `FROM ${this.props.collectionName}\n${query}`;
    }

    isQueryValid = (query: string) => {
        return true;
    }

    render() {
        return (
            <>
                <Col>
                    <Form.Group>
                        <Form.Label>Query:</Form.Label>
                        <Form.Control
                            as="textarea"
                            style={{ height: '90px' }}
                            value={this.state.query}
                            onChange={(e: any) => this.setState({ query: e.target.value })}
                        />
                    </Form.Group>
                </Col>
                <Col>
                    <Button
                        style={{ marginTop: '30px' }}
                        onClick={this.runQuery}
                        disabled={this.props.isRunningQuery}
                    >
                        {this.props.isRunningQuery ? 'Running...' : 'Run'}
                    </Button>
                </Col>
            </>
        );
    }

}

export default QueryEditor;