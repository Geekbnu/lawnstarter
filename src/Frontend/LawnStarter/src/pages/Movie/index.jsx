import { useParams } from "react-router-dom";
import { Link } from "react-router-dom";
import { SearchMovieSwapiService } from "../../services/SearchMovieSwapiService";
import { useState, useEffect, React } from "react";


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
    <div>
      <h1>{data.title}</h1>
      <h2>Opening Crawl</h2>
      <p>{data.opening_crawl}</p>
      
      <Link to={`/`}>
        <button>Back to Search</button>
      </Link>
      
      <h2>Characters</h2>
      {data.characters && data.characters.length > 0 ?
        data.characters
          .map(item => (
            <Link to={`/person/${item.uid}`} key={item.uid}>
              {item.name}
            </Link>
          ))
          .reduce((prev, curr) => [prev, ", ", curr])
        : ("No characters found")}


    </div>
  );
}

export default Movie;