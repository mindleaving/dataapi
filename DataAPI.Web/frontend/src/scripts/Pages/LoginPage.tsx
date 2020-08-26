import React, { FormEvent, useState } from 'react';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import { dataApiClient } from '../Communication/DataApiClient';
import { useHistory } from 'react-router-dom';
import { LoginMethod } from '../../types/dataApiDataStructuresEnums';

interface LoginPageProps {
    onLoginSuccess?: () => void;
}

const LoginPage = (props: LoginPageProps) => {

    const [ loginMethod, setLoginMethod ] = useState(LoginMethod.JsonWebToken);
    const [ username, setUsername ] = useState('');
    const [ password, setPassword ] = useState('');
    const [ isLoggingIn, setIsLoggingIn ] = useState(false);
    const [ errorMessage, setErrorMessage ] = useState<string | null>(null);
    const history = useHistory();

    const login = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoggingIn(true);
        try {
            let authenticationResult;
            if(loginMethod === LoginMethod.ActiveDirectory) {
                authenticationResult = await dataApiClient.loginWithAD();
            } else if(loginMethod === LoginMethod.JsonWebToken) {
                authenticationResult = await dataApiClient.login(username, password);
            } else {
                throw new Error(`Unsupported login method '${loginMethod}'`);
            }
            if(authenticationResult.isAuthenticated) {
                if(props.onLoginSuccess) {
                    props.onLoginSuccess();
                }
                history.push('/');
            } else {
                setErrorMessage(authenticationResult.error.toString()); // TODO: Format/improve text
            }
        } catch(e) {
            setErrorMessage(e.message);
        } finally {
            setIsLoggingIn(false);
        }
    }

    return (
        <Container>
            <Row>
                <Col>
                    <h1>Login</h1>
                </Col>
            </Row>
            <Row className="m-3">
                <Col sm={'auto'}>
                    Login method:
                </Col>
                <Col>
                    <Button
                        className="mx-2"
                        variant={loginMethod === LoginMethod.JsonWebToken ? 'primary': 'outline-primary'} 
                        onClick={() => setLoginMethod(LoginMethod.JsonWebToken)}
                    >
                        Username/password
                    </Button>
                    <Button 
                        className="mx-2"
                        variant={loginMethod === LoginMethod.ActiveDirectory ? 'primary': 'outline-primary'} 
                        onClick={() => setLoginMethod(LoginMethod.ActiveDirectory)}
                    >
                        Windows
                    </Button>
                </Col>
            </Row>
            <Row>
                <Col>
                    <Form className="needs-validation was-validated" onSubmit={login}>
                        {loginMethod === LoginMethod.JsonWebToken ?
                        <>
                            <Form.Group>
                                <Form.Label>Username</Form.Label>
                                <Form.Control
                                    value={username}
                                    onChange={(e: any) => setUsername(e.target.value)}
                                />
                            </Form.Group>
                            <Form.Group>
                                <Form.Label>Password</Form.Label>
                                <Form.Control
                                    type="password"
                                    value={password}
                                    onChange={(e: any) => setPassword(e.target.value)}
                                />
                            </Form.Group>
                        </> : null}
                        {errorMessage ? 
                            <div className="my-2">
                                <b style={{ color: 'red' }}>{errorMessage}</b>
                            </div> : null
                        }
                        <Button
                            className="m-3"
                            type="submit"
                            disabled={isLoggingIn}
                        >
                            {isLoggingIn
                                ? 'Logging in...'
                                : 'Login'
                            }
                        </Button>
                    </Form>
                </Col>
            </Row>
        </Container>
    );

}

export default LoginPage;