import { Link } from "react-router-dom";
import { useAuth } from "../../api/features/auth/AuthContext.tsx";
import viaLogo from "../Assets/via-logo-small.png";

const TopBar = () => {
    const { user } = useAuth();

    return (
        <header className="via-topbar">
            <Link className="via-brand" to="/Main">
                <img src={viaLogo} alt="VIA University College" />
                <span>Exam Plan</span>
            </Link>
            {user && (
                <span className="via-user" title={user.email}>
                    {user.name || user.email}
                </span>
            )}
        </header>
    );
};

export default TopBar;
