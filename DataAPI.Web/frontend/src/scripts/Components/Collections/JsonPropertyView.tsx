import React from 'react';
import JsonObjectView from './JsonObjectView';
import JsonArrayView from './JsonArrayView';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';

interface JsonPropertyViewProps {
    name: string;
    value: any;
}

const JsonPropertyView = (props: JsonPropertyViewProps) => {

    if(props.value && typeof props.value === "object") {
        if(Array.isArray(props.value)) {
            return (
                <>
                    <span>{props.name}</span>
                    <div className="indent-40 border-left border-bottom border-warning">
                        <JsonArrayView items={props.value} />
                    </div>
                </>
            );
        }
        return (
            <>
                <span>{props.name}:</span>
                <div className="indent-40 border-left border-bottom border-danger">
                    <JsonObjectView obj={props.value} />
                </div>
            </>
        );
    }

    return (
        <Row>
            <Col xs={4}>
                {props.name}:
            </Col>
            <Col xs={8}>
                {props.value ?? 'null'}
            </Col>
        </Row>
    );

}

export default JsonPropertyView;