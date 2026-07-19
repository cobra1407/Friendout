interface EquipmentListCardSkeletonProps {
}

const EquipmentListCardSkeleton = ({ }: EquipmentListCardSkeletonProps) => {
    return (
        <div className="w-full h-[200px] m-3 rounded-lg bg-muted animate-pulse" />
    );
};

export default EquipmentListCardSkeleton;
