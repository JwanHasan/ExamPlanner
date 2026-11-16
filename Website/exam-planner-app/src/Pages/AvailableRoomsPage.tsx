import {useEffect, useRef, useState} from "react";
import {useNavigate} from "react-router-dom";
import {getAvailableRooms} from "../api/features/rooms/roomApi.ts";
import type {AvailableRoom} from "../api/features/rooms/roomTypes.ts";
import {getErrorMessage} from "../api/errorHandler.ts";
import TopBar from "./Shared/TopBar.tsx";
import "./Themes/MitVia.css";
import "./Themes/AvailableRoomsPage.css";

const campusesFrom = (
    rooms: AvailableRoom[]
): string[] => {
    return Array.from(
        new Set(
            rooms
                .map(
                    room =>
                        room.campus.trim()
                )
                .filter(
                    campus =>
                        campus.length > 0
                )
        )
    )
        .sort(
            (
                first,
                second
            ) =>
                first.localeCompare(
                    second
                )
        );
};

const AvailableRoomsPage = () => {
    const navigate =
        useNavigate();
    
    const requestSequence =
        useRef(0);
    
    const [
        rooms,
        setRooms
    ] =
        useState<
            AvailableRoom[]
        >([]);
    
    const [
        campuses,
        setCampuses
    ] =
        useState<string[]>([]);
    
    const [
        search,
        setSearch
    ] =
        useState("");
    
    const [
        campus,
        setCampus
    ] =
        useState("");
    
    const [
        date,
        setDate
    ] =
        useState("");
    
    const [
        filtersOpen,
        setFiltersOpen
    ] =
        useState(true);
    
    const [
        loading,
        setLoading
    ] =
        useState(true);
    
    const [
        error,
        setError
    ] =
        useState<string | null>(
            null
        );

    useEffect(
        () => {
            const requestId =
                ++requestSequence.current;

            let cancelled =
                false;
            
            getAvailableRooms()
                .then(
                    result => {
                        if (
                            cancelled ||
                            requestId !==
                            requestSequence.current
                        ) {
                            return;
                        }
                        
                        setRooms(
                            result
                        );

                        setCampuses(
                            campusesFrom(
                                result
                            )
                        );

                        setError(null);
                    }
                )
                .catch(
                    loadError => {
                        if (
                            cancelled ||
                            requestId !==
                            requestSequence.current
                        ) {
                            return;
                        }

                        setError(
                            getErrorMessage(
                                loadError
                            )
                        );
                    }
                )
                .finally(
                    () => {
                        if (
                            !cancelled &&
                            requestId ===
                            requestSequence.current
                        ) {
                            setLoading(
                                false
                            );
                        }
                    }
                );

            return () => {

                cancelled =
                    true;
            };
        },
        []
    );

    const refreshRooms =
        async (
            nextCampus: string,
            nextDate: string
        ) => {

            const requestId =
                ++requestSequence.current;
            
            setLoading(true);
            setError(null);
            
            try {

                const result =
                    await getAvailableRooms({
                        campus:
                            nextCampus ||
                            undefined,
                        date:
                            nextDate ||
                            undefined
                    });

                if (
                    requestId !==
                    requestSequence.current
                ) {
                    return;
                }

                setRooms(
                    result
                );

            }
            
            catch (loadError) {

                if (
                    requestId !==
                    requestSequence.current
                ) {
                    return;
                }
                
                setError(
                    getErrorMessage(
                        loadError
                    )
                );
            }
            
            finally {
                if (
                    requestId ===
                    requestSequence.current
                ) {
                    setLoading(
                        false
                    );
                }
            }
        };
    
    const changeCampus = (
        nextCampus: string) => {

        setCampus(
            nextCampus
        );

        void refreshRooms(
            nextCampus,
            date
        );
    };

    const changeDate = (
        nextDate: string) => {

        setDate(
            nextDate
        );

        void refreshRooms(
            campus,
            nextDate
        );
    };
    
    const clearFilters = () => {
        setCampus("");
        setDate("");
        setSearch("");
        void refreshRooms(
            "",
            ""
        );
    };

    const normalizedSearch =
        search
            .trim()
            .toLowerCase();

    const visibleRooms =
        normalizedSearch
            ? rooms.filter(
                room =>
                    room.name
                        .toLowerCase()
                        .includes(
                            normalizedSearch
                        )
                    ||
                    room.campus
                        .toLowerCase()
                        .includes(
                            normalizedSearch
                        )
            )
            : rooms;

    return (
        <div className="via-page">
            <TopBar />
            <div className="via-titlebar">
                <div>
                    <h1>
                        Find available rooms
                    </h1>
                    <p className="room-title-caption">
                        Search rooms and filter
                        them by location and date.
                    </p>
                </div>
            </div>
            
            <main className="via-content room-page-content">
                
                {/* Search + filter toggle */}
                <section className="room-search-controls">
                    <div className="room-search-row">
                        <label className="room-search-box">
                            <svg
                                viewBox="0 0 24 24"
                                aria-hidden="true"
                            >
                                <circle
                                    cx="11"
                                    cy="11"
                                    r="7"
                                />
                                <path d="M16.5 16.5 21 21"/>
                            </svg>
                            <input
                                type="search"
                                value={
                                    search
                                }
                                placeholder="Search available rooms"
                                aria-label="Search available rooms"
                                onChange={
                                    event =>
                                        setSearch(
                                            event.target.value
                                        )
                                }
                            />
                        </label>
                        <button
                            type="button"
                            className="room-filter-toggle"
                            aria-expanded={
                                filtersOpen
                            }
                            onClick={
                                () =>
                                    setFiltersOpen(
                                        current =>
                                            !current
                                    )
                            }
                        >
                            <span
                                aria-hidden="true"
                                className="room-filter-arrow"
                            >
                                {
                                    filtersOpen
                                        ? "⌃"
                                        : "⌄"
                                }
                            </span>
                            {
                                filtersOpen
                                    ? "Hide filters"
                                    : "Show filters"
                            }
                        </button>
                    </div>
                    
                    {/* Only the filters required by Sprint 5 */}
                    {filtersOpen && (
                        <div className="room-filter-panel">
                            <label className="room-filter-field">
                                <span>
                                    Location
                                </span>
                                <select
                                    value={
                                        campus
                                    }
                                    onChange={
                                        event =>
                                            changeCampus(
                                                event.target.value
                                            )
                                    }
                                >
                                    <option value="">
                                        All locations
                                    </option>
                                    {
                                        campuses.map(
                                            item => (
                                                <option
                                                    key={
                                                        item
                                                    }
                                                    value={
                                                        item
                                                    }
                                                >
                                                    {
                                                        item
                                                    }
                                                </option>
                                            )
                                        )
                                    }
                                </select>
                            </label>
                            <label className="room-filter-field">
                                <span>
                                    Date
                                </span>
                                <input
                                    type="date"
                                    value={
                                        date
                                    }
                                    onChange={
                                        event =>
                                            changeDate(
                                                event.target.value
                                            )
                                    }
                                />
                            </label>
                            <button
                                type="button"
                                className="via-button-secondary room-clear-button"
                                onClick={
                                    clearFilters
                                }
                            >
                                Clear filters
                            </button>
                        </div>
                    )}
                </section>
                
                {/* Result summary */}
                <section className="room-results-header">
                    <div>
                        <h2>
                            {
                                date
                                    ? "Available rooms"
                                    : "Rooms"
                            }
                        </h2>
                        <p className="via-caption">
                            {loading
                                ? "Checking rooms..."
                                : `${visibleRooms.length} room${visibleRooms.length === 1 ? "" : "s"} found.`}

                            {!date &&
                                !loading && (
                                    <>
                                        {" "}
                                        Choose a date
                                        to check actual
                                        room availability.
                                    </>
                                )}
                        </p>
                    </div>
                    
                    {(campus ||
                        date ||
                        search) && (
                        <div className="room-active-filters">
                            {campus && (
                                <span>
                                    {
                                        campus
                                    }
                                </span>
                            )}
                            
                            {date && (
                                <span>
                                    {
                                        date
                                    }
                                </span>
                            )}
                            
                            {search && (
                                <span>
                                    Search: {
                                    search
                                }
                                </span>
                            )}
                        </div>
                    )}
                </section>
                
                {/* Error */}
                {error && (

                    <div
                        className="via-field-error"
                        role="alert"
                    >
                        {error}
                    </div>
                )}
                
                {/* Loading */}
                {loading && (
                    <div className="room-empty-state">
                        Checking available rooms...
                    </div>
                )}
                
                {/* Empty */}
                {!loading &&
                    !error &&
                    visibleRooms.length === 0 && (
                        <div className="room-empty-state">
                            <h2>
                                No rooms found
                            </h2>
                            <p>
                                Try another room name,
                                location or date.
                            </p>
                        </div>
                    )}
                
                {/* Results */}
                {!loading &&
                    !error &&
                    visibleRooms.length > 0 && (
                        <div className="room-results-grid">
                            {
                                visibleRooms.map(
                                    room => (
                                        <article
                                            key={room.id}
                                            className="room-result-card"
                                        >
                                            <div className="room-result-heading">
                                                <h3>
                                                    {
                                                        room.name
                                                    }
                                                </h3>
                                                {date && (
                                                    <span className="room-available-tag">
                                                    Available
                                                </span>
                                                )}
                                            </div>
                                            <dl className="room-result-details">
                                                <div>
                                                    <dt>
                                                        Location
                                                    </dt>
                                                    <dd>
                                                        {
                                                            room.campus ||
                                                            "Not specified"
                                                        }
                                                    </dd>
                                                </div>
                                                <div>
                                                    <dt>
                                                        Capacity
                                                    </dt>
                                                    <dd>
                                                        {
                                                            room.capacity
                                                        }
                                                    </dd>
                                                </div>
                                            </dl>
                                        </article>
                                    )
                                )
                            }
                        </div>
                    )}
                
                <footer className="via-actions">
                    <button
                        type="button"
                        className="via-button-secondary"
                        onClick={
                            () =>
                                navigate(
                                    "/Main"
                                )
                        }
                    >
                        « Main page
                    </button>
                </footer>
            </main>
        </div>
    );
};

export default AvailableRoomsPage;