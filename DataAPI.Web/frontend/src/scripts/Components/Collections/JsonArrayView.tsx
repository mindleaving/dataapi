import React from 'react';
import JsonObjectView from './JsonObjectView';

interface JsonArrayViewProps {
    items: any[];
}

const JsonArrayView = (props: JsonArrayViewProps) => {

    const renderItem = (item: any, itemIndex: number) => {
        if(typeof item === "object") {
            if(Array.isArray(item)) {
                return (<JsonArrayView key={itemIndex + ''} items={item} />);
            }
            return (<JsonObjectView obj={item} />);
        }
        return (<>{item}</>);
    }

    return (
        <>
            {props.items.map((item: any, itemIndex: number) => (
                <div>
                    {renderItem(item, itemIndex)}
                </div>
            ))}
        </>
    );

}

export default JsonArrayView;