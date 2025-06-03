const SWAPI_URL = 'https://localhost:44308/api';
const CACHE_DURATION = 5 * 60 * 1000; 

export const SearchPersonSwapiService = async (id) => {
    
    const getCacheKey = (id) => 
        `person:${id}`;

    const getFromCache = (cacheKey) => {
        try {
            const cached = sessionStorage.getItem(cacheKey);
            if (!cached) return null;
            
            const { data, timestamp } = JSON.parse(cached);
            const isExpired = Date.now() - timestamp > CACHE_DURATION;
            
            if (isExpired) {
                sessionStorage.removeItem(cacheKey);
                return null;
            }
            
            return data;
        } catch {
            return null;
        }
    };

    const setCache = (cacheKey, data) => {
        try {
            sessionStorage.setItem(cacheKey, JSON.stringify({
                data,
                timestamp: Date.now()
            }));
        } catch (error) {
            console.warn('Falha ao salvar no cache:', error);
        }
    };

    if (!id) {
        return null;
    }

    const cacheKey = getCacheKey(id);
    
    const cachedData = getFromCache(cacheKey);
    if (cachedData) {
        return cachedData;
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

        setCache(cacheKey, mappedData);
        
        return mappedData;

    } catch (error) {
        console.error('Erro ao buscar pessoa:', error);
        throw error;
    }
};