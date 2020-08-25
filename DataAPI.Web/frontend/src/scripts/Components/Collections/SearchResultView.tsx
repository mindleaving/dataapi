import React from 'react';
import Accordion from 'react-bootstrap/Accordion';
import SearchResultItemView from './SearchResultItemView';

interface SearchResultViewProps {
    collectionName: string;
    items: any[],
    onEntryDeleted?: (dataType: string, id: string) => void;
}

const SearchResultView = (props: SearchResultViewProps) => {

    return (
        <div style={{ maxHeight: '800px', overflow: 'auto' }}>
            <Accordion>
                {props.items.map((item, idx) => (
                    <SearchResultItemView
                        key={idx + ''}
                        collectionName={props.collectionName}
                        item={item}
                        itemIndex={idx}
                        onDeleted={props.onEntryDeleted}
                    />
                ))}
            </Accordion>
        </div>
    );

}

export default SearchResultView;