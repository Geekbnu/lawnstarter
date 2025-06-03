import './Results.css';

import { Link } from "react-router-dom";
function Results({ data = [], error, isLoading = false }) {

  var content = "";

  if (error) {
    content = <p>{error}</p>;
  }

  if (!data || !Array.isArray(data) || data.length === 0) {
    content = <div className='no-results'>There are zero matches. <br/> Use the form to search for People or Movies.</div>;
  }
  else if (isLoading) {
    content = <div className='no-results'>Searching...</div>;
  }
  else {
    content = <>
      {data.map((item) => (
        <div className="result-item" key={item.uid}>
          <div className="result-name">{item.name}</div>
          <Link to={`/${item.resource === "People" ? "person" : "movie"}/${item.uid}`}>
            <button className="result-details-button">
              SEE DETAILS
            </button>
          </Link>
        </div>
      ))}
    </>;
  }

  return (
    <>
      <section className="results-section">
        <h2 className="results-title">Results</h2>
        <div className="results-divider"></div>
        {content}
      </section>
    </>
  );
}

export default Results;