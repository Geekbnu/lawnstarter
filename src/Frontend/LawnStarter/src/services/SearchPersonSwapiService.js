const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:9090/api';

export const SearchPersonSwapiService = async (id) => {
    if (!id) {
        return null;
    }

    const url = `${BACKEND_URL}/people/${id}`;

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