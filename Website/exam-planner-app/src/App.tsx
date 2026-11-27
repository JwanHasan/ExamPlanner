import { HashRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./api/features/auth/AuthContext.tsx";
import { RequireAuth } from "./api/features/auth/RequireAuth.tsx";
import { ADMIN, STUDENT, TEACHER } from "./api/features/auth/roles.ts";
import Login from "./Pages/Login.tsx";
import Main from "./Pages/Main.tsx";
import EditPlan from "./Pages/EditPlan/EditPlan.tsx";
import CreatePlan from "./Pages/CreatePlan.tsx";
import ImportPage from "./Pages/Import/ImportPage.tsx";
import ImportedDataPage from "./Pages/ImportedDataPage.tsx";
import ExamNotesPage from "./Pages/ExamNotesPage.tsx";
import MyCoursesPage from "./Pages/MyCoursesPage.tsx";
import "./App.css";

function App() {
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>
                    <Route path="/" element={<Login />} />

                    {/* Planners only */}
                    <Route path="/Main" element={<RequireAuth roles={[ADMIN]}><Main /></RequireAuth>} />
                    <Route path="/EditPlan" element={<RequireAuth roles={[ADMIN]}><EditPlan /></RequireAuth>} />
                    <Route path="/CreatePlan" element={<RequireAuth roles={[ADMIN]}><CreatePlan /></RequireAuth>} />
                    <Route path="/import" element={<RequireAuth roles={[ADMIN]}><ImportPage /></RequireAuth>} />

                    {/* Planners and teachers */}
                    <Route path="/imported-data" element={<RequireAuth roles={[ADMIN, TEACHER]}><ImportedDataPage /></RequireAuth>} />
                    <Route path="/exam-notes" element={<RequireAuth roles={[ADMIN, TEACHER]}><ExamNotesPage /></RequireAuth>} />

                    {/* Students */}
                    <Route path="/my-courses" element={<RequireAuth roles={[STUDENT]}><MyCoursesPage /></RequireAuth>} />

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
