import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { getEquipmentListIcon, getEquipmentListIconColorClasses } from "@/features/equipmentList/utils/equipmentListIcons";
import type { EquipmentList } from "@/features/equipmentList/types/equipmentList.type";
import { getTranslation } from "@/i18n";
import { Pencil, Trash2 } from "lucide-react";

const MAX_VISIBLE_ITEMS = 5;

interface EquipmentListCardProps {
    list: EquipmentList;
    onView: (list: EquipmentList) => void;
    onEdit: (list: EquipmentList) => void;
    onDelete: (list: EquipmentList) => void;
}

export function EquipmentListCard({ list, onView, onEdit, onDelete }: EquipmentListCardProps) {
    const visibleItems = list.items.slice(0, MAX_VISIBLE_ITEMS);
    const remainingCount = list.items.length - visibleItems.length;
    const Icon = getEquipmentListIcon(list.icon);
    const colors = getEquipmentListIconColorClasses(list.icon);

    return (
        <Card
            className="relative flex flex-col overflow-hidden pl-2 hover:shadow-lg transition-shadow cursor-pointer"
            onClick={() => onView(list)}
        >
            <div className={`absolute inset-y-0 left-0 w-1 ${colors.solid}`} />

            <CardHeader className="pb-3">
                <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-3 min-w-0">
                        <div className={`flex items-center justify-center w-9 h-9 rounded-full shrink-0 ${colors.bg} ${colors.icon}`}>
                            <Icon className="w-[18px] h-[18px]" />
                        </div>
                        <p className="text-base font-semibold truncate">{list.name}</p>
                    </div>
                    <Badge variant="outline" className="shrink-0 text-xs">
                        {list.items.length === 1
                            ? getTranslation("equipment_list.item_count_one")
                            : getTranslation("equipment_list.item_count", { count: String(list.items.length) })}
                    </Badge>
                </div>
            </CardHeader>

            <div className="border-t border-dashed mx-6" />

            <CardContent className="flex-grow pt-3">
                {list.items.length > 0 ? (
                    <div className="flex flex-wrap gap-1.5">
                        {visibleItems.map((item) => (
                            <Badge key={item} variant="secondary" className="font-normal">
                                {item}
                            </Badge>
                        ))}
                        {remainingCount > 0 && (
                            <Badge variant="secondary" className="font-normal text-muted-foreground">
                                {getTranslation("equipment_list.more_items", { count: String(remainingCount) })}
                            </Badge>
                        )}
                    </div>
                ) : (
                    <p className="text-sm text-muted-foreground">
                        {getTranslation("equipment_list.empty_items")}
                    </p>
                )}
            </CardContent>

            <CardFooter className="flex justify-end gap-2">
                <Button variant="outline" size="sm" onClick={(e) => { e.stopPropagation(); onEdit(list); }}>
                    <Pencil className="w-3.5 h-3.5" />
                    {getTranslation("common.edit")}
                </Button>
                <Button variant="outline" size="sm" onClick={(e) => { e.stopPropagation(); onDelete(list); }}>
                    <Trash2 className="w-3.5 h-3.5 text-destructive" />
                    {getTranslation("common.delete")}
                </Button>
            </CardFooter>
        </Card>
    );
}
