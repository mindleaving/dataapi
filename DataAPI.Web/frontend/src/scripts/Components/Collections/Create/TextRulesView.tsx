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
            spellCheck={false}
            style={{ height: '160px', overflowX: 'auto' }}
            value={props.rules.split(';').map(x => x.trim()).join('\n')}
            onChange={(e:any) => props.onTextRulesChanged(e.target.value)}
        />
    );

}

export default TextRulesView;