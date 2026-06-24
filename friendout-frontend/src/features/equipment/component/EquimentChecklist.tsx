import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Package, CheckCircle2, Circle, Loader2 } from 'lucide-react';
import type { Equipment } from '../types/equipment.type';
import type { UserEquipment } from '../types/userEquipment';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCircleInfo } from '@fortawesome/free-solid-svg-icons';
import { getTranslation } from '@/i18n';

interface EquipmentChecklistProps {
    activityEquipment: Equipment[];
    userEquipments: UserEquipment[];
    onToggleEquipment: (equipmentId: string, quantity: number) => void;
}

export default function EquipmentChecklist({ activityEquipment, userEquipments, onToggleEquipment }: EquipmentChecklistProps) {
    const [isLoading, setIsLoading] = useState(true);

    // Charger les équipements de l'utilisateur depuis l'API
    useEffect(() => {
        const fetchUserEquipments = async () => {
            setIsLoading(false);
        };

        fetchUserEquipments();
    }, [userEquipments]);

    const handleItemToggle = (equipmentId: string, quantity: number) => {
        onToggleEquipment(equipmentId, quantity);
    };

    const requiredEquipmentIds = new Set(activityEquipment.map((equipment) => equipment.equipmentId));
    const checkedEquipmentIds = new Set(
        (userEquipments ?? [])
            .filter((equipment) => requiredEquipmentIds.has(equipment.equipmentId))
            .map((equipment) => equipment.equipmentId)
    );
    const checkedCount = checkedEquipmentIds.size;

    const getProgressPercentage = () => {
        if (!activityEquipment || activityEquipment.length === 0) return 0;
        return Math.round((checkedCount / activityEquipment.length) * 100);
    };

    const getProgressColor = (percentage: number) => {
        if (percentage === 100) return 'text-green-600';
        if (percentage >= 75) return 'text-blue-600';
        if (percentage >= 50) return 'text-yellow-600';
        return 'text-red-600';
    };

    if (!activityEquipment || activityEquipment.length === 0) {
        return null;
    }

    if (isLoading) {
        return (
            <Card>
                <CardHeader>
                    <CardTitle className="text-lg flex items-center gap-2">
                        <Package className="w-5 h-5" />
                        {getTranslation('equipment.loading')}
                    </CardTitle>
                </CardHeader>
                <CardContent className="flex justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                </CardContent>
            </Card>
        );
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-lg flex items-center gap-2">
                    <Package className="w-5 h-5" />
                    {getTranslation('equipment.my_equipment')}
                </CardTitle>
                <div className="text-sm text-muted-foreground">
                    {getTranslation('equipment.check_instruction')}
                </div>
            </CardHeader>
            <CardContent className="space-y-4">
                {/* Progress bar */}
                <div className="space-y-2">
                    <div className="flex justify-between items-center text-sm">
                        <span>{getTranslation('equipment.progress')}</span>
                        <span className={`font-medium ${getProgressColor(getProgressPercentage())}`}>
                            {checkedCount} / {activityEquipment.length}
                        </span>
                    </div>
                    <div className="w-full bg-muted rounded-full h-2">
                        <div
                            className={`h-2 rounded-full transition-all duration-300 ${getProgressPercentage() === 100
                                ? 'bg-green-500'
                                : getProgressPercentage() >= 75
                                    ? 'bg-blue-500'
                                    : getProgressPercentage() >= 50
                                        ? 'bg-yellow-500'
                                        : 'bg-red-500'
                                }`}
                            style={{ width: `${getProgressPercentage()}%` }}
                        />
                    </div>
                    <div className="text-xs text-muted-foreground">
                        {getProgressPercentage() === 100 && (
                            <span className="text-green-600 font-medium">{getTranslation('equipment.all_equipment_ready')}</span>
                        )}
                        {getProgressPercentage() >= 75 && getProgressPercentage() < 100 && (
                            <span className="text-blue-600">{getTranslation('equipment.almost_ready')}</span>
                        )}
                        {getProgressPercentage() >= 50 && getProgressPercentage() < 75 && (
                            <span className="text-yellow-600">{getTranslation('equipment.half_equipment')}</span>
                        )}
                        {getProgressPercentage() < 50 && (
                            <span className="text-red-600">{getTranslation('equipment.missing_equipment')}</span>
                        )}
                    </div>
                </div>

                {/* Equipment list */}
                <div className="space-y-2 max-h-[400px] overflow-y-auto">
                    {activityEquipment.map((equipment, index) => {
                        const isChecked = checkedEquipmentIds.has(equipment.equipmentId);
                        return (
                            <div
                                key={index}
                                className={`flex items-center gap-3 p-3 rounded-lg border transition-all duration-200 cursor-pointer hover:bg-muted/50 ${isChecked ? 'border-green-500/30 bg-green-500/10' : 'border-border hover:bg-muted/50'
                                    }`}
                                onClick={() => handleItemToggle(equipment.equipmentId, isChecked ? 0 : 1)}
                            >
                                <div className="flex items-center justify-center w-5 h-5">
                                    {isChecked ? (
                                        <CheckCircle2 className="w-5 h-5 text-green-600" />
                                    ) : (
                                        <Circle className="w-5 h-5 text-muted-foreground" />
                                    )}
                                </div>
                                <span className={`text-sm flex-1 ${isChecked ? 'line-through text-muted-foreground' : ''}`}>
                                    {equipment.name}
                                </span>
                                {isChecked && (
                                    <Badge variant="secondary" className="text-xs">
                                        {getTranslation('equipment.owned')}
                                    </Badge>
                                )}
                                {equipment.description && (
                                    <div className='relative inline-block'>
                                        <Tooltip>
                                            <TooltipTrigger asChild>
                                                <FontAwesomeIcon icon={faCircleInfo} className="w-4 h-4 text-muted-foreground mx-2" />
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>{equipment.description}</p>
                                            </TooltipContent>
                                        </Tooltip>
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>

                <div className="pt-3 border-t">
                    <div className="text-sm text-muted-foreground">
                        <span className="font-medium">{checkedCount}</span> {getTranslation('equipment.checked_count', { total: activityEquipment.length })}
                    </div>
                    {checkedCount > 0 && (
                        <div className="text-xs text-muted-foreground">
                            {getTranslation('equipment.auto_saved')}
                        </div>
                    )}
                </div>
            </CardContent>
        </Card>
    );
}
