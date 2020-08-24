import React from 'react';
import JsonPropertyView from './JsonPropertyView';

interface JsonObjectViewProps {
    obj: any;
}

const JsonObjectView = (props: JsonObjectViewProps) => {

    if(!props.obj) {
        return (<span>null</span>);
    }
    return (
        <>
            {Object.entries(props.obj).map(([ name, value]: any) => 
                (<JsonPropertyView key={name} name={name} value={value} />) 
            )}
        </>
    );

}

export default JsonObjectView;