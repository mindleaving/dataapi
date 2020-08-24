import React, { Component } from 'react';
import Accordion from 'react-bootstrap/Accordion';
import SearchResultItemView from './SearchResultItemView';

interface SearchResultViewProps {
    items: any[]
}
interface SearchResultViewState {}

class SearchResultView extends Component<SearchResultViewProps, SearchResultViewState> {

    constructor(props: SearchResultViewProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
            <div style={{ maxHeight: '800px', overflow: 'auto' }}>
                <Accordion>
                    {this.props.items.map((item, idx) => (
                        <SearchResultItemView
                            item={item}
                            itemIndex={idx}
                        />
                    ))}
                </Accordion>
            </div>
        );
    }

}

export default SearchResultView;