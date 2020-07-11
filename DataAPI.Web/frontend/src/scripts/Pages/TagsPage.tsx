import React, { Component } from 'react';

interface TagsPageProps {}
interface TagsPageState {}

class TagsPage extends Component<TagsPageProps, TagsPageState> {

    constructor(props: TagsPageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
            <>
                <h1>Tags</h1>
            </>
        );
    }

}

export default TagsPage;