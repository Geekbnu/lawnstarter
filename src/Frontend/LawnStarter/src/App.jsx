import './styles/App.css';
import Header from './components/Header';
import AppRoutes from "./route.jsx";
function App() {

  return (
    <>
      <Header/>
      <div className="container" >
        <div className="main-content">
          <AppRoutes />
        </div>
      </div>
    </>
  );
}

export default App;