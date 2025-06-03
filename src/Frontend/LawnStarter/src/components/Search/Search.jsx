import { useState, useEffect, useRef } from 'react';

const DEBOUNCE_DELAY = 800;

function Search({ onSearch = () => { }, isloading = false }) {
    const [searchType, setSearchType] = useState('people');
    const [searchTerm, setSearchTerm] = useState('');
    const debounceTimerRef = useRef(null);
    const searchInputRef = useRef(null); 

    const isSearchDisabled = searchTerm.trim() === '';

    useEffect(() => {
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }

        if (searchTerm.trim() !== '') {

            debounceTimerRef.current = setTimeout(() => {
                onSearch(searchType, searchTerm.trim());
            }, DEBOUNCE_DELAY);
        }

        return () => {
            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }
        };
    }, [searchTerm]);

    const handleSearch = () => {

        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
            debounceTimerRef.current = null;
        }

        if (searchTerm.trim() !== '') {
            onSearch(searchType, searchTerm.trim());
        }
    };

    const handleTermChange = (e) => {
        const newTerm = e.target.value;
        setSearchTerm(newTerm);
    };

    const handleTypeChange = (e) => {
        const newType = e.target.value;
        setSearchType(newType);
        setSearchTerm(''); 
        if (searchInputRef.current) {
            searchInputRef.current.focus(); 
        }
    };

    const placeholderText = searchType === 'people'
        ? "e.g. Chewbacca, Yoda, Boba Fett"
        : "e.g. A New Hope, The Empire Strikes Back";

    return (
        <div className="search-container">
            <h2>What are you searching for?</h2>

            <div>
                <div>
                    <input
                        type="radio"
                        id="people"
                        name="searchType"
                        value="people"
                        checked={searchType === 'people'}
                        onChange={handleTypeChange}
                    />
                    <label htmlFor="people">People</label>
                </div>

                <div>
                    <input
                        type="radio"
                        id="movie"
                        name="searchType"
                        value="movie"
                        checked={searchType === 'movie'}
                        onChange={handleTypeChange}
                    />
                    <label htmlFor="movie">Movies</label>
                </div>
            </div>

            <div>
                <input
                    type="text"
                    placeholder={placeholderText}
                    value={searchTerm}
                    ref={searchInputRef}
                    autoFocus
                    onChange={handleTermChange}
                />
            </div>

            <button disabled={isSearchDisabled || isloading} onClick={handleSearch}>
                {isloading ? 'Searching...' : 'Search'}
            </button>
        </div>
    );
}

export default Search;