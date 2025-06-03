import { useParams } from "react-router-dom";
import { Link } from "react-router-dom";
import { SearchMovieSwapiService } from "../../services/SearchMovieSwapiService";
import { useState, useEffect, React } from "react";
import './Movie.css';


function Movie() {

  const { id, type } = useParams();
  const [data, setData] = useState([]);

  useEffect(() => {

    const loadData = async () => {
      try {
        const results = await SearchMovieSwapiService(id);
        setData(results);
      } catch (error) {
        console.log(error);
      }
    }

    loadData();
  }, []);

  return (
    <div className="character-details">
      <h1 className="character-name">{data.title}</h1>

      <div className="details-grid">
        <div className="details-section" style={{ whiteSpace: 'pre-line' }}>
          <h2 className="section-title">Opening Crawl</h2>
            <p>{data.opening_crawl}</p>
        </div>

        <div className="details-section">
          <h2 className="section-title">Characters</h2>

          <div className="detail-item">

            {data.characters && data.characters.length > 0 ?
              data.characters
                .map(item => (
                  <Link to={`/person/${item.uid}`} key={item.uid} className="movie-link">
                    {item.name}
                  </Link>
                ))
                .reduce((prev, curr) => [prev, ", ", curr])
              : ("No movies found")}
          </div>
        </div>
      </div>

      <Link to={`/`}>
        <button className='back-button'>Back to Search</button>
      </Link>
    </div>
  );
}

export default Movie;