import React, { Component } from 'react';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Carousel from 'react-bootstrap/Carousel';
import { RouteComponentProps } from 'react-router-dom';

interface HomePageProps extends RouteComponentProps {}
interface HomePageState {}

class HomePage extends Component<HomePageProps, HomePageState> {

    constructor(props: HomePageProps) {
        super(props);

        this.state = { };
    }

    render() {
        return (
            <Container>
                <Row>
                    <Col>
                        <h1>The Portal To All Your Data!</h1>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <h2>Explore</h2>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        {/* <Carousel>
                            <Carousel.Item onClick={() => this.props.history.push("/explore/collections")}>
                                <div></div>
                                <Carousel.Caption>
                                    <h3>Collections</h3>
                                    <p>Search and explore data</p>
                                </Carousel.Caption>
                            </Carousel.Item>
                            <Carousel.Item onClick={() => this.props.history.push("/explore/dataprojects")}>
                                <div></div>
                                <Carousel.Caption>
                                    <h3>Data projects</h3>
                                    <p>Create data projects and find associated data</p>
                                </Carousel.Caption>
                            </Carousel.Item>
                            <Carousel.Item onClick={() => this.props.history.push("/explore/tags")}>
                                <div></div>
                                <Carousel.Caption>
                                    <h3>Tags</h3>
                                    <p>Explore tagged data</p>
                                </Carousel.Caption>
                            </Carousel.Item>
                        </Carousel> */}
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <h2>Automate</h2>
                    </Col>
                </Row>
                <Row>
                    <Col>
                        <h2>Distribute</h2>
                    </Col>
                </Row>
            </Container>
        );
    }

}

export default HomePage;