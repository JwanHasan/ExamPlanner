import {
    useState
} from "react";

import type {
    FormEvent
} from "react";

import {
    useLocation,
    useNavigate
} from "react-router-dom";

import {
    useAuth
} from "../auth/AuthContext";

import {
    getErrorMessage
} from "../api/authApi";

import viaLogo
    from "./Assets/via-logo.png";

import "./Themes/Login.css";

const Login = () => {

    const [
        email,
        setEmail
    ] =
        useState("");

    const [
        password,
        setPassword
    ] =
        useState("");

    const [
        error,
        setError
    ] =
        useState<string | null>(
            null
        );

    const [
        busy,
        setBusy
    ] =
        useState(false);

    const {
        signIn,
        startUiDemo
    } =
        useAuth();

    const navigate =
        useNavigate();

    const location =
        useLocation();

    const from =
        (
            location.state as
                | {
                from?: string;
            }
                | null
        )?.from ?? "/main";

    const handleSubmit =
        async (
            event: FormEvent
        ) => {

            event.preventDefault();

            setError(null);

            setBusy(true);

            try {

                await signIn({
                    email:
                        email.trim(),

                    password
                });

                navigate(
                    from,
                    {
                        replace: true
                    }
                );

            } catch (err) {

                setError(
                    getErrorMessage(err)
                );

            } finally {

                setBusy(false);
            }
        };

    const handleDemo = () => {

        startUiDemo();

        navigate("/main");
    };

    return (
        <main
            className="login"
        >

            <section
                className="login-panel"
                aria-labelledby={
                    "login-title"
                }
            >

                <img
                    className="login-logo"
                    src={viaLogo}
                    alt={
                        "VIA University College"
                    }
                />

                <p
                    className="login-kicker"
                >
                    Exam Planning
                </p>

                <h1
                    id="login-title"
                >
                    Sign in
                </h1>

                <p
                    className="login-intro"
                >
                    Use your VIA account
                    to open the exam
                    planning system.
                </p>

                <form
                    className="login-form"
                    onSubmit={
                        handleSubmit
                    }
                    noValidate
                >

                    <label
                        className="login-field"
                    >
                        <span>
                            Email address
                        </span>

                        <input
                            type="email"
                            autoComplete={
                                "username"
                            }
                            placeholder={
                                "name@via.dk"
                            }
                            value={email}
                            onChange={
                                event =>
                                    setEmail(
                                        event
                                            .target
                                            .value
                                    )
                            }
                            disabled={
                                busy
                            }
                            required
                        />
                    </label>

                    <label
                        className="login-field"
                    >
                        <span>
                            Password
                        </span>

                        <input
                            type="password"
                            autoComplete={
                                "current-password"
                            }
                            placeholder={
                                "Password"
                            }
                            value={
                                password
                            }
                            onChange={
                                event =>
                                    setPassword(
                                        event
                                            .target
                                            .value
                                    )
                            }
                            disabled={
                                busy
                            }
                            required
                        />
                    </label>

                    {error && (
                        <p
                            className={
                                "login-error"
                            }
                            role="alert"
                        >
                            {error}
                        </p>
                    )}

                    <button
                        className={
                            "primary-button full-width"
                        }
                        type="submit"
                        disabled={
                            busy ||
                            !email ||
                            !password
                        }
                    >
                        {
                            busy
                                ? "Signing in…"
                                : "Log in"
                        }
                    </button>

                </form>

                <button
                    className={
                        "text-button"
                    }
                    type="button"
                >
                    I don't remember
                    my email or password
                </button>

                <div
                    className="demo-box"
                >

                    <span>
                        Frontend development
                    </span>

                    <button
                        className={
                            "secondary-button full-width"
                        }
                        type="button"
                        onClick={
                            handleDemo
                        }
                    >
                        Preview UI as admin
                    </button>

                    <small>
                        Temporary while
                        the backend login
                        endpoint is not
                        implemented.
                    </small>

                </div>

            </section>

        </main>
    );
};

export default Login;