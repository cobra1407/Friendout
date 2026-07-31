import { Header } from "@/components/header";
import ActivityCard from "@/features/activity/components/ActivityCard";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import type { Activity } from "@/features/activity/types/activity.type";
import { ActivityToolbar } from "@/features/activity/components/ActivityToolsBar";
import { getActivities } from "@/features/activity/api/activity.api";
import { useEffect, useState, useRef, useCallback } from "react";
import { useNavigate } from "react-router";
import ActivityCardSkeleton from "@/features/activity/components/ActivityCardSkeleton";
import { authApi } from "@/features/auth/api/auth.api";
import { useAuth } from "@/features/auth/hooks/useAuth";
import type { ActivityFilter, TimeFilter } from "@/features/activity/types/activityFilter.type";
import { useRealtimeActivitiesFeed } from "@/features/realtime/hooks/useRealtimeActivitiesFeed";
import EmptyActivity from "../components/EmptyActivity";

export const ActivitiesPage = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [activities, setActivities] = useState<Activity[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [skip, setSkip] = useState(0);
    const take = 12;
    const [hasMore, setHasMore] = useState(true);

    const [search, setSearch] = useState("");
    const [timeFilter, setTimeFilter] = useState<TimeFilter>("all");
    const [onlyMine, setOnlyMine] = useState(false);
    const [observerEnabled, setObserverEnabled] = useState(true);

    const loaderRef = useRef<HTMLDivElement>(null);

    const loadActivities = async (reset = false, opts?: {
        search?: string;
        timeFilter?: TimeFilter;
        onlyOwnActivity?: boolean;
    }) => {
        const currentSearch = opts?.search ?? search;
        const currentTimeFilter = opts?.timeFilter ?? timeFilter;
        const currentOnlyMine = opts?.onlyOwnActivity ?? onlyMine;

        if (!hasMore && !reset) return;

        if (reset) {
            setIsLoading(true);
        }

        try {
            const data = await getActivities({
                skip: reset ? 0 : skip,
                take,
                search: currentSearch,
                timeFilter: currentTimeFilter,
                onlyOwnActivity: currentOnlyMine
            });

            setActivities(prev => reset ? data : [...prev, ...data]);
            setSkip(prev => reset ? data.length : prev + data.length);
            setHasMore(data.length === take);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadActivities(true);
    }, []);

    // Prepend a newly created activity to the top of the list — but only when it would
    // actually belong there given the current view. A blind insert could show a "past"
    // activity while the user explicitly filtered for past ones, or someone else's activity
    // while "only mine" is active, or silently override an active search. In any of those
    // cases we simply skip the insert; the user sees it next time they clear the filter/search
    // or reload — no worse than before this feature existed.
    const handleNewActivity = useCallback((activity: Activity) => {
        if (search.trim() !== "") return;
        if (timeFilter === "past") return;
        if (onlyMine && activity.createdBy !== user?.userId) return;

        setActivities(prev => {
            if (prev.some(a => a.id === activity.id)) return prev;
            return [activity, ...prev];
        });
    }, [search, timeFilter, onlyMine, user?.userId]);

    const handleDeleteActivity = useCallback((activityId: string) => {
        setActivities(prev => prev.filter(a => a.id !== activityId));
    }, []);

    useRealtimeActivitiesFeed({
        onNewActivity: handleNewActivity,
        onDeletedActivity: handleDeleteActivity
    });

    // infinite scroll observer
    useEffect(() => {
        if (!observerEnabled) return;

        const observer = new IntersectionObserver(
            (entries) => {
                if (entries[0].isIntersecting && !isLoading && hasMore) {
                    loadActivities();
                }
            },
            { threshold: 0.1 }
        );

        if (loaderRef.current) observer.observe(loaderRef.current);
        return () => {
            if (loaderRef.current) observer.unobserve(loaderRef.current);
        };
    }, [isLoading, hasMore, skip, search, timeFilter, onlyMine, observerEnabled]);

    const handleViewDetails = (activityId: string) => {
        navigate(`/activities/${activityId}`);
    };

    const handleLogout = async () => {
        try {
            await authApi.logout();
            navigate('/login');
        } catch (error) {
            console.error(error);
        }
    };

    const handleSearchChange = (val: string) => {
        setObserverEnabled(false); // disable observer while loading
        setSearch(val);
        setSkip(0);
        setActivities([]);
        setHasMore(true);

        // reload activities with new search
        loadActivities(true, { search: val }).then(() => setObserverEnabled(true));
    };


    const handleFilterChange = (filter: ActivityFilter) => {
        setObserverEnabled(false);
        setTimeFilter(filter.timeFilter);
        setOnlyMine(filter.onlyOwnActivity);
        setSkip(0);
        setActivities([]);
        setHasMore(true);

        // reload activities with new filter
        loadActivities(true, { timeFilter: filter.timeFilter, onlyOwnActivity: filter.onlyOwnActivity }).then(() =>
            setObserverEnabled(true)
        );
    };



    return (
        <ActivityLayout
            header={<Header onCreateActivity={() => navigate("/activities/createActivity")} onLogout={handleLogout} />}
        >
            <ActivityToolbar
                search={search}
                filter={{ timeFilter, onlyOwnActivity: onlyMine }}
                onSearchChange={handleSearchChange}
                onFilterChange={handleFilterChange}
            />

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 min-w-full">
                {activities.map(activity => (
                    <ActivityCard
                        key={activity.id}
                        activity={activity}
                        onViewDetails={handleViewDetails}
                    />
                ))}

                {isLoading &&
                    Array.from({ length: 6 }).map((_, i) => (
                        <ActivityCardSkeleton key={i} />
                    ))
                }

                {!isLoading && activities.length === 0 && (
                    <div className="col-span-full">
                        <EmptyActivity />
                    </div>
                )}
            </div>

            {/* invisible div to trigger intersection observer */}
            <div ref={loaderRef} className="h-10"></div>
        </ActivityLayout>
    );
};
