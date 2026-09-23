import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext.tsx";
import { getErrorMessage } from "../api/authApi";
import viaLogo from "./Assets/via-logo.png";
import "./Themes/Login.css";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    const { signIn } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setError(null);
        setBusy(true);

        try {
            await signIn({ email: email.trim(), password });
            navigate("/plan");
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setBusy(false);
        }
    };

    return (
        <main className="login">
            <section className="login-panel">
                <img className="login-logo" src={viaLogo} alt="VIA University College" />

                <form className="login-form" onSubmit={handleSubmit} noValidate>
                    <label className="login-field">
                        <span className="login-label">Email address</span>
                        <input
                            type="email"
                            name="email"
                            autoComplete="username"
                            placeholder="Email address"
                            value={email}
                            onChange={e => setEmail(e.target.value)}
                            disabled={busy}
                            required
                        />
                    </label>

                    <label className="login-field">
                        <span className="login-label">Password</span>
                        <input
                            type="password"
                            name="password"
                            autoComplete="current-password"
                            placeholder="Password"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            disabled={busy}
                            required
                        />
                    </label>

                    {error && <p className="login-error" role="alert">{error}</p>}

                    <button className="login-submit" type="submit" disabled={busy}>
                        {busy ? "Signing in" : "Log in to Exam Plan"}
                    </button>

                    <a className="login-help" href="#/reset-password">
                        I don't remember my email or password
                    </a>
                    <button className="temp-button" onClick={() => navigate("/Main")}>
                        Temporary button to go to the main page while backend don't have shit
                    </button>

                </form>
            </section>
        </main>
    );
};

export default Login;