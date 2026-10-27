import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../api/features/auth/AuthContext.tsx";
import { getCurrentUser, getErrorMessage } from "../api/authApi";
import viaLogo from "./Assets/via-logo.png";
import "./Themes/Login.css";

// Where each role lands after logging in. Exam planners ("Admin" in the token) go to the main page.
// Teachers (and, for now, students) have no page of their own yet: the notification page does not
// exist, so they land on the logged-in placeholder. Change OTHER_HOME when that page is added.
const ADMIN_HOME = "/Main";
const OTHER_HOME = "/plan";

const homeFor = (role: string | undefined) => (role === "Admin" ? ADMIN_HOME : OTHER_HOME);

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
            // signIn has stored the token in the cookie, so the role can be read back from it.
            navigate(homeFor(getCurrentUser()?.role));
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