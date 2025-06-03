import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Movie from "./pages/Movie";
import Person from "./pages/Person";

function AppRoutes() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/Movie/:id" element={<Movie />} />
                <Route path="/Person/:id" element={<Person />} />
            </Routes>
        </BrowserRouter>
    )
}

export default AppRoutes;
