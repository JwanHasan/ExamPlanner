import {
    useNavigate
} from "react-router-dom";

import {
    useAuth
} from "../auth/AuthContext";

import viaLogo
    from "../Pages/Assets/via-logo-small.png";

interface AppHeaderProps {
    title?: string;
    showBack?: boolean;
}

const AppHeader = (
    {
        title,
        showBack = true
    }: AppHeaderProps
) => {

    const navigate =
        useNavigate();

    const {
        user,
        signOut
    } =
        useAuth();

    const handleSignOut = () => {
        signOut();
        navigate("/");
    };

    return (
        <header
            className="app-header"
        >
            <div
                className="app-header-left"
            >

                {showBack && (
                    <button
                        className="icon-button"
                        type="button"
                        onClick={
                            () =>
                                navigate(-1)
                        }
                        aria-label="Go back"
                    >
                        ←
                    </button>
                )}

                <button
                    className="logo-button"
                    type="button"
                    onClick={
                        () =>
                            navigate(
                                "/main"
                            )
                    }
                    aria-label={
                        "Go to dashboard"
                    }
                >
                    <img
                        className="app-logo"
                        src={viaLogo}
                        alt={
                            "VIA University College"
                        }
                    />
                </button>

                {title && (
                    <span
                        className={
                            "app-header-title"
                        }
                    >
                        {title}
                    </span>
                )}

            </div>

            <div
                className="app-header-right"
            >

                {user && (
                    <span
                        className="user-chip"
                    >
                        {
                            user.name ||
                            user.email
                        }
                    </span>
                )}

                <button
                    className={
                        "secondary-button compact"
                    }
                    type="button"
                    onClick={
                        handleSignOut
                    }
                >
                    Sign out
                </button>

            </div>
        </header>
    );
};

export default AppHeader;