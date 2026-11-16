import {HashRouter, Navigate, Route, Routes} from "react-router-dom";
import {AuthProvider} from "./api/features/auth/AuthContext.tsx";
import {RequireAuth} from "./api/features/auth/RequireAuth.tsx";
import Login from "./Pages/Login.tsx";
import Main from "./Pages/Main.tsx";
import EditPlan from "./Pages/EditPlan/EditPlan.tsx";
import CreatePlan from "./Pages/CreatePlan.tsx";
import ImportPage from "./Pages/Import/ImportPage.tsx";
import ImportedDataPage from "./Pages/ImportedDataPage.tsx";
import AvailableRoomsPage from "./Pages/AvailableRoomsPage.tsx";
import "./App.css";

function App() {
    return (
        <AuthProvider>
            <HashRouter>
                <Routes>
                    <Route path="/" element={<Login />}/>
                    
                    <Route path="/Main" element={<Main />}/>
                    
                    <Route path="/EditPlan" element={<RequireAuth><EditPlan /></RequireAuth>}/>

                    <Route path="/CreatePlan" element={<CreatePlan />}/>

                    <Route path="/import" element={<RequireAuth><ImportPage /></RequireAuth>}/>
                    
                    <Route path="/imported-data" element={<RequireAuth><ImportedDataPage /></RequireAuth>}/>
                    
                    <Route path="/rooms" element={<RequireAuth><AvailableRoomsPage /></RequireAuth>}/>
                    
                    <Route path="/plan" element={<RequireAuth>
                        <div>
                            Exam plan goes here
                        </div>
                    </RequireAuth>}/>

                    <Route path="*" element={<Navigate to="/" replace/>}/>
                </Routes>
            </HashRouter>
        </AuthProvider>
    );
}

export default App;