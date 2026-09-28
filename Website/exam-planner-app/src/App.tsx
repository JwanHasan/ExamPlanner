import type { ReactNode } from "react";
import {
    HashRouter,
    Navigate,
    Route,
    Routes
} from "react-router-dom";

import { AuthProvider } from "./auth/AuthContext";
import { RequireAuth } from "./auth/RequireAuth";

import Login from "./Pages/Login";
import Main from "./Pages/Main";
import CreatePlan from "./Pages/CreatePlan";
import EditPlan from "./Pages/EditPlan";
import Issues from "./Pages/Issues";
import ImportData from "./Pages/ImportData";
import Rooms from "./Pages/Rooms";
import Reviews from "./Pages/Reviews";

import "./App.css";

function Protected({ children }: { children: ReactNode }) {
    return (
        <RequireAuth>
            {children}
        </RequireAuth>
    );
}

function App() {
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>

                    <Route
                        path="/"
                        element={<Login />}
                    />

                    <Route
                        path="/main"
                        element={
                            <Protected>
                                <Main />
                            </Protected>
                        }
                    />

                    <Route
                        path="/create-plan"
                        element={
                            <Protected>
                                <CreatePlan />
                            </Protected>
                        }
                    />

                    <Route
                        path="/edit-plan"
                        element={
                            <Protected>
                                <EditPlan />
                            </Protected>
                        }
                    />

                    <Route
                        path="/issues"
                        element={
                            <Protected>
                                <Issues />
                            </Protected>
                        }
                    />

                    <Route
                        path="/import"
                        element={
                            <Protected>
                                <ImportData />
                            </Protected>
                        }
                    />

                    <Route
                        path="/rooms"
                        element={
                            <Protected>
                                <Rooms />
                            </Protected>
                        }
                    />

                    <Route
                        path="/reviews"
                        element={
                            <Protected>
                                <Reviews />
                            </Protected>
                        }
                    />

                    <Route
                        path="/plan"
                        element={<Navigate to="/main" replace />}
                    />

                    <Route
                        path="*"
                        element={<Navigate to="/" replace />}
                    />

                </Routes>
            </HashRouter>
        </AuthProvider>
    );
}

export default App;