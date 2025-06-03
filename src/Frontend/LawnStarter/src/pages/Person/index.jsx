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
    <div>
      <h1>{data.name}</h1>
      <h2>Details</h2>
      <ul>
        <li>Birth Year: {data.birthYear}</li>
        <li>Gender: {data.gender}</li>
        <li>Eye Color: {data.eyecolor}</li>
        <li>Hair Color: {data.hairColor}</li>
        <li>Height: {data.height}</li>
        <li>Mass: {data.mass}</li>
      </ul>

      <Link to={`/`}>
        <button>Back to Search</button>
      </Link>

      <h2>Movies</h2>
      {data.movies && data.movies.length > 0 ?
        data.movies
          .map(item => (
            <Link to={`/movie/${item.uid}`} key={item.uid}>
              {item.title}
            </Link>
          ))
          .reduce((prev, curr) => [prev, ", ", curr])
        : ("No movies found")}


    </div>
  );
}

export default Person;