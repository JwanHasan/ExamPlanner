import {HashRouter, Navigate, Route, Routes} from "react-router-dom";
import {AuthProvider} from "./api/features/auth/AuthContext.tsx";
import {RequireAuth} from "./api/features/auth/RequireAuth.tsx";
import Login from "./Pages/Login.tsx";
import Main from "./Pages/Main.tsx";
import EditPlan from "./Pages/EditPlan.tsx";
import CreatePlan from "./Pages/CreatePlan.tsx";
import ImportPage from "./Pages/Import/ImportPage.tsx";
import ImportedDataPage from "./Pages/ImportedDataPage.tsx";
import EditImportedDataPage from "./Pages/EditImportedDataPage.tsx";
import "./App.css";

function App()
{
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <Login />
                        }
                    />
                    <Route
                        path="/Main"
                        element={
                            <RequireAuth>
                                <Main />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/EditPlan"
                        element={
                            <RequireAuth>
                                <EditPlan />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/CreatePlan"
                        element={
                            <RequireAuth>
                                <CreatePlan />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/import"
                        element={
                            <RequireAuth>
                                <ImportPage />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/imported-data"
                        element={
                            <RequireAuth>
                                <ImportedDataPage />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/imported-data/:id/edit"
                        element={
                            <RequireAuth>
                                <EditImportedDataPage />
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="/plan"
                        element={
                            <RequireAuth>
                                <div>
                                    Exam plan goes here
                                </div>
                            </RequireAuth>
                        }
                    />
                    <Route
                        path="*"
                        element={
                            <Navigate
                                to="/"
                                replace
                            />
                        }
                    />
                </Routes>
            </HashRouter>
        </AuthProvider>
    );
}

export default App;