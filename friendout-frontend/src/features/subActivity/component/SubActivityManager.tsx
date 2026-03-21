import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import EnhancedLocationInput from '@/components/EnhancedLocationInput';
import { Trash2, Clock } from 'lucide-react';
import { useEffect, useRef } from 'react';
import type { SubActivity } from '@/features/subActivity/types/subActivity.type';
import { CalculateDuration } from '@/lib/utils/date.utils';
import AddSubActivityButton from './AddSubActivityButton';
import { getTranslation } from '@/i18n';

interface SubActivityManagerProps {
    subActivities: SubActivity[];
    onChange: (subActivities: SubActivity[]) => void;
}

interface TimeSelectProps {
    value: string;
    onChange: (nextValue: string) => void;
    className?: string
}

function TimeSelect({ value, onChange, className }: TimeSelectProps) {
    const inputRef = useRef<HTMLInputElement | null>(null);

    return (
        <div className={className}>
            <div className="relative">
                <Input
                    type="time"
                    step={300}
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    className="pr-10"
                    ref={inputRef}
                    aria-label={getTranslation("sub_activity_manager.time_picker_aria")}
                />
                <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="absolute right-1 top-1/2 h-7 w-7 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                    onClick={() => {
                        if (!inputRef.current) return;
                        (inputRef.current as HTMLInputElement & { showPicker?: () => void }).showPicker?.();
                        inputRef.current.focus();
                    }}
                    aria-label={getTranslation("sub_activity_manager.time_picker_aria")}
                >
                    <Clock className="h-4 w-4" />
                </Button>
            </div>
        </div>
    );
}

export default function SubActivityManager({ subActivities, onChange }: SubActivityManagerProps) {
    const scrollTargetId = useRef<string | null>(null);

    const addSubActivity = () => {
        const newSubActivity: SubActivity = {
            id: crypto.randomUUID(),
            name: '',
            localisation: null,
            startTime: '',
            endTime: '',
            description: '',
            price: 0,
            activityId: '',
            participants: []
        };

        scrollTargetId.current = newSubActivity.id;
        onChange([...subActivities, newSubActivity]);
    };

    const updateSubActivity = <K extends keyof SubActivity>(
        index: number,
        field: K,
        value: SubActivity[K]
    ) => {
        const updated = subActivities.map((item, i) =>
            i === index ? { ...item, [field]: value } : item
        );
        onChange(updated);
    };

    const removeSubActivity = (index: number) => {
        const updated = subActivities.filter((_, i) => i !== index);

        if (index === subActivities.length - 1 && updated.length > 0) {
            scrollTargetId.current = updated[updated.length - 1].id;
        } else {
            scrollTargetId.current = null;
        }

        onChange(updated);
    };

    useEffect(() => {
        if (scrollTargetId.current) {
            const element = document.getElementById(scrollTargetId.current);
            element?.scrollIntoView({ behavior: 'smooth', block: 'center' });
            scrollTargetId.current = null;
        }
    }, [subActivities]);

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <div>
                    <Label className="text-base font-medium">
                        {getTranslation("sub_activity_manager.section_label")}
                    </Label>
                    <p className="text-sm text-muted-foreground">
                        {getTranslation("sub_activity_manager.section_description")}
                    </p>
                </div>
            </div>

            {subActivities.length > 0 ? (
                <div className="space-y-4">
                    {subActivities.map((subActivity, index) => (
                        <Card key={subActivity.id} id={subActivity.id}>
                            <CardHeader className="pb-3">
                                <div className="flex items-center justify-between">
                                    <CardTitle className="text-sm font-medium flex items-center gap-2">
                                        <Clock className="w-4 h-4" />
                                        {getTranslation("sub_activity_manager.card_title", { index: String(index + 1) })}
                                    </CardTitle>
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="sm"
                                        onClick={() => removeSubActivity(index)}
                                    >
                                        <Trash2 className="w-4 h-4 text-red-600" />
                                    </Button>
                                </div>
                            </CardHeader>

                            <CardContent className="space-y-4">
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div className="space-y-2">
                                        <Label>{getTranslation("sub_activity_manager.name_label")}</Label>
                                        <Input
                                            value={subActivity.name}
                                            onChange={(e) => updateSubActivity(index, 'name', e.target.value)}
                                            required
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>{getTranslation("sub_activity_manager.price_label")}</Label>
                                        <Input
                                            type="number"
                                            min="0"
                                            step="0.01"
                                            value={subActivity.price}
                                            onChange={(e) => updateSubActivity(index, 'price', Number(e.target.value))}
                                        />
                                    </div>
                                </div>

                                <EnhancedLocationInput
                                    value={subActivity.localisation ?? null}
                                    onChange={(localisation) => updateSubActivity(index, 'localisation', localisation)}
                                    placeholder={getTranslation("sub_activity_manager.location_placeholder")}
                                />

                                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                    <div className="space-y-2">
                                        <Label>{getTranslation("sub_activity_manager.start_time_label")}</Label>
                                        <TimeSelect
                                            value={subActivity.startTime}
                                            onChange={(value) => updateSubActivity(index, 'startTime', value)}
                                        />
                                    </div>
                                    <div className="space-y-2">
                                        <Label>{getTranslation("sub_activity_manager.end_time_label")}</Label>
                                        <TimeSelect
                                            value={subActivity.endTime}
                                            onChange={(value) => updateSubActivity(index, 'endTime', value)}
                                            className='w-full'
                                        />
                                    </div>
                                    {subActivity.startTime && subActivity.endTime && (
                                        <div className="space-y-2">
                                            <Label>{getTranslation("sub_activity_manager.duration_label")}</Label>
                                            <Input
                                                value={CalculateDuration(subActivity.startTime, subActivity.endTime)}
                                                readOnly
                                            />
                                        </div>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label>{getTranslation("sub_activity_manager.description_label")}</Label>
                                    <Textarea
                                        value={subActivity.description}
                                        onChange={(e) => updateSubActivity(index, 'description', e.target.value)}
                                        rows={2}
                                    />
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                    <div className="flex justify-end mt-4">
                        <AddSubActivityButton onClick={addSubActivity} />
                    </div>
                </div>
            ) : (
                <div className="text-center py-6 border-2 border-dashed rounded-lg">
                    <Clock className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                    <p className="text-sm text-gray-500">
                        {getTranslation("sub_activity_manager.empty_label")}
                    </p>
                    <AddSubActivityButton onClick={addSubActivity} className="mt-4" />
                </div>
            )}
        </div>
    );
}
