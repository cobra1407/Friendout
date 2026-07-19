import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Plus, Trash2, Package, Edit, Check } from 'lucide-react';
import { getTranslation } from '@/i18n';
import { toast } from 'sonner';

interface EquipmentItem {
    id: string;
    name: string;
    isChecked: boolean;
}

interface EquipmentManagerProps {
    equipment: string[];
    onChange: (equipment: string[]) => void;
    showChecklist?: boolean;
    onChecklistChange?: (checkedItems: string[]) => void;
    checkedItems?: string[];
    /** Optional action rendered next to the section title (e.g. "prefill from a list"). */
    headerAction?: React.ReactNode;
}

export default function EquipmentManager({
    equipment,
    onChange,
    showChecklist = false,
    onChecklistChange,
    checkedItems = [],
    headerAction
}: EquipmentManagerProps) {
    const [newItem, setNewItem] = useState('');
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editValue, setEditValue] = useState('');

    const equipmentItems: EquipmentItem[] = equipment.map((item, index) => ({
        id: `eq-${index}`,
        name: item,
        isChecked: checkedItems.includes(item)
    }));

    const addEquipment = () => {
        const trimmed = newItem.trim();

        // Empty input: nothing to add. Silently no-op instead of toasting "already
        // exists", so Enter on an empty field can be handled by a parent form
        // (e.g. to submit) without a spurious error.
        if (!trimmed) return;

        if (equipment.includes(trimmed)) {
            toast.error(getTranslation("equipment_manager.error.equipment_exist"));
            return;
        }

        onChange([...equipment, trimmed]);
        setNewItem('');
    };

    const removeEquipment = (index: number) => {
        onChange(equipment.filter((_, i) => i !== index));
    };

    const startEditing = (item: EquipmentItem) => {
        setEditingId(item.id);
        setEditValue(item.name);
    };

    const saveEdit = (index: number) => {
        if (editValue.trim() && editValue.trim() !== equipment[index]) {
            const updated = [...equipment];
            updated[index] = editValue.trim();
            onChange(updated);
        }
        setEditingId(null);
        setEditValue('');
    };


    const handleChecklistChange = (itemName: string, isChecked: boolean) => {
        if (onChecklistChange) {
            const updatedCheckedItems = isChecked
                ? [...checkedItems, itemName]
                : checkedItems.filter(item => item !== itemName);
            onChecklistChange(updatedCheckedItems);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            addEquipment();
        }
    };

    const handleEditKeyPress = (e: React.KeyboardEvent, index: number) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            saveEdit(index);
        }
    };

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between gap-2">
                <div>
                    <Label className="text-base font-medium">
                        {getTranslation("equipment_manager.section_label")}
                    </Label>
                    <p className="text-sm text-muted-foreground">
                        {getTranslation("equipment_manager.section_description")}
                    </p>
                </div>
                {headerAction}
            </div>

            <div className="flex items-center gap-2">
                <Input
                    value={newItem}
                    onChange={(e) => setNewItem(e.target.value)}
                    onKeyDown={handleKeyPress}
                    placeholder={getTranslation("equipment_manager.input_placeholder")}
                    className="flex-1"
                    data-equipment-manager-input="true"
                />
                <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={addEquipment}
                    disabled={!newItem.trim()}
                >
                    <Plus className="w-4 h-4" />
                    {getTranslation("equipment_manager.new_button")}
                </Button>
            </div>

            {equipment.length > 0 && (
                <Card>
                    <CardHeader className="pb-3">
                        <CardTitle className="text-sm font-medium flex items-center gap-2">
                            <Package className="w-4 h-4" />
                            {getTranslation("equipment_manager.card_title", { count: String(equipment.length) })}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-2 overflow-y-auto max-h-80">
                            {equipmentItems.map((item, index) => (
                                <div
                                    key={item.id}
                                    className="flex items-center justify-between bg-muted rounded-lg px-3 py-2 "
                                >
                                    <div className="flex items-center gap-3 flex-1">
                                        {showChecklist && (
                                            <Checkbox
                                                checked={item.isChecked}
                                                onCheckedChange={(checked) =>
                                                    handleChecklistChange(item.name, checked === true)
                                                }
                                                className="shrink-0"
                                            />
                                        )}

                                        {editingId === item.id ? (
                                            <div className="flex items-center gap-2 flex-1">
                                                <Input
                                                    value={editValue}
                                                    onChange={(e) => setEditValue(e.target.value)}
                                                    onKeyDown={(e) => handleEditKeyPress(e, index)}
                                                    className="flex-1"
                                                    autoFocus
                                                    data-equipment-manager-input="true"
                                                />
                                                <Button
                                                    type="button"
                                                    variant="ghost"
                                                    size="sm"
                                                    onClick={() => saveEdit(index)}
                                                    className="p-1 h-6 w-6 text-green-600 hover:text-green-700 hover:bg-green-500/10"
                                                >
                                                    <Check className="w-3 h-3" />
                                                </Button>
                                            </div>
                                        ) : (
                                            <span className={`text-sm ${item.isChecked && showChecklist ? 'line-through text-muted-foreground' : ''}`}>
                                                {item.name}
                                            </span>
                                        )}
                                    </div>

                                    <div className="flex items-center gap-1">
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => startEditing(item)}
                                            className="p-1 h-6 w-6 text-blue-600 hover:text-blue-700 hover:bg-blue-500/10"
                                            title={getTranslation("equipment_manager.edit_title")}
                                        >
                                            <Edit className="w-3 h-3" />
                                        </Button>
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="sm"
                                            onClick={() => removeEquipment(index)}
                                            className="p-1 h-6 w-6 text-destructive hover:text-destructive hover:bg-destructive/10"
                                            title={getTranslation("equipment_manager.remove_title")}
                                        >
                                            <Trash2 className="w-3 h-3" />
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </CardContent>
                </Card>
            )}

            {equipment.length === 0 && (
                <div className="text-center py-6 border-2 border-dashed border-border rounded-lg">
                    <Package className="w-8 h-8 text-muted-foreground mx-auto mb-2" />
                    <p className="text-sm text-muted-foreground mb-3">
                        {getTranslation("equipment_manager.empty_label")}
                    </p>
                    <p className="text-xs text-muted-foreground mb-4">
                        {getTranslation("equipment_manager.empty_examples")}
                    </p>
                </div>
            )}

            {showChecklist && checkedItems.length > 0 && (
                <div className="text-sm text-muted-foreground">
                    {getTranslation("equipment_manager.checklist_summary", {
                        checked: String(checkedItems.length),
                        total: String(equipment.length)
                    })}
                </div>
            )}
        </div>
    );
}
