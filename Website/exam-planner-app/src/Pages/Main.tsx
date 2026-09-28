import { useRef } from "react";
import type { ChangeEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../api/features/auth/AuthContext.tsx"
import { useImport } from "../api/features/imports/useImport";
import ImportDialog from "./ImportDialog.tsx";
import viaLogo from "./Assets/via-logo-small.png";
import campusBackground from "./Assets/campus-background-transparent.png";
import "./Themes/Main.css";

const Main = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const importer = useImport();
    const fileInput = useRef<HTMLInputElement>(null);

    const name = user?.name ?? "Guest";

    const handleFileChosen = (event: ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        // Reset so picking the same file again still triggers onChange.
        event.target.value = "";
        if (file) importer.start(file);
    };

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
                        <button
                            className="main-button main-button-small"
                            onClick={() => fileInput.current?.click()}
                            disabled={importer.state.busy}
                        >
                            {importer.state.busy ? "Importing" : "Import data"}
                        </button>
                        <button className="main-button main-button-small">
                            Export exam plan
                        </button>
                    </div>

                    <input
                        ref={fileInput}
                        type="file"
                        accept=".xlsx,.xls"
                        onChange={handleFileChosen}
                        hidden
                    />
                </div>

                <h1 className="main-welcome">
                    Welcome,
                    <br />
                    {name}!
                </h1>
            </section>

            <ImportDialog state={importer.state} onClose={importer.close} />
        </main>
    );
};

export default Main;