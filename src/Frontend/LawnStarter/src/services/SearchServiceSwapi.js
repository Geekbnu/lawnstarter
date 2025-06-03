const SWAPI_URL = 'http://localhost:8080/api';

export const SearchServiceSwapi = async (resource, searchTerm) => {
    if (!resource || !searchTerm) {
        return [];
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
        
        return mappedData;

    } catch (e) {
        if (e.response?.status >= 500) {
            throw new Error("internal error");
        }
        return [];
    }
};