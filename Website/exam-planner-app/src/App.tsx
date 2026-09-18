import { HashRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./Pages/Login.tsx";
import { AuthProvider } from "./auth/AuthContext.tsx";
import { RequireAuth } from "./auth/RequireAuth.tsx";
import "./App.css";

function App() {
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>
                    <Route path="/" element={<Login />} />

                    {/* Everything below needs a signed-in user */}
                    <Route
                        path="/plan"
                        element={
                            <RequireAuth>
                                <div>Exam plan goes here</div>
                            </RequireAuth>
                        }
                    />

                    {/* Anything else goes back to the login page */}
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </HashRouter>
        </AuthProvider>
    );
}

export default App;