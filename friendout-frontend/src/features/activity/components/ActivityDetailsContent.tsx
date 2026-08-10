import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import ActivityMainDetails from "@/features/activity/components/ActivityMainDetails";
import { getTranslation } from "@/i18n";
import ParticiationResponseCard from "@/features/participant/component/ParticipationResponseCard";
import SubActivityDetailsCard from "@/features/subActivity/component/SubActivityDetailsCard";
import EquipmentChecklist from "@/features/equipment/component/EquimentChecklist";
import ParticipantsCard from "@/features/participant/component/ParticipantsCard";
import CommentsSection from "@/features/comment/component/CommentSection";
import { isPast, formatDate } from "@/lib/utils/date.utils";
import ActivityCostSummary from "./ActivityCostSummary";

export interface ActivityDetailsContentProps {
    activity: ActivityDetails;
    currentUserId?: string;
    /** Participation */
    onMainParticipationChange: (status: ParticipationStatus) => void;
    onSubActivitiesParticipationChange: (
        status: ParticipationStatus,
        subActivityIds?: string[]
    ) => void;
    getSubActivitySelectedStatus: (subActivityId: string) => ParticipationStatus | null;
    getSubActivitiesSelectedStatus: () => ParticipationStatus | null;
    /** Equipment */
    onToggleEquipment: (equipmentId: string, quantity: number) => void;
    /** Comments */
    commentsProps: {
        newComment: string;
        currentUserId?: string;
        setNewComment: (val: string) => void;
        isSubmittingComment: boolean;
        onSubmit: () => void;
        editingCommentId?: string;
        editedCommentContent: string;
        setEditedCommentContent: (val: string) => void;
        handleEditComment: (comment: ActivityDetails["comments"][number]) => void;
        handleUpdateComment: (commentId: string) => void;
        cancelEdit: () => void;
        handleDeleteComment: (commentId: string) => void;
    };
}

const commentsSectionPropsFrom = (
    activity: ActivityDetails,
    currentUserId: string | undefined,
    commentsProps: ActivityDetailsContentProps["commentsProps"]
) => ({
    comments: activity.comments ?? [],
    currentUserId: currentUserId,
    newComment: commentsProps.newComment,
    setNewComment: commentsProps.setNewComment,
    isSubmittingComment: commentsProps.isSubmittingComment,
    onSubmit: commentsProps.onSubmit,
    editingCommentId: commentsProps.editingCommentId,
    editedCommentContent: commentsProps.editedCommentContent,
    setEditedCommentContent: commentsProps.setEditedCommentContent,
    handleEditComment: commentsProps.handleEditComment,
    handleUpdateComment: commentsProps.handleUpdateComment,
    cancelEdit: commentsProps.cancelEdit,
    handleDeleteComment: commentsProps.handleDeleteComment,
    formatCommentDate: formatDate,
});

export function ActivityDetailsContent({
    activity,
    currentUserId,
    onMainParticipationChange,
    onSubActivitiesParticipationChange,
    getSubActivitySelectedStatus,
    getSubActivitiesSelectedStatus,
    onToggleEquipment,
    commentsProps,
}: ActivityDetailsContentProps) {
    const commentsSectionProps = commentsSectionPropsFrom(
        activity,
        currentUserId,
        commentsProps
    );

    const pricedSubActivitiesCount = activity.subActivities.filter((s) => s.price).length;

    const mainDetailsProps = {
        title: activity.title,
        description: activity.description,
        startAt: activity.startAt,
        image: activity.image,
        localisation: activity.localisation,
        createdBy: activity.createdBy,
        price: {
            totalPrice: activity.totalPrice,
            estimatedPrice: activity.estimatedPrice,
            pricedSubActivitiesCount,
        },
        equipmentNames: activity.requiredEquipments.map((e) => e.name),
    };

    const mainResponseCard = !isPast(activity.startAt) && (
        <ParticiationResponseCard
            title={getTranslation('activity.response_main_activity')}
            selectedStatus={activity.userMainParticipation?.status ?? null}
            onResponse={onMainParticipationChange}
        />
    );

    const subActivityCards = activity.subActivities.map(sa => (
        <SubActivityDetailsCard
            key={sa.id}
            subActivity={sa}
            onResponse={onSubActivitiesParticipationChange}
            selectedStatus={getSubActivitySelectedStatus(sa.id)}
        />
    ));

    const participantsCard = (
        <ParticipantsCard participants={activity.participants} />
    );

    const commentsSection = (
        <CommentsSection {...commentsSectionProps} />
    );

    const equipmentChecklist =
        activity.requiredEquipments.length > 0 ? (
            <EquipmentChecklist
                activityEquipment={activity.requiredEquipments}
                userEquipments={activity.userEquipments}
                onToggleEquipment={onToggleEquipment}
            />
        ) : null;

    const allSubActivitiesResponseCard =
        activity.subActivities.length > 0 && new Date(activity.startAt) > new Date() ? (
            <ParticiationResponseCard
                title={getTranslation('activity.participation_sub_activities')}
                description={getTranslation('activity.participation_sub_activities_description')}
                selectedStatus={getSubActivitiesSelectedStatus()}
                onResponse={onSubActivitiesParticipationChange}
            />
        ) : null;

    const globalCostSummary = activity.subActivities.length > 0 ? (
        <ActivityCostSummary activity={activity} totalPrice={activity.totalPrice ?? 0} />
    ) : null;


    return (
        <div className="flex flex-col gap-6 lg:grid lg:grid-cols-[1fr_450px]">
            {/* Mobile */}
            <div className="flex flex-col gap-6 lg:hidden">
                <ActivityMainDetails {...mainDetailsProps} />
                {globalCostSummary}
                {mainResponseCard}
                {participantsCard}
                {subActivityCards}
                {equipmentChecklist}
                {commentsSection}
            </div>

            {/* Desktop left */}
            <div className="hidden lg:flex flex-col gap-6 min-w-0">
                <ActivityMainDetails {...mainDetailsProps} />
                {globalCostSummary}
                {subActivityCards}
                {commentsSection}
            </div>

            {/* Desktop right */}
            <div className="hidden lg:flex flex-col gap-6">
                {mainResponseCard}
                {allSubActivitiesResponseCard}
                {participantsCard}
                {equipmentChecklist}
            </div>
        </div>
    );
}
