import { useEffect, useState } from "react";
import { ActivitySearch } from "./ActivitySearch";
import { ActivityFilters } from "./ActivityFilters";
import type { ActivityFilter } from "@/features/activity/types/activityFilter.type";

interface Props {
    search: string;
    onSearchChange: (v: string) => void;
    filter: ActivityFilter;
    onFilterChange: (v: ActivityFilter) => void;
    className?: string;
}

/**
 * A toolbar component for the activity page.
 *
 * It includes a search bar and a filter bar.
 *
 * @param {string} search - The current search value.
 * @param {(v: string) => void} onSearchChange - A callback function to handle search value changes.
 * @param {ActivityFilter} filter - The current filter value.
 * @param {(v: ActivityFilter) => void} onFilterChange - A callback function to handle filter value changes.
 * @param {string} [className] - An optional className for the component.
 * @returns {React.ReactNode} A React component with the toolbar functionality.
 */
export const ActivityToolbar = ({
    search,
    onSearchChange,
    filter,
    onFilterChange,
    className
}: Props) => {
    const [localSearch, setLocalSearch] = useState(search);

    // Sync prop search
    useEffect(() => {
        setLocalSearch(search);
    }, [search]);

    // Debounce
    useEffect(() => {
        const timeout = setTimeout(() => {
            if (localSearch !== search) {
                onSearchChange(localSearch);
            }
        }, 500); // 500ms de debounce

        return () => clearTimeout(timeout);
    }, [localSearch, onSearchChange, search]);

    return (
        <div className={`${className || ""} my-8`}>
            <div className="flex flex-col sm:flex-row gap-4">
                <ActivitySearch
                    value={localSearch}
                    onChange={setLocalSearch}
                />
                <ActivityFilters
                    value={filter}
                    onChange={onFilterChange}
                />
            </div>
        </div>
    );
};
