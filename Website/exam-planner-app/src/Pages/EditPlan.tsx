
import viaLogo from "./Assets/via-logo-small.png";
import campusBackground from "./Assets/campus-background-transparent.png";
import "./Themes/EditPlan.css";

const EditPlan = () => {


    return (
        <main className="main-page" style={{ backgroundImage: `url(${campusBackground})` }}>
            <header className="main-header">
                <img className="main-logo" src={viaLogo} alt="VIA University College" />
            </header>
            <h1 className="tempHeader">My balls itch</h1>
        </main>
    );
};

export default EditPlan;
