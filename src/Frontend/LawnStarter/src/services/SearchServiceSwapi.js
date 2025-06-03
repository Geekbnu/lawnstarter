const SWAPI_URL = 'https://localhost:44308/api';
const CACHE_DURATION = 5 * 60 * 1000; 

const getCacheKey = (resource, searchTerm) => 
    `${resource}_search:${searchTerm.toLowerCase()}`;

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

export const SearchServiceSwapi = async (resource, searchTerm) => {
    if (!resource || !searchTerm) {
        return [];
    }

    const cacheKey = getCacheKey(resource, searchTerm);
    
    const cachedData = getFromCache(cacheKey);
    if (cachedData) {
        return cachedData;
    }

    const url = `${SWAPI_URL}/${resource}/search/?query=${searchTerm}`;

    try {
        const response = await fetch(url);
        const result = await response.json();
        
        const mappedData = result.map(item => ({
            uid: item.uid,
            name: item.name,
            resource: item.type
        }));

        setCache(cacheKey, mappedData);
        
        return mappedData;

    } catch (e) {
        if (e.response?.status >= 500) {
            throw new Error("internal error");
        }
        return [];
    }
};