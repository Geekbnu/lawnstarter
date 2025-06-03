import './Person.css';
import { useParams } from "react-router-dom";
import { Link } from "react-router-dom";
import { SearchPersonSwapiService } from "../../services/SearchPersonSwapiService";
import { useState, useEffect } from "react";


function Person() {

  const { id, type } = useParams();
  const [data, setData] = useState([]);

  useEffect(() => {

    const loadData = async () => {
      try {
        const results = await SearchPersonSwapiService(id);
        setData(results);
      } catch (error) {
        console.log(error);
      }
    }

    loadData();
  }, []);

  return (

    <div className="character-details">
      <h1 className="character-name">{data.name}</h1>

      <div className="details-grid">
        <div className="details-section">
          <h2 className="section-title">Details</h2>

          <ul>
            <li>Birth Year: {data.birthYear}</li>
            <li>Gender: {data.gender}</li>
            <li>Eye Color: {data.eyecolor}</li>
            <li>Hair Color: {data.hairColor}</li>
            <li>Height: {data.height}</li>
            <li>Mass: {data.mass}</li>
          </ul>

        </div>

        <div className="details-section">
          <h2 className="section-title">Movies</h2>

          <div className="detail-item">

            {data.movies && data.movies.length > 0 ?
              data.movies
                .map(item => (
                  <Link to={`/movie/${item.uid}`} key={item.uid} className="movie-link">
                    {item.title}
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

export default Person;