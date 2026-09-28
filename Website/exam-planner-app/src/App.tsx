import { HashRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./Pages/Login.tsx";
import Main from "./Pages/Main.tsx";
import { AuthProvider } from "./api/features/auth/AuthContext.tsx";
import { RequireAuth } from "./api/features/auth/RequireAuth.tsx";
import "./App.css";
import EditPlan from "./Pages/EditPlan.tsx";
import CreatePlan from "./Pages/CreatePlan.tsx";

function App() {
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>
                    <Route path="/" element={<Login />} />

                    <Route path="/Main" element={<Main />} />

                    <Route path="/EditPlan" element={<EditPlan />} />

                    <Route path="/CreatePlan" element={<CreatePlan />} />

                    <Route
                        path="/plan"
                        element={
                            <RequireAuth>
                                <div>Exam plan goes here</div>
                            </RequireAuth>
                        }
                    />

                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </HashRouter>
        </AuthProvider>
    );
}

export default App;