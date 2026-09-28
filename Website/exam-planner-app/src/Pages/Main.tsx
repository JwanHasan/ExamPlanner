import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext.tsx";
import viaLogo from "./Assets/via-logo-small.png";
import campusBackground from "./Assets/campus-background-transparent.png";
import "./Themes/Main.css";

const Main = () => {
    const { user } = useAuth();
    const navigate = useNavigate();

    const name = user?.name ?? "Guest";

    return (
        <main className="main-page" style={{ backgroundImage: `url(${campusBackground})` }}>
            <header className="main-header">
                <img className="main-logo" src={viaLogo} alt="VIA University College" />
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

                    <div className="main-actions-row">
                        <button className="main-button main-button-small">
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
