import Search from '../Search/Search';
import Results from '../Results/Results';
import { useState, useEffect } from 'react';
import { SearchServiceSwapi } from '../../services/SearchServiceSwapi';

function StarWarsSearch() {
    const [searchParams, setSearchParams] = useState({ resource: null, searchTerm: null });
    const [data, setData] = useState([]);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {

        const loadData = async () => {
            const { resource, searchTerm } = searchParams;

            if (!resource || !searchTerm) {
                setData([]);
                return;
            }

            setLoading(true);
            setError(null);
            try {
                const results = await SearchServiceSwapi(resource, searchTerm);
                setData(results);
            } catch (err) {
                console.error("Erro ao buscar dados do Star Wars:", err);
                setData([]);

                if (err.response?.status >= 500) {
                    setError('Erro interno do servidor. Tente novamente mais tarde.');
                } else if (err.response?.status === 404) {
                    setError(null);
                } else {
                    setError('Falha ao buscar dados. Tente novamente.');
                }
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [searchParams.resource, searchParams.searchTerm]);

    const handleSearch = (searchType, searchTerm) => {
        setError(null);
        setSearchParams({
            resource: searchType,
            searchTerm: searchTerm
        });
    };

    return (
        <>
            <Search onSearch={handleSearch} isloading={loading} />
            {<Results data={data} error={error} isLoading={loading} />}
        </>
    );
}

export default StarWarsSearch;