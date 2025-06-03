import './Search.css'
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

    const handleTypeChange = (tipo) => {
        setSearchType(tipo);
        setSearchTerm(''); 
    };

    const placeholderText = searchType === 'people'
        ? "e.g. Chewbacca, Yoda, Boba Fett"
        : "e.g. A New Hope, The Empire Strikes Back";

    return (
        <section className="search-section">
        <h2 className="search-title">What are you searching for?</h2>
        
        <div className="search-options">
            <label className="radio-group">
                <div className={searchType === 'people' ? 'radio-input checked' : 'radio-input'} onClick={e => handleTypeChange('people')}></div>
                <span className="radio-label">People</span>
            </label>
            <label className="radio-group">
                <div className={searchType === 'movies' ? 'radio-input checked' : 'radio-input'} onClick={e => handleTypeChange('movies')}></div>
                <span className="radio-label">Movies</span>
            </label>
        </div>

        <input 
            type="text" 
            className="search-input" 
            placeholder={placeholderText}
            value={searchTerm}
            onChange={handleTermChange}
            ref={searchInputRef}/>

        <button className="search-button" onClick={handleSearch} disabled={isSearchDisabled}>
            Search
        </button>
    </section>


        
        
        
        
    );
}

export default Search;