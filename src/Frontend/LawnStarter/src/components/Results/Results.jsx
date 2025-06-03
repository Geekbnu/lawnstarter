import { Link } from "react-router-dom";
function Results({ data = [], error, isLoading = false }) {

  var content = "";

  if (error) {
    content = <p>{error}</p>;
  }

  if (!data || !Array.isArray(data) || data.length === 0) {
    content = <p>There are zero matches. Use the form to search for People or Movies.</p>;
  }
  else if (isLoading) {
    content = <p>Loading...</p>;
  }
  else {
    content = <div>
      {data.map((item) => (
        <div key={item.uid}>
          <h3>
            {item.name}
          </h3>
          <Link to={`/${item.resource === "People" ? "person" : "movie"}/${item.uid}`}>
            <button>
              SEE DETAILS
            </button>
          </Link>
        </div>
      ))}
    </div>
  }

  return (
    <div>
      <h2>Results</h2>
      {content}
    </div>
  );
}

export default Results;