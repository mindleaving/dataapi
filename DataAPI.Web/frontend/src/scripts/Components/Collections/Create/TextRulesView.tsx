import React from 'react';
import Form from 'react-bootstrap/Form';

interface TextRulesViewProps {
    rules: string,
    onTextRulesChanged: (rules: string) => void;
}

const TextRulesView = (props: TextRulesViewProps) => {

    return (
        <Form.Control required
            as="textarea"
            value={props.rules}
            onChange={(e:any) => props.onTextRulesChanged(e.target.value)}
        />
    );

}

export default TextRulesView;