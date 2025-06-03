const SWAPI_URL = 'http://localhost:8080/api';

export const SearchMovieSwapiService = async (id) => {
    if (!id) {
        return null;
    }

    const url = `${SWAPI_URL}/movies/${id}`;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }

        const result = await response.json();

        const mappedData = {
            title: result.title,
            uid: result.uid,
            opening_crawl: result.openingCrawl,
            characters: result.characters
        };

        return mappedData;

    } catch (error) {
        console.error('Erro ao buscar filme:', error);
        throw error;
    }
};

export const SearchPersonSwapiService = async (id) => {
    if (!id) {
        return null;
    }

    const url = `${SWAPI_URL}/people/${id}`;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }

        const result = await response.json();

        const mappedData = {
            name: result.name,
            birthYear: result.birthYear,
            gender: result.gender,
            height: result.height,
            mass: result.mass,
            hairColor: result.hairColor,
            eyecolor: result.eyeColor,
            movies: result.movies
        };
        
        return mappedData;

    } catch (error) {
        console.error('Erro ao buscar pessoa:', error);
        throw error;
    }
};