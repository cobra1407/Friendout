import { Header } from "@/components/header";
import ActivityCard from "@/features/activity/components/ActivityCard";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import type { Activity } from "@/features/activity/types/activity.type";
import { ActivityToolbar } from "@/features/activity/components/ActivityToolsBar";
import { getActivities } from "@/features/activity/api/activity.api";
import { useCallback, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { type InfiniteData, useInfiniteQuery, useQueryClient } from "@tanstack/react-query";
import ActivityCardSkeleton from "@/features/activity/components/ActivityCardSkeleton";
import { authApi } from "@/features/auth/api/auth.api";
import { useAuth } from "@/features/auth/hooks/useAuth";
import type { ActivityFilter, TimeFilter } from "@/features/activity/types/activityFilter.type";
import { useRealtimeActivitiesFeed } from "@/features/realtime/hooks/useRealtimeActivitiesFeed";
import EmptyActivity from "../components/EmptyActivity";

const TAKE = 12;

export const ActivitiesPage = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const queryClient = useQueryClient();

    const [search, setSearch] = useState("");
    const [timeFilter, setTimeFilter] = useState<TimeFilter>("all");
    const [onlyMine, setOnlyMine] = useState(false);

    const loaderRef = useRef<HTMLDivElement>(null);
    const observerRef = useRef<IntersectionObserver | null>(null);

    // Each distinct combination of filters/search gets its own cache entry — switching back
    const queryKey = useMemo(
        () => ["activities", { search, timeFilter, onlyMine }] as const,
        [search, timeFilter, onlyMine]
    );

    const {
        data,
        isLoading,
        isFetchingNextPage,
        fetchNextPage,
        hasNextPage,
    } = useInfiniteQuery({
        queryKey,
        queryFn: ({ pageParam }) =>
            getActivities({
                skip: pageParam,
                take: TAKE,
                search,
                timeFilter,
                onlyOwnActivity: onlyMine,
            }),
        initialPageParam: 0,
        getNextPageParam: (lastPage, allPages) =>
            lastPage.length === TAKE ? allPages.flat().length : undefined,
    });

    const activities = data?.pages.flat() ?? [];

    const loaderCallbackRef = useCallback(
        (node: HTMLDivElement | null) => {
            loaderRef.current = node;
            if (observerRef.current) observerRef.current.disconnect();
            if (!node) return;

            observerRef.current = new IntersectionObserver(
                (entries) => {
                    if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
                        fetchNextPage();
                    }
                },
                { threshold: 0.1 }
            );
            observerRef.current.observe(node);
        },
        [hasNextPage, isFetchingNextPage, fetchNextPage]
    );

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

        queryClient.setQueryData<InfiniteData<Activity[], number>>(queryKey, (old) => {
            if (!old) return old;
            const alreadyExists = old.pages.some((page) => page.some((a) => a.id === activity.id));
            if (alreadyExists) return old;

            const [firstPage = [], ...restPages] = old.pages;
            return { ...old, pages: [[activity, ...firstPage], ...restPages] };
        });
    }, [search, timeFilter, onlyMine, user?.userId, queryClient, queryKey]);

    const handleDeleteActivity = useCallback((activityId: string) => {
        queryClient.setQueryData<InfiniteData<Activity[], number>>(queryKey, (old) => {
            if (!old) return old;
            return { ...old, pages: old.pages.map((page) => page.filter((a) => a.id !== activityId)) };
        });
    }, [queryClient, queryKey]);

    useRealtimeActivitiesFeed({
        onNewActivity: handleNewActivity,
        onDeletedActivity: handleDeleteActivity
    });

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

    const handleSearchChange = (val: string) => setSearch(val);

    const handleFilterChange = (filter: ActivityFilter) => {
        setTimeFilter(filter.timeFilter);
        setOnlyMine(filter.onlyOwnActivity);
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
            <div ref={loaderCallbackRef} className="h-10"></div>
        </ActivityLayout>
    );
};
