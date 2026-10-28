import { useNavigate } from "react-router-dom";
import { useAuth } from "../api/features/auth/AuthContext.tsx"
import viaLogo from "./Assets/via-logo-small.png";
import campusBackground from "./Assets/campus-background-transparent.png";
import "./Themes/Main.css";
import "./Themes/PageSelect.css";

const Main = () => {
    const { user } = useAuth();
    const navigate = useNavigate();

    const name = user?.name ?? "Guest";

    return (
        <main className="main-page" style={{ backgroundImage: `url(${campusBackground})` }}>
            <header className="calendar-topbar">
                <img className="main-logo" src={viaLogo} alt="VIA University College" />
                <nav className="page-nav">
                    <button className="page-name-selected">Pages overview</button>
                </nav>
            </header>
            <section className="main-panel">
                <div className="main-actions">
                    <button className="main-button" onClick={() => navigate("/CreatePlan")}>
                        Create new exam plan
                    </button>
                    <button className="main-button" onClick={() => navigate("/EditPlan")}>
                        Edit current exam plan
                    </button>
                    <button className="main-button">
                        Scan for issues with current plan
                    </button>
                    <button className="main-button" onClick={() => navigate("/imported-data")}>
                        View imported data
                    </button>

                    <div className="main-actions-row">
                        <button className="main-button main-button-small" onClick={() => navigate("/import")}>
                            Import data
                        </button>
                        <button className="main-button main-button-small">
                            Export exam plan
                        </button>
                    </div>
                </div>

                <h1 className="main-welcome">
                    Welcome,
                    <br />
                    {name}!
                </h1>
            </section>
        </main>
    );
};

export default Main;