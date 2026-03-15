export type TimeFilter = "all" | "upcoming" | "past";

export interface ActivityFilter {
    timeFilter: TimeFilter;
    onlyOwnActivity: boolean;
}
